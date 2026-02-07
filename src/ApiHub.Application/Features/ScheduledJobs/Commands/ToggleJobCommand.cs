using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public record ToggleJobCommand(Guid Id, bool IsEnabled) : IRequest<Result<bool>>;
