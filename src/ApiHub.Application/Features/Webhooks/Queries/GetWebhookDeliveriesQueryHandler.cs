using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Webhooks.Queries;

public class GetWebhookDeliveriesQueryHandler : IRequestHandler<GetWebhookDeliveriesQuery, PaginatedList<WebhookDeliveryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetWebhookDeliveriesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<WebhookDeliveryDto>> Handle(GetWebhookDeliveriesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return new PaginatedList<WebhookDeliveryDto>(new List<WebhookDeliveryDto>(), 0, 1, request.PageSize);
        }

        // Verify the webhook belongs to the current user
        var webhookExists = await _context.Webhooks
            .AnyAsync(w => w.Id == request.WebhookId && w.CreatedById == userId.Value, cancellationToken);

        if (!webhookExists)
        {
            return new PaginatedList<WebhookDeliveryDto>(new List<WebhookDeliveryDto>(), 0, 1, request.PageSize);
        }

        var query = _context.WebhookDeliveries
            .Where(d => d.WebhookId == request.WebhookId)
            .AsQueryable();

        if (request.IsSuccess.HasValue)
        {
            query = query.Where(d => d.IsSuccess == request.IsSuccess.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.TriggeredAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new WebhookDeliveryDto(
                d.Id,
                d.WebhookId,
                d.Event.ToString(),
                d.ResponseStatusCode,
                d.ErrorMessage,
                d.IsSuccess,
                d.AttemptNumber,
                d.DurationMs,
                d.TriggeredAt,
                d.CompletedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedList<WebhookDeliveryDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
