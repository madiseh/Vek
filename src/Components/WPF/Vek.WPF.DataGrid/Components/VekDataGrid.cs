namespace Vek.WPF.DataGrid.Components;

/// <summary>
/// The base WPF data grid component.
/// </summary>
public partial class VekDataGrid : WpfDataGrid, IVekDataGridHost<object, WpfDataTemplate>
{
    private IEnumerable? _sourceItems;
    private readonly VekDataGridEngine<object, WpfDataTemplate> _engine = new();
    private readonly Dictionary<string, object> _columnDefinitions = new(StringComparer.OrdinalIgnoreCase);
    private Func<object, string>? _itemKeySelector;

    /// <summary>Gets or sets whether sorting uses one column or an ordered set of columns.</summary>
    public VekDataGridSortMode SortMode { get; set; } = VekDataGridSortMode.Single;

    /// <summary>Gets or sets the one-based page displayed by this grid.</summary>
    public int CurrentPage
    {
        get => _engine.Page;
        set
        {
            _engine.SetPage(value, _engine.PageSize);
            RefreshLocalItems();
        }
    }

    /// <summary>Gets or sets the number of rows displayed on each page.</summary>
    public int PageSize
    {
        get => _engine.PageSize;
        set
        {
            _engine.SetPage(_engine.Page, value);
            RefreshLocalItems();
        }
    }

    /// <summary>Gets or sets the stable item key selector used by the shared Core selection state.</summary>
    public Func<object, string>? ItemKeySelector
    {
        get => _itemKeySelector;
        set
        {
            _itemKeySelector = value;
            _engine.SetItemKeySelector(value);
            SyncCoreSelection();
        }
    }

    /// <summary>Gets or sets the ordered grouping columns shared with the Blazor adapter.</summary>
    public IReadOnlyList<VekDataGridGroupDefinition> GroupBy
    {
        get => _engine.Groups;
        set
        {
            _engine.SetGroups(value);
            RefreshLocalItems();
        }
    }

    /// <summary>Gets the shared platform-neutral state and behavior for this grid instance.</summary>
    public VekDataGridEngine<object, WpfDataTemplate> Engine
    {
        get
        {
            if (Dispatcher.CheckAccess()) PrepareHostState();
            else Dispatcher.Invoke(PrepareHostState);
            return _engine;
        }
    }

