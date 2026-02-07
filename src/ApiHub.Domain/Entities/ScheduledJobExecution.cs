namespace ApiHub.Domain.Entities;

public class ScheduledJobExecution
{
    public Guid Id { get; set; }
    public Guid ScheduledJobId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty; // Success, Failed, Running
    public int? HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public long? DurationMs { get; set; }

    public virtual ScheduledJob ScheduledJob { get; set; } = null!;
}
