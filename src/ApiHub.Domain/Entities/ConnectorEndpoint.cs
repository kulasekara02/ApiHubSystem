using ApiHub.Domain.Enums;

namespace ApiHub.Domain.Entities;

public class ConnectorEndpoint : BaseEntity
{
    public Guid ConnectorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public HttpMethodType Method { get; set; }
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? RequestBodySchema { get; set; }
    public string? ResponseBodySchema { get; set; }

    // Navigation properties
    public virtual Connector Connector { get; set; } = null!;
}
