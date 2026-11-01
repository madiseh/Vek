namespace Vek.Core.DataGrid;

/// <summary>
/// A serializable snapshot of the grid's persistent state.
/// </summary>
public sealed record VekDataGridSnapshot(
    int Page,
    int PageSize,
    IReadOnlyList<VekDataGridSort> Sorts,
    IReadOnlyList<VekDataGridFilter> Filters,
    IReadOnlyList<VekDataGridGroupDefinition> Groups,
    IReadOnlyList<string> ExpandedGroupKeys,
    IReadOnlyList<string> SelectedKeys);
