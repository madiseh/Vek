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

    /// <summary>
    /// Gets or sets whether this column participates in filtering. Defaults to false; set to true to show its filter
    /// control when the grid's <see cref="VekDataGrid{TItem}.FilterMode"/> enables a filter surface.
    /// </summary>
    [Parameter]
    public bool Filterable { get; set; }

    /// <summary>
    /// Gets or sets the value selector used to compare this column during filtering. When unset, the value from
    /// <see cref="Property"/> is used. This selector is only used when <see cref="Filterable"/> is true.
    /// </summary>
    [Parameter]
    public Func<TItem, object?>? FilterBy { get; set; }

    /// <summary>
    /// Gets or sets the default comparison used by this column's filter control. Defaults to
    /// <see cref="VekDataGridFilterOperator.Contains"/>.
    /// </summary>
    [Parameter]
    public VekDataGridFilterOperator FilterOperator { get; set; } = VekDataGridFilterOperator.Contains;

    /// <summary>
    /// Gets or sets custom content for this column's filter control. The template receives filter actions for this
    /// column and is rendered only when <see cref="Filterable"/> is true.
    /// </summary>
    [Parameter]
    public RenderFragment<VekDataGridFilterContext>? FilterTemplate { get; set; }

    /// <summary>Gets or sets whether this column can enter edit mode.</summary>
    [Parameter]
    public bool Editable { get; set; }

    /// <summary>Gets or sets a custom editor rendered for this column.</summary>
    [Parameter]
    public RenderFragment<VekDataGridEditContext<TItem>>? EditTemplate { get; set; }

    /// <summary>Gets or sets whether this column can group the grid rows.</summary>
    [Parameter]
    public bool Groupable { get; set; } = true;

    /// <summary>Gets or sets an optional value selector used to create groups.</summary>
    [Parameter]
    public Func<TItem, object?>? GroupBy { get; set; }

    /// <summary>Gets or sets a custom group header template.</summary>
    [Parameter]
    public RenderFragment<VekDataGridGroupContext<TItem>>? GroupTemplate { get; set; }

    /// <summary>Gets or sets an optional value selector used by aggregate definitions.</summary>
    [Parameter]
    public Func<TItem, object?>? AggregateBy { get; set; }

    private Func<TItem, TValue>? _propertyGetter;
    private VekDataGrid<TItem>? _registeredGrid;
    private string? _lastPropertyExpression;
    private string? _lastKey;
    private string? _lastTitle;
    private string? _lastFormat;
    private bool _lastSortable;
    private bool _lastFilterable;
    private VekDataGridFilterOperator _lastFilterOperator;
    private bool _lastEditable;

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
            || FilterOperator != _lastFilterOperator
            || Editable != _lastEditable;

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
        _lastEditable = Editable;
    }

    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Title => string.IsNullOrWhiteSpace(Title)
        ? GetDefaultTitle()
        : Title;

    string Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Key => string.IsNullOrWhiteSpace(Key)
        ? _lastPropertyExpression ?? Property.ToString()
        : Key;

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

    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Filterable => Filterable;

    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterBy => !Filterable
        ? null
        : FilterBy ?? (item => _propertyGetter!(item));

    VekDataGridFilterOperator Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.FilterOperator => FilterOperator;

    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridFilterContext, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.FilterTemplate =>
        FilterTemplate is null ? null : context => FilterTemplate(new VekDataGridFilterContext(context.Value, context.Apply, context.Clear));

    bool IVekDataGridColumn<TItem, RenderFragment>.IsSelectionColumn => false;

    bool IVekDataGridColumn<TItem, RenderFragment>.Editable => Editable;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridEditTemplateContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.EditTemplate =>
        EditTemplate is null ? null : context => EditTemplate(Grid!.CreateBlazorEditContext(context));
    /// <inheritdoc />
    public string GetRowLabel(TItem item) => Convert.ToString(_propertyGetter!(item)) ?? string.Empty;
    bool Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.Groupable => Groupable;
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.GroupBy => Groupable ? GroupBy ?? (item => _propertyGetter!(item)) : null;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridGroupContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.GroupTemplate =>
        GroupTemplate is null ? null : context => GroupTemplate(new VekDataGridGroupContext<TItem>(
            context.ColumnKey, context.Title, context.Value, context.Level, context.Items, context.Expanded,
            context.ToggleAsync, context.Aggregates));
    Func<TItem, object?>? Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.AggregateBy => AggregateBy ?? (item => _propertyGetter!(item));
    Func<TItem, object?> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.ValueAccessor => item => SortBy is null ? _propertyGetter!(item) : SortBy(item);
    bool IVekDataGridColumn<TItem, RenderFragment>.IsHierarchyColumn => false;
    VekDataGridTemplate<Vek.Core.DataGrid.VekDataGridRowDetailContext<TItem>, RenderFragment>? IVekDataGridColumn<TItem, RenderFragment>.DetailTemplate => null;

    VekDataGridTemplate<TItem, RenderFragment> IVekDataGridColumn<TItem, RenderFragment>.CellTemplate => item => builder =>
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
            content = formattable.ToString(Format, Grid?.Culture ?? CultureInfo.CurrentCulture);
        }

        builder.AddContent(0, content);
    };

    Comparison<TItem> Vek.Core.DataGrid.IVekDataGridColumn<TItem, RenderFragment>.CompareItems => CompareItems;

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
