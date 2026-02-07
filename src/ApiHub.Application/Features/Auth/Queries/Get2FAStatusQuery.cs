using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApiHub.Application.Features.Auth.Queries;

public record Get2FAStatusQuery : IRequest<Result<Get2FAStatusResponse>>;

public record Get2FAStatusResponse(
    bool IsEnabled,
    DateTime? EnabledAt,
    int RecoveryCodesRemaining);

public class Get2FAStatusQueryHandler : IRequestHandler<Get2FAStatusQuery, Result<Get2FAStatusResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public Get2FAStatusQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Get2FAStatusResponse>> Handle(Get2FAStatusQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Get2FAStatusResponse>.Failure("User not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var token = await _context.UserTwoFactorTokens
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (token == null || !token.IsEnabled)
        {
            return Result<Get2FAStatusResponse>.Success(new Get2FAStatusResponse(
                false,
                null,
                0));
        }

        return Result<Get2FAStatusResponse>.Success(new Get2FAStatusResponse(
            token.IsEnabled,
            token.EnabledAt,
            token.RecoveryCodes.Count));
    }
}
