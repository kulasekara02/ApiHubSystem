namespace ApiHub.Web.Services;

/// <summary>
/// Service for sharing state between Templates and ApiRunner pages.
/// Allows templates to pre-populate the API Runner form.
/// </summary>
public class ApiRunnerStateService
{
    public event Action? OnChange;

    public string? SelectedConnectorId { get; private set; }
    public string? Method { get; private set; }
    public string? Endpoint { get; private set; }
    public string? Headers { get; private set; }
    public string? Body { get; private set; }
    public string? QueryParams { get; private set; }
    public bool HasPendingTemplate { get; private set; }

    public void SetFromTemplate(
        string? connectorId,
        string method,
        string endpoint,
        string? headers,
        string? body,
        string? queryParams)
    {
        SelectedConnectorId = connectorId;
        Method = method;
        Endpoint = endpoint;
        Headers = headers;
        Body = body;
        QueryParams = queryParams;
        HasPendingTemplate = true;
        NotifyStateChanged();
    }

    public void ClearPendingTemplate()
    {
        HasPendingTemplate = false;
    }

    public void Reset()
    {
        SelectedConnectorId = null;
        Method = null;
        Endpoint = null;
        Headers = null;
        Body = null;
        QueryParams = null;
        HasPendingTemplate = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
