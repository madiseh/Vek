namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Displays a collection of items in a table with client-side sorting and pagination.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class VekDataGrid<TItem> : IDisposable, IVekDataGridHost<TItem, RenderFragment>
{
    private readonly List<IVekDataGridColumn<TItem, RenderFragment>> _columns = [];
    private readonly VekDataGridEngine<TItem, RenderFragment> _engine = new() { FilterCaseSensitive = false };
    private IReadOnlyList<TItem> _expandedRows => _engine.Interaction.ExpandedItems;
    private TItem? _editingOriginal => _engine.OriginalEditItem;
    private TItem? _editingDraft => _engine.DraftEditItem;
    private IVekDataGridColumn<TItem, RenderFragment>? _editingColumn => _columns.FirstOrDefault(column => column.Key == _engine.Interaction.EditingColumnKey);
    private EditContext? _editContext;
    private VekDataGridEditContext<TItem>? _activeEditContext;
    private bool _isEditing => _engine.IsEditing;
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

    private sealed record GroupNode(
        VekDataGridGroupDefinition Definition,
        IVekDataGridColumn<TItem, RenderFragment> Column,
        object? Value,
        IReadOnlyList<TItem> Items,
        int Level,
        string Path,
        IReadOnlyList<GroupNode> Children);
    private sealed record ColumnGroupSegment(string? Title, int Span);

    /// <summary>Gets the platform-neutral behavior and state engine for this grid instance.</summary>
    public VekDataGridEngine<TItem, RenderFragment> Engine
    {
        get
        {
            PrepareHostState();
            return _engine;
        }
    }

    private void PrepareHostState()
    {
        SyncCoreState();
        var state = _engine.Interaction;
        state.Items = ServerVirtualData is not null ? _virtualServerItems : ServerData is not null ? _serverItems : Items;
        state.FilteredItemsProvider = GetSortedItems;
        state.VisibleItemsProvider = GetPageItems;
        state.TotalItemsProvider = () => TotalItems;
        state.SelectionMode = SelectionMode;
        state.RowClickSelection = RowClickSelection;
        state.IsRowSelectable = IsRowSelectable;
        state.SelectionComparer = ItemComparer;
        state.SelectedItems = SelectedItems;
        state.SelectedItem = SelectedItem;
        state.FilteredSelectionMode = FilteredSelectionMode;
        state.QuickFilter = QuickFilter;
        state.DetailsComparer = DetailsComparer;
        state.SingleExpandedRow = SingleExpandedRow;
        state.EditMode = EditMode;
        state.CloneItem = CloneItem;
        state.ColumnResizingEnabled = ColumnResizeMode == VekDataGridColumnResizeMode.Column;
        state.MinimumColumnWidth = 32;
    }

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
        if (_engine.Page > totalPages)
        {
            _engine.Page = totalPages;
            await CurrentPageChanged.InvokeAsync(_engine.Page);
        }

        await this.ReconcileSelectionAsync(removeMissing: true);

        var rows = GetCurrentItems().ToList();
        _engine.Interaction.ReconcileExpandedItems(rows);
        await InvokeAsync(StateHasChanged);
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
    public Func<VekDataGridState<TItem>, CancellationToken, Task<VekDataGridData<TItem>>>? ServerData { get; set; } = null;

    /// <summary>Gets or sets custom content displayed while a server request is running.</summary>
    [Parameter]
    public RenderFragment? LoadingContent { get; set; } = null;

    /// <summary>Gets or sets custom content displayed when a server request fails.</summary>
    [Parameter]
    public RenderFragment<Exception>? ErrorContent { get; set; } = null;

    /// <summary>Gets or sets custom content displayed when the current result set is empty.</summary>
    [Parameter]
    public RenderFragment? EmptyContent { get; set; } = null;

    /// <summary>Gets or sets custom actions displayed above the table.</summary>
    [Parameter]
    public RenderFragment? ToolbarTemplate { get; set; } = null;

    /// <summary>Gets or sets additional rows or content rendered inside the table header.</summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; } = null;

    /// <summary>Gets or sets custom cell content for each row. The template must render valid table cells.</summary>
    [Parameter]
    public RenderFragment<TItem>? RowTemplate { get; set; } = null;

    /// <summary>Gets or sets action content appended to each row.</summary>
    [Parameter]
    public RenderFragment<VekDataGridRowContext<TItem>>? RowActionsTemplate { get; set; } = null;

    /// <summary>Gets or sets action content rendered inside each data cell.</summary>
    [Parameter]
    public RenderFragment<VekDataGridCellContext<TItem>>? CellActionsTemplate { get; set; } = null;

    /// <summary>Gets or sets the culture used to format values, aggregates, and edit contexts.</summary>
    [Parameter]
    public CultureInfo? Culture { get; set; } = null;

    /// <summary>Gets or sets the accessible name announced for the table.</summary>
    [Parameter]
    public string? AriaLabel { get; set; } = "Data grid";

    /// <summary>Gets or sets additional attributes applied to the table element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; } = null;

    /// <summary>Gets or sets whether the grid displays a column options panel.</summary>
    [Parameter]
    public bool ShowColumnPanel { get; set; } = false;

    /// <summary>Gets or sets the surfaces that can reorder columns.</summary>
    [Parameter]
    public VekDataGridColumnReorderMode ColumnReorderMode { get; set; } = VekDataGridColumnReorderMode.None;

    /// <summary>Gets or sets a callback invoked after the column order changes.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>> ColumnReordered { get; set; } = default;

    /// <summary>Gets or sets the visible column groups shown above the table headings.</summary>
    [Parameter]
    public IReadOnlyList<VekDataGridColumnGroup> ColumnGroups { get; set; } = Array.Empty<VekDataGridColumnGroup>();

    /// <summary>Gets or sets the resizing surface enabled for the grid.</summary>
    [Parameter]
    public VekDataGridColumnResizeMode ColumnResizeMode { get; set; } = VekDataGridColumnResizeMode.None;

    /// <summary>Gets or sets the row density.</summary>
    [Parameter]
    public VekDataGridDensity Density { get; set; } = VekDataGridDensity.Comfortable;

    /// <summary>Gets or sets whether the grid draws cell borders.</summary>
    [Parameter]
    public bool Bordered { get; set; } = false;

    /// <summary>Gets or sets whether alternating rows receive a contrasting background.</summary>
    [Parameter]
    public bool Striped { get; set; } = false;

    /// <summary>Gets or sets whether the table header remains visible while its container scrolls.</summary>
    [Parameter]
    public bool FixedHeader { get; set; } = false;

    /// <summary>Gets or sets whether the grid applies narrow-screen overflow constraints.</summary>
    [Parameter]
    public bool Responsive { get; set; } = false;

    /// <summary>Gets or sets the initial width of a resizable grid container in pixels.</summary>
    [Parameter]
    public int? ContainerWidth { get; set; } = null;

    /// <summary>Gets or sets a function that supplies a CSS class for a row.</summary>
    [Parameter]
    public Func<TItem, int, string?>? RowClass { get; set; } = null;

    /// <summary>Gets or sets a function that supplies inline CSS for a row.</summary>
    [Parameter]
    public Func<TItem, int, string?>? RowStyle { get; set; } = null;

    /// <summary>Gets or sets a function that supplies a CSS class for a cell.</summary>
    [Parameter]
    public Func<TItem, string, int, string?>? CellClass { get; set; } = null;

    /// <summary>Gets or sets a function that supplies inline CSS for a cell.</summary>
    [Parameter]
    public Func<TItem, string, int, string?>? CellStyle { get; set; } = null;

    /// <summary>Gets or sets a function that supplies a CSS class for a column heading.</summary>
    [Parameter]
    public Func<string, string?>? HeaderClass { get; set; } = null;

    /// <summary>Gets or sets a function that supplies inline CSS for a column heading.</summary>
    [Parameter]
    public Func<string, string?>? HeaderStyle { get; set; } = null;

    /// <summary>Gets or sets a function that supplies a CSS class for the aggregate footer.</summary>
    [Parameter]
    public Func<string?>? FooterClass { get; set; } = null;

    /// <summary>Gets or sets a function that supplies inline CSS for the aggregate footer.</summary>
    [Parameter]
    public Func<string?>? FooterStyle { get; set; } = null;

    /// <summary>Gets or sets whether rows are rendered through the framework virtualizer.</summary>
    [Parameter]
    public bool VirtualizeRows { get; set; } = false;

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
    public Func<VekDataGridState<TItem>, int, int, CancellationToken, Task<VekDataGridData<TItem>>>? ServerVirtualData { get; set; } = null;

    /// <summary>Gets whether a server request is currently running.</summary>
    public bool IsLoading => _serverLoading;

    /// <summary>Gets the total number of matching items before paging.</summary>
    public int TotalItems => ServerData is null ? GetSortedItems().Count : _serverTotalItems;

    private IReadOnlyList<IVekDataGridColumn<TItem, RenderFragment>> VisibleColumns => _columns.Where(column => column.Visible).ToArray();
    private IEnumerable<IVekDataGridColumn<TItem, RenderFragment>> SearchableColumns => string.IsNullOrWhiteSpace(_columnSearch)
        ? _columns
        : _columns.Where(column => column.Title.Contains(_columnSearch.Trim(), StringComparison.OrdinalIgnoreCase));
    private int VisibleColumnCount => Math.Max(VisibleColumns.Count, 1) + (RowActionsTemplate is null ? 0 : 1);
    private bool CanReorderInPanel => ColumnReorderMode is VekDataGridColumnReorderMode.Panel or VekDataGridColumnReorderMode.Both;
    private bool CanReorderInHeader => ColumnReorderMode is VekDataGridColumnReorderMode.Header or VekDataGridColumnReorderMode.Both;
    private int? CurrentContainerWidth => _containerWidth ?? ContainerWidth;
    private int? _containerWidth;

    /// <summary>Updates native column order and notifies bound consumers after shared column changes.</summary>
    public async ValueTask AfterColumnChangeAsync(bool orderChanged)
    {
        await InvokeAsync(async () =>
        {
            _columns.Clear();
            _columns.AddRange(_engine.Columns);
            if (orderChanged) await ColumnReordered.InvokeAsync(_columns.Select(column => column.Key).ToArray());
            StateHasChanged();
        });
    }

    /// <summary>Synchronizes paging parameters and loads the requested native result projection.</summary>
    public async ValueTask AfterPageChangeAsync(bool pageSizeChanged)
    {
        await InvokeAsync(async () =>
        {
            if (pageSizeChanged)
            {
                PageSize = _engine.PageSize;
                await PageSizeChanged.InvokeAsync(PageSize);
            }
            await CurrentPageChanged.InvokeAsync(_engine.Page);
            await ReloadAfterStateChangeAsync();
        });
    }

    private Task ResizeContainerAsync(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var width))
            _containerWidth = Math.Clamp(width, 320, 1440);
        return InvokeAsync(StateHasChanged);
    }

    private Task ResizeColumnFromInputAsync(string columnKey, ChangeEventArgs args)
        => int.TryParse(args.Value?.ToString(), out var width)
            ? this.ResizeColumnAsync(columnKey, width)
            : Task.CompletedTask;

    private int GetColumnWidthValue(IVekDataGridColumn<TItem, RenderFragment> column) => column.Width ?? 160;

    private string? GetColumnWidthStyle(IVekDataGridColumn<TItem, RenderFragment> column)
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

    private string? GetCellStyle(TItem item, IVekDataGridColumn<TItem, RenderFragment> column, int index)
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
            : ServerData is not null ? ((_engine.Page - 1) * EffectivePageSize) + itemIndex : itemIndex;
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
        => string.Join(' ', new[] { this.IsItemSelected(item) ? "is-selected" : null, RowClass?.Invoke(item, index) }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private VekDataGridRowContext<TItem> CreateRowActionsContext(TItem item, int index) => new(
        item,
        index,
        this.IsItemSelected(item),
        () => this.SelectItemAsync(item),
        () => this.ToggleRowDetailsAsync(item),
        () => this.EditItemAsync(item));

    private VekDataGridCellContext<TItem> CreateCellActionsContext(
        TItem item,
        IVekDataGridColumn<TItem, RenderFragment> column,
        int rowIndex) => new(
            item,
            column.Key,
            column.Title,
            rowIndex,
            () => this.EditCellAsync(item, column.Key));

    private void ToggleColumnPanel() => _columnPanelOpen = !_columnPanelOpen;

    private Task MoveColumnInPanelAsync(string key, int offset)
    {
        if (!CanReorderInPanel) return Task.CompletedTask;
        var index = _columns.FindIndex(column => column.Key == key);
        return this.ReorderColumnAsync(key, index + offset);
    }

    private Task MoveColumnFromHeaderByKeyAsync(string key, KeyboardEventArgs args)
    {
        if (!CanReorderInHeader || args.Key is not "ArrowLeft" and not "ArrowRight") return Task.CompletedTask;
        var index = _columns.FindIndex(column => column.Key == key);
        return this.ReorderColumnAsync(key, index + (args.Key == "ArrowLeft" ? -1 : 1));
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
            await this.ReorderColumnAsync(_draggedColumnKey!, targetIndex);
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
    public int PageSize { get; set; } = 0;

    /// <summary>Gets or sets the current one-based page for two-way binding.</summary>
    [Parameter]
    public int CurrentPage
    {
        get => _engine.Page;
        set => _engine.Page = Math.Max(value, 1);
    }

    /// <summary>Gets or sets the available choices in the rows-per-page selector.</summary>
    [Parameter]
    public IReadOnlyList<int> PageSizeOptions { get; set; } = new[] { 10, 25, 50 };

    /// <summary>Gets or sets a callback invoked when the current page changes.</summary>
    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; } = default;

    /// <summary>Gets or sets a callback invoked when the page size changes.</summary>
    [Parameter]
    public EventCallback<int> PageSizeChanged { get; set; } = default;

    /// <summary>Gets or sets optional content used in place of the built-in pager.</summary>
    [Parameter]
    public RenderFragment<VekDataGridPagerContext<TItem>>? PagerContent { get; set; } = null;

    /// <summary>Gets or sets the row selection mode.</summary>
    [Parameter]
    public VekDataGridSelectionMode SelectionMode { get; set; } = VekDataGridSelectionMode.None;

    /// <summary>Gets or sets the selected item in single-selection mode.</summary>
    [Parameter]
    public TItem? SelectedItem { get; set; } = default;

    /// <summary>Gets or sets the callback invoked when the selected item changes.</summary>
    [Parameter]
    public EventCallback<TItem?> SelectedItemChanged { get; set; } = default;

    /// <summary>Gets or sets the selected items in multiple-selection mode.</summary>
    [Parameter]
    public IReadOnlyList<TItem> SelectedItems { get; set; } = Array.Empty<TItem>();

    /// <summary>Gets or sets the callback invoked when the selected item collection changes.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> SelectedItemsChanged { get; set; } = default;

    /// <summary>Gets or sets whether clicking a row toggles its selection.</summary>
    [Parameter]
    public bool RowClickSelection { get; set; } = false;

    /// <summary>Gets or sets a predicate that disables selection for matching rows.</summary>
    [Parameter]
    public Func<TItem, bool>? IsRowSelectable { get; set; } = null;

    /// <summary>Gets or sets the comparer used to identify selected rows across data refreshes.</summary>
    [Parameter]
    public IEqualityComparer<TItem>? SelectionComparer { get; set; } = null;

    /// <summary>Gets or sets the stable key selector used by the shared Core selection state.</summary>
    [Parameter]
    public Func<TItem, string>? ItemKeySelector { get; set; } = null;

    /// <summary>Gets or sets the policy used when filters hide selected rows.</summary>
    [Parameter]
    public VekDataGridFilteredSelectionMode FilteredSelectionMode { get; set; } = VekDataGridFilteredSelectionMode.Keep;

    /// <summary>Gets or sets the grid's editing mode.</summary>
    [Parameter]
    public VekDataGridEditMode EditMode { get; set; } = VekDataGridEditMode.None;

    /// <summary>Gets or sets a callback invoked after a draft is created.</summary>
    [Parameter]
    public EventCallback<TItem> EditStarted { get; set; } = default;

    /// <summary>Gets or sets a strategy that creates an isolated draft item.</summary>
    [Parameter]
    public Func<TItem, TItem>? CloneItem { get; set; } = null;

    /// <summary>Gets or sets a callback that validates and accepts a draft. Return false to keep editing.</summary>
    [Parameter]
    public Func<TItem, Task<bool>>? CommitEdit { get; set; } = null;

    /// <summary>Gets or sets a callback invoked after a draft is committed.</summary>
    [Parameter]
    public EventCallback<TItem> ItemCommitted { get; set; } = default;

    /// <summary>Gets or sets a callback invoked after an edit draft is canceled.</summary>
    [Parameter]
    public EventCallback<TItem> EditCancelled { get; set; } = default;

    /// <summary>Gets or sets the ordered grouping columns.</summary>
    [Parameter]
    public IReadOnlyList<VekDataGridGroupDefinition> GroupBy { get; set; } = Array.Empty<VekDataGridGroupDefinition>();

    /// <summary>Gets or sets a callback invoked when a group is expanded or collapsed.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<VekDataGridGroupDefinition>> GroupChanged { get; set; } = default;

    /// <summary>Gets or sets the calculations displayed in the grid footer and group headers.</summary>
    [Parameter]
    public IReadOnlyList<VekDataGridAggregateDefinition<TItem>> Aggregates { get; set; } = Array.Empty<VekDataGridAggregateDefinition<TItem>>();

    /// <summary>Gets or sets custom content for the aggregate footer.</summary>
    [Parameter]
    public RenderFragment<VekDataGridFooterContext<TItem>>? FooterTemplate { get; set; } = null;

    /// <summary>Gets or sets a predicate that marks rows initially expanded.</summary>
    [Parameter]
    public Func<TItem, bool>? InitiallyExpanded { get; set; } = null;

    /// <summary>Gets or sets whether only one row can be expanded at a time.</summary>
    [Parameter]
    public bool SingleExpandedRow { get; set; } = false;

    /// <summary>Gets or sets the comparer used to preserve expanded rows across refreshes.</summary>
    [Parameter]
    public IEqualityComparer<TItem>? RowDetailsComparer { get; set; } = null;

    /// <summary>Gets or sets a callback invoked when the expanded row set changes.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<TItem>> RowDetailsExpandedChanged { get; set; } = default;

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
    public Func<TItem, bool>? QuickFilter { get; set; } = null;

    /// <summary>Gets or sets the text matched across the configured searchable columns.</summary>
    [Parameter]
    public string GlobalSearchText { get; set; } = string.Empty;

    /// <summary>Gets or sets explicit column keys searched instead of columns marked searchable.</summary>
    [Parameter]
    public IReadOnlyList<string>? SearchColumnKeys { get; set; }

    /// <summary>Gets or sets a callback invoked when the global search text changes.</summary>
    [Parameter]
    public EventCallback<string> GlobalSearchTextChanged { get; set; } = default;

    private async Task OnGlobalSearchInputAsync(ChangeEventArgs args)
    {
        GlobalSearchText = args.Value?.ToString() ?? string.Empty;
        _engine.SetGlobalSearch(GlobalSearchText, SearchColumnKeys);
        await GlobalSearchTextChanged.InvokeAsync(GlobalSearchText);
        if (ServerData is not null)
            await ReloadAfterStateChangeAsync();
        else
            await InvokeAsync(StateHasChanged);
    }

    /// <summary>Gets or sets where the grid displays column filter controls.</summary>
    [Parameter]
    public VekDataGridFilterMode FilterMode { get; set; } = VekDataGridFilterMode.Simple;

    /// <summary>Gets or sets whether the grid displays a filter row below its column headings.</summary>
    [Parameter]
    public bool ShowFilterRow { get; set; } = false;

    /// <summary>Gets or sets whether string comparisons distinguish letter case.</summary>
    [Parameter]
    public bool FilterCaseSensitive { get => _engine.FilterCaseSensitive; set => _engine.FilterCaseSensitive = value; }

    /// <summary>Gets or sets a callback invoked after filters are applied or cleared.</summary>
    [Parameter]
    public EventCallback<IReadOnlyList<VekDataGridFilter>> FilterChanged { get; set; } = default;

    /// <summary>
    /// Gets or sets a callback invoked after the active sort definitions change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<VekDataGridSort>> SortChanged { get; set; } = default;

    /// <summary>
    /// Gets the current page, sort, and filter state of this grid.
    /// </summary>
    public VekDataGridState<TItem> CurrentState
    {
        get
        {
            SyncCoreState();
            var request = _engine.CreateServerRequest();
            return new VekDataGridState<TItem>(request.Page, request.PageSize, request.Sorts, request.Filters,
                request.GlobalSearchText, request.SearchColumnKeys);
        }
    }

    /// <summary>Notifies filter bindings and refreshes results after a shared filtering operation.</summary>
    public async ValueTask AfterCoreChangeAsync()
    {
        await InvokeAsync(async () =>
        {
            await FilterChanged.InvokeAsync(_engine.Filters.ToArray());
            await ReloadAfterStateChangeAsync();
        });
    }

    /// <summary>Notifies sort bindings and refreshes results after a shared sorting change.</summary>
    public async ValueTask AfterSortChangeAsync()
    {
        await InvokeAsync(async () =>
        {
            await SortChanged.InvokeAsync(_engine.Sorts.ToArray());
            await ReloadAfterStateChangeAsync();
        });
    }

    /// <summary>Reloads the current data source or rerenders local items.</summary>
    public Task ReloadAsync() => ReloadAfterStateChangeAsync();

    internal string? GetFilterValue(string key) => _engine.Filters.FirstOrDefault(filter => filter.ColumnKey == key)?.Value?.ToString();

    internal Task ApplyTextFilter(IVekDataGridColumn<TItem, RenderFragment> column, string? value) =>
        this.ApplyFilterAsync(column.Key, column.FilterOperator, value).AsTask();

    internal Task ClearColumnFilter(IVekDataGridColumn<TItem, RenderFragment> column) => this.RemoveFilterAsync(column.Key).AsTask();

    /// <summary>Publishes the Core selection projection through native binding callbacks.</summary>
    public async ValueTask AfterSelectionChangeAsync()
    {
        await InvokeAsync(async () =>
        {
            SelectedItems = _engine.Interaction.SelectedItems;
            SelectedItem = _engine.Interaction.SelectedItem;
            await SelectedItemsChanged.InvokeAsync(SelectedItems);
            await SelectedItemChanged.InvokeAsync(SelectedItem);
            StateHasChanged();
        });
    }

    /// <summary>Publishes the Core row-detail projection through native binding callbacks.</summary>
    public async ValueTask AfterRowDetailsChangeAsync()
    {
        await InvokeAsync(async () =>
        {
            await RowDetailsExpandedChanged.InvokeAsync(_expandedRows.ToArray());
            StateHasChanged();
        });
    }

    private IEqualityComparer<TItem> ItemComparer => SelectionComparer ?? EqualityComparer<TItem>.Default;

    private void SyncCoreSelection()
    {
        _engine.SetItemKeySelector(ItemKeySelector);
        _engine.SetSelectedKeys(SelectedItems.Select(_engine.GetItemKey));
    }

    /// <summary>Gets whether the configured shared editing policy permits a visible editable row.</summary>
    public bool CanEditRow => this.CanEditRows();

    /// <summary>Gets whether the requested item and column are being edited.</summary>
    private bool IsEditing(TItem item, IVekDataGridColumn<TItem, RenderFragment> column) => _isEditing
        && ItemComparer.Equals(_editingOriginal!, item)
        && (EditMode != VekDataGridEditMode.Cell || ReferenceEquals(_editingColumn, column));

    /// <summary>Gets the active draft template context when the requested cell is being edited.</summary>
    private Vek.Core.DataGrid.VekDataGridEditTemplateContext<TItem>? GetEditContext(
        TItem item,
        IVekDataGridColumn<TItem, RenderFragment> column) =>
        IsEditing(item, column)
            ? new Vek.Core.DataGrid.VekDataGridEditTemplateContext<TItem>(
                _editingDraft!,
                Culture ?? CultureInfo.CurrentCulture,
                item => this.UpdateEditDraft(item),
                this.CommitEditAsync,
                this.CancelEditAsync)
            : null;

    internal VekDataGridEditContext<TItem> CreateBlazorEditContext(
        Vek.Core.DataGrid.VekDataGridEditTemplateContext<TItem> context) => new(
            context.Item,
            _editContext ?? new EditContext(context.Item!),
            context.UpdateItem,
            context.CommitAsync,
            context.CancelAsync,
            context.Culture);

    /// <summary>Creates native validation state for the shared edit draft.</summary>
    public async ValueTask AfterEditStartedAsync(TItem item)
    {
        await InvokeAsync(async () =>
        {
            RebuildEditContext();
            await EditStarted.InvokeAsync(item);
            StateHasChanged();
        });
    }

    /// <summary>Validates a draft through the native form context.</summary>
    public async ValueTask<bool> ValidateEditDraftAsync(TItem item)
    {
        var valid = false;
        await InvokeAsync(() => valid = _editContext?.Validate() == true);
        return valid;
    }

    /// <summary>Invokes the configured persistence callback for the shared draft.</summary>
    public async ValueTask<bool> CommitEditDraftAsync(TItem original, TItem draft)
    {
        var accepted = true;
        if (CommitEdit is not null) await InvokeAsync(async () => accepted = await CommitEdit(draft));
        return accepted;
    }

    /// <summary>Notifies native bindings after a completed shared commit.</summary>
    public async ValueTask AfterEditCommittedAsync(TItem item)
    {
        await InvokeAsync(async () =>
        {
            await ItemCommitted.InvokeAsync(item);
            _editContext = null;
            _activeEditContext = null;
            StateHasChanged();
        });
    }

    /// <summary>Notifies native bindings after shared cancellation.</summary>
    public async ValueTask AfterEditCancelledAsync(TItem item)
    {
        await InvokeAsync(async () =>
        {
            await EditCancelled.InvokeAsync(item);
            _editContext = null;
            _activeEditContext = null;
            StateHasChanged();
        });
    }

    /// <summary>Updates native validation state when the shared draft is replaced.</summary>
    public void AfterEditDraftChanged()
    {
        _ = InvokeAsync(() =>
        {
            RebuildEditContext();
            StateHasChanged();
        });
    }

    private void RebuildEditContext()
    {
        _editContext = new EditContext(_editingDraft!);
        _activeEditContext = new VekDataGridEditContext<TItem>(
            _editingDraft!, _editContext, item => this.UpdateEditDraft(item),
            () => this.CommitEditAsync(), () => this.CancelEditAsync(), Culture ?? CultureInfo.CurrentCulture);
    }

    internal string GetRowLabel(TItem item) => VisibleColumns.FirstOrDefault(column => column.Editable)?.GetRowLabel(item)
        ?? VisibleColumns.FirstOrDefault(column => !column.IsHierarchyColumn && !column.IsSelectionColumn)?.GetRowLabel(item)
        ?? Convert.ToString(item, Culture ?? CultureInfo.CurrentCulture)
        ?? "row";

    private IEqualityComparer<TItem> DetailsComparer => RowDetailsComparer ?? SelectionComparer ?? EqualityComparer<TItem>.Default;

    private VekDataGridRowDetailContext<TItem> CreateRowDetailContext(TItem item) => new(
        item,
        this.IsRowExpanded(item),
        () => this.ToggleRowDetailsAsync(item));

    private Vek.Core.DataGrid.VekDataGridRowDetailContext<TItem> CreateCoreRowDetailContext(TItem item) => new(
        item,
        this.IsRowExpanded(item),
        () => this.ToggleRowDetailsAsync(item));

    private RenderFragment RenderRowDetails(TItem item) => builder =>
    {
        var detailTemplate = VisibleColumns.FirstOrDefault(column => column.IsHierarchyColumn)?.DetailTemplate;
        if (detailTemplate is null || !this.IsRowExpanded(item)) return;
        builder.OpenElement(0, "tr");
        builder.AddAttribute(1, "class", "detail-row");
        builder.AddAttribute(2, "data-for-row", GetRowLabel(item));
        builder.OpenElement(3, "td");
        builder.AddAttribute(4, "colspan", VisibleColumnCount);
        builder.AddContent(5, detailTemplate(CreateCoreRowDetailContext(item)));
        builder.CloseElement();
        builder.CloseElement();
    };

    private List<TItem> GetPageItems() => ServerVirtualData is not null
        ? _virtualServerItems.ToList()
        : ServerData is not null
            ? _serverItems.ToList()
            : GetCorePageItems();

    private List<TItem> GetCorePageItems()
    {
        SyncCoreState();
        return _engine.GetLocalData(Items.Where(item => QuickFilter?.Invoke(item) ?? true)).Items.ToList();
    }

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
        builder.AddAttribute(sequence++, "aria-selected", this.IsItemSelected(item));
        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => this.SelectRowAsync(item)));

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
                        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => this.EditCellAsync(item, column.Key)));
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
                        builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => this.EditItemAsync(item)));
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

    private IReadOnlyList<(VekDataGridGroupDefinition Definition, IVekDataGridColumn<TItem, RenderFragment> Column, Func<TItem, object?> Selector)> EffectiveGroups => GroupBy
        .Select(definition => (Definition: definition, Column: _columns.FirstOrDefault(column => column.Key == definition.ColumnKey)))
        .Where(pair => pair.Column?.Groupable == true && pair.Column.GroupBy is not null)
        .Select(pair => (pair.Definition, pair.Column!, pair.Column!.GroupBy!))
        .ToArray();

    private List<GroupNode> BuildGroupNodes(IEnumerable<TItem> items)
    {
        var definitions = EffectiveGroups;
        if (definitions.Count == 0)
        {
            _engine.SetKnownGroupKeys(Array.Empty<string>());
            return [];
        }
        SyncCoreState();
        _engine.SetGroups(definitions.Select(group => group.Definition));
        var groups = _engine.ComputeGroups(items)
            .Select(group => CreateGroupNode(group, definitions, 0))
            .ToList();
        _engine.SetKnownGroupKeys(EnumerateGroupKeys(groups));
        return groups;
    }

    private static IEnumerable<string> EnumerateGroupKeys(IEnumerable<GroupNode> groups) => groups
        .SelectMany(group => new[] { group.Path }.Concat(EnumerateGroupKeys(group.Children)));

    private GroupNode CreateGroupNode(
        Vek.Core.DataGrid.VekDataGridGroup<TItem> group,
        IReadOnlyList<(VekDataGridGroupDefinition Definition, IVekDataGridColumn<TItem, RenderFragment> Column, Func<TItem, object?> Selector)> definitions,
        int level)
    {
        var (definition, column, _) = definitions[level];
        var path = group.Key;
        var children = (group.Children ?? Array.Empty<Vek.Core.DataGrid.VekDataGridGroup<TItem>>())
            .Select(child => CreateGroupNode(child, definitions, level + 1))
            .ToArray();
        return new GroupNode(definition, column, group.Value, group.Items, level, path, children);
    }

    private bool IsGroupExpanded(string path) => _engine.IsGroupExpanded(path);

    /// <summary>Notifies native templates of a completed shared group-expansion transition.</summary>
    public async ValueTask AfterGroupChangeAsync()
    {
        await InvokeAsync(async () =>
        {
            await GroupChanged.InvokeAsync(GroupBy);
            StateHasChanged();
        });
    }

    private VekDataGridGroupContext<TItem> CreateGroupContext(GroupNode node) => new(
        node.Column.Key,
        node.Column.Title,
        node.Value,
        node.Level,
        node.Items,
        IsGroupExpanded(node.Path),
        () => this.ToggleGroupAsync(node.Path),
        ComputeAggregates(node.Items, VekDataGridAggregateScope.Group));

    private Vek.Core.DataGrid.VekDataGridGroupContext<TItem> CreateCoreGroupContext(GroupNode node) => new(
        node.Column.Key,
        node.Column.Title,
        node.Value,
        node.Level,
        node.Items,
        IsGroupExpanded(node.Path),
        () => this.ToggleGroupAsync(node.Path),
        ComputeAggregates(node.Items, VekDataGridAggregateScope.Group));

    private VekDataGridFooterContext<TItem> CreateFooterContext()
    {
        var items = GetSortedItems();
        return new VekDataGridFooterContext<TItem>(items, ComputeAggregates(items, VekDataGridAggregateScope.Footer));
    }

    private IReadOnlyList<VekDataGridAggregateResult> ComputeAggregates(
        IReadOnlyList<TItem> items,
        VekDataGridAggregateScope scope)
    {
        SyncCoreState();
        return _engine.ComputeAggregates(items, scope);
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
                builder.AddContent(sequence++, group.Column.GroupTemplate(CreateCoreGroupContext(group)));
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
                builder.AddContent(sequence++, group.Children.Count > 0 ? RenderGroupRows(group.Children) : RenderDataRows(group.Items));
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
            builder.AddAttribute(sequence++, "aria-selected", this.IsItemSelected(item));
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => this.SelectRowAsync(item)));
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
                            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => this.EditCellAsync(item, column.Key)));
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
                            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, () => this.EditItemAsync(item)));
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
    public string? Class { get; set; } = null;

    /// <summary>
    /// Gets or sets the property columns displayed by the grid.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; } = null;

    private int EffectivePageSize => PageSize > 0
        ? PageSize
        : ServerData is not null
            ? 10
            : Math.Max(Items.Count(), 1);

    /// <summary>Clamps the current page when incoming data or parameters reduce the page count.</summary>
    protected override async Task OnParametersSetAsync()
    {
        _engine.SetItemKeySelector(ItemKeySelector);

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
        if (_engine.Page > lastPage)
        {
            _engine.Page = lastPage;
            await CurrentPageChanged.InvokeAsync(_engine.Page);
        }

        await this.ReconcileSelectionAsync();

        var rows = GetCurrentItems().ToList();
        if (!_initialExpansionApplied)
        {
            _engine.Interaction.SetExpandedItems(InitiallyExpanded is null ? [] : rows.Where(InitiallyExpanded));
            _initialExpansionApplied = true;
        }
        else
        {
            _engine.Interaction.ReconcileExpandedItems(rows);
        }
        _engine.Interaction.SetExpandedItems(_expandedRows);
    }

    internal void RegisterColumn(IVekDataGridColumn<TItem, RenderFragment> column)
    {
        if (_columns.Contains(column))
        {
            return;
        }

        _columns.Add(column);
        RegisterCoreColumn(column);
        _ = InvokeAsync(StateHasChanged);
    }

    private void RegisterCoreColumn(IVekDataGridColumn<TItem, RenderFragment> column)
    {
        _engine.AddOrUpdateColumn(column);
    }

    internal void UnregisterColumn(IVekDataGridColumn<TItem, RenderFragment> column)
    {
        if (_columns.Remove(column))
        {
            _engine.RemoveColumn(column.Key);
            _ = InvokeAsync(StateHasChanged);
        }
    }

    internal void RefreshColumn(IVekDataGridColumn<TItem, RenderFragment> column)
    {
        if (_columns.Contains(column))
        {
            RegisterCoreColumn(column);
            _ = InvokeAsync(StateHasChanged);
        }
    }

    private List<TItem> GetSortedItems()
    {
        if (ServerData is not null) return _serverItems.ToList();
        SyncCoreState();
        return _engine.GetFilteredAndSortedItems(Items.Where(item => QuickFilter?.Invoke(item) ?? true)).ToList();
    }

    private void SyncCoreState()
    {
        foreach (var column in _columns) RegisterCoreColumn(column);
        _engine.SetPage(_engine.Page, EffectivePageSize);
        _engine.SetGlobalSearch(GlobalSearchText, SearchColumnKeys);
        _engine.Culture = Culture ?? CultureInfo.CurrentCulture;
        _engine.SetGroups(GroupBy);
        _engine.SetAggregates(Aggregates.Select(definition =>
            (Vek.Core.DataGrid.VekDataGridAggregateDefinition<TItem>)definition));
        SyncCoreSelection();
    }

    private int GetTotalPages(int itemCount) => _engine.GetPageCount(itemCount, EffectivePageSize);

    private async Task ChangePageSizeAsync(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var pageSize) && pageSize > 0)
            await this.SetPageSizeAsync(pageSize);
    }

    private VekDataGridPagerContext<TItem> CreatePagerContext(int totalItems, int totalPages) => new(
        CurrentState,
        totalItems,
        totalPages,
        this.SetCurrentPageAsync,
        this.SetPageSizeAsync);

    private string GetAriaSort(IVekDataGridColumn<TItem, RenderFragment> column)
    {
        var activeSort = _engine.Sorts.FirstOrDefault(sort => string.Equals(sort.ColumnKey, column.Key, StringComparison.Ordinal));
        if (activeSort is null)
        {
            return "none";
        }

        return activeSort.Direction == VekSortDirection.Ascending ? "ascending" : "descending";
    }

    private int? GetSortPriority(IVekDataGridColumn<TItem, RenderFragment> column)
    {
        var sort = _engine.Sorts.FirstOrDefault(sort => string.Equals(sort.ColumnKey, column.Key, StringComparison.Ordinal));
        return sort is null ? null : _engine.Sorts.ToList().IndexOf(sort) + 1;
    }

    private string GetSortIndicator(IVekDataGridColumn<TItem, RenderFragment> column)
    {
        var sort = _engine.Sorts.First(activeSort => string.Equals(activeSort.ColumnKey, column.Key, StringComparison.Ordinal));
        var arrow = sort.Direction == VekSortDirection.Ascending ? "\u2191" : "\u2193";
        return SortMode == VekDataGridSortMode.Multiple
            ? $"{arrow}{GetSortPriority(column)}"
            : arrow;
    }

    private async Task SortBy(IVekDataGridColumn<TItem, RenderFragment> column, MouseEventArgs args)
    {
        await this.SortAsync(
            column.Key,
            SortMode,
            AllowUnsorted,
            removeSort: args.AltKey,
            preserveExistingSorts: args.CtrlKey);
    }

}
