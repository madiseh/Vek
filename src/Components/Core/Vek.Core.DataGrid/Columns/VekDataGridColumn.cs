namespace Vek.Core.DataGrid;

/// <summary>Defines platform-neutral column behavior and its platform-native templates.</summary>
/// <typeparam name="TItem">The row item type.</typeparam>
/// <typeparam name="TTemplate">The native template type supplied by the UI adapter.</typeparam>
public class VekDataGridColumn<TItem, TTemplate> : IVekDataGridColumn<TItem, TTemplate>
{
    /// <summary>Initializes a new DataGrid column definition.</summary>
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
    /// <param name="cellTemplate">The typed cell template.</param>
    /// <param name="filterTemplate">The typed filter template.</param>
    /// <param name="groupTemplate">The typed group template.</param>
    /// <param name="editTemplate">The typed edit template.</param>
    /// <param name="detailTemplate">The typed row detail template.</param>
    /// <param name="editable">Whether the column can enter edit mode.</param>
    /// <param name="isSelectionColumn">Whether the column represents row selection.</param>
    /// <param name="isHierarchyColumn">Whether the column displays row details.</param>
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
        VekDataGridTemplate<TItem, TTemplate>? cellTemplate = null,
        VekDataGridTemplate<VekDataGridFilterContext, TTemplate>? filterTemplate = null,
        VekDataGridTemplate<VekDataGridGroupContext<TItem>, TTemplate>? groupTemplate = null,
        VekDataGridTemplate<VekDataGridEditTemplateContext<TItem>, TTemplate>? editTemplate = null,
        VekDataGridTemplate<VekDataGridRowDetailContext<TItem>, TTemplate>? detailTemplate = null,
        bool editable = false,
        bool isSelectionColumn = false,
        bool isHierarchyColumn = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(valueAccessor);

        Key = key;
        ValueAccessor = valueAccessor;
        Title = title ?? key;
        Sortable = sortable;
        Filterable = filterable;
        Groupable = groupable;
        CompareItems = comparer ?? ((left, right) => Comparer<object?>.Default.Compare(ValueAccessor(left), ValueAccessor(right)));
        FilterBy = filterBy ?? ValueAccessor;
        GroupBy = groupBy ?? ValueAccessor;
        AggregateBy = aggregateBy ?? ValueAccessor;
        FilterOperator = filterOperator;
        CellTemplate = cellTemplate;
        FilterTemplate = filterTemplate;
        GroupTemplate = groupTemplate;
        EditTemplate = editTemplate;
        DetailTemplate = detailTemplate;
        Editable = editable;
        IsSelectionColumn = isSelectionColumn;
        IsHierarchyColumn = isHierarchyColumn;
    }

    /// <summary>Gets the stable key identifying the column.</summary>
    public string Key { get; }

    /// <summary>Gets the display title.</summary>
    public string Title { get; }

    /// <summary>Gets or sets whether the column is visible.</summary>
    public bool Visible { get; set; } = true;

    /// <summary>Gets or sets the preferred column width.</summary>
    public int? Width { get; set; }

    /// <summary>Gets the minimum permitted column width.</summary>
    public int MinWidth { get; set; } = 80;

    /// <summary>Gets the maximum permitted column width.</summary>
    public int MaxWidth { get; set; } = 600;

    /// <summary>Changes column visibility.</summary>
    /// <param name="visible">The requested visibility.</param>
    public virtual Task SetVisibleAsync(bool visible)
    {
        Visible = visible;
        return Task.CompletedTask;
    }

    /// <summary>Changes column width.</summary>
    /// <param name="width">The requested width.</param>
    public virtual Task SetWidthAsync(int width)
    {
        Width = Math.Clamp(width, MinWidth, MaxWidth);
        return Task.CompletedTask;
    }

    /// <summary>Gets whether the column supports sorting.</summary>
    public bool Sortable { get; }

    /// <summary>Gets whether the column supports filtering.</summary>
    public bool Filterable { get; }

    /// <summary>Gets whether the column supports grouping.</summary>
    public bool Groupable { get; }

    /// <summary>Gets the value accessor delegate.</summary>
    public Func<TItem, object?> ValueAccessor { get; }

    /// <summary>Gets the value selector used by filters.</summary>
    public Func<TItem, object?>? FilterBy { get; }

    /// <summary>Gets the value selector used by grouping.</summary>
    public Func<TItem, object?>? GroupBy { get; }

    /// <summary>Gets the value selector used by aggregation.</summary>
    public Func<TItem, object?>? AggregateBy { get; }

    /// <summary>Gets the comparison used to order items by this column.</summary>
    public Comparison<TItem> CompareItems { get; }

    /// <summary>Gets the default filter operator.</summary>
    public VekDataGridFilterOperator FilterOperator { get; }

    /// <summary>Gets the typed cell template.</summary>
    public VekDataGridTemplate<TItem, TTemplate>? CellTemplate { get; }

    /// <summary>Gets the typed filter template.</summary>
    public VekDataGridTemplate<VekDataGridFilterContext, TTemplate>? FilterTemplate { get; }

    /// <summary>Gets the typed group template.</summary>
    public VekDataGridTemplate<VekDataGridGroupContext<TItem>, TTemplate>? GroupTemplate { get; }

    /// <summary>Gets the typed edit template.</summary>
    public VekDataGridTemplate<VekDataGridEditTemplateContext<TItem>, TTemplate>? EditTemplate { get; }

    /// <summary>Gets the typed row detail template.</summary>
    public VekDataGridTemplate<VekDataGridRowDetailContext<TItem>, TTemplate>? DetailTemplate { get; }

    /// <summary>Gets whether the column can enter edit mode.</summary>
    public bool Editable { get; }

    /// <summary>Gets whether the column represents row selection.</summary>
    public bool IsSelectionColumn { get; }

    /// <summary>Gets whether the column displays hierarchical row details.</summary>
    public bool IsHierarchyColumn { get; }

    /// <summary>Gets the label used to identify an item's row.</summary>
    /// <param name="item">The row item.</param>
    public virtual string GetRowLabel(TItem item) => Convert.ToString(ValueAccessor(item), CultureInfo.CurrentCulture) ?? string.Empty;
}

