using System.Net.Http.Headers;
using System.Text;
using ApiHub.Application.Common.Interfaces;

namespace ApiHub.Infrastructure.ExternalApis;

public abstract class BaseApiClient : IExternalApiClient
{
    protected readonly HttpClient HttpClient;
    public abstract string ConnectorName { get; }
    public abstract string BaseUrl { get; }

    protected BaseApiClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public virtual void SetAuthenticationHeader(string? apiKey, string? token = null)
    {
        if (!string.IsNullOrEmpty(token))
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public virtual async Task<HttpResponseMessage> GetAsync(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, endpoint, headers);
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> PostAsync(
        string endpoint,
        string? body,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, endpoint, headers);
        if (!string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> PutAsync(
        string endpoint,
        string? body,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Put, endpoint, headers);
        if (!string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> PatchAsync(
        string endpoint,
        string? body,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Patch, endpoint, headers);
        if (!string.IsNullOrEmpty(body))
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    public virtual async Task<HttpResponseMessage> DeleteAsync(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Delete, endpoint, headers);
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    protected virtual HttpRequestMessage CreateRequest(
        HttpMethod method,
        string endpoint,
        Dictionary<string, string>? headers = null)
    {
        var request = new HttpRequestMessage(method, endpoint);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return request;
    }
}
