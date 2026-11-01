namespace Vek.Core.DataGrid;

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

