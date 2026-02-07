namespace ApiHub.Domain.Entities;

public class ScheduledJob
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ConnectorId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string? Headers { get; set; }
    public string? Body { get; set; }
    public string CronExpression { get; set; } = string.Empty; // e.g., "0 */5 * * *"
    public bool IsEnabled { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public string? LastRunStatus { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Connector Connector { get; set; } = null!;
    public virtual ApplicationUser CreatedBy { get; set; } = null!;
    public virtual ICollection<ScheduledJobExecution> Executions { get; set; } = new List<ScheduledJobExecution>();
}
