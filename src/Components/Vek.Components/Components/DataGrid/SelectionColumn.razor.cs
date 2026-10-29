namespace Vek.Components.Components.DataGrid;

/// <summary>Renders accessible checkboxes for selecting rows in a data grid.</summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
public partial class SelectionColumn<TItem> : IDisposable, IVekDataGridColumn<TItem>
{
    [CascadingParameter]
    private VekDataGrid<TItem>? Grid { get; set; }

    private VekDataGrid<TItem>? _registeredGrid;

    /// <summary>Registers the selection column with its parent grid.</summary>
    protected override void OnParametersSet()
    {
        if (Grid is null) throw new InvalidOperationException("SelectionColumn must be declared inside a VekDataGrid.");
        if (_registeredGrid == Grid) return;
        _registeredGrid?.UnregisterColumn(this);
        _registeredGrid = Grid;
        _registeredGrid.RegisterColumn(this);
    }

    string IVekDataGridColumn<TItem>.Key => "__selection";
    bool IVekDataGridColumn<TItem>.Sortable => false;
    bool IVekDataGridColumn<TItem>.Filterable => false;
    Func<TItem, object?>? IVekDataGridColumn<TItem>.FilterBy => null;
    VekDataGridFilterOperator IVekDataGridColumn<TItem>.FilterOperator => VekDataGridFilterOperator.Contains;
    RenderFragment<VekDataGridFilterContext>? IVekDataGridColumn<TItem>.FilterTemplate => null;
    bool IVekDataGridColumn<TItem>.IsSelectionColumn => true;
    bool IVekDataGridColumn<TItem>.Editable => false;
    RenderFragment<VekDataGridEditContext<TItem>>? IVekDataGridColumn<TItem>.EditTemplate => null;
    string IVekDataGridColumn<TItem>.GetRowLabel(TItem item) => "Select";
    bool IVekDataGridColumn<TItem>.Groupable => false;
    Func<TItem, object?>? IVekDataGridColumn<TItem>.GroupBy => null;
    RenderFragment<VekDataGridGroupContext<TItem>>? IVekDataGridColumn<TItem>.GroupTemplate => null;
    Func<TItem, object?>? IVekDataGridColumn<TItem>.AggregateBy => null;
    bool IVekDataGridColumn<TItem>.IsHierarchyColumn => false;
    RenderFragment<VekDataGridRowDetailContext<TItem>>? IVekDataGridColumn<TItem>.DetailTemplate => null;
    string IVekDataGridColumn<TItem>.Title => "Select";
    Comparison<TItem> IVekDataGridColumn<TItem>.CompareItems => static (_, _) => 0;
    RenderFragment<TItem> IVekDataGridColumn<TItem>.CellTemplate => item => builder =>
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "type", "checkbox");
        builder.AddAttribute(2, "aria-label", $"Select {item}");
        builder.AddAttribute(3, "checked", Grid!.IsItemSelected(item));
        builder.AddAttribute(4, "disabled", !Grid.CanSelectItem(item));
        builder.AddAttribute(5, "onclick:stopPropagation", true);
        builder.AddAttribute(6, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this,
            args => Grid.ToggleSelectionAsync(item, args.Value is true)));
        builder.CloseElement();
    };

    /// <inheritdoc />
    public void Dispose() => _registeredGrid?.UnregisterColumn(this);
}
