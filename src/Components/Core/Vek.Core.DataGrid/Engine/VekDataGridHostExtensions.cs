namespace Vek.Core.DataGrid;

/// <summary>Implements shared filtering workflows for grid hosts.</summary>
/// <remarks>
/// A successful mutation precedes the awaited host continuation. A failed continuation does not
/// roll back or retry that mutation. These operations have no knowledge of rendering or native controls.
/// </remarks>
public static class VekDataGridHostExtensions
{
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
