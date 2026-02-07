using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.ScheduledJobs.Queries;

public class GetJobExecutionHistoryQueryHandler : IRequestHandler<GetJobExecutionHistoryQuery, PaginatedList<JobExecutionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetJobExecutionHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<JobExecutionDto>> Handle(GetJobExecutionHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ScheduledJobExecutions
            .Include(e => e.ScheduledJob)
            .Where(e => e.ScheduledJobId == request.ScheduledJobId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(e => e.Status == request.Status);
        }

        var projectedQuery = query
            .OrderByDescending(e => e.StartedAt)
            .Select(e => new JobExecutionDto(
                e.Id,
                e.ScheduledJobId,
                e.ScheduledJob.Name,
                e.StartedAt,
                e.CompletedAt,
                e.Status,
                e.HttpStatusCode,
                e.ResponseBody,
                e.ErrorMessage,
                e.DurationMs));

        return await PaginatedList<JobExecutionDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
