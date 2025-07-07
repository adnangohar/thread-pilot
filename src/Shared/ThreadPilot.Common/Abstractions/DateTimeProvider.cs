namespace ThreadPilot.Common.Abstractions;

public class DateTimeProvider : IDateTimeProvider
{
     public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
