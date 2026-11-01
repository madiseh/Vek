namespace Vek.Core.DataGrid;

/// <summary>
/// Describes one level in the grid's nested grouping order.
/// </summary>
/// <param name="ColumnKey">The stable key of the column used to group rows.</param>
/// <param name="SortOrder">The order in which distinct group values appear.</param>
public sealed record VekDataGridGroupDefinition(
    string ColumnKey,
    VekDataGridGroupSortOrder SortOrder = VekDataGridGroupSortOrder.Ascending);

