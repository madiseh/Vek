namespace Vek.Components.Components.DataGrid;

/// <summary>Adds row controls and detail templates to a data grid.</summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
public partial class HierarchyColumn<TItem> : IDisposable, IVekDataGridColumn<TItem, RenderFragment>
{
    [CascadingParameter]
    private VekDataGrid<TItem>? Grid { get; set; }

    /// <summary>Gets or sets the content displayed when a row is expanded.</summary>
    [Parameter, EditorRequired]
    public RenderFragment<VekDataGridRowDetailContext<TItem>> DetailTemplate { get; set; } = default!;

    private VekDataGrid<TItem>? _registeredGrid;

    /// <summary>Gets or sets whether this column is included in the rendered table.</summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>Gets or sets the callback invoked when this column's visibility changes.</summary>
    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; }

    /// <summary>Gets or sets the initial or current width of the column in pixels.</summary>
    [Parameter]
    public int? Width { get; set; }

    /// <summary>Gets or sets the minimum resizable width in pixels.</summary>
    [Parameter]
    public int MinWidth { get; set; } = 80;

    /// <summary>Gets or sets the maximum resizable width in pixels.</summary>
    [Parameter]
    public int MaxWidth { get; set; } = 600;

    /// <summary>Gets or sets the callback invoked when this column's width changes.</summary>
    [Parameter]
    public EventCallback<int?> WidthChanged { get; set; }

    /// <summary>Registers the hierarchy column with its parent grid.</summary>
    protected override void OnParametersSet()
    {
        if (Grid is null) throw new InvalidOperationException("HierarchyColumn must be declared inside a VekDataGrid.");
        if (_registeredGrid == Grid) return;
        _registeredGrid?.UnregisterColumn(this);
        _registeredGrid = Grid;
        _registeredGrid.RegisterColumn(this);
    }

    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Key => "__hierarchy";
    bool IVekDataGridColumn<TItem, RenderFragment>.Visible { get => Visible; set => Visible = value; }
    async Task IVekDataGridColumn<TItem, RenderFragment>.SetVisibleAsync(bool visible)
    {
        Visible = visible;
        await VisibleChanged.InvokeAsync(visible);
    }
    int? IVekDataGridColumn<TItem, RenderFragment>.Width { get => Width; set => Width = value; }
    int IVekDataGridColumn<TItem, RenderFragment>.MinWidth => MinWidth;
    int IVekDataGridColumn<TItem, RenderFragment>.MaxWidth => MaxWidth;
    async Task IVekDataGridColumn<TItem, RenderFragment>.SetWidthAsync(int width)
    {
        Width = width;
        await WidthChanged.InvokeAsync(width);
    }
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Sortable => false;
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Filterable => false;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterBy => null;
    VekDataGridFilterOperator Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterOperator => VekDataGridFilterOperator.Contains;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridFilterContext, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.FilterTemplate => null;
    bool IVekDataGridColumn<TItem, RenderFragment>.IsSelectionColumn => false;
    bool IVekDataGridColumn<TItem, RenderFragment>.Editable => false;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridEditTemplateContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.EditTemplate => null;
    string IVekDataGridColumn<TItem, RenderFragment>.GetRowLabel(TItem item) => "Details";
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Groupable => false;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.GroupBy => null;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridGroupContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.GroupTemplate => null;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.AggregateBy => null;
    bool IVekDataGridColumn<TItem, RenderFragment>.IsHierarchyColumn => true;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridRowDetailContext<TItem>, RenderFragment> IVekDataGridColumn<TItem, RenderFragment>.DetailTemplate =>
        context => DetailTemplate(new VekDataGridRowDetailContext<TItem>(context.Item, context.Expanded, context.ToggleAsync));
    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Title => "Details";
    Comparison<TItem> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.CompareItems => static (_, _) => 0;
    Func<TItem, object?> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.ValueAccessor => static _ => null;
    VekDataGridTemplate<TItem, RenderFragment> IVekDataGridColumn<TItem, RenderFragment>.CellTemplate => item => builder =>
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
