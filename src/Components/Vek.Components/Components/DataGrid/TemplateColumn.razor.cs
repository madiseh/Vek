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
    public string? Key { get; set; }

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
    public bool Sortable { get; set; }

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

    string IVekDataGridColumn<TItem>.Key => Key ?? Title;

    bool IVekDataGridColumn<TItem>.Sortable => Sortable;
    bool IVekDataGridColumn<TItem>.Filterable => false;
    Func<TItem, object?>? IVekDataGridColumn<TItem>.FilterBy => null;
    VekDataGridFilterOperator IVekDataGridColumn<TItem>.FilterOperator => VekDataGridFilterOperator.Contains;
    RenderFragment<VekDataGridFilterContext>? IVekDataGridColumn<TItem>.FilterTemplate => null;
    bool IVekDataGridColumn<TItem>.IsSelectionColumn => false;
    bool IVekDataGridColumn<TItem>.Editable => false;
    RenderFragment<VekDataGridEditContext<TItem>>? IVekDataGridColumn<TItem>.EditTemplate => null;
    string IVekDataGridColumn<TItem>.GetRowLabel(TItem item) => Title;

    string IVekDataGridColumn<TItem>.Title => Title;

    RenderFragment<TItem> IVekDataGridColumn<TItem>.CellTemplate => CellTemplate;

    Comparison<TItem> IVekDataGridColumn<TItem>.CompareItems => static (_, _) => 0;

    /// <summary>
    /// Removes this column from its grid when the component is disposed.
    /// </summary>
    public void Dispose() => _registeredGrid?.UnregisterColumn(this);
}
