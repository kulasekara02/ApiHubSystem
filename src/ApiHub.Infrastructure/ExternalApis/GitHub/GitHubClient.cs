using System.Net.Http.Headers;

namespace ApiHub.Infrastructure.ExternalApis.GitHub;

public class GitHubClient : BaseApiClient
{
    public override string ConnectorName => "GitHub";
    public override string BaseUrl => "https://api.github.com";

    public GitHubClient(HttpClient httpClient) : base(httpClient)
    {
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("ApiHubSystem", "1.0"));
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    public override void SetAuthenticationHeader(string? apiKey, string? token = null)
    {
        if (!string.IsNullOrEmpty(token))
        {
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public void SetApiVersion(string version = "2022-11-28")
    {
        HttpClient.DefaultRequestHeaders.Remove("X-GitHub-Api-Version");
        HttpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", version);
    }
}
