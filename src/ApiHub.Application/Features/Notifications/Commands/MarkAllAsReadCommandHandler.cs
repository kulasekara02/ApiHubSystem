using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Notifications.Commands;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public MarkAllAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<int>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result<int>.Failure("User not authenticated");
        }

        var userGuid = Guid.Parse(userId);
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userGuid && !n.IsRead)
            .ToListAsync(cancellationToken);

        var count = unreadNotifications.Count;
        var now = _dateTime.UtcNow;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(count);
    }
}
