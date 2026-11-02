namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Defines a grid column whose cells are rendered with a custom template.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
public partial class TemplateColumn<TItem>
{
    [CascadingParameter]
    private VekDataGrid<TItem>? Grid { get; set; }

    /// <summary>
    /// Gets or sets the stable key used to identify this column.
    /// </summary>
    [Parameter]
    public string? Key { get; set; } = null;

    /// <summary>Gets or sets whether this column is included in the rendered table.</summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>Gets or sets the callback invoked when this column's visibility changes.</summary>
    [Parameter]
    public EventCallback<bool> VisibleChanged { get; set; } = default;

    /// <summary>Gets or sets the initial or current width of the column in pixels.</summary>
    [Parameter]
    public int? Width { get; set; } = null;

    /// <summary>Gets or sets the minimum resizable width in pixels.</summary>
    [Parameter]
    public int MinWidth { get; set; } = 80;

    /// <summary>Gets or sets the maximum resizable width in pixels.</summary>
    [Parameter]
    public int MaxWidth { get; set; } = 600;

    /// <summary>Gets or sets the callback invoked when this column's width changes.</summary>
    [Parameter]
    public EventCallback<int?> WidthChanged { get; set; } = default;

    /// <summary>
    /// Gets or sets the column heading.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template used to render each cell.
    /// </summary>
    [Parameter, EditorRequired]
    public RenderFragment<TItem> CellTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets whether users can sort by this column.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = false;

    private VekDataGrid<TItem>? _registeredGrid;
    private string? _lastKey;
    private string? _lastTitle;
    private bool _lastSortable;

    /// <summary>
    /// Registers the column when its parameters are set.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (Grid is null)
        {
            throw new InvalidOperationException("TemplateColumn must be declared inside a VekDataGrid.");
        }

        if (_registeredGrid != Grid)
        {
            _registeredGrid?.UnregisterColumn(this);
            _registeredGrid = Grid;
            _registeredGrid.RegisterColumn(this);
        }
        else if (Key != _lastKey || Title != _lastTitle || Sortable != _lastSortable)
        {
            _registeredGrid.RefreshColumn(this);
        }

        _lastKey = Key;
        _lastTitle = Title;
        _lastSortable = Sortable;
    }

    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Key => Key ?? Title;

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

    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Sortable => Sortable;
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Filterable => false;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterBy => null;
    VekDataGridFilterOperator Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterOperator => VekDataGridFilterOperator.Contains;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridFilterContext, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.FilterTemplate => null;
    bool IVekDataGridColumn<TItem, RenderFragment>.IsSelectionColumn => false;
    bool IVekDataGridColumn<TItem, RenderFragment>.Editable => false;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridEditTemplateContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.EditTemplate => null;
    /// <inheritdoc />
    public string GetRowLabel(TItem item) => Title;
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Groupable => false;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.GroupBy => null;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridGroupContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.GroupTemplate => null;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.AggregateBy => null;
    bool IVekDataGridColumn<TItem, RenderFragment>.IsHierarchyColumn => false;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridRowDetailContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.DetailTemplate => null;

    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Title => Title;

    VekDataGridTemplate<TItem, RenderFragment> IVekDataGridColumn<TItem, RenderFragment>.CellTemplate => item => CellTemplate(item);

    Comparison<TItem> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.CompareItems => static (_, _) => 0;
    Func<TItem, object?> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.ValueAccessor => static _ => null;

    /// <summary>
    /// Removes this column from its grid when the component is disposed.
    /// </summary>
    public void Dispose() => _registeredGrid?.UnregisterColumn(this);
}
