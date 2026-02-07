namespace ApiHub.Domain.Entities;

public class UserTwoFactorToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EnabledAt { get; set; }
    public List<string> RecoveryCodes { get; set; } = new();

    public virtual ApplicationUser User { get; set; } = null!;
}
