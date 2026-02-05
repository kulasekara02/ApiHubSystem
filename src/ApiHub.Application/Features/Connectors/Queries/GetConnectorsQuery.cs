using ApiHub.Application.Common.Models;
using ApiHub.Domain.Enums;
using MediatR;

namespace ApiHub.Application.Features.Connectors.Queries;

public record GetConnectorsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    ConnectorStatus? Status = null) : IRequest<PaginatedList<ConnectorDto>>;

public record ConnectorDto(
    Guid Id,
    string Name,
    string Description,
    string BaseUrl,
    AuthenticationType AuthType,
    ConnectorStatus Status,
    bool IsPublic,
    int EndpointCount,
    DateTime CreatedAt);
