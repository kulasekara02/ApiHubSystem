using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Notifications.Queries;

public record GetNotificationsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool? IsRead = null) : IRequest<PaginatedList<NotificationDto>>;

public record NotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    string Type,
    string? ActionUrl,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt);
