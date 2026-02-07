using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public record CreateScheduledJobCommand(
    string Name,
    string? Description,
    Guid ConnectorId,
    string Endpoint,
    string Method,
    string? Headers,
    string? Body,
    string CronExpression,
    bool IsEnabled) : IRequest<Result<Guid>>;
