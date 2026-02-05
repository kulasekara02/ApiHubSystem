using System.Diagnostics;
using System.Text;
using ApiHub.Application.Common.Interfaces;
using ApiHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiHub.Infrastructure.Resilience;

public class ApiRunnerService : IApiRunnerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ApiRunnerService> _logger;

    public ApiRunnerService(
        IHttpClientFactory httpClientFactory,
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IEncryptionService encryptionService,
        ILogger<ApiRunnerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
        _currentUser = currentUser;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<ApiResponse> SendRequestAsync(ApiRequest request, CancellationToken cancellationToken = default)
    {
        var connector = await _context.Connectors
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ConnectorId, cancellationToken);

        if (connector == null)
        {
            return new ApiResponse(0, new Dictionary<string, string>(), null, 0, false, "Connector not found", 0);
        }

        var httpClient = _httpClientFactory.CreateClient($"Connector_{connector.Name}");

        var url = BuildUrl(connector.BaseUrl, request.Endpoint, request.QueryParams);
        var httpRequest = new HttpRequestMessage(ToHttpMethod(request.Method), url);

        await AddAuthenticationAsync(httpRequest, connector, cancellationToken);
        AddHeaders(httpRequest, request.Headers);

        if (!string.IsNullOrEmpty(request.Body) &&
            request.Method is HttpMethodType.POST or HttpMethodType.PUT or HttpMethodType.PATCH)
        {
            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
        }

        var stopwatch = Stopwatch.StartNew();
        int retryCount = 0;

        try
        {
            var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            stopwatch.Stop();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseHeaders = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            return new ApiResponse(
                (int)response.StatusCode,
                responseHeaders,
                responseBody,
                stopwatch.ElapsedMilliseconds,
                response.IsSuccessStatusCode,
                null,
                retryCount);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "API request failed for connector {ConnectorId}: {Message}", request.ConnectorId, ex.Message);

            return new ApiResponse(
                0,
                new Dictionary<string, string>(),
                null,
                stopwatch.ElapsedMilliseconds,
                false,
                ex.Message,
                retryCount);
        }
    }

    private static HttpMethod ToHttpMethod(HttpMethodType method) => method switch
    {
        HttpMethodType.GET => HttpMethod.Get,
        HttpMethodType.POST => HttpMethod.Post,
        HttpMethodType.PUT => HttpMethod.Put,
        HttpMethodType.PATCH => HttpMethod.Patch,
        HttpMethodType.DELETE => HttpMethod.Delete,
        _ => HttpMethod.Get
    };

    private static string BuildUrl(string baseUrl, string endpoint, Dictionary<string, string>? queryParams)
    {
        var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        if (queryParams != null && queryParams.Count > 0)
        {
            var queryString = string.Join("&", queryParams.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{url}?{queryString}";
        }

        return url;
    }

    private async Task AddAuthenticationAsync(HttpRequestMessage request, Domain.Entities.Connector connector, CancellationToken cancellationToken)
    {
        if (connector.AuthType == AuthenticationType.None)
            return;

        if (!_currentUser.UserId.HasValue)
            return;

        var secret = await _context.UserConnectorSecrets
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.UserId == _currentUser.UserId.Value &&
                s.ConnectorId == connector.Id,
                cancellationToken);

        if (secret == null)
            return;

        var apiKey = _encryptionService.Decrypt(secret.EncryptedApiKey);
        var token = secret.EncryptedToken != null ? _encryptionService.Decrypt(secret.EncryptedToken) : null;

        switch (connector.AuthType)
        {
            case AuthenticationType.ApiKey:
                if (!string.IsNullOrEmpty(connector.ApiKeyHeaderName))
                {
                    request.Headers.TryAddWithoutValidation(connector.ApiKeyHeaderName, apiKey);
                }
                else if (!string.IsNullOrEmpty(connector.ApiKeyQueryParamName))
                {
                    var uri = new UriBuilder(request.RequestUri!);
                    var query = uri.Query.TrimStart('?');
                    query = string.IsNullOrEmpty(query)
                        ? $"{connector.ApiKeyQueryParamName}={Uri.EscapeDataString(apiKey)}"
                        : $"{query}&{connector.ApiKeyQueryParamName}={Uri.EscapeDataString(apiKey)}";
                    uri.Query = query;
                    request.RequestUri = uri.Uri;
                }
                break;

            case AuthenticationType.BearerToken:
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token ?? apiKey);
                break;

            case AuthenticationType.BasicAuth:
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{token ?? string.Empty}"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                break;
        }
    }

    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null) return;

        foreach (var header in headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}
