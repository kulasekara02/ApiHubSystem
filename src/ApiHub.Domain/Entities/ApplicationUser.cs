using Microsoft.AspNetCore.Identity;

namespace ApiHub.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation properties
    public virtual ICollection<ApiRecord> ApiRecords { get; set; } = new List<ApiRecord>();
    public virtual ICollection<UserConnectorSecret> ConnectorSecrets { get; set; } = new List<UserConnectorSecret>();
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    public virtual ICollection<UploadedFile> UploadedFiles { get; set; } = new List<UploadedFile>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
}
