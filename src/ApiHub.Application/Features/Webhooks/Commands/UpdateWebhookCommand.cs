using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Webhooks.Commands;

public record UpdateWebhookCommand(
    Guid Id,
    string Name,
    string Url,
    string? Secret,
    List<string> Events,
    bool IsEnabled) : IRequest<Result>;
