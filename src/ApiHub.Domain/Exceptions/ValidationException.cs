namespace ApiHub.Domain.Exceptions;

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("VALIDATION_ERROR", "One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message)
        : base("VALIDATION_ERROR", message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", "One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}
