using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Cronos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public class ToggleJobCommandHandler : IRequestHandler<ToggleJobCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public ToggleJobCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _context = context;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<Result<bool>> Handle(ToggleJobCommand request, CancellationToken cancellationToken)
    {
        var scheduledJob = await _context.ScheduledJobs
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (scheduledJob == null)
        {
            return Result<bool>.Failure("Scheduled job not found");
        }

        var oldEnabled = scheduledJob.IsEnabled;
        scheduledJob.IsEnabled = request.IsEnabled;

        if (request.IsEnabled)
        {
            try
            {
                var cronExpression = CronExpression.Parse(scheduledJob.CronExpression);
                scheduledJob.NextRunAt = cronExpression.GetNextOccurrence(_dateTime.UtcNow, TimeZoneInfo.Utc);
            }
            catch (CronFormatException)
            {
                return Result<bool>.Failure("Invalid cron expression format");
            }
        }
        else
        {
            scheduledJob.NextRunAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.ScheduledJobToggled,
            nameof(ScheduledJob),
            scheduledJob.Id.ToString(),
            oldValues: new { IsEnabled = oldEnabled },
            newValues: new { IsEnabled = request.IsEnabled },
            cancellationToken: cancellationToken);

        return Result<bool>.Success(true);
    }
}
