namespace Vek;

/// <summary>
/// Describes a calendar day supplied to a date picker day template.
/// </summary>
public readonly struct DatePickerDayContext
{
    internal DatePickerDayContext(DateTime date, int day, bool isDisabled, bool isAdjacentMonth)
    {
        Date = date;
        Day = day;
        IsDisabled = isDisabled;
        IsAdjacentMonth = isAdjacentMonth;
    }

    /// <summary>
    /// The date represented by the calendar cell.
    /// </summary>
    public DateTime Date { get; }

    /// <summary>
    /// The day number in the picker's active calendar.
    /// </summary>
    public int Day { get; }

    /// <summary>
    /// Whether the date cannot be selected.
    /// </summary>
    public bool IsDisabled { get; }

    /// <summary>
    /// Whether the date belongs to an adjacent month shown in the calendar grid.
    /// </summary>
    public bool IsAdjacentMonth { get; }
}
