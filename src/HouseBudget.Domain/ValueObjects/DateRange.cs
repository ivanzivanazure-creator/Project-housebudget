using HouseBudget.Domain.Exceptions;

namespace HouseBudget.Domain.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Of(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new DomainException("End date must be on or after start date.");
        return new DateRange(start, end);
    }

    public static DateRange CurrentMonth()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = new DateOnly(today.Year, today.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new DateRange(start, end);
    }

    public static DateRange CurrentYear()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return new DateRange(new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31));
    }

    public int TotalDays => End.DayNumber - Start.DayNumber + 1;
    public bool Contains(DateOnly date) => date >= Start && date <= End;
    public bool Overlaps(DateRange other) => Start <= other.End && End >= other.Start;

    public bool Equals(DateRange? other) => other is not null && Start == other.Start && End == other.End;
    public override bool Equals(object? obj) => obj is DateRange d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Start, End);
    public override string ToString() => $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
}
