namespace ApiHub.Domain.Entities;

public class UserConnectorSecret : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ConnectorId { get; set; }
    public string EncryptedApiKey { get; set; } = string.Empty;
    public string? EncryptedToken { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Connector Connector { get; set; } = null!;
}
