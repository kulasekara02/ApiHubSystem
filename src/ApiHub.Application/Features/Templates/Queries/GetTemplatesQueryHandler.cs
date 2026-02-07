using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Templates.Queries;

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PaginatedList<TemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTemplatesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var query = _context.RequestTemplates
            .Include(t => t.Connector)
            .Include(t => t.CreatedBy)
            .AsNoTracking();

        // Filter by visibility - show public templates and user's own templates
        if (request.IncludePrivate && userId.HasValue)
        {
            query = query.Where(t => t.IsPublic || t.CreatedById == userId.Value);
        }
        else if (userId.HasValue)
        {
            query = query.Where(t => t.IsPublic || t.CreatedById == userId.Value);
        }
        else
        {
            query = query.Where(t => t.IsPublic);
        }

        // Apply additional filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(t =>
                t.Name.Contains(request.SearchTerm) ||
                (t.Description != null && t.Description.Contains(request.SearchTerm)) ||
                t.Endpoint.Contains(request.SearchTerm));
        }

        if (request.IsPublic.HasValue)
        {
            query = query.Where(t => t.IsPublic == request.IsPublic.Value);
        }

        if (request.ConnectorId.HasValue)
        {
            query = query.Where(t => t.ConnectorId == request.ConnectorId.Value);
        }

        var projectedQuery = query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TemplateDto(
                t.Id,
                t.Name,
                t.Description,
                t.ConnectorId,
                t.Connector != null ? t.Connector.Name : null,
                t.Method,
                t.Endpoint,
                t.Headers,
                t.Body,
                t.QueryParams,
                t.IsPublic,
                t.CreatedById,
                t.CreatedBy.FirstName + " " + t.CreatedBy.LastName,
                t.CreatedAt,
                t.UpdatedAt));

        return await PaginatedList<TemplateDto>.CreateAsync(
            projectedQuery,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
