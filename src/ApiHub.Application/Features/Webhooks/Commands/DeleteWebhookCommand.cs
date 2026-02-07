using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Webhooks.Commands;

public record DeleteWebhookCommand(Guid Id) : IRequest<Result>;
