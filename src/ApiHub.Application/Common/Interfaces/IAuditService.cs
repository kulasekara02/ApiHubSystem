using ApiHub.Domain.Enums;

namespace ApiHub.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        AuditAction action,
        string entityType,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? additionalData = null,
        CancellationToken cancellationToken = default);
}
