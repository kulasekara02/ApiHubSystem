namespace ApiHub.Shared.Utilities;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= maxLength) return value;
        return value[..(maxLength - suffix.Length)] + suffix;
    }

    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return string.Concat(
            value.Select((c, i) =>
                i > 0 && char.IsUpper(c) ? "_" + c : c.ToString()))
            .ToLowerInvariant();
    }

    public static string MaskSensitive(this string value, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length <= visibleChars) return new string('*', value.Length);
        return value[..visibleChars] + new string('*', value.Length - visibleChars);
    }
}
