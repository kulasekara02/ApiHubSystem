using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public class DeleteScheduledJobCommandHandler : IRequestHandler<DeleteScheduledJobCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public DeleteScheduledJobCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<Result<bool>> Handle(DeleteScheduledJobCommand request, CancellationToken cancellationToken)
    {
        var scheduledJob = await _context.ScheduledJobs
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (scheduledJob == null)
        {
            return Result<bool>.Failure("Scheduled job not found");
        }

        var jobName = scheduledJob.Name;

        _context.ScheduledJobs.Remove(scheduledJob);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.ScheduledJobDeleted,
            nameof(ScheduledJob),
            request.Id.ToString(),
            oldValues: new { Name = jobName },
            cancellationToken: cancellationToken);

        return Result<bool>.Success(true);
    }
}
