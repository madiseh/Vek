namespace Vek.Components.Components.DataGrid;

/// <summary>Renders accessible checkboxes for selecting rows in a data grid.</summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
public partial class SelectionColumn<TItem> : IDisposable, IVekDataGridColumn<TItem, RenderFragment>
{
    [CascadingParameter]
    private VekDataGrid<TItem>? Grid { get; set; }

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

    /// <summary>Registers the selection column with its parent grid.</summary>
    protected override void OnParametersSet()
    {
        if (Grid is null) throw new InvalidOperationException("SelectionColumn must be declared inside a VekDataGrid.");
        if (_registeredGrid == Grid) return;
        _registeredGrid?.UnregisterColumn(this);
        _registeredGrid = Grid;
        _registeredGrid.RegisterColumn(this);
    }

    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Key => "__selection";
    bool IVekDataGridColumn<TItem, RenderFragment>.Visible { get => Visible; set => Visible = value; }
    /// <inheritdoc />
    public async Task SetVisibleAsync(bool visible)
    {
        Visible = visible;
        await VisibleChanged.InvokeAsync(visible);
    }
    int? IVekDataGridColumn<TItem, RenderFragment>.Width { get => Width; set => Width = value; }
    int IVekDataGridColumn<TItem, RenderFragment>.MinWidth => MinWidth;
    int IVekDataGridColumn<TItem, RenderFragment>.MaxWidth => MaxWidth;
    /// <inheritdoc />
    public async Task SetWidthAsync(int width)
    {
        Width = width;
        await WidthChanged.InvokeAsync(width);
    }
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Sortable => false;
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Filterable => false;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterBy => null;
    VekDataGridFilterOperator Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterOperator => VekDataGridFilterOperator.Contains;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridFilterContext, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.FilterTemplate => null;
    bool IVekDataGridColumn<TItem, RenderFragment>.IsSelectionColumn => true;
    bool IVekDataGridColumn<TItem, RenderFragment>.Editable => false;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridEditTemplateContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.EditTemplate => null;
    /// <inheritdoc />
    public string GetRowLabel(TItem item) => "Select";
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Groupable => false;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.GroupBy => null;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridGroupContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.GroupTemplate => null;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.AggregateBy => null;
    bool IVekDataGridColumn<TItem, RenderFragment>.IsHierarchyColumn => false;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridRowDetailContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.DetailTemplate => null;
    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Title => "Select";
    Comparison<TItem> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.CompareItems => static (_, _) => 0;
    Func<TItem, object?> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.ValueAccessor => static _ => null;
    VekDataGridTemplate<TItem, RenderFragment> IVekDataGridColumn<TItem, RenderFragment>.CellTemplate => item => builder =>
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "type", "checkbox");
        builder.AddAttribute(2, "aria-label", $"Select {item}");
        builder.AddAttribute(3, "checked", Grid!.IsItemSelected(item));
        builder.AddAttribute(4, "disabled", !Grid!.CanSelectItem(item));
        builder.AddAttribute(5, "onclick:stopPropagation", true);
        builder.AddAttribute(6, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this,
            args => Grid!.ToggleSelectionAsync(item, args.Value is true)));
        builder.CloseElement();
    };

    /// <inheritdoc />
    public void Dispose() => _registeredGrid?.UnregisterColumn(this);
}
