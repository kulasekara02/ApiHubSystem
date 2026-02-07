using ApiHub.Domain.Enums;

namespace ApiHub.Application.Common.Interfaces;

public interface IWebhookService
{
    Task TriggerAsync(WebhookEvent webhookEvent, object payload, CancellationToken cancellationToken = default);
    Task RetryDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken = default);
}
