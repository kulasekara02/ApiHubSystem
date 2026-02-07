using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Notifications.Commands;

public record DeleteNotificationCommand(Guid NotificationId) : IRequest<Result<bool>>;
