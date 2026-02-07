using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Templates.Queries;

public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, Result<TemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTemplateByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TemplateDto>> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var template = await _context.RequestTemplates
            .Include(t => t.Connector)
            .Include(t => t.CreatedBy)
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
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
                t.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null)
        {
            return Result<TemplateDto>.Failure("Template not found.");
        }

        // Check access - allow if public or user is the creator
        if (!template.IsPublic && (!userId.HasValue || template.CreatedById != userId.Value))
        {
            return Result<TemplateDto>.Failure("You do not have permission to view this template.");
        }

        return Result<TemplateDto>.Success(template);
    }
}
