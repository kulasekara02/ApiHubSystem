namespace ApiHub.Infrastructure.ExternalApis.ReqRes;

public class ReqResClient : BaseApiClient
{
    public override string ConnectorName => "ReqRes";
    public override string BaseUrl => "https://reqres.in/api";

    public ReqResClient(HttpClient httpClient) : base(httpClient)
    {
        httpClient.BaseAddress = new Uri(BaseUrl);
    }
}
