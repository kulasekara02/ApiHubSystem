using ApiHub.Domain.Entities;

namespace ApiHub.Application.Common.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles);
    Task<string> GenerateRefreshTokenAsync();
    Task<(Guid userId, string email)?> ValidateAccessTokenAsync(string token);
}
