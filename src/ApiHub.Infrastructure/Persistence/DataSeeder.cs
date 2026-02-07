using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiHub.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext.ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var dbProvider = configuration["Database:Provider"]?.ToLower() ?? "postgresql";
        if (dbProvider == "inmemory")
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            await context.Database.MigrateAsync();
        }

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedConnectorsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        var roles = new[]
        {
            new ApplicationRole("Admin", "Full system access"),
            new ApplicationRole("Analyst", "Can view analytics and run reports"),
            new ApplicationRole("Viewer", "Read-only access to API records")
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                role.CreatedAt = DateTime.UtcNow;
                await roleManager.CreateAsync(role);
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@apihub.local";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(admin, "Admin@123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var analystEmail = "analyst@apihub.local";
        if (await userManager.FindByEmailAsync(analystEmail) == null)
        {
            var analyst = new ApplicationUser
            {
                UserName = analystEmail,
                Email = analystEmail,
                FirstName = "Demo",
                LastName = "Analyst",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(analyst, "Analyst@123!");
            await userManager.AddToRoleAsync(analyst, "Analyst");
        }

        var viewerEmail = "viewer@apihub.local";
        if (await userManager.FindByEmailAsync(viewerEmail) == null)
        {
            var viewer = new ApplicationUser
            {
                UserName = viewerEmail,
                Email = viewerEmail,
                FirstName = "Demo",
                LastName = "Viewer",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(viewer, "Viewer@123!");
            await userManager.AddToRoleAsync(viewer, "Viewer");
        }
    }

    private static async Task SeedConnectorsAsync(DbContext.ApplicationDbContext context)
    {
        if (await context.Connectors.AnyAsync())
            return;

        var connectors = new List<Connector>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "JSONPlaceholder",
                Description = "Free fake REST API for testing and prototyping",
                BaseUrl = "https://jsonplaceholder.typicode.com",
                AuthType = AuthenticationType.None,
                Status = ConnectorStatus.Active,
                IsPublic = true,
                TimeoutSeconds = 30,
                MaxRetries = 3,
                CreatedAt = DateTime.UtcNow,
                Endpoints = new List<ConnectorEndpoint>
                {
                    new() { Id = Guid.NewGuid(), Name = "Get Posts", Path = "/posts", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Get Post", Path = "/posts/{id}", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Create Post", Path = "/posts", Method = HttpMethodType.POST, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Update Post", Path = "/posts/{id}", Method = HttpMethodType.PUT, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Delete Post", Path = "/posts/{id}", Method = HttpMethodType.DELETE, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Get Users", Path = "/users", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Get Comments", Path = "/comments", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Swagger Petstore",
                Description = "Sample Petstore server with OpenAPI 3.0 spec",
                BaseUrl = "https://petstore.swagger.io/v2",
                AuthType = AuthenticationType.ApiKey,
                ApiKeyHeaderName = "api_key",
                Status = ConnectorStatus.Active,
                IsPublic = true,
                TimeoutSeconds = 30,
                MaxRetries = 3,
                CreatedAt = DateTime.UtcNow,
                Endpoints = new List<ConnectorEndpoint>
                {
                    new() { Id = Guid.NewGuid(), Name = "Find Pets by Status", Path = "/pet/findByStatus", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Get Pet by ID", Path = "/pet/{petId}", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Add Pet", Path = "/pet", Method = HttpMethodType.POST, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Update Pet", Path = "/pet", Method = HttpMethodType.PUT, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Get Store Inventory", Path = "/store/inventory", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "ReqRes",
                Description = "Test your front-end against a real API",
                BaseUrl = "https://reqres.in/api",
                AuthType = AuthenticationType.None,
                Status = ConnectorStatus.Active,
                IsPublic = true,
                TimeoutSeconds = 30,
                MaxRetries = 3,
                CreatedAt = DateTime.UtcNow,
                Endpoints = new List<ConnectorEndpoint>
                {
                    new() { Id = Guid.NewGuid(), Name = "List Users", Path = "/users", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Get User", Path = "/users/{id}", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Create User", Path = "/users", Method = HttpMethodType.POST, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Update User", Path = "/users/{id}", Method = HttpMethodType.PUT, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Delete User", Path = "/users/{id}", Method = HttpMethodType.DELETE, IsEnabled = true, CreatedAt = DateTime.UtcNow }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "GitHub API",
                Description = "GitHub REST API v3",
                BaseUrl = "https://api.github.com",
                AuthType = AuthenticationType.BearerToken,
                VersionHeaderName = "X-GitHub-Api-Version",
                DefaultVersion = "2022-11-28",
                Status = ConnectorStatus.Active,
                IsPublic = false,
                TimeoutSeconds = 30,
                MaxRetries = 3,
                CreatedAt = DateTime.UtcNow,
                Endpoints = new List<ConnectorEndpoint>
                {
                    new() { Id = Guid.NewGuid(), Name = "Get Authenticated User", Path = "/user", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "List Repos", Path = "/user/repos", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Get Repo", Path = "/repos/{owner}/{repo}", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "List Issues", Path = "/repos/{owner}/{repo}/issues", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "OpenWeather",
                Description = "Current weather and forecasts",
                BaseUrl = "https://api.openweathermap.org/data/2.5",
                AuthType = AuthenticationType.ApiKey,
                ApiKeyQueryParamName = "appid",
                Status = ConnectorStatus.Active,
                IsPublic = false,
                TimeoutSeconds = 30,
                MaxRetries = 3,
                CreatedAt = DateTime.UtcNow,
                Endpoints = new List<ConnectorEndpoint>
                {
                    new() { Id = Guid.NewGuid(), Name = "Current Weather", Path = "/weather", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "5 Day Forecast", Path = "/forecast", Method = HttpMethodType.GET, IsEnabled = true, CreatedAt = DateTime.UtcNow }
                }
            }
        };

        context.Connectors.AddRange(connectors);
        await context.SaveChangesAsync();
    }
}
