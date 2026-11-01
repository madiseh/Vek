namespace Vek.Core.DataGrid;

/// <summary>Implements shared grid workflows without requiring forwarding methods in each host.</summary>
/// <remarks>Mutations precede optional observers and continuations. Their failures propagate without rollback or retry.</remarks>
public static class VekDataGridHostExtensions
{
    /// <summary>Applies a column filter, resets the page, and awaits any optional query work and continuation.</summary>
    public static async Task ApplyFilterAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid,
        string columnKey, VekDataGridFilterOperator op, object? value,
        bool? caseSensitive = null, Func<ValueTask>? afterApplied = null)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnKey);
        grid.Engine.ApplyFilter(columnKey, op, value, caseSensitive ?? grid.Engine.FilterCaseSensitive);
        await CompleteQueryAsync(grid, VekDataGridQueryChange.Filters | VekDataGridQueryChange.Page);
        if (afterApplied is not null) await afterApplied();
    }

    /// <summary>Removes one filter or all filters, then awaits optional query work and continuation.</summary>
    public static async Task ClearFilterAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid, string? columnKey = null,
        Func<ValueTask>? afterRemoved = null)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.RemoveFilter(columnKey);
        await CompleteQueryAsync(grid, VekDataGridQueryChange.Filters | VekDataGridQueryChange.Page);
        if (afterRemoved is not null) await afterRemoved();
    }

    /// <summary>Removes one filter or all filters using the shared clearing workflow.</summary>
    public static Task RemoveFilterAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid, string? columnKey = null,
        Func<ValueTask>? afterRemoved = null) => grid.ClearFilterAsync(columnKey, afterRemoved);

    /// <summary>Clears every active filter.</summary>
    public static Task ClearFiltersAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid) =>
        grid.ClearFilterAsync();

    /// <summary>Clears active sorting and completes optional query work.</summary>
    public static async Task ClearSortAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.SetSorts([]);
        await CompleteQueryAsync(grid, VekDataGridQueryChange.Sorts);
    }

    /// <summary>Applies the shared sorting interaction rules and resets the page.</summary>
    public static async Task SortAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid, string columnKey,
        VekDataGridSortMode mode, bool allowUnsorted = false,
        bool removeSort = false, bool preserveExistingSorts = false)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.ApplySortInteraction(columnKey, mode, allowUnsorted, removeSort, preserveExistingSorts);
        grid.Engine.Page = 1;
        await CompleteQueryAsync(grid, VekDataGridQueryChange.Sorts | VekDataGridQueryChange.Page);
    }

    /// <summary>Requests a page, optionally clamping it to the known result count.</summary>
    public static async Task SetPageAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid, int page, int? totalItems = null)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.Page = totalItems is int count
            ? Math.Clamp(page, 1, grid.Engine.GetPageCount(count))
            : page;
        await CompleteQueryAsync(grid, VekDataGridQueryChange.Page);
    }

    /// <summary>Changes the page size and starts at the first page.</summary>
    public static async Task SetPageSizeAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        grid.Engine.SetPage(1, pageSize);
        await CompleteQueryAsync(grid, VekDataGridQueryChange.PageSize | VekDataGridQueryChange.Page);
    }

    /// <summary>Changes grouping and completes optional query work.</summary>
    public static async Task SetGroupsAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid, IEnumerable<VekDataGridGroupDefinition> groups)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.SetGroups(groups);
        await CompleteQueryAsync(grid, VekDataGridQueryChange.Groups);
    }

    /// <summary>Restores a shared snapshot and completes optional query work.</summary>
    public static async Task ImportSnapshotAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid, VekDataGridSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.ImportSnapshot(snapshot);
        await CompleteQueryAsync(grid, VekDataGridQueryChange.State);
    }

    private static ValueTask CompleteQueryAsync<TItem, TTemplate>(
        IVekDataGridHost<TItem, TTemplate> grid, VekDataGridQueryChange change) =>
        grid is IVekDataGridQueryObserver observer
            ? observer.OnQueryChangedAsync(change)
            : ValueTask.CompletedTask;
}
