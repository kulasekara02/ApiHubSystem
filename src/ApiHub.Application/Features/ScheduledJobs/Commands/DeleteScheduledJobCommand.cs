using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.ScheduledJobs.Commands;

public record DeleteScheduledJobCommand(Guid Id) : IRequest<Result<bool>>;
