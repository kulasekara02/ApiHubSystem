using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Webhooks.Commands;

public class TestWebhookCommandHandler : IRequestHandler<TestWebhookCommand, Result<TestWebhookResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDateTime _dateTime;

    public TestWebhookCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHttpClientFactory httpClientFactory,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpClientFactory = httpClientFactory;
        _dateTime = dateTime;
    }

    public async Task<Result<TestWebhookResult>> Handle(TestWebhookCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result<TestWebhookResult>.Failure("User not authenticated.");
        }

        var webhook = await _context.Webhooks
            .FirstOrDefaultAsync(w => w.Id == request.Id && w.CreatedById == userId.Value, cancellationToken);

        if (webhook == null)
        {
            return Result<TestWebhookResult>.Failure("Webhook not found.");
        }

        var testPayload = new
        {
            @event = "test",
            timestamp = _dateTime.UtcNow,
            webhookId = webhook.Id,
            message = "This is a test webhook delivery from ApiHub."
        };

        var jsonPayload = JsonSerializer.Serialize(testPayload);
        var signature = ComputeHmacSha256(jsonPayload, webhook.Secret);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, webhook.Url)
            {
                Content = content
            };
            requestMessage.Headers.Add("X-Webhook-Signature", signature);
            requestMessage.Headers.Add("X-Webhook-Event", "test");
            requestMessage.Headers.Add("X-Webhook-Id", webhook.Id.ToString());

            var response = await client.SendAsync(requestMessage, cancellationToken);
            stopwatch.Stop();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            return Result<TestWebhookResult>.Success(new TestWebhookResult(
                IsSuccess: response.IsSuccessStatusCode,
                StatusCode: (int)response.StatusCode,
                ResponseBody: responseBody.Length > 1000 ? responseBody[..1000] + "..." : responseBody,
                ErrorMessage: null,
                DurationMs: stopwatch.ElapsedMilliseconds
            ));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Result<TestWebhookResult>.Success(new TestWebhookResult(
                IsSuccess: false,
                StatusCode: null,
                ResponseBody: null,
                ErrorMessage: ex.Message,
                DurationMs: stopwatch.ElapsedMilliseconds
            ));
        }
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return $"sha256={Convert.ToHexString(hashBytes).ToLowerInvariant()}";
    }
}
