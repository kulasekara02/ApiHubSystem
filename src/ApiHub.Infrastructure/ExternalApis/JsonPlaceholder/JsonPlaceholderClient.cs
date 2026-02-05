namespace ApiHub.Infrastructure.ExternalApis.JsonPlaceholder;

public class JsonPlaceholderClient : BaseApiClient
{
    public override string ConnectorName => "JSONPlaceholder";
    public override string BaseUrl => "https://jsonplaceholder.typicode.com";

    public JsonPlaceholderClient(HttpClient httpClient) : base(httpClient)
    {
        httpClient.BaseAddress = new Uri(BaseUrl);
    }
}
