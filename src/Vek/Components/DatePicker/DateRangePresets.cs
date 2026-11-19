namespace Vek;

/// <summary>
/// Represents a predefined date range with a display label.
/// </summary>
public class DateRangePreset : IEquatable<DateRangePreset>
{
    /// <summary>
    /// Gets or sets the display label of the preset.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dynamic date range generator function.
    /// </summary>
    public Func<DateTime, DateRange>? RangeFunc { get; set; }

    /// <summary>
    /// Gets or sets the fixed date range for this preset.
    /// </summary>
    public DateRange? Range { get; set; }

    /// <summary>
    /// Creates a new empty preset.
    /// </summary>
    public DateRangePreset()
    {
    }

    /// <summary>
    /// Creates a preset with the specified label and fixed date range.
    /// </summary>
    /// <param name="label">The display label.</param>
    /// <param name="range">The static date range.</param>
    public DateRangePreset(string label, DateRange range)
    {
        Label = label;
        Range = range;
    }

    /// <summary>
    /// Creates a preset with the specified label, start date, and end date.
    /// </summary>
    /// <param name="label">The display label.</param>
    /// <param name="start">The start date.</param>
    /// <param name="end">The end date.</param>
    public DateRangePreset(string label, DateTime start, DateTime end)
    {
        Label = label;
        Range = new DateRange(start, end);
    }

    /// <summary>
    /// Creates a preset with the specified label and dynamic date range generator.
    /// </summary>
    /// <param name="label">The display label.</param>
    /// <param name="rangeFunc">The function generating the date range given a reference date.</param>
    public DateRangePreset(string label, Func<DateTime, DateRange> rangeFunc)
    {
        Label = label;
        RangeFunc = rangeFunc;
    }

    /// <summary>
    /// Creates a preset with the specified label and parameterless date range generator.
    /// </summary>
    /// <param name="label">The display label.</param>
    /// <param name="rangeFunc">The function generating the date range.</param>
    public DateRangePreset(string label, Func<DateRange> rangeFunc)
    {
        Label = label;
        RangeFunc = _ => rangeFunc();
    }

    /// <summary>
    /// Computes the effective date range for this preset relative to the specified date.
    /// </summary>
    /// <param name="referenceDate">The reference date, typically the current local date.</param>
    /// <returns>The resolved <see cref="DateRange"/>.</returns>
    public DateRange GetRange(DateTime referenceDate)
    {
        if (RangeFunc is not null)
        {
            return RangeFunc(referenceDate);
        }

        if (Range is not null)
        {
            return Range;
        }

        return new DateRange();
    }

    /// <inheritdoc />
    public override string ToString() => Label;

    /// <inheritdoc />
    public bool Equals(DateRangePreset? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Label, other.Label, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as DateRangePreset);

    /// <inheritdoc />
    public override int GetHashCode() => Label?.GetHashCode(StringComparison.Ordinal) ?? 0;
}

/// <summary>
/// Provides predefined date range presets and standard presets collections for <see cref="VekDateRangePicker"/>.
/// </summary>
public static class DateRangePresets
{
    /// <summary>
    /// Creates a preset representing today.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "Today".</param>
    public static DateRangePreset Today(string label = "Today") =>
        new(label, date => new DateRange(date.Date, date.Date));

    /// <summary>
    /// Creates a preset representing yesterday.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "Yesterday".</param>
    public static DateRangePreset Yesterday(string label = "Yesterday") =>
        new(label, date =>
        {
            var yesterday = date.Date.AddDays(-1);
            return new DateRange(yesterday, yesterday);
        });

    /// <summary>
    /// Creates a preset representing the current week.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "This Week".</param>
    /// <param name="firstDayOfWeek">The first day of the week. Defaults to <see cref="DayOfWeek.Monday"/>.</param>
    public static DateRangePreset ThisWeek(string label = "This Week", DayOfWeek firstDayOfWeek = DayOfWeek.Monday) =>
        new(label, date =>
        {
            var diff = (7 + (date.DayOfWeek - firstDayOfWeek)) % 7;
            var start = date.Date.AddDays(-diff);
            var end = start.AddDays(6);
            return new DateRange(start, end);
        });

    /// <summary>
    /// Creates a preset representing the previous week.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "Last Week".</param>
    /// <param name="firstDayOfWeek">The first day of the week. Defaults to <see cref="DayOfWeek.Monday"/>.</param>
    public static DateRangePreset LastWeek(string label = "Last Week", DayOfWeek firstDayOfWeek = DayOfWeek.Monday) =>
        new(label, date =>
        {
            var diff = (7 + (date.DayOfWeek - firstDayOfWeek)) % 7;
            var start = date.Date.AddDays(-diff - 7);
            var end = start.AddDays(6);
            return new DateRange(start, end);
        });

    /// <summary>
    /// Creates a preset representing the last 7 days inclusive of today.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "Last 7 Days".</param>
    public static DateRangePreset Last7Days(string label = "Last 7 Days") =>
        new(label, date => new DateRange(date.Date.AddDays(-6), date.Date));

    /// <summary>
    /// Creates a preset representing the last 30 days inclusive of today.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "Last 30 Days".</param>
    public static DateRangePreset Last30Days(string label = "Last 30 Days") =>
        new(label, date => new DateRange(date.Date.AddDays(-29), date.Date));

    /// <summary>
    /// Creates a preset representing the current calendar month.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "This Month".</param>
    public static DateRangePreset ThisMonth(string label = "This Month") =>
        new(label, date =>
        {
            var start = new DateTime(date.Year, date.Month, 1);
            var end = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            return new DateRange(start, end);
        });

    /// <summary>
    /// Creates a preset representing the previous calendar month.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "Last Month".</param>
    public static DateRangePreset LastMonth(string label = "Last Month") =>
        new(label, date =>
        {
            var prev = date.Date.AddMonths(-1);
            var start = new DateTime(prev.Year, prev.Month, 1);
            var end = new DateTime(prev.Year, prev.Month, DateTime.DaysInMonth(prev.Year, prev.Month));
            return new DateRange(start, end);
        });

    /// <summary>
    /// Creates a preset representing the current calendar year.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "This Year".</param>
    public static DateRangePreset ThisYear(string label = "This Year") =>
        new(label, date => new DateRange(new DateTime(date.Year, 1, 1), new DateTime(date.Year, 12, 31)));

    /// <summary>
    /// Creates a preset representing the period from the start of the current year through today.
    /// </summary>
    /// <param name="label">Optional custom label. Defaults to "Year to Date".</param>
    public static DateRangePreset YearToDate(string label = "Year to Date") =>
        new(label, date => new DateRange(new DateTime(date.Year, 1, 1), date.Date));

    /// <summary>
    /// A standard collection of default quick preset ranges.
    /// </summary>
    public static IReadOnlyList<DateRangePreset> Default { get; } =
    [
        Today(),
        ThisWeek(),
        Last7Days(),
        Last30Days(),
        ThisMonth(),
        YearToDate()
    ];
}
