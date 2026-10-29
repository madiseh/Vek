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

    /// <summary>
    /// Gets or sets whether users can sort by this column.
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets a custom value selector used when sorting this column.
    /// </summary>
    [Parameter]
    public Func<TItem, object?>? SortBy { get; set; }

    /// <summary>
    /// Gets or sets a custom comparer used when sorting this column.
    /// </summary>
    [Parameter]
    public IComparer<object?>? SortComparer { get; set; }

    /// <summary>Gets or sets whether the column can be filtered.</summary>
    [Parameter]
    public bool Filterable { get; set; } = true;

    /// <summary>Gets or sets an optional value selector for filtering.</summary>
    [Parameter]
    public Func<TItem, object?>? FilterBy { get; set; }

    /// <summary>Gets or sets the default comparison used by the column filter.</summary>
    [Parameter]
    public VekDataGridFilterOperator FilterOperator { get; set; } = VekDataGridFilterOperator.Contains;

    /// <summary>Gets or sets a custom filter control template.</summary>
    [Parameter]
    public RenderFragment<VekDataGridFilterContext>? FilterTemplate { get; set; }

    private Func<TItem, TValue>? _propertyGetter;
    private VekDataGrid<TItem>? _registeredGrid;
    private string? _lastPropertyExpression;
    private string? _lastKey;
    private string? _lastTitle;
    private string? _lastFormat;
    private bool _lastSortable;
    private bool _lastFilterable;
    private VekDataGridFilterOperator _lastFilterOperator;

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
            || Sortable != _lastSortable
            || Filterable != _lastFilterable
            || FilterOperator != _lastFilterOperator;

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
        _lastSortable = Sortable;
        _lastFilterable = Filterable;
        _lastFilterOperator = FilterOperator;
    }

    string IVekDataGridColumn<TItem>.Title => string.IsNullOrWhiteSpace(Title)
        ? GetDefaultTitle()
        : Title;

    string IVekDataGridColumn<TItem>.Key => string.IsNullOrWhiteSpace(Key)
        ? _lastPropertyExpression!
        : Key;

    bool IVekDataGridColumn<TItem>.Sortable => Sortable;

    bool IVekDataGridColumn<TItem>.Filterable => Filterable;

    Func<TItem, object?>? IVekDataGridColumn<TItem>.FilterBy => !Filterable
        ? null
        : FilterBy ?? (item => _propertyGetter!(item));

    VekDataGridFilterOperator IVekDataGridColumn<TItem>.FilterOperator => FilterOperator;

    RenderFragment<VekDataGridFilterContext>? IVekDataGridColumn<TItem>.FilterTemplate => FilterTemplate;

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

    private int CompareItems(TItem left, TItem right)
    {
        var leftValue = SortBy is null ? _propertyGetter!(left) : SortBy(left);
        var rightValue = SortBy is null ? _propertyGetter!(right) : SortBy(right);
        return (SortComparer ?? Comparer<object?>.Default).Compare(leftValue, rightValue);
    }

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
