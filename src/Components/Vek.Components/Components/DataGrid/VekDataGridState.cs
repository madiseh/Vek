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

/// <summary>Controls whether the grid permits no, one, or multiple selected rows.</summary>
public enum VekDataGridSelectionMode
{
    /// <summary>Row selection is disabled.</summary>
    None,
    /// <summary>Only one row can be selected at a time.</summary>
    Single,
    /// <summary>Multiple rows can be selected at a time.</summary>
    Multiple
}

/// <summary>Controls selection retention when an active filter hides selected rows.</summary>
public enum VekDataGridFilteredSelectionMode
{
    /// <summary>Keep selected items while they are filtered out.</summary>
    Keep,
    /// <summary>Remove selected items when a filter hides them.</summary>
    Clear
}

/// <summary>Controls how grid rows expose their editable fields.</summary>
public enum VekDataGridEditMode
{
    /// <summary>Editing is disabled.</summary>
    None,
    /// <summary>Edits one cell at a time.</summary>
    Cell,
    /// <summary>Edits all configured cells for one row at a time.</summary>
    Row,
    /// <summary>Edits one row through a form-oriented template.</summary>
    Form
}

/// <summary>Defines the ordering applied to the values in a grouped column.</summary>
public enum VekDataGridGroupSortOrder
{
    /// <summary>Orders group values from lowest to highest.</summary>
    Ascending,
    /// <summary>Orders group values from highest to lowest.</summary>
    Descending
}

/// <summary>Describes one level in the grid's nested grouping order.</summary>
/// <param name="ColumnKey">The stable key of the column used to group rows.</param>
/// <param name="SortOrder">The order in which distinct group values appear.</param>
public sealed record VekDataGridGroupDefinition(
    string ColumnKey,
    VekDataGridGroupSortOrder SortOrder = VekDataGridGroupSortOrder.Ascending);

/// <summary>Provides a group definition, its rows, and its expand/collapse action to a template.</summary>
/// <typeparam name="TItem">The type of item represented by the grid.</typeparam>
public sealed class VekDataGridGroupContext<TItem>
{
    internal VekDataGridGroupContext(string columnKey, string title, object? value, int level, IReadOnlyList<TItem> items, bool expanded, Func<Task> toggleAsync)
        => (ColumnKey, Title, Value, Level, Items, Count, Expanded, ToggleAsync) =
            (columnKey, title, value, level, items, items.Count, expanded, toggleAsync);

    /// <summary>Gets the stable key of the grouping column.</summary>
    public string ColumnKey { get; }
    /// <summary>Gets the title of the grouping column.</summary>
    public string Title { get; }
    /// <summary>Gets the distinct value represented by this group.</summary>
    public object? Value { get; }
    /// <summary>Gets the zero-based nesting level of this group.</summary>
    public int Level { get; }
    /// <summary>Gets the items contained by this group.</summary>
    public IReadOnlyList<TItem> Items { get; }
    /// <summary>Gets the number of items contained by this group.</summary>
    public int Count { get; }
    /// <summary>Gets whether the group is currently expanded.</summary>
    public bool Expanded { get; }
    /// <summary>Toggles whether the group's children are visible.</summary>
    public Func<Task> ToggleAsync { get; }
}

/// <summary>Provides a draft item, validation context, and lifecycle actions to an edit template.</summary>
/// <typeparam name="TItem">The type of item represented by the grid.</typeparam>
public sealed class VekDataGridEditContext<TItem>
{
    private readonly Action<TItem> _updateItem;
    private readonly Func<Task<bool>> _commit;
    private readonly Func<Task> _cancel;

    internal VekDataGridEditContext(TItem item, EditContext editContext, Action<TItem> updateItem, Func<Task<bool>> commit, Func<Task> cancel)
        => (Item, EditContext, _updateItem, _commit, _cancel) = (item, editContext, updateItem, commit, cancel);

    /// <summary>Gets the current editable draft item.</summary>
    public TItem Item { get; }
    /// <summary>Gets the validation context associated with the draft.</summary>
    public EditContext EditContext { get; }
    /// <summary>Replaces the current draft item.</summary>
    public void UpdateItem(TItem item) => _updateItem(item);
    /// <summary>Validates and attempts to commit the draft.</summary>
    public Func<Task<bool>> CommitAsync => _commit;
    /// <summary>Discards the draft and exits edit mode.</summary>
    public Func<Task> CancelAsync => _cancel;
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

/// <summary>Provides paging state, counts, and actions to custom pager content.</summary>
/// <typeparam name="TItem">The type of item represented by the grid.</typeparam>
public sealed class VekDataGridPagerContext<TItem>
{
    internal VekDataGridPagerContext(
        VekDataGridState<TItem> state,
        int totalItems,
        int totalPages,
        Func<int, Task> setPageAsync,
        Func<int, Task> setPageSizeAsync)
        => (State, TotalItems, TotalPages, SetPageAsync, SetPageSizeAsync) =
            (state, totalItems, totalPages, setPageAsync, setPageSizeAsync);

    /// <summary>Gets the current page, page size, sort, and filter state.</summary>
    public VekDataGridState<TItem> State { get; }
    /// <summary>Gets the total number of items after filtering and before paging.</summary>
    public int TotalItems { get; }
    /// <summary>Gets the total number of available pages.</summary>
    public int TotalPages { get; }
    /// <summary>Moves to the requested one-based page.</summary>
    public Func<int, Task> SetPageAsync { get; }
    /// <summary>Changes the number of rows displayed per page.</summary>
    public Func<int, Task> SetPageSizeAsync { get; }
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
