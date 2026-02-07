using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Templates.Queries;

public record GetTemplatesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsPublic = null,
    Guid? ConnectorId = null,
    bool IncludePrivate = false) : IRequest<PaginatedList<TemplateDto>>;

public record TemplateDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ConnectorId,
    string? ConnectorName,
    string Method,
    string Endpoint,
    string? Headers,
    string? Body,
    string? QueryParams,
    bool IsPublic,
    Guid CreatedById,
    string CreatedByName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
