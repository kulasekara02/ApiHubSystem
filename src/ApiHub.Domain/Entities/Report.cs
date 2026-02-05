using ApiHub.Domain.Enums;

namespace ApiHub.Domain.Entities;

public class Report : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateType { get; set; } = string.Empty;
    public ReportFormat Format { get; set; }
    public string? FilterCriteria { get; set; }
    public ReportSchedule Schedule { get; set; } = ReportSchedule.None;
    public DateTime? LastGeneratedAt { get; set; }
    public DateTime? NextScheduledAt { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}
