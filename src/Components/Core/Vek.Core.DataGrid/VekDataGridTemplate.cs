using System.Globalization;

namespace Vek.Core.DataGrid;

/// <summary>Produces a platform-native template for a platform-neutral DataGrid context.</summary>
/// <typeparam name="TContext">The shared context supplied to this template slot.</typeparam>
/// <typeparam name="TTemplate">The template type owned by the UI adapter.</typeparam>
/// <param name="context">The data or action context for the template slot.</param>
/// <returns>The native template value consumed by the UI adapter.</returns>
public delegate TTemplate VekDataGridTemplate<in TContext, out TTemplate>(TContext context);

/// <summary>Provides the context required by a custom column filter template.</summary>
public sealed class VekDataGridFilterContext
{
    /// <summary>Initializes a new filter template context.</summary>
    /// <param name="value">The currently applied filter value.</param>
    /// <param name="apply">Applies a filter value.</param>
    /// <param name="clear">Removes the filter.</param>
    public VekDataGridFilterContext(string? value, Action<string?> apply, Action clear)
        => (Value, Apply, Clear) = (value, apply, clear);

    /// <summary>Gets the currently applied filter value.</summary>
    public string? Value { get; }

    /// <summary>Applies a value using the column's configured filter operator.</summary>
    public Action<string?> Apply { get; }

    /// <summary>Removes the active filter.</summary>
    public Action Clear { get; }
}

/// <summary>Provides the platform-neutral edit state and actions for an edit template.</summary>
/// <typeparam name="TItem">The type of row item.</typeparam>
public sealed class VekDataGridEditTemplateContext<TItem>
{
    private readonly Action<TItem> _updateItem;
    private readonly Func<Task<bool>> _commit;
    private readonly Func<Task> _cancel;

    /// <summary>Initializes a new edit template context.</summary>
    /// <param name="item">The current edit draft.</param>
    /// <param name="culture">The culture configured for the grid.</param>
    /// <param name="updateItem">Replaces the edit draft.</param>
    /// <param name="commit">Validates and commits the draft.</param>
    /// <param name="cancel">Discards the draft.</param>
    public VekDataGridEditTemplateContext(
        TItem item,
        CultureInfo culture,
        Action<TItem> updateItem,
        Func<Task<bool>> commit,
        Func<Task> cancel)
        => (Item, Culture, _updateItem, _commit, _cancel) = (item, culture, updateItem, commit, cancel);

    /// <summary>Gets the current editable draft item.</summary>
    public TItem Item { get; }

    /// <summary>Gets the culture configured for the grid that owns this edit session.</summary>
    public CultureInfo Culture { get; }

    /// <summary>Replaces the current draft item.</summary>
    /// <param name="item">The updated draft.</param>
    public void UpdateItem(TItem item) => _updateItem(item);

    /// <summary>Validates and attempts to commit the draft.</summary>
    public Func<Task<bool>> CommitAsync => _commit;

    /// <summary>Discards the draft and exits edit mode.</summary>
    public Func<Task> CancelAsync => _cancel;
}

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

/// <summary>Provides the row detail state and action for a detail template.</summary>
/// <typeparam name="TItem">The type of row item.</typeparam>
public sealed class VekDataGridRowDetailContext<TItem>
{
    /// <summary>Initializes a new row detail template context.</summary>
    /// <param name="item">The row whose detail content is displayed.</param>
    /// <param name="expanded">Whether this row's details are expanded.</param>
    /// <param name="toggleAsync">Toggles the detail content.</param>
    public VekDataGridRowDetailContext(TItem item, bool expanded, Func<Task> toggleAsync)
        => (Item, Expanded, ToggleAsync) = (item, expanded, toggleAsync);

    /// <summary>Gets the item whose detail content is displayed.</summary>
    public TItem Item { get; }

    /// <summary>Gets whether this row's details are expanded.</summary>
    public bool Expanded { get; }

    /// <summary>Toggles the detail content for this row.</summary>
    public Func<Task> ToggleAsync { get; }
}
