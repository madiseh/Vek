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

