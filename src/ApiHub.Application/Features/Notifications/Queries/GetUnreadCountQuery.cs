using MediatR;

namespace ApiHub.Application.Features.Notifications.Queries;

public record GetUnreadCountQuery() : IRequest<int>;
