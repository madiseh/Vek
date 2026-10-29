namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Describes the direction applied to a grid sort definition.
/// </summary>
public enum VekSortDirection
{
    /// <summary>
    /// Sorts values from lowest to highest.
    /// </summary>
    Ascending,

    /// <summary>
    /// Sorts values from highest to lowest.
    /// </summary>
    Descending
}

/// <summary>
/// Determines how many grid columns may contribute to sorting.
/// </summary>
public enum VekDataGridSortMode
{
    /// <summary>Disables sorting for all columns.</summary>
    None,
    /// <summary>Sorts by one column at a time.</summary>
    Single,
    /// <summary>Allows multiple columns to contribute to the sort order.</summary>
    Multiple
}

/// <summary>
/// Describes a comparison used to filter a grid column.
/// </summary>
public enum VekDataGridFilterOperator
{
    /// <summary>Matches equal values.</summary>
    Equals,
    /// <summary>Matches unequal values.</summary>
    NotEquals,
    /// <summary>Matches strings containing a value.</summary>
    Contains,
    /// <summary>Matches strings beginning with a value.</summary>
    StartsWith,
    /// <summary>Matches strings ending with a value.</summary>
    EndsWith,
    /// <summary>Matches values greater than a value.</summary>
    GreaterThan,
    /// <summary>Matches values greater than or equal to a value.</summary>
    GreaterThanOrEqual,
    /// <summary>Matches values less than a value.</summary>
    LessThan,
    /// <summary>Matches values less than or equal to a value.</summary>
    LessThanOrEqual,
    /// <summary>Matches null or empty values.</summary>
    IsEmpty,
    /// <summary>Matches nonempty values.</summary>
    IsNotEmpty,
    /// <summary>Matches null values.</summary>
    IsNull,
    /// <summary>Matches non-null values.</summary>
    IsNotNull
}

/// <summary>Chooses where a grid displays column filter controls.</summary>
public enum VekDataGridFilterMode
{
    /// <summary>Displays a compact filter panel above the table.</summary>
    Simple,
    /// <summary>Displays filter controls in each column menu.</summary>
    ColumnMenu,
    /// <summary>Displays filter controls in a row below the headings.</summary>
    ColumnRow
}

/// <summary>Provides a custom filter template with apply and clear actions.</summary>
public sealed class VekDataGridFilterContext
{
    internal VekDataGridFilterContext(string? value, Action<string?> apply, Action clear)
        => (Value, Apply, Clear) = (value, apply, clear);

    /// <summary>Gets the currently applied text value.</summary>
    public string? Value { get; }
    /// <summary>Applies a value using the column's configured filter operator.</summary>
    public Action<string?> Apply { get; }
    /// <summary>Removes the active filter.</summary>
    public Action Clear { get; }
}

/// <summary>
/// Describes one ordered sort operation in a grid state.
/// </summary>
/// <param name="ColumnKey">The stable key of the sorted column.</param>
/// <param name="Direction">The direction applied to the column.</param>
/// <param name="Priority">The order in which this sort is applied.</param>
public sealed record VekDataGridSort(string ColumnKey, VekSortDirection Direction, int Priority);

/// <summary>
/// Describes one active filter operation in a grid state.
/// </summary>
/// <param name="ColumnKey">The stable key of the filtered column.</param>
/// <param name="Operator">The comparison applied to the value.</param>
/// <param name="Value">The value used by the comparison.</param>
/// <param name="CaseSensitive">Whether string comparisons distinguish letter case.</param>
public sealed record VekDataGridFilter(
    string ColumnKey,
    VekDataGridFilterOperator Operator,
    object? Value,
    bool CaseSensitive);

/// <summary>
/// Describes the page, sort, and filter state requested from a grid data source.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
/// <param name="Page">The one-based page requested by the grid.</param>
/// <param name="PageSize">The maximum number of items requested for the page.</param>
/// <param name="Sorts">The sort operations in priority order.</param>
/// <param name="Filters">The active filter operations.</param>
public sealed record VekDataGridState<TItem>(
    int Page,
    int PageSize,
    IReadOnlyList<VekDataGridSort> Sorts,
    IReadOnlyList<VekDataGridFilter> Filters);

/// <summary>
/// Contains one page of items and the total number of matching items.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
/// <param name="Items">The items displayed by the current page.</param>
/// <param name="TotalItems">The total count before paging.</param>
public sealed record VekDataGridData<TItem>(IReadOnlyList<TItem> Items, int TotalItems);
