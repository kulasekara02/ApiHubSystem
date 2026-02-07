using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Templates.Commands;

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeleteTemplateCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
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

        // Only the creator can delete their template
        if (template.CreatedById != userId.Value)
        {
            return Result.Failure("You do not have permission to delete this template.");
        }

        _context.RequestTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.TemplateDeleted,
            nameof(RequestTemplate),
            template.Id.ToString(),
            oldValues: new { template.Name, template.Method, template.Endpoint },
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
