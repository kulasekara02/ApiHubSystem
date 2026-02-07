using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.ScheduledJobs.Queries;

public record GetScheduledJobsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsEnabled = null,
    Guid? ConnectorId = null) : IRequest<PaginatedList<ScheduledJobDto>>;

public record ScheduledJobDto(
    Guid Id,
    string Name,
    string? Description,
    Guid ConnectorId,
    string ConnectorName,
    string Endpoint,
    string Method,
    string CronExpression,
    bool IsEnabled,
    DateTime? LastRunAt,
    DateTime? NextRunAt,
    string? LastRunStatus,
    int SuccessCount,
    int FailureCount,
    DateTime CreatedAt);
