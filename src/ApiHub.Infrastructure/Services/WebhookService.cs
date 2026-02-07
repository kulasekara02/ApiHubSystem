using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ApiHub.Application.Common.Interfaces;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiHub.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDateTime _dateTime;
    private readonly ILogger<WebhookService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public WebhookService(
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IDateTime dateTime,
        ILogger<WebhookService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task TriggerAsync(WebhookEvent webhookEvent, object payload, CancellationToken cancellationToken = default)
    {
        var eventName = GetEventName(webhookEvent);

        var webhooks = await _context.Webhooks
            .Where(w => w.IsEnabled && w.Events.Contains(eventName))
            .ToListAsync(cancellationToken);

        if (!webhooks.Any())
        {
            _logger.LogDebug("No webhooks registered for event {Event}", webhookEvent);
            return;
        }

        var webhookPayload = new
        {
            @event = eventName,
            timestamp = _dateTime.UtcNow,
            data = payload
        };

        var jsonPayload = JsonSerializer.Serialize(webhookPayload, JsonOptions);

        foreach (var webhook in webhooks)
        {
            await SendWebhookAsync(webhook, webhookEvent, jsonPayload, cancellationToken);
        }
    }

    public async Task RetryDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken = default)
    {
        var delivery = await _context.WebhookDeliveries
            .Include(d => d.Webhook)
            .FirstOrDefaultAsync(d => d.Id == deliveryId, cancellationToken);

        if (delivery == null)
        {
            _logger.LogWarning("Webhook delivery {DeliveryId} not found for retry", deliveryId);
            return;
        }

        var webhook = delivery.Webhook;
        if (webhook == null)
        {
            _logger.LogWarning("Webhook not found for delivery {DeliveryId}", deliveryId);
            return;
        }

        await SendWebhookAsync(webhook, delivery.Event, delivery.Payload, cancellationToken, delivery.AttemptNumber + 1);
    }

    private async Task SendWebhookAsync(
        Webhook webhook,
        WebhookEvent webhookEvent,
        string jsonPayload,
        CancellationToken cancellationToken,
        int attemptNumber = 1)
    {
        var delivery = new WebhookDelivery
        {
            Id = Guid.NewGuid(),
            WebhookId = webhook.Id,
            Event = webhookEvent,
            Payload = jsonPayload,
            AttemptNumber = attemptNumber,
            TriggeredAt = _dateTime.UtcNow
        };

        var signature = ComputeHmacSha256(jsonPayload, webhook.Secret);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var client = _httpClientFactory.CreateClient("WebhookClient");
            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
            {
                Content = content
            };

            requestMessage.Headers.Add("X-Webhook-Signature", signature);
            requestMessage.Headers.Add("X-Webhook-Event", GetEventName(webhookEvent));
            requestMessage.Headers.Add("X-Webhook-Id", webhook.Id.ToString());
            requestMessage.Headers.Add("X-Webhook-Delivery-Id", delivery.Id.ToString());
            requestMessage.Headers.Add("User-Agent", "ApiHub-Webhooks/1.0");

            delivery.RequestHeaders = JsonSerializer.Serialize(new
            {
                XWebhookSignature = signature,
                XWebhookEvent = GetEventName(webhookEvent),
                XWebhookId = webhook.Id.ToString(),
                XWebhookDeliveryId = delivery.Id.ToString()
            }, JsonOptions);

            var response = await client.SendAsync(requestMessage, cancellationToken);
            stopwatch.Stop();

            delivery.ResponseStatusCode = (int)response.StatusCode;
            delivery.ResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            delivery.IsSuccess = response.IsSuccessStatusCode;
            delivery.DurationMs = stopwatch.ElapsedMilliseconds;
            delivery.CompletedAt = _dateTime.UtcNow;

            // Update webhook status
            webhook.LastTriggeredAt = _dateTime.UtcNow;
            webhook.LastResponseStatus = $"{(int)response.StatusCode} {response.StatusCode}";

            if (response.IsSuccessStatusCode)
            {
                webhook.FailureCount = 0;
            }
            else
            {
                webhook.FailureCount++;
                _logger.LogWarning(
                    "Webhook {WebhookId} delivery failed with status {StatusCode}. Failure count: {FailureCount}",
                    webhook.Id, response.StatusCode, webhook.FailureCount);
            }

            // Disable webhook after 10 consecutive failures
            if (webhook.FailureCount >= 10)
            {
                webhook.IsEnabled = false;
                _logger.LogWarning(
                    "Webhook {WebhookId} has been disabled after {FailureCount} consecutive failures",
                    webhook.Id, webhook.FailureCount);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            delivery.IsSuccess = false;
            delivery.ErrorMessage = ex.Message;
            delivery.DurationMs = stopwatch.ElapsedMilliseconds;
            delivery.CompletedAt = _dateTime.UtcNow;

            webhook.LastTriggeredAt = _dateTime.UtcNow;
            webhook.LastResponseStatus = $"Error: {ex.Message}";
            webhook.FailureCount++;

            _logger.LogError(ex, "Webhook {WebhookId} delivery failed with exception", webhook.Id);

            if (webhook.FailureCount >= 10)
            {
                webhook.IsEnabled = false;
                _logger.LogWarning(
                    "Webhook {WebhookId} has been disabled after {FailureCount} consecutive failures",
                    webhook.Id, webhook.FailureCount);
            }
        }

        _context.WebhookDeliveries.Add(delivery);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return $"sha256={Convert.ToHexString(hashBytes).ToLowerInvariant()}";
    }

    private static string GetEventName(WebhookEvent webhookEvent)
    {
        return webhookEvent switch
        {
            WebhookEvent.ApiRequestCompleted => "api.request.completed",
            WebhookEvent.ApiRequestFailed => "api.request.failed",
            WebhookEvent.ConnectorCreated => "connector.created",
            WebhookEvent.ConnectorUpdated => "connector.updated",
            WebhookEvent.ConnectorDeleted => "connector.deleted",
            WebhookEvent.UserRegistered => "user.registered",
            WebhookEvent.ReportGenerated => "report.generated",
            WebhookEvent.FileUploaded => "file.uploaded",
            _ => webhookEvent.ToString().ToLowerInvariant()
        };
    }
}