    /// <summary>Registers a column defined by the shared Core contract and creates its native WPF column.</summary>
    /// <typeparam name="TItem">The row item type declared by the shared column.</typeparam>
    /// <param name="column">The platform-neutral column definition and its WPF templates.</param>
    /// <returns>The native WPF column created for the definition.</returns>
    public WpfDataGridTemplateColumn AddColumn<TItem>(IVekDataGridColumn<TItem, WpfDataTemplate> column)
    {
        ArgumentNullException.ThrowIfNull(column);
        if (_columnDefinitions.ContainsKey(column.Key))
        {
            throw new ArgumentException($"A column with key '{column.Key}' is already registered.", nameof(column));
        }

        var adapter = new WpfColumnAdapter<TItem>(column);
        _engine.AddOrUpdateColumn(adapter);
        _columnDefinitions.Add(column.Key, column);

        var nativeColumn = new WpfDataGridTemplateColumn
        {
            Header = column.Title,
            SortMemberPath = column.Key,
            CanUserSort = column.Sortable,
            IsReadOnly = !column.Editable,
            Visibility = column.Visible ? Visibility.Visible : Visibility.Collapsed,
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
        if (column.Width is int width) nativeColumn.Width = new WpfDataGridLength(width);
        Columns.Add(nativeColumn);
        return nativeColumn;
    }

    /// <summary>Gets the original shared column definition registered under a stable key.</summary>
    /// <typeparam name="TItem">The row item type declared by the shared column.</typeparam>
    /// <param name="key">The stable column key.</param>
    /// <returns>The shared column definition, or null when the key or item type does not match.</returns>
    public IVekDataGridColumn<TItem, WpfDataTemplate>? GetColumn<TItem>(string key) =>
        _columnDefinitions.TryGetValue(key, out var definition)
        ? definition as IVekDataGridColumn<TItem, WpfDataTemplate>
        : null;

    /// <summary>Refreshes native rows after a shared filtering operation on the control's dispatcher.</summary>
    public ValueTask AfterCoreChangeAsync()
    {
        if (Dispatcher.CheckAccess())
        {
            RefreshLocalItems();
            return ValueTask.CompletedTask;
        }
        return new ValueTask(Dispatcher.InvokeAsync(RefreshLocalItems).Task);
    }

    /// <summary>Refreshes native rows and sort indicators after a shared sorting change.</summary>
    public ValueTask AfterSortChangeAsync()
    {
        if (Dispatcher.CheckAccess())
        {
            RefreshSortProjection();
            return ValueTask.CompletedTask;
        }
        return new ValueTask(Dispatcher.InvokeAsync(RefreshSortProjection).Task);
    }

    private void RefreshSortProjection()
    {
        RefreshLocalItems();
        foreach (var column in Columns)
        {
            var sort = _engine.Sorts.FirstOrDefault(candidate =>
                string.Equals(candidate.ColumnKey, column.SortMemberPath, StringComparison.OrdinalIgnoreCase));
            column.SortDirection = sort is null ? null
                : sort.Direction == VekSortDirection.Descending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
        }
    }

    /// <summary>Synchronizes the native WPF sort gesture with the shared DataGrid engine.</summary>
    protected override async void OnSorting(WpfDataGridSortingEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);
        e.Handled = true;

        var columnKey = string.IsNullOrWhiteSpace(e.Column.SortMemberPath)
            ? Convert.ToString(e.Column.Header, CultureInfo.InvariantCulture)
            : e.Column.SortMemberPath;
        if (string.IsNullOrWhiteSpace(columnKey)) return;

        _sourceItems ??= ItemsSource;
        PropertyInfo? GetProperty(object item) => item.GetType().GetProperty(columnKey, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (!_engine.Columns.Any(column => string.Equals(column.Key, columnKey, StringComparison.OrdinalIgnoreCase)))
        {
            _engine.AddOrUpdateColumn(new VekDataGridColumn<object, WpfDataTemplate>(
                columnKey,
                item => GetProperty(item)?.GetValue(item),
                title: Convert.ToString(e.Column.Header, CultureInfo.InvariantCulture) ?? columnKey,
                comparer: (left, right) => CompareByProperty(left, right, GetProperty(left))));
        }

        await this.SortAsync(
            columnKey,
            SortMode,
            preserveExistingSorts: SortMode == VekDataGridSortMode.Multiple);
        var activeSort = _engine.Sorts.FirstOrDefault(sort =>
            string.Equals(sort.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase));
        e.Column.SortDirection = activeSort?.Direction == VekSortDirection.Descending
            ? ListSortDirection.Descending
            : activeSort is null ? null : ListSortDirection.Ascending;
    }

    /// <summary>Copies native WPF selection state into the shared selected-key state.</summary>
    protected override void OnSelectionChanged(WpfSelectionChangedEventArgs e)
    {
        if (_projecting) { base.OnSelectionChanged(e); return; }
        if (_suppressRowActivationSelection) { ApplyNativeSelection(); return; }
        var previousItems = _engine.Interaction.SelectedItems;
        var previousItem = _engine.Interaction.SelectedItem;
        var rejected = e.AddedItems.Cast<object>().Where(item => !this.CanSelectItem(item)).ToArray();
        if (rejected.Length > 0)
        {
            var restored = previousItems.Concat(SelectedItems.Cast<object>().Except(rejected))
                .Where(item => this.CanSelectItem(item)).Distinct(_engine.Interaction.SelectionComparer).ToArray();
            _engine.Interaction.SelectedItems = restored;
            _engine.Interaction.SelectedItem = previousItem is not null && restored.Contains(previousItem)
                ? previousItem : restored.FirstOrDefault();
            ApplyNativeSelection();
            SyncCoreSelection();
            return;
        }
        base.OnSelectionChanged(e);
        SyncCoreSelection();
    }

    private static int CompareByProperty(object left, object right, PropertyInfo? property)
    {
        var leftValue = property?.GetValue(left);
        var rightValue = property?.GetValue(right);
        return Comparer<object?>.Default.Compare(leftValue, rightValue);
    }

    private void RefreshLocalItems()
    {
        _sourceItems ??= ItemsSource;
        if (_sourceItems is IEnumerable source)
        {
            var items = _engine.GetLocalData(source.Cast<object>()).Items;
            var view = CollectionViewSource.GetDefaultView(items) as CollectionView;
            if (view is not null)
            {
                view.GroupDescriptions.Clear();
                foreach (var group in _engine.Groups)
                {
                    var column = _engine.Columns.FirstOrDefault(candidate => candidate.Key == group.ColumnKey);
                    if (column is null) continue;

                    var groupDescription = new WpfGroupDescription(column.GroupBy ?? column.ValueAccessor)
                    {
                        CustomSort = new WpfGroupValueComparer(group.SortOrder)
                    };
                    view.GroupDescriptions.Add(groupDescription);
                }
            }

            _projecting = true;
            try { ItemsSource = (IEnumerable?)view ?? items; }
            finally { _projecting = false; }
            ApplyNativeSelection();
        }
    }

    private void SyncCoreSelection()
    {
        _engine.SetItemKeySelector(_itemKeySelector);
        PrepareHostState();
        _engine.SetSelectedKeys(_engine.Interaction.SelectedItems.Select(_engine.GetItemKey));
    }

    private sealed class WpfGroupDescription(Func<object, object?> groupValueSelector) : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture) =>
            groupValueSelector(item) ?? string.Empty;
    }

    private sealed class WpfGroupValueComparer(VekDataGridGroupSortOrder sortOrder) : IComparer
    {
        public int Compare(object? x, object? y)
        {
            var comparison = Comparer.DefaultInvariant.Compare(
                x is CollectionViewGroup left ? left.Name : x,
                y is CollectionViewGroup right ? right.Name : y);
            return sortOrder == VekDataGridGroupSortOrder.Descending ? -comparison : comparison;
        }
    }

    private VekDataGridEditTemplateContext<TItem> CreateEditTemplateContext<TItem>(TItem item)
    {
        var draft = _engine.IsEditing && ReferenceEquals(_engine.OriginalEditItem, item)
            && _engine.DraftEditItem is TItem current ? current : item;
        return new VekDataGridEditTemplateContext<TItem>(
            draft,
            CultureInfo.CurrentCulture,
            updatedItem => this.UpdateEditDraft(updatedItem!),
            () => this.CommitEditAsync(),
            () => this.CancelEditAsync());
    }

    private sealed class WpfTemplateSelector(Func<object?, WpfDataTemplate?> selectTemplate)
        : WpfDataTemplateSelector
    {
        public override WpfDataTemplate? SelectTemplate(object item, WpfDependencyObject container) =>
            selectTemplate(item) ?? base.SelectTemplate(item, container);
    }

    private sealed class WpfColumnAdapter<TItem>(IVekDataGridColumn<TItem, WpfDataTemplate> column)
        : IVekDataGridColumn<object, WpfDataTemplate>
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
        public VekDataGridTemplate<VekDataGridFilterContext, WpfDataTemplate>? FilterTemplate => column.FilterTemplate;
        public bool IsSelectionColumn => column.IsSelectionColumn;
        public bool Editable => column.Editable;
        public VekDataGridTemplate<VekDataGridEditTemplateContext<object>, WpfDataTemplate>? EditTemplate =>
            column.EditTemplate is null ? null : context => column.EditTemplate(new VekDataGridEditTemplateContext<TItem>(
                (TItem)context.Item,
                context.Culture,
                item => context.UpdateItem(item!),
                context.CommitAsync,
                context.CancelAsync));
        public string GetRowLabel(object item) => column.GetRowLabel((TItem)item);
        public VekDataGridTemplate<VekDataGridGroupContext<object>, WpfDataTemplate>? GroupTemplate =>
            column.GroupTemplate is null ? null : context => column.GroupTemplate(new VekDataGridGroupContext<TItem>(
                context.ColumnKey, context.Title, context.Value, context.Level, context.Items.Cast<TItem>().ToArray(),
                context.Expanded, context.ToggleAsync, context.Aggregates));
        public bool IsHierarchyColumn => column.IsHierarchyColumn;
        public VekDataGridTemplate<VekDataGridRowDetailContext<object>, WpfDataTemplate>? DetailTemplate =>
            column.DetailTemplate is null ? null : context => column.DetailTemplate(new VekDataGridRowDetailContext<TItem>(
                (TItem)context.Item, context.Expanded, context.ToggleAsync));
        public VekDataGridTemplate<object, WpfDataTemplate>? CellTemplate =>
            column.CellTemplate is null ? null : item => column.CellTemplate((TItem)item);
        public Comparison<object> CompareItems => (left, right) => column.CompareItems((TItem)left, (TItem)right);
    }
}
