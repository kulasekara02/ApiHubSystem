using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using MediatR;

namespace ApiHub.Application.Features.Webhooks.Commands;

public class CreateWebhookCommandHandler : IRequestHandler<CreateWebhookCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public CreateWebhookCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<Guid>> Handle(CreateWebhookCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result<Guid>.Failure("User not authenticated.");
        }

        var webhook = new Webhook
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            Secret = request.Secret,
            Events = string.Join(",", request.Events),
            IsEnabled = request.IsEnabled,
            FailureCount = 0,
            CreatedById = userId.Value,
            CreatedAt = _dateTime.UtcNow
        };

        _context.Webhooks.Add(webhook);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(webhook.Id);
    }
}
