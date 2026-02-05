using ApiHub.Application.Common.Models;
using ApiHub.Domain.Enums;
using MediatR;

namespace ApiHub.Application.Features.ApiRunner.Commands;

public record SendApiRequestCommand(
    Guid ConnectorId,
    string Endpoint,
    HttpMethodType Method,
    Dictionary<string, string>? Headers,
    string? Body,
    Dictionary<string, string>? QueryParams,
    bool SaveAsDataset = false) : IRequest<Result<ApiRequestResult>>;

public record ApiRequestResult(
    Guid ApiRecordId,
    int StatusCode,
    Dictionary<string, string> ResponseHeaders,
    string? ResponseBody,
    long DurationMs,
    bool IsSuccess,
    string? ErrorMessage,
    Guid? DatasetId);
