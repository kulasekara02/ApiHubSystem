using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Connectors.Queries;

public class GetConnectorsQueryHandler : IRequestHandler<GetConnectorsQuery, PaginatedList<ConnectorDto>>
{
    private readonly IApplicationDbContext _context;

    public GetConnectorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ConnectorDto>> Handle(GetConnectorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Connectors
            .Include(c => c.Endpoints)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(request.SearchTerm) ||
                c.Description.Contains(request.SearchTerm));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        var projectedQuery = query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConnectorDto(
                c.Id,
                c.Name,
                c.Description,
                c.BaseUrl,
                c.AuthType,
                c.Status,
                c.IsPublic,
                c.Endpoints.Count,
                c.CreatedAt));

        return await PaginatedList<ConnectorDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
