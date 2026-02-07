using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.ScheduledJobs.Queries;

public class GetScheduledJobsQueryHandler : IRequestHandler<GetScheduledJobsQuery, PaginatedList<ScheduledJobDto>>
{
    private readonly IApplicationDbContext _context;

    public GetScheduledJobsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ScheduledJobDto>> Handle(GetScheduledJobsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ScheduledJobs
            .Include(j => j.Connector)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(j =>
                j.Name.Contains(request.SearchTerm) ||
                (j.Description != null && j.Description.Contains(request.SearchTerm)) ||
                j.Endpoint.Contains(request.SearchTerm));
        }

        if (request.IsEnabled.HasValue)
        {
            query = query.Where(j => j.IsEnabled == request.IsEnabled.Value);
        }

        if (request.ConnectorId.HasValue)
        {
            query = query.Where(j => j.ConnectorId == request.ConnectorId.Value);
        }

        var projectedQuery = query
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new ScheduledJobDto(
                j.Id,
                j.Name,
                j.Description,
                j.ConnectorId,
                j.Connector.Name,
                j.Endpoint,
                j.Method,
                j.CronExpression,
                j.IsEnabled,
                j.LastRunAt,
                j.NextRunAt,
                j.LastRunStatus,
                j.SuccessCount,
                j.FailureCount,
                j.CreatedAt));

        return await PaginatedList<ScheduledJobDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
