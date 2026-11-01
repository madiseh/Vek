namespace Vek.Core.DataGrid;

/// <summary>
/// Represents a platform-neutral server load result.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
/// <param name="Items">Items for the requested page.</param>
/// <param name="TotalItems">Total matching items across all pages.</param>
public sealed record VekDataGridServerResult<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalItems);

