using System.Diagnostics;
using ApiHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApiHub.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;
        var correlationId = _currentUserService.CorrelationId;

        _logger.LogInformation(
            "Handling {RequestName} for User {UserId} CorrelationId {CorrelationId}",
            requestName, userId, correlationId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} for User {UserId} in {ElapsedMs}ms",
                requestName, userId, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Error handling {RequestName} for User {UserId} after {ElapsedMs}ms: {ErrorMessage}",
                requestName, userId, stopwatch.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
}
