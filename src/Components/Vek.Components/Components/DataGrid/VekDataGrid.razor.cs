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
        Array.Empty<VekDataGridFilter>());

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
        var items = Items?.ToList() ?? [];
        if (_sorts.Count == 0)
        {
            return items;
        }

        var comparer = Comparer<TItem>.Create(CompareByActiveSorts);
        return items.OrderBy(item => item, comparer).ToList();
    }

    private int GetTotalPages(int itemCount) => itemCount == 0
        ? 1
        : (itemCount - 1) / EffectivePageSize + 1;

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

    private void PreviousPage() => _currentPage = Math.Max(1, _currentPage - 1);

    private void NextPage() => _currentPage = Math.Min(GetTotalPages(GetSortedItems().Count), _currentPage + 1);
}
