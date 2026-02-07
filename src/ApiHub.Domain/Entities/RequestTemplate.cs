namespace ApiHub.Domain.Entities;

public class RequestTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ConnectorId { get; set; }
    public string Method { get; set; } = "GET";
    public string Endpoint { get; set; } = string.Empty;
    public string? Headers { get; set; } // JSON
    public string? Body { get; set; } // JSON
    public string? QueryParams { get; set; } // JSON
    public bool IsPublic { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Connector? Connector { get; set; }
    public virtual ApplicationUser CreatedBy { get; set; } = null!;
}
