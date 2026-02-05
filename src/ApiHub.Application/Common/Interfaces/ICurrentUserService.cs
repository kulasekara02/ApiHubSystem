namespace ApiHub.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsAnalyst { get; }
    string? IpAddress { get; }
    string? CorrelationId { get; }
}
