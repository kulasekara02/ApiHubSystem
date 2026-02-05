using ApiHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Analytics.Queries;

public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, DashboardData>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetDashboardDataQueryHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<DashboardData> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? _dateTime.UtcNow.AddDays(-30);
        var toDate = request.ToDate ?? _dateTime.UtcNow;

        var query = _context.ApiRecords
            .Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
            .AsNoTracking();

        if (request.ConnectorId.HasValue)
            query = query.Where(r => r.ConnectorId == request.ConnectorId.Value);

        var records = await query.ToListAsync(cancellationToken);

        var totalRequests = records.Count;
        var successfulRequests = records.Count(r => r.IsSuccess);
        var failedRequests = totalRequests - successfulRequests;
        var successRate = totalRequests > 0 ? (double)successfulRequests / totalRequests * 100 : 0;
        var averageLatency = records.Any() ? records.Average(r => r.DurationMs) : 0;

        var statusDistribution = records
            .GroupBy(r => r.StatusCode)
            .Select(g => new StatusCodeDistribution(g.Key, g.Count()))
            .OrderBy(s => s.StatusCode)
            .ToList();

        var requestsOverTime = records
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new RequestsOverTime(
                g.Key,
                g.Count(r => r.IsSuccess),
                g.Count(r => !r.IsSuccess)))
            .OrderBy(r => r.Date)
            .ToList();

        var topConnectors = await _context.ApiRecords
            .Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate)
            .GroupBy(r => new { r.ConnectorId, r.Connector.Name })
            .Select(g => new TopConnectors(
                g.Key.ConnectorId,
                g.Key.Name,
                g.Count(),
                g.Count(r => r.IsSuccess) * 100.0 / g.Count()))
            .OrderByDescending(c => c.RequestCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        var sortedLatencies = records.Select(r => r.DurationMs).OrderBy(l => l).ToList();
        var latencyPercentiles = new List<LatencyPercentiles>();
        if (sortedLatencies.Any())
        {
            latencyPercentiles.Add(new LatencyPercentiles("p50", GetPercentile(sortedLatencies, 50)));
            latencyPercentiles.Add(new LatencyPercentiles("p75", GetPercentile(sortedLatencies, 75)));
            latencyPercentiles.Add(new LatencyPercentiles("p90", GetPercentile(sortedLatencies, 90)));
            latencyPercentiles.Add(new LatencyPercentiles("p95", GetPercentile(sortedLatencies, 95)));
            latencyPercentiles.Add(new LatencyPercentiles("p99", GetPercentile(sortedLatencies, 99)));
        }

        return new DashboardData(
            totalRequests,
            successfulRequests,
            failedRequests,
            Math.Round(successRate, 2),
            Math.Round(averageLatency, 2),
            statusDistribution,
            requestsOverTime,
            topConnectors,
            latencyPercentiles);
    }

    private static double GetPercentile(List<long> sortedValues, int percentile)
    {
        if (sortedValues.Count == 0) return 0;
        var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
        return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Count - 1))];
    }
}
