using ApiHub.Application.Common.Models;
using ApiHub.Domain.Enums;
using MediatR;

namespace ApiHub.Application.Features.Connectors.Commands;

public record CreateConnectorCommand(
    string Name,
    string Description,
    string BaseUrl,
    AuthenticationType AuthType,
    string? ApiKeyHeaderName,
    string? ApiKeyQueryParamName,
    string? VersionHeaderName,
    string? DefaultVersion,
    int TimeoutSeconds,
    int MaxRetries,
    bool IsPublic,
    List<CreateEndpointDto>? Endpoints) : IRequest<Result<Guid>>;

public record CreateEndpointDto(
    string Name,
    string Path,
    HttpMethodType Method,
    string? Description);
