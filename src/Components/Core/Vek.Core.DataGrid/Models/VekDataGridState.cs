namespace Vek.Core.DataGrid;

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

