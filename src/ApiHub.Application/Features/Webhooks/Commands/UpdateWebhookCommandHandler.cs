using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Webhooks.Commands;

public class UpdateWebhookCommandHandler : IRequestHandler<UpdateWebhookCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateWebhookCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateWebhookCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result.Failure("User not authenticated.");
        }

        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == request.Id && w.CreatedById == userId.Value, cancellationToken);

        if (webhook == null)
        {
            return Result.Failure("Webhook not found.");
        }

        webhook.Name = request.Name;
        webhook.Url = request.Url;
        webhook.Events = string.Join(",", request.Events);
        webhook.IsEnabled = request.IsEnabled;

        if (!string.IsNullOrEmpty(request.Secret))
        {
            webhook.Secret = request.Secret;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
