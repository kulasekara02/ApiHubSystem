using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Templates.Commands;

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public UpdateTemplateCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result.Failure("User not authenticated.");
        }

        var template = await _context.RequestTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template == null)
        {
            return Result.Failure("Template not found.");
        }

        // Only the creator can update their template
        if (template.CreatedById != userId.Value)
        {
            return Result.Failure("You do not have permission to update this template.");
        }

        var oldValues = new { template.Name, template.Method, template.Endpoint };

        template.Name = request.Name;
        template.Description = request.Description;
        template.ConnectorId = request.ConnectorId;
        template.Method = request.Method.ToUpperInvariant();
        template.Endpoint = request.Endpoint;
        template.Headers = request.Headers;
        template.Body = request.Body;
        template.QueryParams = request.QueryParams;
        template.IsPublic = request.IsPublic;
        template.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.TemplateUpdated,
            nameof(RequestTemplate),
            template.Id.ToString(),
            oldValues: oldValues,
            newValues: new { template.Name, template.Method, template.Endpoint },
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
