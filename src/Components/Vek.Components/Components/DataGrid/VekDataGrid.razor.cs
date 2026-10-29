namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Displays a collection of items in a table with client-side sorting and pagination.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class VekDataGrid<TItem>
{
    private readonly List<IVekDataGridColumn<TItem>> _columns = [];
    private readonly List<ActiveSort> _sorts = [];
    private readonly List<VekDataGridFilter> _filters = [];
    private readonly HashSet<string> _collapsedGroups = new(StringComparer.Ordinal);
    private int _currentPage = 1;
    private TItem? _editingOriginal;
    private TItem? _editingDraft;
    private IVekDataGridColumn<TItem>? _editingColumn;
    private EditContext? _editContext;
    private VekDataGridEditContext<TItem>? _activeEditContext;
    private bool _isEditing;

    private sealed record ActiveSort(IVekDataGridColumn<TItem> Column, VekSortDirection Direction);
    private sealed record GroupNode(
        VekDataGridGroupDefinition Definition,
        IVekDataGridColumn<TItem> Column,
        object? Value,
        IReadOnlyList<TItem> Items,
        int Level,
        string Path);

    /// <summary>
    /// Gets or sets the complete collection of items displayed by the grid.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = Array.Empty<TItem>();

    /// <summary>
    /// Gets or sets the maximum number of rows displayed on each page. Defaults to 10.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>Gets or sets the current one-based page for two-way binding.</summary>
    [Parameter]
    public int CurrentPage
    {
        get => _currentPage;
        set => _currentPage = Math.Max(value, 1);
    }

    /// <summary>Gets or sets the available choices in the rows-per-page selector.</summary>
    [Parameter]
    public IReadOnlyList<int> PageSizeOptions { get; set; } = new[] { 10, 25, 50 };

    /// <summary>Gets or sets a callback invoked when the current page changes.</summary>
    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }

    /// <summary>Gets or sets a callback invoked when the page size changes.</summary>
    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; }

    /// <summary>Gets or sets optional content used in place of the built-in pager.</summary>
    [Parameter]
    public RenderFragment<VekDataGridPagerContext<TItem>>? PagerContent { get; set; }

    /// <summary>Gets or sets the row selection mode.</summary>
    [Parameter]
    public VekDataGridSelectionMode SelectionMode { get; set; }

    /// <summary>Gets or sets the selected item in single-selection mode.</summary>
    [Parameter]
    public TItem? SelectedItem { get; set; }

    /// <summary>Gets or sets the callback invoked when the selected item changes.</summary>
    [Parameter]
    public EventCallback<TItem?> SelectedItemChanged { get; set; }

    /// <summary>Gets or sets the selected items in multiple-selection mode.</summary>
    [Parameter]
    public IReadOnlyList<TItem> SelectedItems { get; set; } = Array.Empty<TItem>();

    /// <summary>Gets or sets the callback invoked when the selected item collection changes.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>Gets or sets whether clicking a row toggles its selection.</summary>
    [Parameter]
    public bool RowClickSelection { get; set; }

    /// <summary>Gets or sets a predicate that disables selection for matching rows.</summary>
    [Parameter]
    public Func<TItem, bool>? IsRowSelectable { get; set; }

    /// <summary>Gets or sets the comparer used to identify selected rows across data refreshes.</summary>
    [Parameter]
    public IEqualityComparer<TItem>? SelectionComparer { get; set; }

    /// <summary>Gets or sets the policy used when filters hide selected rows.</summary>
    [Parameter]
    public VekDataGridFilteredSelectionMode FilteredSelectionMode { get; set; }

    /// <summary>Gets or sets the grid's editing mode.</summary>
    [Parameter]
    public VekDataGridEditMode EditMode { get; set; }

    /// <summary>Gets or sets a callback invoked after a draft is created.</summary>
    [Parameter]
    public EventCallback<TItem> EditStarted { get; set; }

    /// <summary>Gets or sets a strategy that creates an isolated draft item.</summary>
    [Parameter]
    public Func<TItem, TItem>? CloneItem { get; set; }

    /// <summary>Gets or sets a callback that validates and accepts a draft. Return false to keep editing.</summary>
    [Parameter]
    public Func<TItem, Task<bool>>? CommitEdit { get; set; }

    /// <summary>Gets or sets a callback invoked after a draft is committed.</summary>
    [Parameter]
    public EventCallback<TItem> ItemCommitted { get; set; }

    /// <summary>Gets or sets a callback invoked after an edit draft is canceled.</summary>
    [Parameter]
    public EventCallback<TItem> EditCancelled { get; set; }

    /// <summary>Gets or sets the ordered grouping columns.</summary>
    [Parameter]
    public IReadOnlyList<VekDataGridGroupDefinition> GroupBy { get; set; } = Array.Empty<VekDataGridGroupDefinition>();

    /// <summary>Gets or sets a callback invoked when a group is expanded or collapsed.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<VekDataGridGroupDefinition>> GroupChanged { get; set; }

    /// <summary>Gets or sets the calculations displayed in the grid footer and group headers.</summary>
    [Parameter]
    public IReadOnlyList<VekDataGridAggregateDefinition<TItem>> Aggregates { get; set; } = Array.Empty<VekDataGridAggregateDefinition<TItem>>();

    /// <summary>Gets or sets custom content for the aggregate footer.</summary>
    [Parameter]
    public RenderFragment<VekDataGridFooterContext<TItem>>? FooterTemplate { get; set; }

    /// <summary>
    /// Gets or sets whether sorting is disabled, limited to one column, or applied to multiple columns.
    /// </summary>
    [Parameter]
    public VekDataGridSortMode SortMode { get; set; } = VekDataGridSortMode.Single;

    /// <summary>
    /// Gets or sets whether a sorted column can return to an unsorted state when clicked again.
    /// </summary>
    [Parameter]
    public bool AllowUnsorted { get; set; }

    /// <summary>Gets or sets an optional predicate applied before column filters.</summary>
    [Parameter]
    public Func<TItem, bool>? QuickFilter { get; set; }

    /// <summary>Gets or sets where the grid displays column filter controls.</summary>
    [Parameter]
    public VekDataGridFilterMode FilterMode { get; set; } = VekDataGridFilterMode.Simple;

    /// <summary>Gets or sets whether string comparisons distinguish letter case.</summary>
    [Parameter]
    public bool FilterCaseSensitive { get; set; }

    /// <summary>Gets or sets a callback invoked after filters are applied or cleared.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<VekDataGridFilter>> FilterChanged { get; set; }

    /// <summary>
    /// Gets or sets a callback invoked after the active sort definitions change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<VekDataGridSort>> SortChanged { get; set; }

    /// <summary>
    /// Gets the current page, sort, and filter state of this grid.
    /// </summary>
    public VekDataGridState<TItem> CurrentState => new(
        _currentPage,
        EffectivePageSize,
        GetSortState(),
        _filters.ToArray());

    /// <summary>Applies or replaces the filter for a column and resets the current page.</summary>
    public async Task ApplyFilterAsync(string columnKey, VekDataGridFilterOperator op, object? value, bool? caseSensitive = null)
    {
        _filters.RemoveAll(filter => filter.ColumnKey == columnKey);
        if (op is not VekDataGridFilterOperator.IsNull and not VekDataGridFilterOperator.IsNotNull
            and not VekDataGridFilterOperator.IsEmpty and not VekDataGridFilterOperator.IsNotEmpty
            && value is null or "")
        {
            _currentPage = 1;
            await FilterChanged.InvokeAsync(_filters.ToArray());
            await NotifySelectionAfterFilteringAsync();
            await InvokeAsync(StateHasChanged);
            return;
        }

        _filters.Add(new VekDataGridFilter(columnKey, op, value, caseSensitive ?? FilterCaseSensitive));
        _currentPage = 1;
        await FilterChanged.InvokeAsync(_filters.ToArray());
        await NotifySelectionAfterFilteringAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Clears one column filter or all filters when no key is supplied.</summary>
    public async Task ClearFilterAsync(string? columnKey = null)
    {
        if (columnKey is null) _filters.Clear();
        else _filters.RemoveAll(filter => filter.ColumnKey == columnKey);
        _currentPage = 1;
        await FilterChanged.InvokeAsync(_filters.ToArray());
        await NotifySelectionAfterFilteringAsync();
        await InvokeAsync(StateHasChanged);
    }

    internal string? GetFilterValue(string key) => _filters.FirstOrDefault(filter => filter.ColumnKey == key)?.Value?.ToString();

    internal Task ApplyTextFilter(IVekDataGridColumn<TItem> column, string? value) =>
        ApplyFilterAsync(column.Key, column.FilterOperator, value);

    internal Task ClearColumnFilter(IVekDataGridColumn<TItem> column) => ClearFilterAsync(column.Key);

    /// <summary>Gets whether an item can currently be selected.</summary>
    public bool CanSelectItem(TItem item) => SelectionMode != VekDataGridSelectionMode.None
        && (IsRowSelectable?.Invoke(item) ?? true);

    /// <summary>Gets whether an item is selected using the configured comparer.</summary>
    public bool IsItemSelected(TItem item) => SelectedItems.Any(selected => ItemComparer.Equals(selected, item));

    /// <summary>Toggles selection for one row and invokes the appropriate binding callbacks.</summary>
    public async Task ToggleSelectionAsync(TItem item, bool selected)
    {
        if (!CanSelectItem(item)) return;

        if (SelectionMode == VekDataGridSelectionMode.Single)
        {
            SelectedItem = selected ? item : default;
            SelectedItems = selected ? new[] { item } : Array.Empty<TItem>();
            await SelectedItemChanged.InvokeAsync(SelectedItem);
        }
        else if (SelectionMode == VekDataGridSelectionMode.Multiple)
        {
            var selectedItems = SelectedItems.ToList();
            selectedItems.RemoveAll(existing => ItemComparer.Equals(existing, item));
            if (selected) selectedItems.Add(item);
            SelectedItems = selectedItems;
            SelectedItem = selectedItems.FirstOrDefault();
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
            await SelectedItemChanged.InvokeAsync(SelectedItem);
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Selects a row from its click interaction when row-click selection is enabled.</summary>
    public Task SelectRowAsync(TItem item) => RowClickSelection && CanSelectItem(item)
        ? ToggleSelectionAsync(item, SelectionMode == VekDataGridSelectionMode.Single || !IsItemSelected(item))
        : Task.CompletedTask;

    /// <summary>Gets whether every row on the current page is selected.</summary>
    public bool AreAllVisibleItemsSelected()
    {
        var visibleItems = GetPageItems().Where(CanSelectItem).ToList();
        return visibleItems.Count > 0 && visibleItems.All(IsItemSelected);
    }

    /// <summary>Selects or clears all selectable rows on the current page.</summary>
    public async Task ToggleAllVisibleItemsAsync(bool selected)
    {
        foreach (var item in GetPageItems().Where(CanSelectItem))
        {
            var selectedItems = SelectedItems.ToList();
            selectedItems.RemoveAll(existing => ItemComparer.Equals(existing, item));
            if (selected) selectedItems.Add(item);
            SelectedItems = selectedItems;
        }
        SelectedItem = SelectedItems.FirstOrDefault();
        await SelectedItemsChanged.InvokeAsync(SelectedItems);
        await SelectedItemChanged.InvokeAsync(SelectedItem);
        await InvokeAsync(StateHasChanged);
    }

    private IEqualityComparer<TItem> ItemComparer => SelectionComparer ?? EqualityComparer<TItem>.Default;

    /// <summary>Gets whether the row has any editable columns.</summary>
    public bool CanEditRow => EditMode != VekDataGridEditMode.None
        && _columns.Any(column => column.Editable && column.EditTemplate is not null);

    /// <summary>Gets whether the requested item and column are being edited.</summary>
    private bool IsEditing(TItem item, IVekDataGridColumn<TItem> column) => _isEditing
        && ItemComparer.Equals(_editingOriginal!, item)
        && (EditMode != VekDataGridEditMode.Cell || ReferenceEquals(_editingColumn, column));

    /// <summary>Gets the active draft template context when the requested cell is being edited.</summary>
    private VekDataGridEditContext<TItem>? GetEditContext(TItem item, IVekDataGridColumn<TItem> column) =>
        IsEditing(item, column) ? _activeEditContext : null;

    /// <summary>Starts editing a row or a single cell using an isolated draft.</summary>
    private async Task StartEditAsync(TItem item, IVekDataGridColumn<TItem>? column = null)
    {
        if (!CanEditRow || _isEditing || (EditMode == VekDataGridEditMode.Cell && (column is null || !column.Editable)))
            return;

        _editingOriginal = item;
        _editingDraft = CloneItem is null ? CloneByMemberwiseCopy(item) : CloneItem(item);
        _editingColumn = column;
        _isEditing = true;
        _editContext = new EditContext(_editingDraft!);
        _activeEditContext = new VekDataGridEditContext<TItem>(
            _editingDraft!, _editContext, UpdateEditDraft, CommitActiveEditAsync, CancelActiveEditAsync);
        await EditStarted.InvokeAsync(_editingOriginal);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Replaces the active edit draft and its validation context.</summary>
    public void UpdateEditDraft(TItem item)
    {
        if (!_isEditing) return;
        _editingDraft = item;
        _editContext = new EditContext(item!);
        _activeEditContext = new VekDataGridEditContext<TItem>(
            item, _editContext, UpdateEditDraft, CommitActiveEditAsync, CancelActiveEditAsync);
        _ = InvokeAsync(StateHasChanged);
    }

    private static TItem CloneByMemberwiseCopy(TItem item)
    {
        if (item is null) return item;
        var clone = item.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return clone is null ? item : (TItem)clone.Invoke(item, null)!;
    }

    private async Task<bool> CommitActiveEditAsync()
    {
        if (!_isEditing || _editingDraft is null || _editContext is null || !_editContext.Validate()) return false;
        if (CommitEdit is not null && !await CommitEdit(_editingDraft)) return false;

        await ItemCommitted.InvokeAsync(_editingDraft);
        ClearEditState();
        await InvokeAsync(StateHasChanged);
        return true;
    }

    private async Task CancelActiveEditAsync()
    {
        if (!_isEditing) return;
        await EditCancelled.InvokeAsync(_editingOriginal!);
        ClearEditState();
        await InvokeAsync(StateHasChanged);
    }

    private void ClearEditState()
    {
        _editingOriginal = default;
        _editingDraft = default;
        _editingColumn = null;
        _editContext = null;
        _activeEditContext = null;
        _isEditing = false;
    }

    private string GetEditLabel(TItem item) => _columns.FirstOrDefault(column => column.Editable)?.GetRowLabel(item)
        ?? Convert.ToString(item, CultureInfo.CurrentCulture)
        ?? "row";

    private List<TItem> GetPageItems() => GetSortedItems()
        .Skip((_currentPage - 1) * EffectivePageSize)
        .Take(EffectivePageSize)
        .ToList();

    private IReadOnlyList<(VekDataGridGroupDefinition Definition, IVekDataGridColumn<TItem> Column, Func<TItem, object?> Selector)> EffectiveGroups => GroupBy
        .Select(definition => (Definition: definition, Column: _columns.FirstOrDefault(column => column.Key == definition.ColumnKey)))
        .Where(pair => pair.Column?.Groupable == true && pair.Column.GroupBy is not null)
        .Select(pair => (pair.Definition, pair.Column!, pair.Column!.GroupBy!))
        .ToArray();

    private List<GroupNode> BuildGroupNodes(IEnumerable<TItem> items, int level, string parentPath)
    {
        var definitions = EffectiveGroups;
        if (level >= definitions.Count) return [];
        var (definition, column, selector) = definitions[level];
        var groups = items.GroupBy(selector)
            .OrderBy(group => group.Key, Comparer<object?>.Create(CompareGroupValues))
            .ToList();
        if (definition.SortOrder == VekDataGridGroupSortOrder.Descending) groups.Reverse();
        return groups.Select(group => new GroupNode(
            definition,
            column,
            group.Key,
            group.ToArray(),
            level,
            $"{parentPath}/{column.Key}:{group.Key?.ToString()?.Length ?? 0}:{group.Key}"))
            .ToList();
    }

    private static int CompareGroupValues(object? left, object? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left is null) return -1;
        if (right is null) return 1;
        if (left is string leftText && right is string rightText)
            return StringComparer.CurrentCulture.Compare(leftText, rightText);
        return left is IComparable comparable ? comparable.CompareTo(right) :
            StringComparer.CurrentCulture.Compare(left.ToString(), right.ToString());
    }

    private bool IsGroupExpanded(string path) => !_collapsedGroups.Contains(path);

    private async Task ToggleGroupAsync(string path)
    {
        if (!_collapsedGroups.Add(path)) _collapsedGroups.Remove(path);
        await GroupChanged.InvokeAsync(GroupBy);
        await InvokeAsync(StateHasChanged);
    }

    private VekDataGridGroupContext<TItem> CreateGroupContext(GroupNode node) => new(
        node.Column.Key,
        node.Column.Title,
        node.Value,
        node.Level,
        node.Items,
        IsGroupExpanded(node.Path),
        () => ToggleGroupAsync(node.Path),
        ComputeAggregates(node.Items, VekDataGridAggregateScope.Group));

    private VekDataGridFooterContext<TItem> CreateFooterContext()
    {
        var items = GetSortedItems();
        return new VekDataGridFooterContext<TItem>(items, ComputeAggregates(items, VekDataGridAggregateScope.Footer));
    }

    private IReadOnlyList<VekDataGridAggregateResult> ComputeAggregates(
        IReadOnlyList<TItem> items,
        VekDataGridAggregateScope scope) => Aggregates
        .Where(definition => definition.Scope == VekDataGridAggregateScope.Both || definition.Scope == scope)
        .Select(definition => ComputeAggregate(definition, items))
        .ToArray();

    private VekDataGridAggregateResult ComputeAggregate(VekDataGridAggregateDefinition<TItem> definition, IReadOnlyList<TItem> items)
    {
        var column = _columns.FirstOrDefault(candidate => candidate.Key == definition.ColumnKey);
        var selector = definition.Selector ?? column?.AggregateBy;
        if (selector is null)
            return new VekDataGridAggregateResult(definition.Key, definition.Key, null, string.Empty);

        var values = items.Select(selector).ToArray();
        var nonNullValues = values.Where(value => value is not null).ToArray();
        object? result = definition.Type switch
        {
            VekDataGridAggregateType.Count => nonNullValues.Length,
            VekDataGridAggregateType.Custom => definition.CustomAggregate?.Invoke(values),
            VekDataGridAggregateType.Sum => NumericValues(nonNullValues).Sum(),
            VekDataGridAggregateType.Average => NumericValues(nonNullValues) is { Length: > 0 } numbers ? numbers.Average() : null,
            VekDataGridAggregateType.Min => nonNullValues.OrderBy(value => value, Comparer<object?>.Create(CompareGroupValues)).FirstOrDefault(),
            VekDataGridAggregateType.Max => nonNullValues.OrderBy(value => value, Comparer<object?>.Create(CompareGroupValues)).LastOrDefault(),
            _ => null
        };

        var formatted = result is IFormattable formattable && definition.Format is not null
            ? formattable.ToString(definition.Format, CultureInfo.CurrentCulture) ?? string.Empty
            : Convert.ToString(result, CultureInfo.CurrentCulture) ?? string.Empty;
        return new VekDataGridAggregateResult(definition.Key, definition.Key, result, formatted);
    }

    private static decimal[] NumericValues(IEnumerable<object?> values)
    {
        try
        {
            return values.Select(value => Convert.ToDecimal(value, CultureInfo.CurrentCulture)).ToArray();
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            return [];
        }
    }

    private RenderFragment RenderAggregate(VekDataGridAggregateResult result, IReadOnlyList<TItem> items) => builder =>
    {
        var definition = Aggregates.FirstOrDefault(aggregate => aggregate.Key == result.Key);
        if (definition?.Template is not null)
        {
            builder.AddContent(0, definition.Template(new VekDataGridAggregateContext<TItem>(
                definition, items, result.Value, result.FormattedValue)));
            return;
        }
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "aggregate-value");
        builder.AddContent(2, $"{result.Title}: {result.FormattedValue}");
        builder.CloseElement();
    };

    private RenderFragment RenderGroupRows(IReadOnlyList<GroupNode> groups) => builder =>
    {
        var sequence = 0;
        foreach (var group in groups)
        {
            var context = CreateGroupContext(group);
            builder.OpenElement(sequence++, "tr");
            builder.AddAttribute(sequence++, "class", "group-header");
            builder.AddAttribute(sequence++, "data-group-level", group.Level);
            builder.AddAttribute(sequence++, "data-group-column", group.Column.Key);
            builder.OpenElement(sequence++, "td");
            builder.AddAttribute(sequence++, "colspan", Math.Max(_columns.Count, 1));
            builder.OpenElement(sequence++, "button");
            builder.AddAttribute(sequence++, "type", "button");
            builder.AddAttribute(sequence++, "aria-expanded", context.Expanded ? "true" : "false");
            builder.AddAttribute(sequence++, "aria-label", $"Toggle {group.Column.Title} group {group.Value}");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, context.ToggleAsync));
            builder.AddContent(sequence++, context.Expanded ? "−" : "+");
            builder.CloseElement();
            if (group.Column.GroupTemplate is not null)
                builder.AddContent(sequence++, group.Column.GroupTemplate(context));
            else
                builder.AddContent(sequence++, $"{group.Column.Title}: {group.Value ?? "(empty)"} ({group.Items.Count}) ");
            foreach (var aggregate in context.Aggregates)
            {
                builder.AddContent(sequence++, RenderAggregate(aggregate, group.Items));
            }
            builder.CloseElement();
            builder.CloseElement();

            if (context.Expanded)
            {
                var nested = BuildGroupNodes(group.Items, group.Level + 1, group.Path);
                builder.AddContent(sequence++, nested.Count > 0 ? RenderGroupRows(nested) : RenderDataRows(group.Items));
            }
        }
    };

    private RenderFragment RenderDataRows(IEnumerable<TItem> items) => builder =>
    {
        var sequence = 0;
        foreach (var item in items)
        {
            builder.OpenElement(sequence++, "tr");
            builder.AddAttribute(sequence++, "class", IsItemSelected(item) ? "is-selected" : null);
            builder.AddAttribute(sequence++, "aria-selected", IsItemSelected(item));
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => SelectRowAsync(item)));
            foreach (var column in _columns)
            {
                builder.OpenElement(sequence++, "td");
                var editContext = GetEditContext(item, column);
                if (editContext is not null && column.EditTemplate is not null)
                {
                    builder.OpenComponent<CascadingValue<EditContext>>(sequence++);
                    builder.AddAttribute(sequence++, nameof(CascadingValue<EditContext>.Value), _editContext);
                    builder.AddAttribute(sequence++, nameof(CascadingValue<EditContext>.ChildContent), column.EditTemplate(editContext));
                    builder.CloseComponent();
                }
                else
                {
                    builder.AddContent(sequence++, column.CellTemplate(item));
                    if (EditMode == VekDataGridEditMode.Cell && column.Editable && column.EditTemplate is not null)
                    {
                        builder.OpenElement(sequence++, "button");
                        builder.AddAttribute(sequence++, "type", "button");
                        builder.AddAttribute(sequence++, "aria-label", $"Edit {column.Title}");
                        builder.AddAttribute(sequence++, "onclick:stopPropagation", true);
                        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => StartEditAsync(item, column)));
                        builder.AddContent(sequence++, "Edit");
                        builder.CloseElement();
                    }
                    if ((EditMode is VekDataGridEditMode.Row or VekDataGridEditMode.Form)
                        && CanEditRow && ReferenceEquals(column, _columns.FirstOrDefault()))
                    {
                        builder.OpenElement(sequence++, "button");
                        builder.AddAttribute(sequence++, "type", "button");
                        builder.AddAttribute(sequence++, "aria-label", $"Edit row {GetEditLabel(item)}");
                        builder.AddAttribute(sequence++, "onclick:stopPropagation", true);
                        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => StartEditAsync(item)));
                        builder.AddContent(sequence++, "Edit");
                        builder.CloseElement();
                    }
                }
                builder.CloseElement();
            }
            builder.CloseElement();
        }
    };

    /// <summary>
    /// Gets or sets an additional CSS class applied to the grid container.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the property columns displayed by the grid.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private int EffectivePageSize => Math.Max(PageSize, 1);

    /// <summary>Clamps the current page when incoming data or parameters reduce the page count.</summary>
    protected override async Task OnParametersSetAsync()
    {
        var lastPage = GetTotalPages(GetSortedItems().Count);
        if (_currentPage > lastPage)
        {
            _currentPage = lastPage;
            await CurrentPageChanged.InvokeAsync(_currentPage);
        }

        var selectionChanged = ReconcileSelection();
        if (selectionChanged)
        {
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
            await SelectedItemChanged.InvokeAsync(SelectedItem);
        }
    }

    private bool ReconcileSelection()
    {
        if (SelectedItems.Count == 0 && SelectedItem is null) return false;
        var currentItems = Items?.ToList() ?? [];
        var reconciled = SelectedItems.Select(selected =>
                currentItems.FirstOrDefault(current => ItemComparer.Equals(current, selected)) ?? selected)
            .ToList();
        var selectedItem = SelectedItem is null
            ? reconciled.FirstOrDefault()
            : currentItems.FirstOrDefault(current => ItemComparer.Equals(current, SelectedItem)) ?? SelectedItem;

        if (FilteredSelectionMode == VekDataGridFilteredSelectionMode.Clear)
        {
            reconciled.RemoveAll(item => !MatchesCurrentFilters(item));
            if (selectedItem is not null && !MatchesCurrentFilters(selectedItem)) selectedItem = default;
        }

        var changed = !reconciled.SequenceEqual(SelectedItems, ItemComparer)
            || (SelectedItem is not null && selectedItem is not null && !ReferenceEquals(SelectedItem, selectedItem));
        SelectedItems = reconciled;
        SelectedItem = selectedItem;
        return changed;
    }

    private bool MatchesCurrentFilters(TItem item) => (QuickFilter?.Invoke(item) ?? true) && MatchesFilters(item);

    private async Task NotifySelectionAfterFilteringAsync()
    {
        if (FilteredSelectionMode == VekDataGridFilteredSelectionMode.Clear && ReconcileSelection())
        {
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
            await SelectedItemChanged.InvokeAsync(SelectedItem);
        }
    }

    /// <summary>Gets the total item count after filtering and before paging.</summary>
    public int TotalItems => GetSortedItems().Count;

    internal void RegisterColumn(IVekDataGridColumn<TItem> column)
    {
        if (_columns.Contains(column))
        {
            return;
        }

        _columns.Add(column);
        _ = InvokeAsync(StateHasChanged);
    }

    internal void UnregisterColumn(IVekDataGridColumn<TItem> column)
    {
        if (_columns.Remove(column))
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }

    internal void RefreshColumn(IVekDataGridColumn<TItem> column)
    {
        if (_columns.Contains(column))
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }

    private List<TItem> GetSortedItems()
    {
        var items = Items?.Where(item => QuickFilter?.Invoke(item) ?? true)
            .Where(MatchesFilters).ToList() ?? [];
        if (_sorts.Count == 0)
        {
            return items;
        }

        var comparer = Comparer<TItem>.Create(CompareByActiveSorts);
        return items.OrderBy(item => item, comparer).ToList();
    }

    private bool MatchesFilters(TItem item)
    {
        foreach (var filter in _filters)
        {
            var column = _columns.FirstOrDefault(candidate => candidate.Key == filter.ColumnKey);
            var selector = column?.FilterBy;
            if (selector is null || !MatchesValue(selector(item), filter)) return false;
        }
        return true;
    }

    private static bool MatchesValue(object? source, VekDataGridFilter filter)
    {
        if (filter.Operator == VekDataGridFilterOperator.IsNull) return source is null;
        if (filter.Operator == VekDataGridFilterOperator.IsNotNull) return source is not null;
        if (filter.Operator == VekDataGridFilterOperator.IsEmpty) return source is null || source is string { Length: 0 };
        if (filter.Operator == VekDataGridFilterOperator.IsNotEmpty) return source is not null && source is not string { Length: 0 };
        if (source is null || filter.Value is null) return filter.Operator == VekDataGridFilterOperator.NotEquals && source != filter.Value;
        if (source is string sourceText)
        {
            var value = Convert.ToString(filter.Value, System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty;
            var comparison = filter.CaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            return filter.Operator switch
            {
                VekDataGridFilterOperator.Equals => string.Equals(sourceText, value, comparison),
                VekDataGridFilterOperator.NotEquals => !string.Equals(sourceText, value, comparison),
                VekDataGridFilterOperator.StartsWith => sourceText.StartsWith(value, comparison),
                VekDataGridFilterOperator.EndsWith => sourceText.EndsWith(value, comparison),
                _ => sourceText.Contains(value, comparison)
            };
        }

        int result;
        try
        {
            result = source is IComparable comparable
                ? comparable.CompareTo(Convert.ChangeType(filter.Value, source.GetType(), System.Globalization.CultureInfo.CurrentCulture))
                : Equals(source, filter.Value) ? 0 : -1;
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            return false;
        }
        return filter.Operator switch
        {
            VekDataGridFilterOperator.Equals => result == 0,
            VekDataGridFilterOperator.NotEquals => result != 0,
            VekDataGridFilterOperator.GreaterThan => result > 0,
            VekDataGridFilterOperator.GreaterThanOrEqual => result >= 0,
            VekDataGridFilterOperator.LessThan => result < 0,
            VekDataGridFilterOperator.LessThanOrEqual => result <= 0,
            _ => false
        };
    }

    private int GetTotalPages(int itemCount) => itemCount == 0
        ? 1
        : (itemCount - 1) / EffectivePageSize + 1;

    private async Task SetCurrentPageAsync(int page)
    {
        _currentPage = Math.Clamp(page, 1, GetTotalPages(TotalItems));
        await CurrentPageChanged.InvokeAsync(_currentPage);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SetPageSizeAsync(ChangeEventArgs args)
    {
        if (!int.TryParse(args.Value?.ToString(), out var pageSize)) return;
        await SetPageSizeAsync(pageSize);
    }

    private async Task SetPageSizeAsync(int pageSize)
    {
        if (pageSize < 1) return;
        PageSize = pageSize;
        _currentPage = 1;
        await PageSizeChanged.InvokeAsync(pageSize);
        await CurrentPageChanged.InvokeAsync(_currentPage);
        await InvokeAsync(StateHasChanged);
    }

    private VekDataGridPagerContext<TItem> CreatePagerContext(int totalItems, int totalPages) => new(
        CurrentState,
        totalItems,
        totalPages,
        SetCurrentPageAsync,
        SetPageSizeAsync);

    private string GetAriaSort(IVekDataGridColumn<TItem> column)
    {
        var activeSort = _sorts.FirstOrDefault(sort => ReferenceEquals(sort.Column, column));
        if (activeSort is null)
        {
            return "none";
        }

        return activeSort.Direction == VekSortDirection.Ascending ? "ascending" : "descending";
    }

    private int? GetSortPriority(IVekDataGridColumn<TItem> column)
    {
        var index = _sorts.FindIndex(sort => ReferenceEquals(sort.Column, column));
        return index < 0 ? null : index + 1;
    }

    private string GetSortIndicator(IVekDataGridColumn<TItem> column)
    {
        var sort = _sorts.First(activeSort => ReferenceEquals(activeSort.Column, column));
        var arrow = sort.Direction == VekSortDirection.Ascending ? "\u2191" : "\u2193";
        return SortMode == VekDataGridSortMode.Multiple
            ? $"{arrow}{GetSortPriority(column)}"
            : arrow;
    }

    private async Task SortBy(IVekDataGridColumn<TItem> column, MouseEventArgs args)
    {
        if (SortMode == VekDataGridSortMode.None || !column.Sortable)
        {
            return;
        }

        var index = _sorts.FindIndex(sort => ReferenceEquals(sort.Column, column));
        if (args.AltKey && SortMode == VekDataGridSortMode.Multiple)
        {
            if (index >= 0)
            {
                _sorts.RemoveAt(index);
            }
        }
        else
        {
            if (index >= 0)
            {
                if (AllowUnsorted)
                {
                    _sorts.RemoveAt(index);
                }
                else
                {
                    var current = _sorts[index];
                    _sorts[index] = current with
                    {
                        Direction = current.Direction == VekSortDirection.Ascending
                            ? VekSortDirection.Descending
                            : VekSortDirection.Ascending
                    };
                }
            }
            else
            {
                if (SortMode == VekDataGridSortMode.Single || !args.CtrlKey)
                {
                    _sorts.Clear();
                }

                _sorts.Add(new ActiveSort(column, VekSortDirection.Ascending));
            }
        }

        _currentPage = 1;
        await SortChanged.InvokeAsync(GetSortState());
    }

    private int CompareByActiveSorts(TItem left, TItem right)
    {
        foreach (var sort in _sorts)
        {
            var comparison = sort.Column.CompareItems(left, right);
            if (comparison != 0)
            {
                return sort.Direction == VekSortDirection.Ascending ? comparison : -comparison;
            }
        }

        return 0;
    }

    private IReadOnlyList<VekDataGridSort> GetSortState() => _sorts
        .Select((sort, index) => new VekDataGridSort(
            sort.Column.Key,
            sort.Direction,
            index + 1))
        .ToArray();

}
