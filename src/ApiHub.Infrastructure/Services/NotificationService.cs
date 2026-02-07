using ApiHub.Application.Common.Interfaces;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiHub.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IApplicationDbContext context,
        IDateTime dateTime,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task SendAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type.ToString().ToLowerInvariant(),
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = _dateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
    }

    public async Task SendToRoleAsync(
        string role,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        CancellationToken cancellationToken = default)
    {
        var usersInRole = await _context.Users
            .Where(u => u.IsActive)
            .Join(
                _context.Roles.Where(r => r.Name == role),
                u => u.Id,
                r => r.Id,
                (u, r) => u.Id)
            .ToListAsync(cancellationToken);

        // Alternative approach using UserRoles if needed
        // For now, we'll get all active users and filter by role assignment
        var allActiveUsers = await _context.Users
            .Where(u => u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var now = _dateTime.UtcNow;
        var typeString = type.ToString().ToLowerInvariant();

        foreach (var userId in allActiveUsers)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = typeString,
                IsRead = false,
                CreatedAt = now
            };

            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification sent to role {Role}: {Title}", role, title);
    }

    public async Task SendToAllAsync(
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        CancellationToken cancellationToken = default)
    {
        var allActiveUserIds = await _context.Users
            .Where(u => u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var now = _dateTime.UtcNow;
        var typeString = type.ToString().ToLowerInvariant();

        foreach (var userId in allActiveUserIds)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = typeString,
                IsRead = false,
                CreatedAt = now
            };

            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notification sent to all users: {Title}", title);
    }
}
