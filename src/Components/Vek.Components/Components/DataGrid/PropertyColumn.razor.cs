namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Defines a sortable grid column bound to a property of the row item.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
/// <typeparam name="TValue">The type of value selected from each row.</typeparam>
public partial class PropertyColumn<TItem, TValue>
{
    [CascadingParameter]
    private VekDataGrid<TItem>? Grid { get; set; }

    /// <summary>
    /// Gets or sets the property expression used to display and sort this column's values.
    /// </summary>
    [Parameter, EditorRequired]
    public Expression<Func<TItem, TValue>> Property { get; set; } = default!;

    /// <summary>
    /// Gets or sets the column heading. When omitted, the selected property's name is used.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the stable key used to identify this column.
    /// </summary>
    [Parameter]
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the format string used when displaying formattable values.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the optional template used to render this column's cells.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? CellTemplate { get; set; }

    private Func<TItem, TValue>? _propertyGetter;
    private VekDataGrid<TItem>? _registeredGrid;
    private string? _lastPropertyExpression;
    private string? _lastKey;
    private string? _lastTitle;
    private string? _lastFormat;
    private RenderFragment<TItem>? _lastCellTemplate;

    /// <summary>
    /// Updates the compiled property accessor and registers this column with its grid.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (Grid is null)
        {
            throw new InvalidOperationException("PropertyColumn must be declared inside a VekDataGrid.");
        }

        var propertyExpression = Property.ToString();
        var definitionChanged = _propertyGetter is null
            || propertyExpression != _lastPropertyExpression
            || Key != _lastKey
            || Title != _lastTitle
            || Format != _lastFormat
            || CellTemplate != _lastCellTemplate;

        if (_propertyGetter is null || propertyExpression != _lastPropertyExpression)
        {
            _propertyGetter = Property.Compile();
        }

        if (_registeredGrid != Grid)
        {
            _registeredGrid?.UnregisterColumn(this);
            _registeredGrid = Grid;
            _registeredGrid.RegisterColumn(this);
            definitionChanged = false;
        }
        else if (definitionChanged)
        {
            _registeredGrid.RefreshColumn(this);
        }

        _lastPropertyExpression = propertyExpression;
        _lastKey = Key;
        _lastTitle = Title;
        _lastFormat = Format;
        _lastCellTemplate = CellTemplate;
    }

    string IVekDataGridColumn<TItem>.Title => string.IsNullOrWhiteSpace(Title)
        ? GetDefaultTitle()
        : Title;

    string IVekDataGridColumn<TItem>.Key => string.IsNullOrWhiteSpace(Key)
        ? _lastPropertyExpression!
        : Key;

    RenderFragment<TItem> IVekDataGridColumn<TItem>.CellTemplate => item => builder =>
    {
        if (CellTemplate is not null)
        {
            builder.AddContent(0, CellTemplate(item));
            return;
        }

        var value = _propertyGetter!(item);
        object? content = value;
        if (Format is not null && value is IFormattable formattable)
        {
            content = formattable.ToString(Format, CultureInfo.CurrentCulture);
        }

        builder.AddContent(0, content);
    };

    Comparison<TItem> IVekDataGridColumn<TItem>.CompareItems => CompareItems;

    private int CompareItems(TItem left, TItem right) =>
        Comparer<TValue>.Default.Compare(_propertyGetter!(left), _propertyGetter(right));

    private string GetDefaultTitle()
    {
        Expression expression = Property.Body;
        while (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
        {
            expression = unaryExpression.Operand;
        }

        if (expression is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new InvalidOperationException(
            "PropertyColumn requires Property to select a property when Title is not supplied.");
    }

    /// <summary>
    /// Removes this column from its grid when the component is disposed.
    /// </summary>
    public void Dispose() => _registeredGrid?.UnregisterColumn(this);
}
