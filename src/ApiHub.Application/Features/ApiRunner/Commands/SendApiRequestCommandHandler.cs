using System.Text.Json;
using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using ApiHub.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.ApiRunner.Commands;

public class SendApiRequestCommandHandler : IRequestHandler<SendApiRequestCommand, Result<ApiRequestResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IApiRunnerService _apiRunner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public SendApiRequestCommandHandler(
        IApplicationDbContext context,
        IApiRunnerService apiRunner,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _context = context;
        _apiRunner = apiRunner;
        _currentUser = currentUser;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<Result<ApiRequestResult>> Handle(SendApiRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var connector = await _context.Connectors
            .FirstOrDefaultAsync(c => c.Id == request.ConnectorId, cancellationToken);

        if (connector == null)
        {
            throw new NotFoundException(nameof(Connector), request.ConnectorId);
        }

        if (connector.Status != ConnectorStatus.Active)
        {
            return Result<ApiRequestResult>.Failure("Connector is not active.");
        }

        var correlationId = _currentUser.CorrelationId ?? Guid.NewGuid().ToString();

        var apiRequest = new ApiRequest(
            request.ConnectorId,
            request.Endpoint,
            request.Method,
            request.Headers,
            request.Body,
            request.QueryParams);

        var response = await _apiRunner.SendRequestAsync(apiRequest, cancellationToken);

        var apiRecord = new ApiRecord
        {
            Id = Guid.NewGuid(),
            UserId = _currentUser.UserId.Value,
            ConnectorId = request.ConnectorId,
            CorrelationId = correlationId,
            Method = request.Method,
            RequestUrl = $"{connector.BaseUrl}{request.Endpoint}",
            RequestHeaders = request.Headers != null ? JsonSerializer.Serialize(SanitizeHeaders(request.Headers)) : null,
            RequestBody = TruncateBody(request.Body),
            StatusCode = response.StatusCode,
            ResponseHeaders = JsonSerializer.Serialize(response.Headers),
            ResponseBody = TruncateBody(response.Body),
            DurationMs = response.DurationMs,
            IsSuccess = response.IsSuccess,
            ErrorMessage = response.ErrorMessage,
            RetryCount = response.RetryCount,
            CreatedAt = _dateTime.UtcNow
        };

        _context.ApiRecords.Add(apiRecord);

        Guid? datasetId = null;
        if (request.SaveAsDataset && response.IsSuccess && !string.IsNullOrEmpty(response.Body))
        {
            var dataset = new Dataset
            {
                Id = Guid.NewGuid(),
                ApiRecordId = apiRecord.Id,
                Name = $"Dataset_{apiRecord.Id:N}",
                JsonSnapshot = response.Body,
                RecordCount = CountJsonRecords(response.Body),
                CreatedAt = _dateTime.UtcNow
            };

            _context.Datasets.Add(dataset);
            datasetId = dataset.Id;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.ApiRequestSent,
            nameof(ApiRecord),
            apiRecord.Id.ToString(),
            newValues: new { request.ConnectorId, request.Method, request.Endpoint, response.StatusCode },
            cancellationToken: cancellationToken);

        return Result<ApiRequestResult>.Success(new ApiRequestResult(
            apiRecord.Id,
            response.StatusCode,
            response.Headers,
            response.Body,
            response.DurationMs,
            response.IsSuccess,
            response.ErrorMessage,
            datasetId));
    }

    private static Dictionary<string, string> SanitizeHeaders(Dictionary<string, string> headers)
    {
        var sensitiveKeys = new[] { "authorization", "api-key", "x-api-key", "bearer", "token" };
        return headers.ToDictionary(
            h => h.Key,
            h => sensitiveKeys.Any(k => h.Key.ToLower().Contains(k)) ? "[REDACTED]" : h.Value);
    }

    private static string? TruncateBody(string? body, int maxLength = 50000)
    {
        if (string.IsNullOrEmpty(body)) return body;
        return body.Length > maxLength ? body[..maxLength] + "...[TRUNCATED]" : body;
    }

    private static int CountJsonRecords(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Array
                ? doc.RootElement.GetArrayLength()
                : 1;
        }
        catch
        {
            return 0;
        }
    }
}
