namespace ApiHub.Application.Common.Interfaces;

public interface IExternalApiClient
{
    string ConnectorName { get; }
    string BaseUrl { get; }

    Task<HttpResponseMessage> GetAsync(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string endpoint, string? body, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PutAsync(string endpoint, string? body, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PatchAsync(string endpoint, string? body, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DeleteAsync(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    void SetAuthenticationHeader(string? apiKey, string? token = null);
}
