using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;
    private readonly IApplicationDbContext _context;
    private readonly ITwoFactorService _twoFactorService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IAuditService auditService,
        IDateTime dateTime,
        IApplicationDbContext context,
        ITwoFactorService twoFactorService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _auditService = auditService;
        _dateTime = dateTime;
        _context = context;
        _twoFactorService = twoFactorService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Result<LoginResponse>.Failure("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure("This account has been deactivated.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Result<LoginResponse>.Failure("This account has been locked. Please try again later.");
        }

        if (!result.Succeeded)
        {
            return Result<LoginResponse>.Failure("Invalid email or password.");
        }

        // Check if 2FA is enabled for the user
        var twoFactorToken = await _context.UserTwoFactorTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.IsEnabled, cancellationToken);

        if (twoFactorToken != null)
        {
            // 2FA is enabled, check if code is provided
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
            {
                // Return response indicating 2FA is required
                return Result<LoginResponse>.Success(new LoginResponse(
                    user.Id,
                    user.Email!,
                    user.FirstName,
                    user.LastName,
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>(),
                    RequiresTwoFactor: true));
            }

            // Validate the 2FA code
            var isValidCode = _twoFactorService.ValidateCode(twoFactorToken.SecretKey, request.TwoFactorCode);

            // If TOTP code is invalid, check recovery codes
            if (!isValidCode)
            {
                var recoveryCodeIndex = twoFactorToken.RecoveryCodes.IndexOf(request.TwoFactorCode.ToUpperInvariant());
                if (recoveryCodeIndex >= 0)
                {
                    // Valid recovery code, remove it from the list
                    twoFactorToken.RecoveryCodes.RemoveAt(recoveryCodeIndex);
                    await _context.SaveChangesAsync(cancellationToken);
                    isValidCode = true;
                }
            }

            if (!isValidCode)
            {
                return Result<LoginResponse>.Failure("Invalid two-factor authentication code.");
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync();

        user.LastLoginAt = _dateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await _auditService.LogAsync(
            AuditAction.Login,
            nameof(ApplicationUser),
            user.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken,
            roles));
    }
}
