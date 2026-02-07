using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Templates.Commands;

public record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? ConnectorId,
    string Method,
    string Endpoint,
    string? Headers,
    string? Body,
    string? QueryParams,
    bool IsPublic) : IRequest<Result>;
