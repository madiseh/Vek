namespace Vek.Core.DataGrid;

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
