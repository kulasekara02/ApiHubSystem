namespace ApiHub.Api.Contracts;

public record ErrorResponse(IEnumerable<string> Errors);

public record ValidationErrorResponse(
    string Message,
    IDictionary<string, string[]> Errors);
