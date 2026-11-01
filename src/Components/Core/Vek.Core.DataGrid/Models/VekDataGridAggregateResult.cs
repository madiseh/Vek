namespace Vek.Core.DataGrid;

/// <summary>
/// Represents the result of an aggregate computation.
/// </summary>
/// <param name="Key">The aggregate key.</param>
/// <param name="Title">The aggregate or column title.</param>
/// <param name="Value">The raw aggregate value.</param>
/// <param name="FormattedValue">The formatted value string.</param>
public sealed record VekDataGridAggregateResult(
    string Key,
    string Title,
    object? Value,
    string FormattedValue);

