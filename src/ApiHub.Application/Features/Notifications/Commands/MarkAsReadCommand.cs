using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Notifications.Commands;

public record MarkAsReadCommand(Guid NotificationId) : IRequest<Result<bool>>;
