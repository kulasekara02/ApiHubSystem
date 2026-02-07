using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Webhooks.Commands;

public record CreateWebhookCommand(
    string Name,
    string Url,
    string Secret,
    List<string> Events,
    bool IsEnabled = true) : IRequest<Result<Guid>>;
