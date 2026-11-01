namespace Vek.Core.DataGrid;

/// <summary>
/// Describes one active filter operation in a grid state.
/// </summary>
/// <param name="ColumnKey">The stable key of the filtered column.</param>
/// <param name="Operator">The comparison applied to the value.</param>
/// <param name="Value">The value used by the comparison.</param>
/// <param name="CaseSensitive">Whether string comparisons distinguish letter case.</param>
public sealed record VekDataGridFilter(
    string ColumnKey,
    VekDataGridFilterOperator Operator,
    object? Value,
    bool CaseSensitive = false);

