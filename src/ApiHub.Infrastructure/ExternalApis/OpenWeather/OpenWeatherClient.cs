using Microsoft.AspNetCore.WebUtilities;

namespace ApiHub.Infrastructure.ExternalApis.OpenWeather;

public class OpenWeatherClient : BaseApiClient
{
    private string? _apiKey;

    public override string ConnectorName => "OpenWeather";
    public override string BaseUrl => "https://api.openweathermap.org/data/2.5";

    public OpenWeatherClient(HttpClient httpClient) : base(httpClient)
    {
        httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public override void SetAuthenticationHeader(string? apiKey, string? token = null)
    {
        _apiKey = apiKey;
    }

    public override async Task<HttpResponseMessage> GetAsync(
        string endpoint,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var url = AppendApiKey(endpoint);
        var request = CreateRequest(HttpMethod.Get, url, headers);
        return await HttpClient.SendAsync(request, cancellationToken);
    }

    private string AppendApiKey(string endpoint)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return endpoint;

        var uri = new Uri(endpoint, UriKind.RelativeOrAbsolute);
        var query = new Dictionary<string, string?> { { "appid", _apiKey } };

        return QueryHelpers.AddQueryString(endpoint, query);
    }
}
