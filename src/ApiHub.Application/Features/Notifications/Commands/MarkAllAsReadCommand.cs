using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Notifications.Commands;

public record MarkAllAsReadCommand() : IRequest<Result<int>>;
