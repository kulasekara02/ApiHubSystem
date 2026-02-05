using ApiHub.Domain.Enums;

namespace ApiHub.Domain.Entities;

public class UploadedFile : BaseEntity
{
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? ParsedDataJson { get; set; }
    public int? RowCount { get; set; }
    public int? ColumnCount { get; set; }
    public string? ColumnHeaders { get; set; }
    public bool IsProcessed { get; set; }
    public string? ProcessingError { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}
