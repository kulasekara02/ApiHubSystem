using ApiHub.Domain.Enums;

namespace ApiHub.Domain.Entities;

public class WebhookDelivery
{
    public Guid Id { get; set; }
    public Guid WebhookId { get; set; }
    public WebhookEvent Event { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string? RequestHeaders { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; }
    public int AttemptNumber { get; set; }
    public long DurationMs { get; set; }
    public DateTime TriggeredAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual Webhook Webhook { get; set; } = null!;
}
