using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Webhooks.Commands;

public class DeleteWebhookCommandHandler : IRequestHandler<DeleteWebhookCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteWebhookCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteWebhookCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result.Failure("User not authenticated.");
        }

        var webhook = await _context.Webhooks
            .Include(w => w.CreatedBy)
            .FirstOrDefaultAsync(w => w.Id == request.Id && w.CreatedById == userId.Value, cancellationToken);

        if (webhook == null)
        {
            return Result.Failure("Webhook not found.");
        }

        // Delete associated deliveries
        var deliveries = await _context.WebhookDeliveries
            .Where(d => d.WebhookId == request.Id)
            .ToListAsync(cancellationToken);

        _context.WebhookDeliveries.RemoveRange(deliveries);
        _context.Webhooks.Remove(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
