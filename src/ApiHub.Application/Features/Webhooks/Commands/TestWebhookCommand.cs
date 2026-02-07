using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Webhooks.Commands;

public record TestWebhookCommand(Guid Id) : IRequest<Result<TestWebhookResult>>;

public record TestWebhookResult(
    bool IsSuccess,
    int? StatusCode,
    string? ResponseBody,
    string? ErrorMessage,
    long DurationMs);
