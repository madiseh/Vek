namespace Vek.Components.Components.DataGrid;

/// <summary>Adds row controls and detail templates to a data grid.</summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
public partial class HierarchyColumn<TItem> : IDisposable, IVekDataGridColumn<TItem>
{
    [CascadingParameter]
    private VekDataGrid<TItem>? Grid { get; set; }

    /// <summary>Gets or sets the content displayed when a row is expanded.</summary>
    [Parameter, EditorRequired]
    public RenderFragment<VekDataGridRowDetailContext<TItem>> DetailTemplate { get; set; } = default!;

    private VekDataGrid<TItem>? _registeredGrid;

    /// <summary>Registers the hierarchy column with its parent grid.</summary>
    protected override void OnParametersSet()
    {
        if (Grid is null) throw new InvalidOperationException("HierarchyColumn must be declared inside a VekDataGrid.");
        if (_registeredGrid == Grid) return;
        _registeredGrid?.UnregisterColumn(this);
        _registeredGrid = Grid;
        _registeredGrid.RegisterColumn(this);
    }

    string IVekDataGridColumn<TItem>.Key => "__hierarchy";
    bool IVekDataGridColumn<TItem>.Sortable => false;
    bool IVekDataGridColumn<TItem>.Filterable => false;
    Func<TItem, object?>? IVekDataGridColumn<TItem>.FilterBy => null;
    VekDataGridFilterOperator IVekDataGridColumn<TItem>.FilterOperator => VekDataGridFilterOperator.Contains;
    RenderFragment<VekDataGridFilterContext>? IVekDataGridColumn<TItem>.FilterTemplate => null;
    bool IVekDataGridColumn<TItem>.IsSelectionColumn => false;
    bool IVekDataGridColumn<TItem>.Editable => false;
    RenderFragment<VekDataGridEditContext<TItem>>? IVekDataGridColumn<TItem>.EditTemplate => null;
    string IVekDataGridColumn<TItem>.GetRowLabel(TItem item) => "Details";
    bool IVekDataGridColumn<TItem>.Groupable => false;
    Func<TItem, object?>? IVekDataGridColumn<TItem>.GroupBy => null;
    RenderFragment<VekDataGridGroupContext<TItem>>? IVekDataGridColumn<TItem>.GroupTemplate => null;
    Func<TItem, object?>? IVekDataGridColumn<TItem>.AggregateBy => null;
    bool IVekDataGridColumn<TItem>.IsHierarchyColumn => true;
    RenderFragment<VekDataGridRowDetailContext<TItem>>? IVekDataGridColumn<TItem>.DetailTemplate => DetailTemplate;
    string IVekDataGridColumn<TItem>.Title => "Details";
    Comparison<TItem> IVekDataGridColumn<TItem>.CompareItems => static (_, _) => 0;
    RenderFragment<TItem> IVekDataGridColumn<TItem>.CellTemplate => item => builder =>
    {
        var expanded = Grid!.IsRowExpanded(item);
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "class", "row-detail-toggle");
        builder.AddAttribute(3, "aria-expanded", expanded ? "true" : "false");
        builder.AddAttribute(4, "aria-label", $"{(expanded ? "Collapse" : "Expand")} details for {Grid.GetRowLabel(item)}");
        builder.AddAttribute(5, "onclick:stopPropagation", true);
        builder.AddAttribute(6, "onclick", EventCallback.Factory.Create(this, () => Grid.ToggleRowDetailsAsync(item)));
        builder.AddContent(7, expanded ? "−" : "+");
        builder.CloseElement();
    };

    /// <inheritdoc />
    public void Dispose() => _registeredGrid?.UnregisterColumn(this);
}
