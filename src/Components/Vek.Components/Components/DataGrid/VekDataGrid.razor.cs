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
    private int _currentPage = 1;

    private sealed record ActiveSort(IVekDataGridColumn<TItem> Column, VekSortDirection Direction);

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

    private List<TItem> GetPageItems() => GetSortedItems()
        .Skip((_currentPage - 1) * EffectivePageSize)
        .Take(EffectivePageSize)
        .ToList();

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
