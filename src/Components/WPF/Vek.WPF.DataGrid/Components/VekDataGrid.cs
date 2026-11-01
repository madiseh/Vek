using System.Collections;
using System.Reflection;
using Vek.Core.DataGrid;

namespace Vek.WPF.DataGrid.Components;

/// <summary>
/// The base WPF data grid component.
/// </summary>
public class VekDataGrid : System.Windows.Controls.DataGrid
{
    private IEnumerable? _sourceItems;
    private readonly VekDataGridEngine<object, System.Windows.DataTemplate> _engine = new();
    private readonly Dictionary<string, object> _columnDefinitions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the shared platform-neutral state and behavior for this grid instance.</summary>
    public VekDataGridEngine<object, System.Windows.DataTemplate> Engine => _engine;

    /// <summary>Registers a column defined by the shared Core contract and creates its native WPF column.</summary>
    /// <typeparam name="TItem">The row item type declared by the shared column.</typeparam>
    /// <param name="column">The platform-neutral column definition and its WPF templates.</param>
    /// <returns>The native WPF column created for the definition.</returns>
    public System.Windows.Controls.DataGridTemplateColumn AddColumn<TItem>(IVekDataGridColumn<TItem, System.Windows.DataTemplate> column)
    {
        ArgumentNullException.ThrowIfNull(column);
        if (_columnDefinitions.ContainsKey(column.Key))
        {
            throw new ArgumentException($"A column with key '{column.Key}' is already registered.", nameof(column));
        }

        var adapter = new WpfColumnAdapter<TItem>(column);
        _engine.AddOrUpdateColumn(adapter);
        _columnDefinitions.Add(column.Key, column);

        var nativeColumn = new System.Windows.Controls.DataGridTemplateColumn
        {
            Header = column.Title,
            SortMemberPath = column.Key,
            CanUserSort = column.Sortable,
            IsReadOnly = !column.Editable,
            Visibility = column.Visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
            MinWidth = column.MinWidth,
            MaxWidth = column.MaxWidth,
            CellTemplateSelector = new WpfTemplateSelector(item => item is TItem typedItem
                ? column.CellTemplate?.Invoke(typedItem)
                : null),
            CellEditingTemplateSelector = column.EditTemplate is null
                ? null
                : new WpfTemplateSelector(item => item is TItem typedItem
                    ? column.EditTemplate(CreateEditTemplateContext(typedItem))
                    : null)
        };
        if (column.Width is int width) nativeColumn.Width = new System.Windows.Controls.DataGridLength(width);
        Columns.Add(nativeColumn);
        return nativeColumn;
    }

    /// <summary>Gets the original shared column definition registered under a stable key.</summary>
    /// <typeparam name="TItem">The row item type declared by the shared column.</typeparam>
    /// <param name="key">The stable column key.</param>
    /// <returns>The shared column definition, or null when the key or item type does not match.</returns>
    public IVekDataGridColumn<TItem, System.Windows.DataTemplate>? GetColumn<TItem>(string key) =>
        _columnDefinitions.TryGetValue(key, out var definition)
        ? definition as IVekDataGridColumn<TItem, System.Windows.DataTemplate>
        : null;

