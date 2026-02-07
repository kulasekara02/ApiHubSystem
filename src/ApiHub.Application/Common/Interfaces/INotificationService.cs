using ApiHub.Domain.Enums;

namespace ApiHub.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);

    Task SendToRoleAsync(
        string role,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        CancellationToken cancellationToken = default);

    Task SendToAllAsync(
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        CancellationToken cancellationToken = default);
}
