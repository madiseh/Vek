namespace Vek;

/// <summary>
/// Defines the drop position relative to the target tree item.
/// </summary>
public enum TreeItemDropPosition
{
    /// <summary>
    /// Drop before the target item as a sibling.
    /// </summary>
    Before,

    /// <summary>
    /// Drop inside the target item as a child.
    /// </summary>
    Into,

    /// <summary>
    /// Drop after the target item as a sibling.
    /// </summary>
    After
}

/// <summary>
/// Provides data for the tree node drop event.
/// </summary>
/// <typeparam name="T">The type of item value.</typeparam>
public class TreeNodeDropEventArgs<T>
{
    /// <summary>
    /// The item being dragged.
    /// </summary>
    public VekTreeViewItem<T>? SourceItem { get; init; }

    /// <summary>
    /// The target item where the drop occurs.
    /// </summary>
    public VekTreeViewItem<T>? TargetItem { get; init; }

    /// <summary>
    /// The data value of the dragged item.
    /// </summary>
    public T? SourceValue { get; init; }

    /// <summary>
    /// The data value of the target item.
    /// </summary>
    public T? TargetValue { get; init; }

    /// <summary>
    /// The drop position relative to the target item.
    /// </summary>
    public TreeItemDropPosition DropPosition { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the drop operation should be canceled.
    /// </summary>
    public bool Cancel { get; set; }
}
