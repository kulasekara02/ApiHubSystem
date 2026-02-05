using System.Text.Json;
using ApiHub.Application.Common.Interfaces;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace ApiHub.Infrastructure.Security;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _dateTime;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime dateTime,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _currentUser = currentUser;
        _dateTime = dateTime;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        AuditAction action,
        string entityType,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = _currentUser.IpAddress,
            UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].FirstOrDefault(),
            CorrelationId = _currentUser.CorrelationId,
            AdditionalData = additionalData,
            CreatedAt = _dateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
