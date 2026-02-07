using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ApiHub.Application.Common.Interfaces;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Cronos;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiHub.Infrastructure.BackgroundJobs;

public interface IScheduledJobService
{
    Task RegisterAllJobsAsync(CancellationToken cancellationToken = default);
    Task RegisterJobAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task UnregisterJobAsync(Guid jobId);
    Task ExecuteJobAsync(Guid jobId, CancellationToken cancellationToken = default);
}

public class ScheduledJobService : IScheduledJobService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEncryptionService _encryptionService;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IDateTime _dateTime;
    private readonly ILogger<ScheduledJobService> _logger;

    public ScheduledJobService(
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IEncryptionService encryptionService,
        IRecurringJobManager recurringJobManager,
        IDateTime dateTime,
        ILogger<ScheduledJobService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _encryptionService = encryptionService;
        _recurringJobManager = recurringJobManager;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task RegisterAllJobsAsync(CancellationToken cancellationToken = default)
    {
        var enabledJobs = await _context.ScheduledJobs
            .Where(j => j.IsEnabled)
            .ToListAsync(cancellationToken);

        foreach (var job in enabledJobs)
        {
            RegisterRecurringJob(job);
        }

        _logger.LogInformation("Registered {Count} scheduled jobs", enabledJobs.Count);
    }

    public async Task RegisterJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _context.ScheduledJobs
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("Scheduled job {JobId} not found", jobId);
            return;
        }

        if (job.IsEnabled)
        {
            RegisterRecurringJob(job);
            _logger.LogInformation("Registered scheduled job {JobId}: {JobName}", jobId, job.Name);
        }
        else
        {
            UnregisterRecurringJob(jobId);
            _logger.LogInformation("Unregistered scheduled job {JobId}: {JobName}", jobId, job.Name);
        }
    }

    public Task UnregisterJobAsync(Guid jobId)
    {
        UnregisterRecurringJob(jobId);
        _logger.LogInformation("Unregistered scheduled job {JobId}", jobId);
        return Task.CompletedTask;
    }

    private void RegisterRecurringJob(ScheduledJob job)
    {
        var jobKey = GetJobKey(job.Id);

        _recurringJobManager.AddOrUpdate<IScheduledJobService>(
            jobKey,
            service => service.ExecuteJobAsync(job.Id, CancellationToken.None),
            job.CronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }

    private void UnregisterRecurringJob(Guid jobId)
    {
        var jobKey = GetJobKey(jobId);
        _recurringJobManager.RemoveIfExists(jobKey);
    }

    private static string GetJobKey(Guid jobId) => $"scheduled-job-{jobId}";

    public async Task ExecuteJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _context.ScheduledJobs
            .Include(j => j.Connector)
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("Scheduled job {JobId} not found for execution", jobId);
            return;
        }

        if (!job.IsEnabled)
        {
            _logger.LogInformation("Scheduled job {JobId} is disabled, skipping execution", jobId);
            return;
        }

        var execution = new ScheduledJobExecution
        {
            Id = Guid.NewGuid(),
            ScheduledJobId = jobId,
            StartedAt = _dateTime.UtcNow,
            Status = "Running"
        };

        _context.ScheduledJobExecutions.Add(execution);
        await _context.SaveChangesAsync(cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Executing scheduled job {JobId}: {JobName}", jobId, job.Name);

            var response = await SendRequestAsync(job, cancellationToken);

            stopwatch.Stop();

            execution.CompletedAt = _dateTime.UtcNow;
            execution.DurationMs = stopwatch.ElapsedMilliseconds;
            execution.HttpStatusCode = response.StatusCode;
            execution.ResponseBody = TruncateResponse(response.Body);

            if (response.IsSuccess)
            {
                execution.Status = "Success";
                job.SuccessCount++;
                job.LastRunStatus = "Success";
            }
            else
            {
                execution.Status = "Failed";
                execution.ErrorMessage = response.ErrorMessage ?? $"HTTP {response.StatusCode}";
                job.FailureCount++;
                job.LastRunStatus = "Failed";
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            execution.CompletedAt = _dateTime.UtcNow;
            execution.DurationMs = stopwatch.ElapsedMilliseconds;
            execution.Status = "Failed";
            execution.ErrorMessage = ex.Message;

            job.FailureCount++;
            job.LastRunStatus = "Failed";

            _logger.LogError(ex, "Scheduled job {JobId} failed with exception", jobId);
        }

        job.LastRunAt = _dateTime.UtcNow;

        // Calculate next run time
        try
        {
            var cronExpression = CronExpression.Parse(job.CronExpression);
            job.NextRunAt = cronExpression.GetNextOccurrence(_dateTime.UtcNow, TimeZoneInfo.Utc);
        }
        catch (CronFormatException ex)
        {
            _logger.LogError(ex, "Invalid cron expression for job {JobId}: {CronExpression}", jobId, job.CronExpression);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Scheduled job {JobId} completed with status {Status} in {Duration}ms",
            jobId,
            execution.Status,
            execution.DurationMs);
    }

    private async Task<JobExecutionResponse> SendRequestAsync(ScheduledJob job, CancellationToken cancellationToken)
    {
        var connector = job.Connector;
        var httpClient = _httpClientFactory.CreateClient($"Connector_{connector.Name}");

        var url = $"{connector.BaseUrl.TrimEnd('/')}/{job.Endpoint.TrimStart('/')}";
        var httpMethod = new HttpMethod(job.Method);
        var httpRequest = new HttpRequestMessage(httpMethod, url);

        // Add headers from job configuration
        if (!string.IsNullOrEmpty(job.Headers))
        {
            try
            {
                var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(job.Headers);
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse headers for job {JobId}", job.Id);
            }
        }

        // Add authentication based on connector configuration
        await AddAuthenticationAsync(httpRequest, connector, job.CreatedById, cancellationToken);

        // Add body if applicable
        if (!string.IsNullOrEmpty(job.Body) &&
            job.Method is "POST" or "PUT" or "PATCH")
        {
            httpRequest.Content = new StringContent(job.Body, Encoding.UTF8, "application/json");
        }

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        return new JobExecutionResponse(
            (int)response.StatusCode,
            responseBody,
            response.IsSuccessStatusCode,
            response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode}");
    }

    private async Task AddAuthenticationAsync(
        HttpRequestMessage request,
        Connector connector,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (connector.AuthType == AuthenticationType.None)
            return;

        var secret = await _context.UserConnectorSecrets
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.ConnectorId == connector.Id,
                cancellationToken);

        if (secret == null)
            return;

        var apiKey = _encryptionService.Decrypt(secret.EncryptedApiKey);
        var token = secret.EncryptedToken != null ? _encryptionService.Decrypt(secret.EncryptedToken) : null;

        switch (connector.AuthType)
        {
            case AuthenticationType.ApiKey:
                if (!string.IsNullOrEmpty(connector.ApiKeyHeaderName))
                {
                    request.Headers.TryAddWithoutValidation(connector.ApiKeyHeaderName, apiKey);
                }
                else if (!string.IsNullOrEmpty(connector.ApiKeyQueryParamName))
                {
                    var uri = new UriBuilder(request.RequestUri!);
                    var query = uri.Query.TrimStart('?');
                    query = string.IsNullOrEmpty(query)
                        ? $"{connector.ApiKeyQueryParamName}={Uri.EscapeDataString(apiKey)}"
                        : $"{query}&{connector.ApiKeyQueryParamName}={Uri.EscapeDataString(apiKey)}";
                    uri.Query = query;
                    request.RequestUri = uri.Uri;
                }
                break;

            case AuthenticationType.BearerToken:
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token ?? apiKey);
                break;

            case AuthenticationType.BasicAuth:
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{token ?? string.Empty}"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                break;
        }
    }

    private static string? TruncateResponse(string? response, int maxLength = 10000)
    {
        if (string.IsNullOrEmpty(response))
            return response;

        return response.Length <= maxLength
            ? response
            : response[..maxLength] + "... [truncated]";
    }

    private record JobExecutionResponse(int StatusCode, string? Body, bool IsSuccess, string? ErrorMessage);
}
