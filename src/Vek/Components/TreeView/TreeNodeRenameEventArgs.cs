namespace Vek;

/// <summary>
/// Provides event data for tree node text renaming and inline editing operations.
/// </summary>
/// <typeparam name="T">The type of item value.</typeparam>
public class TreeNodeRenameEventArgs<T>
{
    /// <summary>
    /// The tree item being renamed.
    /// </summary>
    public VekTreeViewItem<T>? Item { get; init; }

    /// <summary>
    /// The data value associated with the item.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// The original text before editing began.
    /// </summary>
    public string? OldText { get; init; }

    /// <summary>
    /// The new text entered by the user.
    /// </summary>
    public string? NewText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rename operation should be canceled.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item should remain in edit mode when canceled.
    /// </summary>
    public bool KeepEditing { get; set; }

    /// <summary>
    /// Gets or sets an optional error message when validation fails.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
