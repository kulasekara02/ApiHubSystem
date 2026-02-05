namespace ApiHub.Domain.ValueObjects;

public record DateTimeRange(DateTime Start, DateTime End)
{
    public bool Contains(DateTime dateTime) => dateTime >= Start && dateTime <= End;

    public TimeSpan Duration => End - Start;

    public static DateTimeRange Today => new(
        DateTime.UtcNow.Date,
        DateTime.UtcNow.Date.AddDays(1).AddTicks(-1));

    public static DateTimeRange LastWeek => new(
        DateTime.UtcNow.Date.AddDays(-7),
        DateTime.UtcNow);

    public static DateTimeRange LastMonth => new(
        DateTime.UtcNow.Date.AddMonths(-1),
        DateTime.UtcNow);
}
