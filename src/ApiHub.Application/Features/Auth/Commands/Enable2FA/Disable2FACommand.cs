using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Auth.Commands.Enable2FA;

public record Disable2FACommand(string Password) : IRequest<Result<Disable2FAResponse>>;

public record Disable2FAResponse(bool IsDisabled, string Message);

public class Disable2FACommandHandler : IRequestHandler<Disable2FACommand, Result<Disable2FAResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public Disable2FACommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<Result<Disable2FAResponse>> Handle(Disable2FACommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Disable2FAResponse>.Failure("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return Result<Disable2FAResponse>.Failure("User not found.");
        }

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            return Result<Disable2FAResponse>.Failure("Invalid password.");
        }

        var token = await _context.UserTwoFactorTokens
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (token == null || !token.IsEnabled)
        {
            return Result<Disable2FAResponse>.Failure("Two-factor authentication is not enabled.");
        }

        _context.UserTwoFactorTokens.Remove(token);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Disable2FAResponse>.Success(new Disable2FAResponse(
            true,
            "Two-factor authentication has been disabled."));
    }
}
