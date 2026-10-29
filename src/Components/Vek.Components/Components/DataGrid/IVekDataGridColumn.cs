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
    bool Visible { get; set; }
    Task SetVisibleAsync(bool visible);
    int? Width { get; set; }
    int MinWidth { get; }
    int MaxWidth { get; }
    Task SetWidthAsync(int width);

    /// <summary>
    /// Gets whether this column can be sorted.
    /// </summary>
    bool Sortable { get; }
    bool Filterable { get; }
    Func<TItem, object?>? FilterBy { get; }
    VekDataGridFilterOperator FilterOperator { get; }
    RenderFragment<VekDataGridFilterContext>? FilterTemplate { get; }
    bool IsSelectionColumn { get; }
    bool Editable { get; }
    RenderFragment<VekDataGridEditContext<TItem>>? EditTemplate { get; }
    string GetRowLabel(TItem item);
    bool Groupable { get; }
    Func<TItem, object?>? GroupBy { get; }
    RenderFragment<VekDataGridGroupContext<TItem>>? GroupTemplate { get; }
    Func<TItem, object?>? AggregateBy { get; }
    bool IsHierarchyColumn { get; }
    RenderFragment<VekDataGridRowDetailContext<TItem>>? DetailTemplate { get; }

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
