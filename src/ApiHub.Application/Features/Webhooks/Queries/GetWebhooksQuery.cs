using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Webhooks.Queries;

public record GetWebhooksQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsEnabled = null) : IRequest<PaginatedList<WebhookDto>>;

public record WebhookDto(
    Guid Id,
    string Name,
    string Url,
    List<string> Events,
    bool IsEnabled,
    int FailureCount,
    DateTime? LastTriggeredAt,
    string? LastResponseStatus,
    DateTime CreatedAt);
