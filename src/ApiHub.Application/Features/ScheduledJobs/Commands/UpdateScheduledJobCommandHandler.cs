using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Cronos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public class UpdateScheduledJobCommandHandler : IRequestHandler<UpdateScheduledJobCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public UpdateScheduledJobCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _context = context;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<Result<bool>> Handle(UpdateScheduledJobCommand request, CancellationToken cancellationToken)
    {
        var scheduledJob = await _context.ScheduledJobs
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (scheduledJob == null)
        {
            return Result<bool>.Failure("Scheduled job not found");
        }

        // Validate connector exists
        var connectorExists = await _context.Connectors
            .AnyAsync(c => c.Id == request.ConnectorId, cancellationToken);

        if (!connectorExists)
        {
            return Result<bool>.Failure("Connector not found");
        }

        // Validate cron expression
        DateTime? nextRunAt = null;
        try
        {
            var cronExpression = CronExpression.Parse(request.CronExpression);
            nextRunAt = cronExpression.GetNextOccurrence(_dateTime.UtcNow, TimeZoneInfo.Utc);
        }
        catch (CronFormatException)
        {
            return Result<bool>.Failure("Invalid cron expression format");
        }

        var oldValues = new
        {
            scheduledJob.Name,
            scheduledJob.CronExpression,
            scheduledJob.Endpoint,
            scheduledJob.IsEnabled
        };

        scheduledJob.Name = request.Name;
        scheduledJob.Description = request.Description;
        scheduledJob.ConnectorId = request.ConnectorId;
        scheduledJob.Endpoint = request.Endpoint;
        scheduledJob.Method = request.Method.ToUpper();
        scheduledJob.Headers = request.Headers;
        scheduledJob.Body = request.Body;
        scheduledJob.CronExpression = request.CronExpression;
        scheduledJob.IsEnabled = request.IsEnabled;
        scheduledJob.NextRunAt = request.IsEnabled ? nextRunAt : null;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.ScheduledJobUpdated,
            nameof(ScheduledJob),
            scheduledJob.Id.ToString(),
            oldValues: oldValues,
            newValues: new { scheduledJob.Name, scheduledJob.CronExpression, scheduledJob.Endpoint, scheduledJob.IsEnabled },
            cancellationToken: cancellationToken);

        return Result<bool>.Success(true);
    }
}
