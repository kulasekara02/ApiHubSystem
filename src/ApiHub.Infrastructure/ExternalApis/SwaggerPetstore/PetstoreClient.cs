namespace ApiHub.Infrastructure.ExternalApis.SwaggerPetstore;

public class PetstoreClient : BaseApiClient
{
    public override string ConnectorName => "SwaggerPetstore";
    public override string BaseUrl => "https://petstore.swagger.io/v2";

    public PetstoreClient(HttpClient httpClient) : base(httpClient)
    {
        httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public override void SetAuthenticationHeader(string? apiKey, string? token = null)
    {
        if (!string.IsNullOrEmpty(apiKey))
        {
            HttpClient.DefaultRequestHeaders.Remove("api_key");
            HttpClient.DefaultRequestHeaders.Add("api_key", apiKey);
        }
        base.SetAuthenticationHeader(apiKey, token);
    }
}
