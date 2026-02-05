using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken,
    IEnumerable<string> Roles);
