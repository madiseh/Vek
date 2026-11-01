namespace Vek.Core.DataGrid;

/// <summary>Provides the platform-neutral data and actions for a group template.</summary>
/// <typeparam name="TItem">The type of row item.</typeparam>
public sealed class VekDataGridGroupContext<TItem>
{
    /// <summary>Initializes a new group template context.</summary>
    /// <param name="columnKey">The stable grouping column key.</param>
    /// <param name="title">The grouping column title.</param>
    /// <param name="value">The group value.</param>
    /// <param name="level">The zero-based nesting level.</param>
    /// <param name="items">The items contained in the group.</param>
    /// <param name="expanded">Whether the group is expanded.</param>
    /// <param name="toggleAsync">Toggles group expansion.</param>
    /// <param name="aggregates">The aggregates calculated for this group.</param>
    public VekDataGridGroupContext(
        string columnKey,
        string title,
        object? value,
        int level,
        IReadOnlyList<TItem> items,
        bool expanded,
        Func<Task> toggleAsync,
        IReadOnlyList<VekDataGridAggregateResult> aggregates)
        => (ColumnKey, Title, Value, Level, Items, Count, Expanded, ToggleAsync, Aggregates) =
            (columnKey, title, value, level, items, items.Count, expanded, toggleAsync, aggregates);

    /// <summary>Gets the stable key of the grouping column.</summary>
    public string ColumnKey { get; }

    /// <summary>Gets the title of the grouping column.</summary>
    public string Title { get; }

    /// <summary>Gets the distinct value represented by this group.</summary>
    public object? Value { get; }

    /// <summary>Gets the zero-based nesting level of this group.</summary>
    public int Level { get; }

    /// <summary>Gets the items contained by this group.</summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>Gets the number of items contained by this group.</summary>
    public int Count { get; }

    /// <summary>Gets whether the group is currently expanded.</summary>
    public bool Expanded { get; }

    /// <summary>Toggles whether the group's children are visible.</summary>
    public Func<Task> ToggleAsync { get; }

    /// <summary>Gets the aggregate results calculated for this group.</summary>
    public IReadOnlyList<VekDataGridAggregateResult> Aggregates { get; }
}

