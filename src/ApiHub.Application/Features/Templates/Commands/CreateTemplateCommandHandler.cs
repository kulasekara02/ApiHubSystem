using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;

namespace ApiHub.Application.Features.Templates.Commands;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public CreateTemplateCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result<Guid>.Failure("User not authenticated.");
        }

        var template = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ConnectorId = request.ConnectorId,
            Method = request.Method.ToUpperInvariant(),
            Endpoint = request.Endpoint,
            Headers = request.Headers,
            Body = request.Body,
            QueryParams = request.QueryParams,
            IsPublic = request.IsPublic,
            CreatedById = userId.Value,
            CreatedAt = _dateTime.UtcNow
        };

        _context.RequestTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.TemplateCreated,
            nameof(RequestTemplate),
            template.Id.ToString(),
            newValues: new { template.Name, template.Method, template.Endpoint },
            cancellationToken: cancellationToken);

        return Result<Guid>.Success(template.Id);
    }
}
