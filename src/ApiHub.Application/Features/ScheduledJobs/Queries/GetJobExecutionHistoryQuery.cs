using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.ScheduledJobs.Queries;

public record GetJobExecutionHistoryQuery(
    Guid ScheduledJobId,
    int PageNumber = 1,
    int PageSize = 20,
    string? Status = null) : IRequest<PaginatedList<JobExecutionDto>>;

public record JobExecutionDto(
    Guid Id,
    Guid ScheduledJobId,
    string JobName,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Status,
    int? HttpStatusCode,
    string? ResponseBody,
    string? ErrorMessage,
    long? DurationMs);
