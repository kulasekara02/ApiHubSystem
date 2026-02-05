using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;

namespace ApiHub.Application.Features.Connectors.Commands;

public class CreateConnectorCommandHandler : IRequestHandler<CreateConnectorCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public CreateConnectorCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _context = context;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<Result<Guid>> Handle(CreateConnectorCommand request, CancellationToken cancellationToken)
    {
        var connector = new Connector
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            BaseUrl = request.BaseUrl.TrimEnd('/'),
            AuthType = request.AuthType,
            ApiKeyHeaderName = request.ApiKeyHeaderName,
            ApiKeyQueryParamName = request.ApiKeyQueryParamName,
            VersionHeaderName = request.VersionHeaderName,
            DefaultVersion = request.DefaultVersion,
            TimeoutSeconds = request.TimeoutSeconds,
            MaxRetries = request.MaxRetries,
            IsPublic = request.IsPublic,
            Status = ConnectorStatus.Active,
            CreatedAt = _dateTime.UtcNow
        };

        if (request.Endpoints != null)
        {
            foreach (var endpointDto in request.Endpoints)
            {
                connector.Endpoints.Add(new ConnectorEndpoint
                {
                    Id = Guid.NewGuid(),
                    ConnectorId = connector.Id,
                    Name = endpointDto.Name,
                    Path = endpointDto.Path,
                    Method = endpointDto.Method,
                    Description = endpointDto.Description,
                    IsEnabled = true,
                    CreatedAt = _dateTime.UtcNow
                });
            }
        }

        _context.Connectors.Add(connector);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            AuditAction.ConnectorCreated,
            nameof(Connector),
            connector.Id.ToString(),
            newValues: new { connector.Name, connector.BaseUrl, connector.AuthType },
            cancellationToken: cancellationToken);

        return Result<Guid>.Success(connector.Id);
    }
}
