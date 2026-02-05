namespace ApiHub.Domain.ValueObjects;

public enum ApiKeyLocation
{
    Header,
    QueryString
}

public record ApiKeySettings(
    ApiKeyLocation Location,
    string ParameterName);
