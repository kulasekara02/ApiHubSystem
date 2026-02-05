using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace ApiHub.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static IHttpClientBuilder AddApiResilienceHandler(this IHttpClientBuilder builder, string name)
    {
        builder.AddResilienceHandler($"{name}-pipeline", (resilienceBuilder, context) =>
        {
            // Rate limiter
            resilienceBuilder.AddConcurrencyLimiter(
                permitLimit: 100,
                queueLimit: 50);

            // Retry policy
            resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
            });

            // Circuit breaker
            resilienceBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = args => ValueTask.FromResult(ShouldBreak(args.Outcome))
            });

            // Timeout
            resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(30));
        });

        return builder;
    }

    private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception != null)
            return true;

        var response = outcome.Result;
        if (response == null)
            return true;

        return response.StatusCode is
            HttpStatusCode.RequestTimeout or
            HttpStatusCode.TooManyRequests or
            HttpStatusCode.InternalServerError or
            HttpStatusCode.BadGateway or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout;
    }

    private static bool ShouldBreak(Outcome<HttpResponseMessage> outcome)
    {
        if (outcome.Exception != null)
            return true;

        var response = outcome.Result;
        if (response == null)
            return true;

        return response.StatusCode is
            HttpStatusCode.InternalServerError or
            HttpStatusCode.BadGateway or
            HttpStatusCode.ServiceUnavailable or
            HttpStatusCode.GatewayTimeout;
    }
}
