namespace Vek.Core.DataGrid;

/// <summary>Defines the shared DataGrid column contract for every UI adapter.</summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
/// <typeparam name="TTemplate">The native template type supplied by the UI adapter.</typeparam>
public interface IVekDataGridColumn<TItem, TTemplate>
{
    /// <summary>Gets the stable key identifying the column.</summary>
    string Key { get; }

    /// <summary>Gets the display title.</summary>
    string Title { get; }

    /// <summary>Gets or sets whether the column is visible.</summary>
    bool Visible { get; set; }

    /// <summary>Changes column visibility and completes any adapter notification.</summary>
    /// <param name="visible">The requested visibility.</param>
    Task SetVisibleAsync(bool visible);

    /// <summary>Gets or sets the preferred column width.</summary>
    int? Width { get; set; }

    /// <summary>Gets the minimum permitted column width.</summary>
    int MinWidth { get; }

    /// <summary>Gets the maximum permitted column width.</summary>
    int MaxWidth { get; }

    /// <summary>Changes column width and completes any adapter notification.</summary>
    /// <param name="width">The requested width.</param>
    Task SetWidthAsync(int width);

    /// <summary>Gets whether the column supports sorting.</summary>
    bool Sortable { get; }

    /// <summary>Gets whether the column supports filtering.</summary>
    bool Filterable { get; }

    /// <summary>Gets whether the column supports grouping.</summary>
    bool Groupable { get; }

    /// <summary>Gets the cell value for an item.</summary>
    Func<TItem, object?> ValueAccessor { get; }

    /// <summary>Gets the value selector used for filtering.</summary>
    Func<TItem, object?>? FilterBy { get; }

    /// <summary>Gets the value selector used for grouping.</summary>
    Func<TItem, object?>? GroupBy { get; }

    /// <summary>Gets the value selector used for aggregation.</summary>
    Func<TItem, object?>? AggregateBy { get; }

    /// <summary>Gets the default filter operator for this column.</summary>
    VekDataGridFilterOperator FilterOperator { get; }

    /// <summary>Gets the strongly typed filter template for this column.</summary>
    VekDataGridTemplate<VekDataGridFilterContext, TTemplate>? FilterTemplate { get; }

    /// <summary>Gets whether the column represents row selection.</summary>
    bool IsSelectionColumn { get; }

    /// <summary>Gets whether the column can enter edit mode.</summary>
    bool Editable { get; }

    /// <summary>Gets the strongly typed edit template for this column.</summary>
    VekDataGridTemplate<VekDataGridEditTemplateContext<TItem>, TTemplate>? EditTemplate { get; }

    /// <summary>Gets the label used to identify an item's row.</summary>
    /// <param name="item">The row item.</param>
    string GetRowLabel(TItem item);

    /// <summary>Gets the strongly typed group template for this column.</summary>
    VekDataGridTemplate<VekDataGridGroupContext<TItem>, TTemplate>? GroupTemplate { get; }

    /// <summary>Gets whether the column displays hierarchical row details.</summary>
    bool IsHierarchyColumn { get; }

    /// <summary>Gets the strongly typed row detail template for this column.</summary>
    VekDataGridTemplate<VekDataGridRowDetailContext<TItem>, TTemplate>? DetailTemplate { get; }

    /// <summary>Gets the strongly typed cell template for this column.</summary>
    VekDataGridTemplate<TItem, TTemplate>? CellTemplate { get; }

    /// <summary>Compares two items by this column's value.</summary>
    Comparison<TItem> CompareItems { get; }
}
