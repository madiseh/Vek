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

    private VekDataGrid<TItem>? _registeredGrid;
    private string? _lastKey;
    private string? _lastTitle;
    private RenderFragment<TItem>? _lastCellTemplate;

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
        else if (Key != _lastKey || Title != _lastTitle || CellTemplate != _lastCellTemplate)
        {
            _registeredGrid.RefreshColumn(this);
        }

        _lastKey = Key;
        _lastTitle = Title;
        _lastCellTemplate = CellTemplate;
    }

    string IVekDataGridColumn<TItem>.Key => Key ?? Title;

    string IVekDataGridColumn<TItem>.Title => Title;

    RenderFragment<TItem> IVekDataGridColumn<TItem>.CellTemplate => CellTemplate;

    Comparison<TItem> IVekDataGridColumn<TItem>.CompareItems => static (_, _) => 0;

    /// <summary>
    /// Removes this column from its grid when the component is disposed.
    /// </summary>
    public void Dispose() => _registeredGrid?.UnregisterColumn(this);
}
