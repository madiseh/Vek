using Microsoft.AspNetCore.Components;

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

    private Func<TItem, TValue>? _propertyGetter;
    private VekDataGrid<TItem>? _registeredGrid;
    private string? _lastPropertyExpression;
    private string? _lastTitle;

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
            || Title != _lastTitle;

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
        _lastTitle = Title;
    }

    string IVekDataGridColumn<TItem>.Title => string.IsNullOrWhiteSpace(Title)
        ? GetDefaultTitle()
        : Title;

    RenderFragment<TItem> IVekDataGridColumn<TItem>.CellTemplate => item => builder =>
        builder.AddContent(0, _propertyGetter!(item));

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
