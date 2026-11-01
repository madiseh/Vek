namespace Vek.Core.DataGrid;

/// <summary>Implements shared filtering and sorting workflows for grid hosts.</summary>
/// <remarks>
/// A successful mutation precedes the awaited host continuation. A failed continuation does not
/// roll back or retry that mutation. These operations have no knowledge of rendering or native controls.
/// </remarks>
public static class VekDataGridHostExtensions
{
    /// <summary>Applies a sort interaction, returns to the first page, and awaits the optional sorting continuation.</summary>
    /// <typeparam name="TItem">The row item type.</typeparam>
    /// <typeparam name="TTemplate">An opaque template value supplied by the host.</typeparam>
    /// <param name="grid">The host supplying shared state and any sorting continuation.</param>
    /// <param name="columnKey">The stable column key.</param>
    /// <param name="mode">The sorting policy.</param>
    /// <param name="allowUnsorted">Whether cycling past descending removes the sort.</param>
    /// <param name="removeSort">Whether the interaction explicitly removes a sort in multiple-sort mode.</param>
    /// <param name="preserveExistingSorts">Whether adding a sort retains other active sorts.</param>
    /// <returns>Completion of the accepted interaction and its host continuation; rejected interactions leave state unchanged.</returns>
    public static async ValueTask SortAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid,
        string columnKey,
        VekDataGridSortMode mode,
        bool allowUnsorted = false,
        bool removeSort = false,
        bool preserveExistingSorts = false)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnKey);
        if (!grid.Engine.ApplySortInteraction(columnKey, mode, allowUnsorted, removeSort, preserveExistingSorts))
            return;
        grid.Engine.Page = 1;
        await grid.AfterSortChangeAsync();
    }

    /// <summary>Clears active sorts, preserves other query state, and awaits the optional sorting continuation.</summary>
    /// <typeparam name="TItem">The row item type.</typeparam>
    /// <typeparam name="TTemplate">An opaque template value supplied by the host.</typeparam>
    /// <param name="grid">The host supplying shared state and any sorting continuation.</param>
    /// <returns>Completion of the sorting change and its optional host continuation.</returns>
    public static async ValueTask ClearSortAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.SetSorts([]);
        await grid.AfterSortChangeAsync();
    }

    /// <summary>Applies a column filter, resets paging, and awaits host-owned follow-up work.</summary>
    /// <typeparam name="TItem">The row item type.</typeparam>
    /// <typeparam name="TTemplate">An opaque template value supplied by the host.</typeparam>
    /// <param name="grid">The host supplying shared state and the continuation.</param>
    /// <param name="columnKey">The stable column key.</param>
    /// <param name="op">The filter comparison.</param>
    /// <param name="value">The comparison value.</param>
    /// <param name="caseSensitive">Overrides the engine's default string comparison sensitivity.</param>
    /// <returns>Completion of the filtering operation and its host continuation.</returns>
    public static async ValueTask ApplyFilterAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid,
        string columnKey,
        VekDataGridFilterOperator op,
        object? value,
        bool? caseSensitive = null)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnKey);
        grid.Engine.ApplyFilter(columnKey, op, value, caseSensitive ?? grid.Engine.FilterCaseSensitive);
        await grid.AfterCoreChangeAsync();
    }

    /// <summary>Removes one filter, or every filter when no key is supplied, and awaits host-owned follow-up work.</summary>
    /// <typeparam name="TItem">The row item type.</typeparam>
    /// <typeparam name="TTemplate">An opaque template value supplied by the host.</typeparam>
    /// <param name="grid">The host supplying shared state and the continuation.</param>
    /// <param name="columnKey">The stable column key, or null to remove every filter.</param>
    /// <returns>Completion of the removal operation and its host continuation.</returns>
    public static async ValueTask RemoveFilterAsync<TItem, TTemplate>(
        this IVekDataGridHost<TItem, TTemplate> grid,
        string? columnKey = null)
    {
        ArgumentNullException.ThrowIfNull(grid);
        grid.Engine.RemoveFilter(columnKey);
        await grid.AfterCoreChangeAsync();
    }
}
