using MediatR;

namespace ApiHub.Application.Features.Analytics.Queries;

public record GetDashboardDataQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? ConnectorId = null) : IRequest<DashboardData>;

public record DashboardData(
    int TotalRequests,
    int SuccessfulRequests,
    int FailedRequests,
    double SuccessRate,
    double AverageLatencyMs,
    List<StatusCodeDistribution> StatusCodeDistribution,
    List<RequestsOverTime> RequestsOverTime,
    List<TopConnectors> TopConnectors,
    List<LatencyPercentiles> LatencyPercentiles);

public record StatusCodeDistribution(int StatusCode, int Count);
public record RequestsOverTime(DateTime Date, int SuccessCount, int FailureCount);
public record TopConnectors(Guid ConnectorId, string ConnectorName, int RequestCount, double SuccessRate);
public record LatencyPercentiles(string Percentile, double ValueMs);
