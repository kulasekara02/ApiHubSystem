using ApiHub.Domain.Enums;

namespace ApiHub.Domain.Entities;

public class ApiRecord : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ConnectorId { get; set; }
    public Guid? EndpointId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public HttpMethodType Method { get; set; }
    public string RequestUrl { get; set; } = string.Empty;
    public string? RequestHeaders { get; set; }
    public string? RequestBody { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseHeaders { get; set; }
    public string? ResponseBody { get; set; }
    public long DurationMs { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Connector Connector { get; set; } = null!;
    public virtual ConnectorEndpoint? Endpoint { get; set; }
    public virtual Dataset? Dataset { get; set; }
}
