using Vek.Blazor.DataGrid.Components.DataGrid;

namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Displays a collection of items in a table with client-side sorting and pagination.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class VekDataGrid<TItem> : IDisposable
{
    private readonly List<IVekDataGridColumn<TItem>> _columns = [];
    private readonly List<ActiveSort> _sorts = [];
    private readonly List<VekDataGridFilter> _filters = [];
    private List<TItem> _expandedRows = [];
    private readonly HashSet<string> _collapsedGroups = new(StringComparer.Ordinal);
    private int _currentPage = 1;
    private TItem? _editingOriginal;
    private TItem? _editingDraft;
    private IVekDataGridColumn<TItem>? _editingColumn;
    private EditContext? _editContext;
    private VekDataGridEditContext<TItem>? _activeEditContext;
    private bool _isEditing;
    private bool _initialExpansionApplied;
    private IEnumerable<TItem> _items = Array.Empty<TItem>();
    private INotifyCollectionChanged? _observableItems;
    private bool _disposed;
    private bool _itemsParameterWasSet;
    private IReadOnlyList<TItem> _serverItems = Array.Empty<TItem>();
    private int _serverTotalItems;
    private bool _serverLoading;
    private Exception? _serverError;
    private CancellationTokenSource? _serverLoadCancellation;
    private long _serverRequestVersion;
    private bool _serverInitialized;
    private IReadOnlyList<TItem> _virtualServerItems = Array.Empty<TItem>();
    private int _virtualServerStartIndex;
    private Virtualize<TItem>? _virtualizer;
    private bool _columnPanelOpen;
    private string _columnSearch = string.Empty;
    private string? _draggedColumnKey;
    private string? _dropTargetColumnKey;

    private sealed record ActiveSort(IVekDataGridColumn<TItem> Column, VekSortDirection Direction);
    private sealed record GroupNode(
        VekDataGridGroupDefinition Definition,
        IVekDataGridColumn<TItem> Column,
        object? Value,
        IReadOnlyList<TItem> Items,
        int Level,
        string Path);
    private sealed record ColumnGroupSegment(string? Title, int Span);

    /// <summary>
    /// Gets or sets the complete collection of items displayed by the grid.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> Items
    {
        get => _items;
        set
        {
            var nextItems = value ?? Array.Empty<TItem>();
            if (!ReferenceEquals(_items, nextItems))
            {
                if (_observableItems is not null) _observableItems.CollectionChanged -= ItemsCollectionChanged;
                _items = nextItems;
                _observableItems = _items as INotifyCollectionChanged;
                if (_observableItems is not null) _observableItems.CollectionChanged += ItemsCollectionChanged;
            }
            _itemsParameterWasSet = true;
        }
    }

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_disposed) return;
        _ = InvokeAsync(RefreshAfterCollectionChangeAsync);
    }

    private async Task RefreshAfterCollectionChangeAsync()
    {
        if (_disposed) return;
        var totalPages = GetTotalPages(TotalItems);
        if (_currentPage > totalPages)
        {
            _currentPage = totalPages;
            await CurrentPageChanged.InvokeAsync(_currentPage);
        }

        if (ReconcileObservableSelection())
        {
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
            await SelectedItemChanged.InvokeAsync(SelectedItem);
        }

        var rows = GetCurrentItems().ToList();
        _expandedRows = _expandedRows
            .Select(expanded => rows.FirstOrDefault(row => DetailsComparer.Equals(row, expanded)))
            .Where(row => row is not null)
            .Cast<TItem>()
            .Distinct(DetailsComparer)
            .ToList();
        if (SingleExpandedRow && _expandedRows.Count > 1) _expandedRows = _expandedRows.Take(1).ToList();
        await InvokeAsync(StateHasChanged);
    }

    private bool ReconcileObservableSelection()
    {
        var currentItems = GetCurrentItems().ToList();
        var reconciled = SelectedItems
            .Select(selected => currentItems.FirstOrDefault(current => ItemComparer.Equals(current, selected)))
            .Where(item => item is not null)
            .Cast<TItem>()
            .Distinct(ItemComparer)
            .ToList();
        var selectedItem = SelectedItem is null
            ? reconciled.FirstOrDefault()
            : currentItems.FirstOrDefault(current => ItemComparer.Equals(current, SelectedItem));
        if (FilteredSelectionMode == VekDataGridFilteredSelectionMode.Clear)
        {
            reconciled.RemoveAll(item => !MatchesCurrentFilters(item));
            if (selectedItem is not null && !MatchesCurrentFilters(selectedItem)) selectedItem = default;
        }

        var changed = !reconciled.SequenceEqual(SelectedItems, ItemComparer)
            || (SelectedItem is not null && selectedItem is not null && !ReferenceEquals(SelectedItem, selectedItem))
            || (SelectedItem is not null && selectedItem is null);
        SelectedItems = reconciled;
        SelectedItem = selectedItem;
        return changed;
    }

    /// <summary>Stops observing item changes and cancels an active server request.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_observableItems is not null) _observableItems.CollectionChanged -= ItemsCollectionChanged;
        _observableItems = null;
        _serverLoadCancellation?.Cancel();
    }

    /// <summary>Gets or sets the asynchronous page source. Use instead of the local Items parameter.</summary>
    [Parameter]
    public Func<VekDataGridState<TItem>, CancellationToken, Task<VekDataGridData<TItem>>>? ServerData { get; set; }

    /// <summary>Gets or sets custom content displayed while a server request is running.</summary>
    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    /// <summary>Gets or sets custom content displayed when a server request fails.</summary>
    [Parameter]
    public RenderFragment<Exception>? ErrorContent { get; set; }

    /// <summary>Gets or sets custom content displayed when the current result set is empty.</summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>Gets or sets custom actions displayed above the table.</summary>
    [Parameter]
    public RenderFragment? ToolbarTemplate { get; set; }

    /// <summary>Gets or sets additional rows or content rendered inside the table header.</summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>Gets or sets custom cell content for each row. The template must render valid table cells.</summary>
    [Parameter]
    public RenderFragment<TItem>? RowTemplate { get; set; }

    /// <summary>Gets or sets action content appended to each row.</summary>
    [Parameter]
    public RenderFragment<VekDataGridRowContext<TItem>>? RowActionsTemplate { get; set; }

    /// <summary>Gets or sets action content rendered inside each data cell.</summary>
    [Parameter]
    public RenderFragment<VekDataGridCellContext<TItem>>? CellActionsTemplate { get; set; }

    /// <summary>Gets or sets the culture used to format values, aggregates, and edit contexts.</summary>
    [Parameter]
    public CultureInfo? Culture { get; set; }

    /// <summary>Gets or sets the accessible name announced for the table.</summary>
    [Parameter]
    public string? AriaLabel { get; set; } = "Data grid";

    /// <summary>Gets or sets additional attributes applied to the table element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>Gets or sets whether the grid displays a column options panel.</summary>
    [Parameter]
    public bool ShowColumnPanel { get; set; }

    /// <summary>Gets or sets the surfaces that can reorder columns.</summary>
    [Parameter]
    public VekDataGridColumnReorderMode ColumnReorderMode { get; set; }

    /// <summary>Gets or sets a callback invoked after the column order changes.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>> ColumnReordered { get; set; }

    /// <summary>Gets or sets the visible column groups shown above the table headings.</summary>
    [Parameter]
    public IReadOnlyList<VekDataGridColumnGroup> ColumnGroups { get; set; } = Array.Empty<VekDataGridColumnGroup>();

    /// <summary>Gets or sets the resizing surface enabled for the grid.</summary>
    [Parameter]
    public VekDataGridColumnResizeMode ColumnResizeMode { get; set; }

    /// <summary>Gets or sets the row density.</summary>
    [Parameter]
    public VekDataGridDensity Density { get; set; } = VekDataGridDensity.Comfortable;

    /// <summary>Gets or sets whether the grid draws cell borders.</summary>
    [Parameter]
    public bool Bordered { get; set; }

    /// <summary>Gets or sets whether alternating rows receive a contrasting background.</summary>
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>Gets or sets whether the table header remains visible while its container scrolls.</summary>
    [Parameter]
    public bool FixedHeader { get; set; }

    /// <summary>Gets or sets whether the grid applies narrow-screen overflow constraints.</summary>
    [Parameter]
    public bool Responsive { get; set; }

    /// <summary>Gets or sets the initial width of a resizable grid container in pixels.</summary>
    [Parameter]
    public int? ContainerWidth { get; set; }

    /// <summary>Gets or sets a function that supplies a CSS class for a row.</summary>
    [Parameter]
    public Func<TItem, int, string?>? RowClass { get; set; }

    /// <summary>Gets or sets a function that supplies inline CSS for a row.</summary>
    [Parameter]
    public Func<TItem, int, string?>? RowStyle { get; set; }

    /// <summary>Gets or sets a function that supplies a CSS class for a cell.</summary>
    [Parameter]
    public Func<TItem, string, int, string?>? CellClass { get; set; }

    /// <summary>Gets or sets a function that supplies inline CSS for a cell.</summary>
    [Parameter]
    public Func<TItem, string, int, string?>? CellStyle { get; set; }

    /// <summary>Gets or sets a function that supplies a CSS class for a column heading.</summary>
    [Parameter]
    public Func<string, string?>? HeaderClass { get; set; }

    /// <summary>Gets or sets a function that supplies inline CSS for a column heading.</summary>
    [Parameter]
    public Func<string, string?>? HeaderStyle { get; set; }

    /// <summary>Gets or sets a function that supplies a CSS class for the aggregate footer.</summary>
    [Parameter]
    public Func<string?>? FooterClass { get; set; }

    /// <summary>Gets or sets a function that supplies inline CSS for the aggregate footer.</summary>
    [Parameter]
    public Func<string?>? FooterStyle { get; set; }

    /// <summary>Gets or sets whether rows are rendered through the framework virtualizer.</summary>
    [Parameter]
    public bool VirtualizeRows { get; set; }

    /// <summary>Gets or sets the estimated height in pixels for each virtualized row.</summary>
    [Parameter]
    public float VirtualItemSize { get; set; } = 48;

    /// <summary>Gets or sets the number of additional rows rendered before and after the viewport.</summary>
    [Parameter]
    public int VirtualOverscanCount { get; set; } = 3;

    /// <summary>Gets or sets the maximum number of rows the virtualizer may render at once.</summary>
    [Parameter]
    public int VirtualMaxItemCount { get; set; } = 100;

    /// <summary>Gets or sets the asynchronous range source used for server virtualization.</summary>
    [Parameter]
    public Func<VekDataGridState<TItem>, int, int, CancellationToken, Task<VekDataGridData<TItem>>>? ServerVirtualData { get; set; }

    /// <summary>Gets whether a server request is currently running.</summary>
    public bool IsLoading => _serverLoading;

    /// <summary>Gets the total number of matching items before paging.</summary>
    public int TotalItems => ServerData is null ? GetSortedItems().Count : _serverTotalItems;

    private IReadOnlyList<IVekDataGridColumn<TItem>> VisibleColumns => _columns.Where(column => column.Visible).ToArray();
    private IEnumerable<IVekDataGridColumn<TItem>> SearchableColumns => string.IsNullOrWhiteSpace(_columnSearch)
        ? _columns
        : _columns.Where(column => column.Title.Contains(_columnSearch.Trim(), StringComparison.OrdinalIgnoreCase));
    private int VisibleColumnCount => Math.Max(VisibleColumns.Count, 1) + (RowActionsTemplate is null ? 0 : 1);
    private bool CanReorderInPanel => ColumnReorderMode is VekDataGridColumnReorderMode.Panel or VekDataGridColumnReorderMode.Both;
    private bool CanReorderInHeader => ColumnReorderMode is VekDataGridColumnReorderMode.Header or VekDataGridColumnReorderMode.Both;
    private int? CurrentContainerWidth => _containerWidth ?? ContainerWidth;
    private int? _containerWidth;

    /// <summary>Changes a column's visibility by its stable key.</summary>
    public async Task SetColumnVisibilityAsync(string columnKey, bool visible)
    {
        var column = _columns.FirstOrDefault(candidate => candidate.Key == columnKey)
            ?? throw new ArgumentException($"No column with key '{columnKey}' is registered.", nameof(columnKey));
        if (column.Visible == visible) return;
        await column.SetVisibleAsync(visible);
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Moves a column to a zero-based insert position in the current column list.</summary>
    public async Task ReorderColumnAsync(string columnKey, int insertIndex)
    {
        var currentIndex = _columns.FindIndex(column => column.Key == columnKey);
        if (currentIndex < 0) throw new ArgumentException($"No column with key '{columnKey}' is registered.", nameof(columnKey));
        var column = _columns[currentIndex];
        _columns.RemoveAt(currentIndex);
        var targetIndex = Math.Clamp(insertIndex, 0, _columns.Count);
        if (targetIndex == currentIndex)
        {
            _columns.Insert(currentIndex, column);
            return;
        }

        _columns.Insert(targetIndex, column);
        await ColumnReordered.InvokeAsync(_columns.Select(candidate => candidate.Key).ToArray());
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Changes a column's width by its stable key, clamped to that column's size limits.</summary>
    public async Task ResizeColumnAsync(string columnKey, int width)
    {
        if (ColumnResizeMode != VekDataGridColumnResizeMode.Column)
            throw new InvalidOperationException("Column resizing must be enabled with ColumnResizeMode.Column.");
        var column = _columns.FirstOrDefault(candidate => candidate.Key == columnKey)
            ?? throw new ArgumentException($"No column with key '{columnKey}' is registered.", nameof(columnKey));
        var minimum = Math.Max(column.MinWidth, 32);
        var maximum = Math.Max(column.MaxWidth, minimum);
        await column.SetWidthAsync(Math.Clamp(width, minimum, maximum));
        await InvokeAsync(StateHasChanged);
    }

    private Task ResizeContainerAsync(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var width))
            _containerWidth = Math.Clamp(width, 320, 1440);
        return InvokeAsync(StateHasChanged);
    }

    private Task ResizeColumnFromInputAsync(string columnKey, ChangeEventArgs args)
        => int.TryParse(args.Value?.ToString(), out var width)
            ? ResizeColumnAsync(columnKey, width)
            : Task.CompletedTask;

    private int GetColumnWidthValue(IVekDataGridColumn<TItem> column) => column.Width ?? 160;

    private string? GetColumnWidthStyle(IVekDataGridColumn<TItem> column)
    {
        var widthStyle = column.Width is int width ? $"width: {width}px;" : null;
        var minWidthStyle = column.MinWidth > 0 ? $"min-width: {column.MinWidth}px;" : null;
        return string.Concat(widthStyle, minWidthStyle);
    }

    private string? GetGridStyle() => CurrentContainerWidth is int width
        ? $"width: {width}px; max-width: 100%;"
        : null;

    private string GetGridClass() => string.Join(' ', new[]
    {
        Class,
        Responsive ? "is-responsive" : null,
        Density == VekDataGridDensity.Compact ? "is-compact" : null,
        Bordered ? "is-bordered" : null,
        Striped ? "is-striped" : null
    }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private string? GetHeaderClass(string key)
        => string.Join(' ', new[] { GetColumnDragClass(key), HeaderClass?.Invoke(key) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private string? GetCellStyle(TItem item, IVekDataGridColumn<TItem> column, int index)
        => string.Concat(GetColumnWidthStyle(column), CellStyle?.Invoke(item, column.Key, index));

    private int GetRowIndex(TItem item)
    {
        var source = ServerVirtualData is not null
            ? _virtualServerItems.ToList()
            : ServerData is not null ? _serverItems.ToList() : GetSortedItems();
        var itemIndex = source.FindIndex(candidate => EqualityComparer<TItem>.Default.Equals(candidate, item));
        if (itemIndex < 0) itemIndex = 0;
        return ServerVirtualData is not null
            ? _virtualServerStartIndex + itemIndex
            : ServerData is not null ? ((_currentPage - 1) * EffectivePageSize) + itemIndex : itemIndex;
    }

    private IReadOnlyList<ColumnGroupSegment> GetColumnGroupSegments()
    {
        var columns = VisibleColumns;
        var segments = new List<ColumnGroupSegment>();
        for (var index = 0; index < columns.Count;)
        {
            var group = ColumnGroups.FirstOrDefault(candidate => candidate.ColumnKeys.Contains(columns[index].Key));
            var span = 1;
            while (index + span < columns.Count)
            {
                var nextGroup = ColumnGroups.FirstOrDefault(candidate => candidate.ColumnKeys.Contains(columns[index + span].Key));
                if (!ReferenceEquals(group, nextGroup)) break;
                span++;
            }

            segments.Add(new ColumnGroupSegment(group?.Title, span));
            index += span;
        }

        return segments;
    }

    private string? GetRowClass(TItem item, int index)
        => string.Join(' ', new[] { IsItemSelected(item) ? "is-selected" : null, RowClass?.Invoke(item, index) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private VekDataGridRowContext<TItem> CreateRowActionsContext(TItem item, int index) => new(
        item,
        index,
        IsItemSelected(item),
        () => SelectItemAsync(item),
        () => ToggleRowDetailsAsync(item),
        () => EditItemAsync(item));

    private VekDataGridCellContext<TItem> CreateCellActionsContext(
        TItem item,
        IVekDataGridColumn<TItem> column,
        int rowIndex) => new(
            item,
            column.Key,
            column.Title,
            rowIndex,
            () => EditCellAsync(item, column.Key));

    private void ToggleColumnPanel() => _columnPanelOpen = !_columnPanelOpen;

    private Task MoveColumnInPanelAsync(string key, int offset)
    {
        if (!CanReorderInPanel) return Task.CompletedTask;
        var index = _columns.FindIndex(column => column.Key == key);
        return ReorderColumnAsync(key, index + offset);
    }

    private Task MoveColumnFromHeaderByKeyAsync(string key, KeyboardEventArgs args)
    {
        if (!CanReorderInHeader || args.Key is not "ArrowLeft" and not "ArrowRight") return Task.CompletedTask;
        var index = _columns.FindIndex(column => column.Key == key);
        return ReorderColumnAsync(key, index + (args.Key == "ArrowLeft" ? -1 : 1));
    }

    private void BeginColumnDrag(string key)
    {
        _draggedColumnKey = key;
        _dropTargetColumnKey = null;
    }

    private void SetColumnDropTarget(string key) => _dropTargetColumnKey = key;

    private async Task DropColumnAsync(string targetKey, bool fromHeader)
    {
        if ((fromHeader && !CanReorderInHeader) || (!fromHeader && !CanReorderInPanel)) return;
        var sourceIndex = _columns.FindIndex(column => column.Key == _draggedColumnKey);
        var targetIndex = _columns.FindIndex(column => column.Key == targetKey);
        if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
        {
            if (sourceIndex > targetIndex) targetIndex++;
            await ReorderColumnAsync(_draggedColumnKey!, targetIndex);
        }
        EndColumnDrag();
    }

    private void EndColumnDrag()
    {
        _draggedColumnKey = null;
        _dropTargetColumnKey = null;
    }

    private string GetColumnDragClass(string key)
        => $"{(_draggedColumnKey == key ? "is-dragging" : string.Empty)} {(_dropTargetColumnKey == key ? "is-drop-target" : string.Empty)}".Trim();

    /// <summary>Reloads the current page from the configured server source.</summary>
    public Task RefreshServerDataAsync() => ServerData is null
        ? throw new InvalidOperationException("ServerData must be configured before refreshing server items.")
        : LoadServerDataAsync();

    private async Task LoadServerDataAsync()
    {
        var source = ServerData ?? throw new InvalidOperationException("ServerData must be configured before loading server items.");
        var previousRequest = _serverLoadCancellation;
        var requestCancellation = new CancellationTokenSource();
        _serverLoadCancellation = requestCancellation;
        var requestVersion = ++_serverRequestVersion;
        previousRequest?.Cancel();
        _serverLoading = true;
        _serverError = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await source(CurrentState, requestCancellation.Token);
            if (requestVersion != _serverRequestVersion || requestCancellation.IsCancellationRequested) return;
            _serverItems = result.Items ?? Array.Empty<TItem>();
            _serverTotalItems = Math.Max(0, result.TotalItems);
        }
        catch (OperationCanceledException) when (requestCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (requestVersion == _serverRequestVersion) _serverError = exception;
        }
        finally
        {
            if (requestVersion == _serverRequestVersion)
            {
                _serverLoading = false;
                _serverLoadCancellation = null;
                await InvokeAsync(StateHasChanged);
            }
            requestCancellation.Dispose();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of rows displayed on each page. Defaults to all client items;
    /// when using server data, the default request size is 10.
    /// </summary>
    [Parameter]
    public int PageSize { get; set; }

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

    /// <summary>Gets or sets a predicate that marks rows initially expanded.</summary>
    [Parameter]
    public Func<TItem, bool>? InitiallyExpanded { get; set; }

    /// <summary>Gets or sets whether only one row can be expanded at a time.</summary>
    [Parameter]
    public bool SingleExpandedRow { get; set; }

    /// <summary>Gets or sets the comparer used to preserve expanded rows across refreshes.</summary>
    [Parameter]
    public IEqualityComparer<TItem>? RowDetailsComparer { get; set; }

    /// <summary>Gets or sets a callback invoked when the expanded row set changes.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> RowDetailsExpandedChanged { get; set; }

    /// <summary>
    /// Gets or sets whether sorting is disabled, limited to one column, or applied to multiple columns.
    /// </summary>
    [Parameter]
    public VekDataGridSortMode SortMode { get; set; } = VekDataGridSortMode.Single;

    /// <summary>
    /// Gets or sets whether a column cycles from descending back to unsorted when clicked again.
    /// </summary>
    [Parameter]
    public bool AllowUnsorted { get; set; } = true;

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
            await ReloadAfterStateChangeAsync();
            return;
        }

        _filters.Add(new VekDataGridFilter(columnKey, op, value, caseSensitive ?? FilterCaseSensitive));
        _currentPage = 1;
        await FilterChanged.InvokeAsync(_filters.ToArray());
        await NotifySelectionAfterFilteringAsync();
        await ReloadAfterStateChangeAsync();
    }

    /// <summary>Clears one column filter or all filters when no key is supplied.</summary>
    public async Task ClearFilterAsync(string? columnKey = null)
    {
        if (columnKey is null) _filters.Clear();
        else _filters.RemoveAll(filter => filter.ColumnKey == columnKey);
        _currentPage = 1;
        await FilterChanged.InvokeAsync(_filters.ToArray());
        await NotifySelectionAfterFilteringAsync();
        await ReloadAfterStateChangeAsync();
    }

    /// <summary>Clears every active filter.</summary>
    public Task ClearFiltersAsync() => ClearFilterAsync();

    /// <summary>Clears every active sort and notifies bound consumers.</summary>
    public async Task ClearSortAsync()
    {
        _sorts.Clear();
        await SortChanged.InvokeAsync(Array.Empty<VekDataGridSort>());
        await ReloadAfterStateChangeAsync();
    }

    /// <summary>Reloads the current data source or rerenders local items.</summary>
    public Task ReloadAsync() => ReloadAfterStateChangeAsync();

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

    /// <summary>Selects or deselects one item according to the current selection mode.</summary>
    public Task SelectItemAsync(TItem item) => !CanSelectItem(item)
        ? Task.CompletedTask
        : ToggleSelectionAsync(item, SelectionMode == VekDataGridSelectionMode.Single || !IsItemSelected(item));

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

    /// <summary>Selects or deselects all selectable items on the current page.</summary>
    public Task SelectAllAsync(bool selected) => ToggleAllVisibleItemsAsync(selected);

    /// <summary>Expands the details for all current items, or the first item in single-row mode.</summary>
    public async Task ExpandAllAsync()
    {
        var rows = GetSortedItems();
        _expandedRows = SingleExpandedRow ? rows.Take(1).ToList() : rows;
        await RowDetailsExpandedChanged.InvokeAsync(_expandedRows.ToArray());
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Collapses every expanded row detail.</summary>
    public async Task CollapseAllAsync()
    {
        _expandedRows.Clear();
        await RowDetailsExpandedChanged.InvokeAsync(_expandedRows.ToArray());
        await InvokeAsync(StateHasChanged);
    }

    private IEqualityComparer<TItem> ItemComparer => SelectionComparer ?? EqualityComparer<TItem>.Default;

    /// <summary>Gets whether the row has any editable columns.</summary>
    public bool CanEditRow => EditMode != VekDataGridEditMode.None
        && VisibleColumns.Any(column => column.Editable && column.EditTemplate is not null);

    /// <summary>Starts editing a row using the configured row or form editing mode.</summary>
    public Task EditItemAsync(TItem item) => StartEditAsync(item);

    /// <summary>Starts editing a cell by its stable column key.</summary>
    public Task EditCellAsync(TItem item, string columnKey)
    {
        var column = _columns.FirstOrDefault(candidate => candidate.Key == columnKey)
            ?? throw new ArgumentException($"No column with key '{columnKey}' is registered.", nameof(columnKey));
        if (!column.Editable || column.EditTemplate is null)
            throw new InvalidOperationException($"Column '{columnKey}' does not have an editable template.");
        return StartEditAsync(item, column);
    }

    /// <summary>Validates and commits the current edit draft.</summary>
    public Task<bool> CommitEditAsync() => CommitActiveEditAsync();

    /// <summary>Cancels the current edit draft.</summary>
    public Task CancelEditAsync() => CancelActiveEditAsync();

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
            _editingDraft!, _editContext, UpdateEditDraft, CommitActiveEditAsync, CancelActiveEditAsync,
            Culture ?? CultureInfo.CurrentCulture);
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
            item, _editContext, UpdateEditDraft, CommitActiveEditAsync, CancelActiveEditAsync,
            Culture ?? CultureInfo.CurrentCulture);
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

    internal string GetRowLabel(TItem item) => VisibleColumns.FirstOrDefault(column => column.Editable)?.GetRowLabel(item)
        ?? VisibleColumns.FirstOrDefault(column => !column.IsHierarchyColumn && !column.IsSelectionColumn)?.GetRowLabel(item)
        ?? Convert.ToString(item, Culture ?? CultureInfo.CurrentCulture)
        ?? "row";

    internal bool IsRowExpanded(TItem item) => _expandedRows.Any(expanded => DetailsComparer.Equals(expanded, item));

    internal async Task ToggleRowDetailsAsync(TItem item)
    {
        var index = _expandedRows.FindIndex(expanded => DetailsComparer.Equals(expanded, item));
        if (index >= 0)
        {
            _expandedRows.RemoveAt(index);
        }
        else
        {
            if (SingleExpandedRow) _expandedRows.Clear();
            _expandedRows.Add(item);
        }
        await RowDetailsExpandedChanged.InvokeAsync(_expandedRows.ToArray());
        await InvokeAsync(StateHasChanged);
    }

    private IEqualityComparer<TItem> DetailsComparer => RowDetailsComparer ?? SelectionComparer ?? EqualityComparer<TItem>.Default;

    private bool AreAllRowsExpanded()
    {
        var rows = GetSortedItems();
        return rows.Count > 0 && rows.All(IsRowExpanded);
    }

    private async Task ToggleAllRowDetailsAsync()
    {
        if (AreAllRowsExpanded()) await CollapseAllAsync();
        else await ExpandAllAsync();
    }

    private VekDataGridRowDetailContext<TItem> CreateRowDetailContext(TItem item) => new(
        item,
        IsRowExpanded(item),
        () => ToggleRowDetailsAsync(item));

    private RenderFragment RenderRowDetails(TItem item) => builder =>
    {
        var detailTemplate = VisibleColumns.FirstOrDefault(column => column.IsHierarchyColumn)?.DetailTemplate;
        if (detailTemplate is null || !IsRowExpanded(item)) return;
        builder.OpenElement(0, "tr");
        builder.AddAttribute(1, "class", "detail-row");
        builder.AddAttribute(2, "data-for-row", GetRowLabel(item));
        builder.OpenElement(3, "td");
        builder.AddAttribute(4, "colspan", VisibleColumnCount);
        builder.AddContent(5, detailTemplate(CreateRowDetailContext(item)));
        builder.CloseElement();
        builder.CloseElement();
    };

    private List<TItem> GetPageItems() => ServerVirtualData is not null
        ? _virtualServerItems.ToList()
        : ServerData is not null
            ? _serverItems.ToList()
            : GetSortedItems().Skip((_currentPage - 1) * EffectivePageSize).Take(EffectivePageSize).ToList();

    private IReadOnlyList<TItem> GetCurrentItems() => ServerVirtualData is not null
        ? _virtualServerItems
        : ServerData is null ? Items.ToArray() : _serverItems;

    private async ValueTask<ItemsProviderResult<TItem>> LoadVirtualItemsAsync(ItemsProviderRequest request)
    {
        var source = ServerVirtualData
            ?? throw new InvalidOperationException("ServerVirtualData must be configured for server virtualization.");
        var result = await source(CurrentState, request.StartIndex, request.Count, request.CancellationToken);
        var totalItems = Math.Max(0, result.TotalItems);
        var availableItems = Math.Max(0, totalItems - request.StartIndex);
        var requestedItems = Math.Min(request.Count, availableItems);
        var items = result.Items.Take(requestedItems).ToArray();
        _virtualServerStartIndex = request.StartIndex;
        _virtualServerItems = items;
        return new ItemsProviderResult<TItem>(items, totalItems);
    }

    private async Task RefreshVirtualDataAsync()
    {
        if (VirtualizeRows && ServerVirtualData is not null && _virtualizer is not null)
            await _virtualizer.RefreshDataAsync();
        await InvokeAsync(StateHasChanged);
    }

    private Task ReloadAfterStateChangeAsync() => ServerData is not null
        ? LoadServerDataAsync()
        : VirtualizeRows && ServerVirtualData is not null
            ? RefreshVirtualDataAsync()
            : InvokeAsync(StateHasChanged);

    private RenderFragment<TItem> VirtualizedRowTemplate => item => builder =>
    {
        if (VirtualizeRows && VisibleColumns.Any(column => column.IsHierarchyColumn))
            throw new InvalidOperationException("Virtualization cannot be combined with hierarchical row details because expanded rows have variable heights.");

        var sequence = 0;
        var rowIndex = GetRowIndex(item);
        builder.OpenElement(sequence++, "tr");
        builder.SetKey(item);
        builder.AddAttribute(sequence++, "class", GetRowClass(item, rowIndex));
        builder.AddAttribute(sequence++, "style", RowStyle?.Invoke(item, rowIndex));
        builder.AddAttribute(sequence++, "aria-selected", IsItemSelected(item));
        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => SelectRowAsync(item)));

        if (RowTemplate is not null)
        {
            builder.AddContent(sequence++, RowTemplate(item));
        }
        else
        {
            foreach (var column in VisibleColumns)
            {
                builder.OpenElement(sequence++, "td");
                builder.AddAttribute(sequence++, "class", CellClass?.Invoke(item, column.Key, rowIndex));
                builder.AddAttribute(sequence++, "style", GetCellStyle(item, column, rowIndex));
                if (IsEditing(item, column) && column.EditTemplate is not null)
                {
                    builder.OpenComponent<CascadingValue<EditContext>>(sequence++);
                    builder.AddAttribute(sequence++, nameof(CascadingValue<EditContext>.Value), _editContext);
                    builder.AddAttribute(sequence++, nameof(CascadingValue<EditContext>.ChildContent),
                        column.EditTemplate(GetEditContext(item, column)!));
                    builder.CloseComponent();
                }
                else
                {
                    builder.AddContent(sequence++, column.CellTemplate(item));
                    if (EditMode == VekDataGridEditMode.Cell && column.Editable && column.EditTemplate is not null)
                    {
                        builder.OpenElement(sequence++, "button");
                        builder.AddAttribute(sequence++, "type", "button");
                        builder.AddAttribute(sequence++, "class", "edit-cell-button");
                        builder.AddAttribute(sequence++, "aria-label", $"Edit {column.Title}");
                        builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);
                        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => StartEditAsync(item, column)));
                        builder.AddContent(sequence++, "Edit");
                        builder.CloseElement();
                    }

                    if ((EditMode is VekDataGridEditMode.Row or VekDataGridEditMode.Form)
                        && CanEditRow && ReferenceEquals(column, VisibleColumns.FirstOrDefault()))
                    {
                        builder.OpenElement(sequence++, "button");
                        builder.AddAttribute(sequence++, "type", "button");
                        builder.AddAttribute(sequence++, "class", "edit-row-button");
                        builder.AddAttribute(sequence++, "aria-label", $"Edit row {GetRowLabel(item)}");
                        builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);
                        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => StartEditAsync(item)));
                        builder.AddContent(sequence++, "Edit");
                        builder.CloseElement();
                    }
                }
                if (CellActionsTemplate is not null)
                {
                    builder.OpenElement(sequence++, "span");
                    builder.AddAttribute(sequence++, "class", "cell-actions");
                    builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);
                    builder.AddContent(sequence++, CellActionsTemplate(CreateCellActionsContext(item, column, rowIndex)));
                    builder.CloseElement();
                }
                builder.CloseElement();
            }
        }

        if (RowActionsTemplate is not null)
        {
            builder.OpenElement(sequence++, "td");
            builder.AddAttribute(sequence++, "class", "row-actions");
            builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);
            builder.AddContent(sequence++, RowActionsTemplate(CreateRowActionsContext(item, rowIndex)));
            builder.CloseElement();
        }

        builder.CloseElement();
        builder.AddContent(sequence, RenderRowDetails(item));
    };

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

    private int CompareGroupValues(object? left, object? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left is null) return -1;
        if (right is null) return 1;
        if (left is string leftText && right is string rightText)
            return StringComparer.Create(Culture ?? CultureInfo.CurrentCulture, ignoreCase: false).Compare(leftText, rightText);
        return left is IComparable comparable ? comparable.CompareTo(right) :
            StringComparer.Create(Culture ?? CultureInfo.CurrentCulture, ignoreCase: false).Compare(left.ToString(), right.ToString());
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
            ? formattable.ToString(definition.Format, Culture ?? CultureInfo.CurrentCulture) ?? string.Empty
            : Convert.ToString(result, Culture ?? CultureInfo.CurrentCulture) ?? string.Empty;
        return new VekDataGridAggregateResult(definition.Key, definition.Key, result, formatted);
    }

    private decimal[] NumericValues(IEnumerable<object?> values)
    {
        try
        {
            return values.Select(value => Convert.ToDecimal(value, Culture ?? CultureInfo.CurrentCulture)).ToArray();
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
            builder.AddAttribute(sequence++, "colspan", VisibleColumnCount);
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
            var rowIndex = GetRowIndex(item);
            builder.OpenElement(sequence++, "tr");
            builder.AddAttribute(sequence++, "class", GetRowClass(item, rowIndex));
            builder.AddAttribute(sequence++, "style", RowStyle?.Invoke(item, rowIndex));
            builder.AddAttribute(sequence++, "aria-selected", IsItemSelected(item));
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => SelectRowAsync(item)));
            if (RowTemplate is not null)
            {
                builder.AddContent(sequence++, RowTemplate(item));
            }
            else
            {
                foreach (var column in VisibleColumns)
                {
                    builder.OpenElement(sequence++, "td");
                    builder.AddAttribute(sequence++, "class", CellClass?.Invoke(item, column.Key, rowIndex));
                    builder.AddAttribute(sequence++, "style", GetCellStyle(item, column, rowIndex));
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
                            && CanEditRow && ReferenceEquals(column, VisibleColumns.FirstOrDefault()))
                        {
                            builder.OpenElement(sequence++, "button");
                            builder.AddAttribute(sequence++, "type", "button");
                            builder.AddAttribute(sequence++, "aria-label", $"Edit row {GetRowLabel(item)}");
                            builder.AddAttribute(sequence++, "onclick:stopPropagation", true);
                            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => StartEditAsync(item)));
                            builder.AddContent(sequence++, "Edit");
                            builder.CloseElement();
                        }
                    }
                    if (CellActionsTemplate is not null)
                    {
                        builder.OpenElement(sequence++, "span");
                        builder.AddAttribute(sequence++, "class", "cell-actions");
                        builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);
                        builder.AddContent(sequence++, CellActionsTemplate(CreateCellActionsContext(item, column, rowIndex)));
                        builder.CloseElement();
                    }
                    builder.CloseElement();
                }
            }
            if (RowActionsTemplate is not null)
            {
                builder.OpenElement(sequence++, "td");
                builder.AddAttribute(sequence++, "class", "row-actions");
                builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);
                builder.AddContent(sequence++, RowActionsTemplate(CreateRowActionsContext(item, rowIndex)));
                builder.CloseElement();
            }
            builder.CloseElement();
            builder.AddContent(sequence++, RenderRowDetails(item));
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

    private int EffectivePageSize => PageSize > 0
        ? PageSize
        : ServerData is not null
            ? 10
            : Math.Max(TotalItems, 1);

    /// <summary>Clamps the current page when incoming data or parameters reduce the page count.</summary>
    protected override async Task OnParametersSetAsync()
    {
        if (VirtualizeRows)
        {
            if (VirtualItemSize <= 0 || VirtualMaxItemCount <= 0 || VirtualOverscanCount < 0)
                throw new InvalidOperationException("Virtualization requires a positive item size and max count, and a non-negative overscan count.");
            if (GroupBy.Count > 0)
                throw new InvalidOperationException("Virtualization cannot be combined with grouping because group rows do not have a fixed height.");
            if (VisibleColumns.Any(column => column.IsHierarchyColumn))
                throw new InvalidOperationException("Virtualization cannot be combined with hierarchical row details because expanded rows have variable heights.");
            if (ServerData is not null)
                throw new InvalidOperationException("Virtualization cannot be combined with paged ServerData. Use ServerVirtualData instead.");
            if (ServerVirtualData is null && !_itemsParameterWasSet)
                throw new InvalidOperationException("Client virtualization requires the Items parameter.");
            if (ServerVirtualData is not null && _itemsParameterWasSet)
                throw new InvalidOperationException("Server virtualization and Items are mutually exclusive. Provide exactly one data source.");
        }
        else if (ServerVirtualData is not null)
        {
            throw new InvalidOperationException("ServerVirtualData requires VirtualizeRows to be enabled.");
        }

        if (ServerData is not null && _itemsParameterWasSet)
            throw new InvalidOperationException("Items and ServerData are mutually exclusive. Provide exactly one data source.");

        if (ServerData is not null)
        {
            if (!_serverInitialized)
            {
                _serverInitialized = true;
                await LoadServerDataAsync();
            }
        }
        else if (_serverInitialized)
        {
            _serverRequestVersion++;
            _serverLoadCancellation?.Cancel();
            _serverItems = Array.Empty<TItem>();
            _serverTotalItems = 0;
            _serverError = null;
            _serverLoading = false;
            _serverInitialized = false;
        }

        var lastPage = GetTotalPages(TotalItems);
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

        var rows = GetCurrentItems().ToList();
        if (!_initialExpansionApplied)
        {
            _expandedRows = InitiallyExpanded is null ? [] : rows.Where(InitiallyExpanded).ToList();
            _initialExpansionApplied = true;
        }
        else
        {
            _expandedRows = _expandedRows
                .Select(expanded => rows.FirstOrDefault(row => DetailsComparer.Equals(row, expanded)))
                .Where(row => row is not null)
                .Cast<TItem>()
                .Distinct(DetailsComparer)
                .ToList();
        }
        if (SingleExpandedRow && _expandedRows.Count > 1) _expandedRows = _expandedRows.Take(1).ToList();
    }

    private bool ReconcileSelection()
    {
        if (SelectedItems.Count == 0 && SelectedItem is null) return false;
        var currentItems = GetCurrentItems().ToList();
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
        if (ServerData is not null) return _serverItems.ToList();
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

    private bool MatchesValue(object? source, VekDataGridFilter filter)
    {
        if (filter.Operator == VekDataGridFilterOperator.IsNull) return source is null;
        if (filter.Operator == VekDataGridFilterOperator.IsNotNull) return source is not null;
        if (filter.Operator == VekDataGridFilterOperator.IsEmpty) return source is null || source is string { Length: 0 };
        if (filter.Operator == VekDataGridFilterOperator.IsNotEmpty) return source is not null && source is not string { Length: 0 };
        if (source is null || filter.Value is null) return filter.Operator == VekDataGridFilterOperator.NotEquals && source != filter.Value;
        if (source is string sourceText)
        {
            var value = Convert.ToString(filter.Value, Culture ?? CultureInfo.CurrentCulture) ?? string.Empty;
            var compareOptions = filter.CaseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase;
            var compareInfo = (Culture ?? CultureInfo.CurrentCulture).CompareInfo;
            return filter.Operator switch
            {
                VekDataGridFilterOperator.Equals => compareInfo.Compare(sourceText, value, compareOptions) == 0,
                VekDataGridFilterOperator.NotEquals => compareInfo.Compare(sourceText, value, compareOptions) != 0,
                VekDataGridFilterOperator.StartsWith => compareInfo.IsPrefix(sourceText, value, compareOptions),
                VekDataGridFilterOperator.EndsWith => compareInfo.IsSuffix(sourceText, value, compareOptions),
                _ => compareInfo.IndexOf(sourceText, value, compareOptions) >= 0
            };
        }

        int result;
        try
        {
            result = source is IComparable comparable
                ? comparable.CompareTo(Convert.ChangeType(filter.Value, source.GetType(), Culture ?? CultureInfo.CurrentCulture))
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
        await ReloadAfterStateChangeAsync();
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
        await ReloadAfterStateChangeAsync();
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
                var current = _sorts[index];
                if (AllowUnsorted && current.Direction == VekSortDirection.Descending)
                {
                    _sorts.RemoveAt(index);
                }
                else
                {
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
        await ReloadAfterStateChangeAsync();
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
