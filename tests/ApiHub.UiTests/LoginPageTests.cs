using Microsoft.Playwright;
using Xunit;

namespace ApiHub.UiTests;

public class LoginPageTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;

    private const string BaseUrl = "https://localhost:5002";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact(Skip = "Requires running application")]
    public async Task LoginPage_ShouldDisplayLoginForm()
    {
        // Navigate to login page
        await _page.GotoAsync($"{BaseUrl}/login");

        // Check for login form elements
        var emailInput = await _page.QuerySelectorAsync("input[placeholder*='email']");
        var passwordInput = await _page.QuerySelectorAsync("input[type='password']");
        var submitButton = await _page.QuerySelectorAsync("button[type='submit']");

        Assert.NotNull(emailInput);
        Assert.NotNull(passwordInput);
        Assert.NotNull(submitButton);
    }

    [Fact(Skip = "Requires running application")]
    public async Task LoginPage_WithValidCredentials_ShouldRedirectToDashboard()
    {
        // Navigate to login page
        await _page.GotoAsync($"{BaseUrl}/login");

        // Fill in credentials
        await _page.FillAsync("input[placeholder*='email']", "admin@apihub.local");
        await _page.FillAsync("input[type='password']", "Admin@123!");

        // Click login button
        await _page.ClickAsync("button[type='submit']");

        // Wait for navigation
        await _page.WaitForURLAsync($"{BaseUrl}/");

        // Verify we're on the dashboard
        var pageTitle = await _page.TitleAsync();
        Assert.Contains("Dashboard", pageTitle);
    }

    [Fact(Skip = "Requires running application")]
    public async Task LoginPage_WithInvalidCredentials_ShouldShowError()
    {
        // Navigate to login page
        await _page.GotoAsync($"{BaseUrl}/login");

        // Fill in invalid credentials
        await _page.FillAsync("input[placeholder*='email']", "wrong@example.com");
        await _page.FillAsync("input[type='password']", "WrongPassword!");

        // Click login button
        await _page.ClickAsync("button[type='submit']");

        // Wait for error message
        await _page.WaitForSelectorAsync(".alert-danger");

        // Verify error is displayed
        var errorMessage = await _page.QuerySelectorAsync(".alert-danger");
        Assert.NotNull(errorMessage);
    }
}
