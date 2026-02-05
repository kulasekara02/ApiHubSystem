using ApiHub.Application.Common.Models;
using ApiHub.Domain.Enums;
using MediatR;

namespace ApiHub.Application.Features.Records.Queries;

public record GetApiRecordsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? ConnectorId = null,
    HttpMethodType? Method = null,
    int? StatusCode = null,
    bool? IsSuccess = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SearchTerm = null) : IRequest<PaginatedList<ApiRecordDto>>;

public record ApiRecordDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ConnectorId,
    string ConnectorName,
    string CorrelationId,
    HttpMethodType Method,
    string RequestUrl,
    int StatusCode,
    long DurationMs,
    bool IsSuccess,
    string? ErrorMessage,
    DateTime CreatedAt);
