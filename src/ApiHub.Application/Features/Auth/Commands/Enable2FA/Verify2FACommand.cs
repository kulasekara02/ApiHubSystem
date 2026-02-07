using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Auth.Commands.Enable2FA;

public record Verify2FACommand(string Code) : IRequest<Result<Verify2FAResponse>>;

public record Verify2FAResponse(
    bool IsEnabled,
    List<string> RecoveryCodes);

public class Verify2FACommandHandler : IRequestHandler<Verify2FACommand, Result<Verify2FAResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IDateTime _dateTime;

    public Verify2FACommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ITwoFactorService twoFactorService,
        IDateTime dateTime)
    {
        _context = context;
        _currentUserService = currentUserService;
        _twoFactorService = twoFactorService;
        _dateTime = dateTime;
    }

    public async Task<Result<Verify2FAResponse>> Handle(Verify2FACommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Verify2FAResponse>.Failure("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var token = await _context.UserTwoFactorTokens
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (token == null)
        {
            return Result<Verify2FAResponse>.Failure("Two-factor authentication setup not found. Please start the setup process first.");
        }

        if (token.IsEnabled)
        {
            return Result<Verify2FAResponse>.Failure("Two-factor authentication is already enabled.");
        }

        var isValid = _twoFactorService.ValidateCode(token.SecretKey, request.Code);

        if (!isValid)
        {
            return Result<Verify2FAResponse>.Failure("Invalid verification code. Please try again.");
        }

        // Generate recovery codes
        var recoveryCodes = _twoFactorService.GenerateRecoveryCodes();

        token.IsEnabled = true;
        token.EnabledAt = _dateTime.UtcNow;
        token.RecoveryCodes = recoveryCodes;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Verify2FAResponse>.Success(new Verify2FAResponse(
            true,
            recoveryCodes));
    }
}
