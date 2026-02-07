using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Templates.Commands;

public record DeleteTemplateCommand(Guid Id) : IRequest<Result>;
