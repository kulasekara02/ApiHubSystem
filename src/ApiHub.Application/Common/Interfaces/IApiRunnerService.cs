using ApiHub.Domain.Enums;

namespace ApiHub.Application.Common.Interfaces;

public record ApiRequest(
    Guid ConnectorId,
    string Endpoint,
    HttpMethodType Method,
    Dictionary<string, string>? Headers = null,
    string? Body = null,
    Dictionary<string, string>? QueryParams = null);

public record ApiResponse(
    int StatusCode,
    Dictionary<string, string> Headers,
    string? Body,
    long DurationMs,
    bool IsSuccess,
    string? ErrorMessage,
    int RetryCount);

public interface IApiRunnerService
{
    Task<ApiResponse> SendRequestAsync(ApiRequest request, CancellationToken cancellationToken = default);
}
