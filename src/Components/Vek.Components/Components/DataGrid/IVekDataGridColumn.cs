namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Describes the rendering and comparison behavior required by a grid column.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
internal interface IVekDataGridColumn<TItem>
{
    /// <summary>
    /// Gets the stable identifier for this column.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets whether this column can be sorted.
    /// </summary>
    bool Sortable { get; }
    bool Filterable { get; }
    Func<TItem, object?>? FilterBy { get; }
    VekDataGridFilterOperator FilterOperator { get; }
    RenderFragment<VekDataGridFilterContext>? FilterTemplate { get; }

    /// <summary>
    /// Gets the heading displayed for the column.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the fragment used to render a cell for an item.
    /// </summary>
    RenderFragment<TItem> CellTemplate { get; }

    /// <summary>
    /// Gets the comparison used to sort two row items by this column.
    /// </summary>
    Comparison<TItem> CompareItems { get; }
}
