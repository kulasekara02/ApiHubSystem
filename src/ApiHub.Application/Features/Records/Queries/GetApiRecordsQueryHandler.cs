using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Records.Queries;

public class GetApiRecordsQueryHandler : IRequestHandler<GetApiRecordsQuery, PaginatedList<ApiRecordDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApiRecordsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ApiRecordDto>> Handle(GetApiRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ApiRecords
            .Include(r => r.User)
            .Include(r => r.Connector)
            .AsNoTracking();

        if (request.ConnectorId.HasValue)
            query = query.Where(r => r.ConnectorId == request.ConnectorId.Value);

        if (request.Method.HasValue)
            query = query.Where(r => r.Method == request.Method.Value);

        if (request.StatusCode.HasValue)
            query = query.Where(r => r.StatusCode == request.StatusCode.Value);

        if (request.IsSuccess.HasValue)
            query = query.Where(r => r.IsSuccess == request.IsSuccess.Value);

        if (request.FromDate.HasValue)
            query = query.Where(r => r.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(r => r.CreatedAt <= request.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(r => r.RequestUrl.Contains(request.SearchTerm) ||
                                     r.CorrelationId.Contains(request.SearchTerm));

        var projectedQuery = query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ApiRecordDto(
                r.Id,
                r.UserId,
                r.User.FirstName + " " + r.User.LastName,
                r.ConnectorId,
                r.Connector.Name,
                r.CorrelationId,
                r.Method,
                r.RequestUrl,
                r.StatusCode,
                r.DurationMs,
                r.IsSuccess,
                r.ErrorMessage,
                r.CreatedAt));

        return await PaginatedList<ApiRecordDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
