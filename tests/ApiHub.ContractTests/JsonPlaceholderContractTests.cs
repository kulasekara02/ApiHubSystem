using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace ApiHub.ContractTests;

public class JsonPlaceholderContractTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly HttpClient _client;

    public JsonPlaceholderContractTests()
    {
        _server = WireMockServer.Start();
        _client = new HttpClient { BaseAddress = new Uri(_server.Url!) };
    }

    [Fact]
    public async Task GetPosts_ShouldReturnArrayOfPosts()
    {
        // Arrange
        _server.Given(
            Request.Create()
                .WithPath("/posts")
                .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""
                        [
                            {
                                "userId": 1,
                                "id": 1,
                                "title": "Test Post",
                                "body": "Test Body"
                            }
                        ]
                        """));

        // Act
        var response = await _client.GetAsync("/posts");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("userId");
        content.Should().Contain("title");
        content.Should().Contain("body");
    }

    [Fact]
    public async Task GetPost_WithValidId_ShouldReturnPost()
    {
        // Arrange
        _server.Given(
            Request.Create()
                .WithPath("/posts/1")
                .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""
                        {
                            "userId": 1,
                            "id": 1,
                            "title": "Test Post",
                            "body": "Test Body"
                        }
                        """));

        // Act
        var response = await _client.GetAsync("/posts/1");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePost_ShouldReturn201()
    {
        // Arrange
        _server.Given(
            Request.Create()
                .WithPath("/posts")
                .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(201)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""
                        {
                            "id": 101,
                            "userId": 1,
                            "title": "New Post",
                            "body": "New Body"
                        }
                        """));

        var content = new StringContent(
            """{"userId": 1, "title": "New Post", "body": "New Body"}""",
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/posts", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
    }

    public void Dispose()
    {
        _client.Dispose();
        _server.Stop();
        _server.Dispose();
    }
}
