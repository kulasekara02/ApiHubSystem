using ApiHub.Application.Common.Interfaces;
using ApiHub.Domain.Entities;
using ApiHub.Infrastructure.ExternalApis.GitHub;
using ApiHub.Infrastructure.ExternalApis.JsonPlaceholder;
using ApiHub.Infrastructure.ExternalApis.OpenWeather;
using ApiHub.Infrastructure.ExternalApis.ReqRes;
using ApiHub.Infrastructure.ExternalApis.SwaggerPetstore;
using ApiHub.Infrastructure.Persistence.DbContext;
using ApiHub.Infrastructure.Resilience;
using ApiHub.Infrastructure.Security;
using ApiHub.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (configuration["Database:Provider"]?.ToLower() == "sqlserver")
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Services
        services.AddScoped<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IApiRunnerService, ApiRunnerService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IFileParser, FileParser>();
        services.AddScoped<IReportGenerator, ReportGenerator>();
        services.AddScoped<ICacheService, CacheService>();

        // Redis Cache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "ApiHub_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        // HTTP Clients for external APIs
        services.AddHttpClient<JsonPlaceholderClient>()
            .AddApiResilienceHandler("JsonPlaceholder");

        services.AddHttpClient<PetstoreClient>()
            .AddApiResilienceHandler("Petstore");

        services.AddHttpClient<ReqResClient>()
            .AddApiResilienceHandler("ReqRes");

        services.AddHttpClient<GitHubClient>()
            .AddApiResilienceHandler("GitHub");

        services.AddHttpClient<OpenWeatherClient>()
            .AddApiResilienceHandler("OpenWeather");

        // Generic HTTP client for dynamic connectors
        services.AddHttpClient("DynamicConnector")
            .AddApiResilienceHandler("DynamicConnector");

        return services;
    }
}
