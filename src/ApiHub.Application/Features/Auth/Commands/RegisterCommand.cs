using ApiHub.Application.Common.Models;
using MediatR;

namespace ApiHub.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName);