    /// <summary>Synchronizes the native WPF sort gesture with the shared DataGrid engine.</summary>
    protected override void OnSorting(System.Windows.Controls.DataGridSortingEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        e.Handled = true;

        var columnKey = string.IsNullOrWhiteSpace(e.Column.SortMemberPath)
            ? Convert.ToString(e.Column.Header, System.Globalization.CultureInfo.InvariantCulture)
            : e.Column.SortMemberPath;
        if (string.IsNullOrWhiteSpace(columnKey)) return;

        _sourceItems ??= ItemsSource;
        PropertyInfo? GetProperty(object item) => item.GetType().GetProperty(columnKey, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (!_engine.Columns.Any(column => string.Equals(column.Key, columnKey, StringComparison.OrdinalIgnoreCase)))
        {
            _engine.AddOrUpdateColumn(new VekDataGridColumn<object, System.Windows.DataTemplate>(
                columnKey,
                item => GetProperty(item)?.GetValue(item),
                title: Convert.ToString(e.Column.Header, System.Globalization.CultureInfo.InvariantCulture) ?? columnKey,
                comparer: (left, right) => CompareByProperty(left, right, GetProperty(left))));
        }

        _engine.ApplySortInteraction(columnKey, VekDataGridSortMode.Single);
        var activeSort = _engine.Sorts.FirstOrDefault();
        e.Column.SortDirection = activeSort?.Direction == VekSortDirection.Descending
            ? System.ComponentModel.ListSortDirection.Descending
            : activeSort is null ? null : System.ComponentModel.ListSortDirection.Ascending;

        if (_sourceItems is IEnumerable source)
        {
            ItemsSource = _engine.GetLocalData(source.Cast<object>()).Items;
        }
    }

    /// <summary>Copies native WPF selection state into the shared selected-key state.</summary>
    protected override void OnSelectionChanged(System.Windows.Controls.SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        _engine.SetSelectedKeys(SelectedItems
            .Cast<object>()
            .Select(item => item.GetHashCode().ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    private static int CompareByProperty(object left, object right, PropertyInfo? property)
    {
        var leftValue = property?.GetValue(left);
        var rightValue = property?.GetValue(right);
        return Comparer<object?>.Default.Compare(leftValue, rightValue);
    }

    private VekDataGridEditTemplateContext<TItem> CreateEditTemplateContext<TItem>(TItem item)
    {
        _engine.BeginEdit(item!);
        return new VekDataGridEditTemplateContext<TItem>(
            item,
            System.Globalization.CultureInfo.CurrentCulture,
            updatedItem => _engine.UpdateDraft(updatedItem!),
            () => _engine.CommitEditAsync().AsTask(),
            () =>
            {
                _engine.CancelEdit();
                return Task.CompletedTask;
            });
    }

    private sealed class WpfTemplateSelector(Func<object?, System.Windows.DataTemplate?> selectTemplate)
        : System.Windows.Controls.DataTemplateSelector
    {
        public override System.Windows.DataTemplate? SelectTemplate(object item, System.Windows.DependencyObject container) =>
            selectTemplate(item) ?? base.SelectTemplate(item, container);
    }

    private sealed class WpfColumnAdapter<TItem>(IVekDataGridColumn<TItem, System.Windows.DataTemplate> column)
        : IVekDataGridColumn<object, System.Windows.DataTemplate>
    {
        public string Key => column.Key;
        public string Title => column.Title;
        public bool Visible { get => column.Visible; set => column.Visible = value; }
        public Task SetVisibleAsync(bool visible) => column.SetVisibleAsync(visible);
        public int? Width { get => column.Width; set => column.Width = value; }
        public int MinWidth => column.MinWidth;
        public int MaxWidth => column.MaxWidth;
        public Task SetWidthAsync(int width) => column.SetWidthAsync(width);
        public bool Sortable => column.Sortable;
        public bool Filterable => column.Filterable;
        public bool Groupable => column.Groupable;
        public Func<object, object?> ValueAccessor => item => column.ValueAccessor((TItem)item);
        public Func<object, object?>? FilterBy => column.FilterBy is null ? null : item => column.FilterBy((TItem)item);
        public Func<object, object?>? GroupBy => column.GroupBy is null ? null : item => column.GroupBy((TItem)item);
        public Func<object, object?>? AggregateBy => column.AggregateBy is null ? null : item => column.AggregateBy((TItem)item);
        public VekDataGridFilterOperator FilterOperator => column.FilterOperator;
        public VekDataGridTemplate<VekDataGridFilterContext, System.Windows.DataTemplate>? FilterTemplate => column.FilterTemplate;
        public bool IsSelectionColumn => column.IsSelectionColumn;
        public bool Editable => column.Editable;
        public VekDataGridTemplate<VekDataGridEditTemplateContext<object>, System.Windows.DataTemplate>? EditTemplate =>
            column.EditTemplate is null ? null : context => column.EditTemplate(new VekDataGridEditTemplateContext<TItem>(
                (TItem)context.Item,
                context.Culture,
                item => context.UpdateItem(item!),
                context.CommitAsync,
                context.CancelAsync));
        public string GetRowLabel(object item) => column.GetRowLabel((TItem)item);
        public VekDataGridTemplate<VekDataGridGroupContext<object>, System.Windows.DataTemplate>? GroupTemplate =>
            column.GroupTemplate is null ? null : context => column.GroupTemplate(new VekDataGridGroupContext<TItem>(
                context.ColumnKey, context.Title, context.Value, context.Level, context.Items.Cast<TItem>().ToArray(),
                context.Expanded, context.ToggleAsync, context.Aggregates));
        public bool IsHierarchyColumn => column.IsHierarchyColumn;
        public VekDataGridTemplate<VekDataGridRowDetailContext<object>, System.Windows.DataTemplate>? DetailTemplate =>
            column.DetailTemplate is null ? null : context => column.DetailTemplate(new VekDataGridRowDetailContext<TItem>(
                (TItem)context.Item, context.Expanded, context.ToggleAsync));
        public VekDataGridTemplate<object, System.Windows.DataTemplate>? CellTemplate =>
            column.CellTemplate is null ? null : item => column.CellTemplate((TItem)item);
        public Comparison<object> CompareItems => (left, right) => column.CompareItems((TItem)left, (TItem)right);
    }
}
