using ApiHub.Domain.Enums;

namespace ApiHub.Domain.Entities;

public class Connector : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public AuthenticationType AuthType { get; set; }
    public string? ApiKeyHeaderName { get; set; }
    public string? ApiKeyQueryParamName { get; set; }
    public string? VersionHeaderName { get; set; }
    public string? VersionQueryParamName { get; set; }
    public string? DefaultVersion { get; set; }
    public ConnectorStatus Status { get; set; } = ConnectorStatus.Active;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool IsPublic { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ConnectorEndpoint> Endpoints { get; set; } = new List<ConnectorEndpoint>();
    public virtual ICollection<UserConnectorSecret> UserSecrets { get; set; } = new List<UserConnectorSecret>();
    public virtual ICollection<ApiRecord> ApiRecords { get; set; } = new List<ApiRecord>();
}
