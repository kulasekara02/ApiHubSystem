using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace ApiHub.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new
        {
            Email = $"test{Guid.NewGuid():N}@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "not-an-email",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange - first register
        var email = $"login{Guid.NewGuid():N}@example.com";
        var password = "TestPassword123!";

        await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User"
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record LoginResponse(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName,
        string AccessToken,
        string RefreshToken,
        IEnumerable<string> Roles);
}
