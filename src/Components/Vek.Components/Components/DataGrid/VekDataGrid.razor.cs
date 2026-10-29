using Microsoft.AspNetCore.Components;

namespace Vek.Components.Components.DataGrid;

/// <summary>
/// Displays a collection of items in a table with client-side sorting and pagination.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
[CascadingTypeParameter(nameof(TItem))]
public partial class VekDataGrid<TItem>
{
    private readonly List<IVekDataGridColumn<TItem>> _columns = [];
    private IVekDataGridColumn<TItem>? _sortColumn;
    private bool _sortAscending = true;
    private int _currentPage = 1;

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
        if (_sortColumn is null)
        {
            return items;
        }

        var comparer = Comparer<TItem>.Create(_sortColumn.CompareItems);
        return _sortAscending
            ? items.OrderBy(item => item, comparer).ToList()
            : items.OrderByDescending(item => item, comparer).ToList();
    }

    private int GetTotalPages(int itemCount) => itemCount == 0
        ? 1
        : (itemCount - 1) / EffectivePageSize + 1;

    private string GetAriaSort(IVekDataGridColumn<TItem> column)
    {
        if (!ReferenceEquals(_sortColumn, column))
        {
            return "none";
        }

        return _sortAscending ? "ascending" : "descending";
    }

    private void SortBy(IVekDataGridColumn<TItem> column)
    {
        if (ReferenceEquals(_sortColumn, column))
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }

        _currentPage = 1;
    }

    private void PreviousPage() => _currentPage = Math.Max(1, _currentPage - 1);

    private void NextPage() => _currentPage = Math.Min(GetTotalPages(GetSortedItems().Count), _currentPage + 1);
}
