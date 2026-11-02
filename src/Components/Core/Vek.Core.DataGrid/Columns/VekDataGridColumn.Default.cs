namespace Vek.Core.DataGrid;

/// <summary>Provides a template-free compatibility column using an object template type.</summary>
/// <typeparam name="TItem">The row item type.</typeparam>
public sealed class VekDataGridColumn<TItem> : VekDataGridColumn<TItem, object>
{
    /// <summary>Initializes a template-free column definition.</summary>
    /// <param name="key">The stable key identifying the column.</param>
    /// <param name="valueAccessor">Function to get the cell value from an item.</param>
    /// <param name="title">Optional display title.</param>
    /// <param name="sortable">Whether sorting is supported.</param>
    /// <param name="filterable">Whether filtering is supported.</param>
    /// <param name="groupable">Whether grouping is supported.</param>
    /// <param name="comparer">Optional comparison used for sorting.</param>
    /// <param name="filterBy">Optional selector used for filtering.</param>
    /// <param name="groupBy">Optional selector used for grouping.</param>
    /// <param name="aggregateBy">Optional selector used for aggregation.</param>
    /// <param name="filterOperator">The default filter operator.</param>
    /// <param name="searchable">Whether global search includes the column by default.</param>
    /// <param name="searchBy">Optional selector used for global search.</param>
    public VekDataGridColumn(
        string key,
        Func<TItem, object?> valueAccessor,
        string? title = null,
        bool sortable = true,
        bool filterable = true,
        bool groupable = true,
        Comparison<TItem>? comparer = null,
        Func<TItem, object?>? filterBy = null,
        Func<TItem, object?>? groupBy = null,
        Func<TItem, object?>? aggregateBy = null,
        VekDataGridFilterOperator filterOperator = VekDataGridFilterOperator.Contains,
        bool searchable = false,
        Func<TItem, object?>? searchBy = null)
        : base(key, valueAccessor, title, sortable, filterable, groupable, comparer, filterBy, groupBy, aggregateBy, filterOperator,
            searchable: searchable, searchBy: searchBy)
    {
    }
}
