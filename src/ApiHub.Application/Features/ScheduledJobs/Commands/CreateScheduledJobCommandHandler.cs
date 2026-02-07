using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Cronos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public class CreateScheduledJobCommandHandler : IRequestHandler<CreateScheduledJobCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public CreateScheduledJobCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<Result<Guid>> Handle(CreateScheduledJobCommand request, CancellationToken cancellationToken)
    {
        // Validate connector exists
        var connectorExists = await _context.Connectors
            .AnyAsync(c => c.Id == request.ConnectorId, cancellationToken);

        if (!connectorExists)
        {
            return Result<Guid>.Failure("Connector not found");
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
            return Result<Guid>.Failure("Invalid cron expression format");
        }

        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Result<Guid>.Failure("User not authenticated");
        }

        var scheduledJob = new ScheduledJob
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ConnectorId = request.ConnectorId,
            Endpoint = request.Endpoint,
            Method = request.Method.ToUpper(),
            Headers = request.Headers,
            Body = request.Body,
            CronExpression = request.CronExpression,
            IsEnabled = request.IsEnabled,
            NextRunAt = request.IsEnabled ? nextRunAt : null,
            CreatedById = userId.Value,
            CreatedAt = _dateTime.UtcNow
        };

        _context.ScheduledJobs.Add(scheduledJob);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.ScheduledJobCreated,
            nameof(ScheduledJob),
            scheduledJob.Id.ToString(),
            newValues: new { scheduledJob.Name, scheduledJob.CronExpression, scheduledJob.Endpoint },
            cancellationToken: cancellationToken);

        return Result<Guid>.Success(scheduledJob.Id);
    }
}
