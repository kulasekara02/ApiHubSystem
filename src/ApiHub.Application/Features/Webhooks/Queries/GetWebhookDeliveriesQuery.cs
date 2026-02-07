using ApiHub.Application.Common.Models;
using ApiHub.Domain.Enums;
using MediatR;

namespace ApiHub.Application.Features.Webhooks.Queries;

public record GetWebhookDeliveriesQuery(
    Guid WebhookId,
    int PageNumber = 1,
    int PageSize = 20,
    bool? IsSuccess = null) : IRequest<PaginatedList<WebhookDeliveryDto>>;

public record WebhookDeliveryDto(
    Guid Id,
    Guid WebhookId,
    string Event,
    int? ResponseStatusCode,
    string? ErrorMessage,
    bool IsSuccess,
    int AttemptNumber,
    long DurationMs,
    DateTime TriggeredAt,
    DateTime? CompletedAt);
