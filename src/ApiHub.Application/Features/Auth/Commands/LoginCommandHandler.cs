using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ApiHub.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _auditService = auditService;
        _dateTime = dateTime;
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
