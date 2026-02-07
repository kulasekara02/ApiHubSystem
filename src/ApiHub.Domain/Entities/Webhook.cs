namespace ApiHub.Domain.Entities;

public class Webhook
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty; // For HMAC signature
    public string Events { get; set; } = string.Empty; // Comma-separated: "api.request.completed,api.request.failed,connector.created"
    public bool IsEnabled { get; set; }
    public int FailureCount { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public string? LastResponseStatus { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ApplicationUser CreatedBy { get; set; } = null!;
}
