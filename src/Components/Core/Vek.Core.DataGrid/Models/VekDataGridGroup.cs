namespace Vek.Core.DataGrid;

/// <summary>
/// Represents a group of items computed by the engine.
/// </summary>
/// <typeparam name="TItem">The type of item.</typeparam>
/// <param name="ColumnKey">The column key grouped by.</param>
/// <param name="Title">The column title.</param>
/// <param name="Value">The distinct group value.</param>
/// <param name="Level">The nesting depth level (0-based).</param>
/// <param name="Items">The items belonging to this group.</param>
/// <param name="Aggregates">The aggregate results computed for this group.</param>
/// <param name="Children">The nested groups contained in this group.</param>
/// <param name="Key">The stable platform-neutral key for this group.</param>
public sealed record VekDataGridGroup<TItem>(
    string ColumnKey,
    string Title,
    object? Value,
    int Level,
    IReadOnlyList<TItem> Items,
    IReadOnlyList<VekDataGridAggregateResult> Aggregates,
    IReadOnlyList<VekDataGridGroup<TItem>>? Children = null,
    string Key = "");
