using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Templates.Queries;

public record GetTemplateByIdQuery(Guid Id) : IRequest<Result<TemplateDto>>;
