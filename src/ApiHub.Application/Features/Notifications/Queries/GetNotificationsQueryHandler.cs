using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Notifications.Queries;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return new PaginatedList<NotificationDto>(new List<NotificationDto>(), 0, request.PageNumber, request.PageSize);
        }

        var query = _context.Notifications
            .Where(n => n.UserId == Guid.Parse(userId))
            .AsNoTracking();

        if (request.IsRead.HasValue)
        {
            query = query.Where(n => n.IsRead == request.IsRead.Value);
        }

        var projectedQuery = query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.Id,
                n.UserId,
                n.Title,
                n.Message,
                n.Type,
                n.ActionUrl,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt));

        return await PaginatedList<NotificationDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
