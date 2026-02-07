using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Webhooks.Queries;

public class GetWebhooksQueryHandler : IRequestHandler<GetWebhooksQuery, PaginatedList<WebhookDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetWebhooksQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<WebhookDto>> Handle(GetWebhooksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return new PaginatedList<WebhookDto>(new List<WebhookDto>(), 0, 1, request.PageSize);
        }

        var query = _context.Webhooks
            .Where(w => w.CreatedById == userId.Value)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(w =>
                w.Name.Contains(request.SearchTerm) ||
                w.Url.Contains(request.SearchTerm));
        }

        if (request.IsEnabled.HasValue)
        {
            query = query.Where(w => w.IsEnabled == request.IsEnabled.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WebhookDto(
                w.Id,
                w.Name,
                w.Url,
                w.Events.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                w.IsEnabled,
                w.FailureCount,
                w.LastTriggeredAt,
                w.LastResponseStatus,
                w.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedList<WebhookDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
