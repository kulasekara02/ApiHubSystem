namespace ApiHub.Domain.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException()
        : base("UNAUTHORIZED", "You are not authorized to perform this action.")
    {
    }

    public UnauthorizedException(string message)
        : base("UNAUTHORIZED", message)
    {
    }
}
