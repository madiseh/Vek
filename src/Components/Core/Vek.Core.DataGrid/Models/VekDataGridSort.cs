namespace Vek.Core.DataGrid;

/// <summary>
/// Describes one ordered sort operation in a grid state.
/// </summary>
/// <param name="ColumnKey">The stable key of the sorted column.</param>
/// <param name="Direction">The direction applied to the column.</param>
/// <param name="Priority">The order in which this sort is applied.</param>
public sealed record VekDataGridSort(string ColumnKey, VekSortDirection Direction, int Priority = 0);

