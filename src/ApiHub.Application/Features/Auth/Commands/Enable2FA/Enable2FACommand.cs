using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Auth.Commands.Enable2FA;

public record Enable2FACommand : IRequest<Result<Enable2FAResponse>>;

public record Enable2FAResponse(
    string SecretKey,
    string QrCodeUri,
    string ManualEntryKey);

public class Enable2FACommandHandler : IRequestHandler<Enable2FACommand, Result<Enable2FAResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IDateTime _dateTime;

    public Enable2FACommandHandler(
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

    public async Task<Result<Enable2FAResponse>> Handle(Enable2FACommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Enable2FAResponse>.Failure("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var userEmail = _currentUserService.Email ?? string.Empty;

        // Check if 2FA token already exists
        var existingToken = await _context.UserTwoFactorTokens
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (existingToken != null && existingToken.IsEnabled)
        {
            return Result<Enable2FAResponse>.Failure("Two-factor authentication is already enabled.");
        }

        var secretKey = _twoFactorService.GenerateSecretKey();
        var qrCodeUri = _twoFactorService.GenerateQrCodeUri(userEmail, secretKey);

        if (existingToken != null)
        {
            // Update existing token with new secret
            existingToken.SecretKey = secretKey;
            existingToken.CreatedAt = _dateTime.UtcNow;
            existingToken.IsEnabled = false;
            existingToken.EnabledAt = null;
            existingToken.RecoveryCodes = new List<string>();
        }
        else
        {
            // Create new token
            var token = new UserTwoFactorToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SecretKey = secretKey,
                IsEnabled = false,
                CreatedAt = _dateTime.UtcNow
            };

            _context.UserTwoFactorTokens.Add(token);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Enable2FAResponse>.Success(new Enable2FAResponse(
            secretKey,
            qrCodeUri,
            secretKey));
    }
}
