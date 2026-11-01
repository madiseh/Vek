namespace Vek.Core.DataGrid;

/// <summary>
/// Describes one ordered sort operation in a grid state.
/// </summary>
/// <param name="ColumnKey">The stable key of the sorted column.</param>
/// <param name="Direction">The direction applied to the column.</param>
/// <param name="Priority">The order in which this sort is applied.</param>
public sealed record VekDataGridSort(string ColumnKey, VekSortDirection Direction, int Priority = 0);

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
    bool CaseSensitive = false);

/// <summary>
/// Describes one level in the grid's nested grouping order.
/// </summary>
/// <param name="ColumnKey">The stable key of the column used to group rows.</param>
/// <param name="SortOrder">The order in which distinct group values appear.</param>
public sealed record VekDataGridGroupDefinition(
    string ColumnKey,
    VekDataGridGroupSortOrder SortOrder = VekDataGridGroupSortOrder.Ascending);

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

/// <summary>
/// Represents a platform-neutral server load request.
/// </summary>
/// <param name="Page">The one-based requested page index.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="Sorts">The active sorts in priority order.</param>
/// <param name="Filters">The active filters.</param>
public sealed record VekDataGridServerRequest(
    int Page,
    int PageSize,
    IReadOnlyList<VekDataGridSort> Sorts,
    IReadOnlyList<VekDataGridFilter> Filters);

/// <summary>
/// Represents a platform-neutral server load result.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
/// <param name="Items">Items for the requested page.</param>
/// <param name="TotalItems">Total matching items across all pages.</param>
public sealed record VekDataGridServerResult<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalItems);

/// <summary>
/// A serializable snapshot of the grid's persistent state.
/// </summary>
public sealed record VekDataGridSnapshot(
    int Page,
    int PageSize,
    IReadOnlyList<VekDataGridSort> Sorts,
    IReadOnlyList<VekDataGridFilter> Filters,
    IReadOnlyList<VekDataGridGroupDefinition> Groups,
    IReadOnlyList<string> ExpandedGroupKeys,
    IReadOnlyList<string> SelectedKeys);
