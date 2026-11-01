namespace Vek.Core.DataGrid;

/// <summary>
/// Contains one page of items and the total number of matching items.
/// </summary>
/// <typeparam name="TItem">The type of item represented by each row.</typeparam>
/// <param name="Items">The items displayed by the current page.</param>
/// <param name="TotalItems">The total count before paging.</param>
public sealed record VekDataGridData<TItem>(IReadOnlyList<TItem> Items, int TotalItems);

