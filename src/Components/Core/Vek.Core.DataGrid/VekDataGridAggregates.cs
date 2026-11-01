namespace Vek.Core.DataGrid;

/// <summary>
/// Defines a platform-neutral aggregate calculation.
/// </summary>
/// <typeparam name="TItem">The type of item.</typeparam>
/// <param name="Key">Unique identifier for the aggregate.</param>
/// <param name="ColumnKey">The column key whose values are aggregated.</param>
/// <param name="Type">The aggregate calculation type.</param>
/// <param name="CustomAggregate">Optional custom calculation function.</param>
/// <param name="Format">Optional display format string.</param>
/// <param name="Scope">The scope where this aggregate applies.</param>
/// <param name="Selector">Optional selector function overriding the column accessor.</param>
/// <param name="CustomMetadata">Optional UI-specific metadata or renderer attached to the aggregate.</param>
public sealed record VekDataGridAggregateDefinition<TItem>(
    string Key,
    string ColumnKey,
    VekDataGridAggregateType Type,
    Func<IReadOnlyList<object?>, object?>? CustomAggregate = null,
    string? Format = null,
    VekDataGridAggregateScope Scope = VekDataGridAggregateScope.Both,
    Func<TItem, object?>? Selector = null,
    object? CustomMetadata = null);

/// <summary>
/// Represents the result of an aggregate computation.
/// </summary>
/// <param name="Key">The aggregate key.</param>
/// <param name="Title">The aggregate or column title.</param>
/// <param name="Value">The raw aggregate value.</param>
/// <param name="FormattedValue">The formatted value string.</param>
public sealed record VekDataGridAggregateResult(
    string Key,
    string Title,
    object? Value,
    string FormattedValue);

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
