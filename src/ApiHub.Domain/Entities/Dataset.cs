namespace ApiHub.Domain.Entities;

public class Dataset : BaseEntity
{
    public Guid ApiRecordId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string JsonSnapshot { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public string? Schema { get; set; }

    // Navigation properties
    public virtual ApiRecord ApiRecord { get; set; } = null!;
}
