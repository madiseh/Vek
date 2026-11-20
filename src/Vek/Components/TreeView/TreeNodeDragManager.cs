namespace Vek;

/// <summary>
/// Coordinates active drag-and-drop state across tree view nodes.
/// </summary>
public class TreeNodeDragManager
{
    /// <summary>
    /// Gets the current dragged item instance (untyped).
    /// </summary>
    public object? ActiveDragItem { get; private set; }

    /// <summary>
    /// Starts dragging an item.
    /// </summary>
    /// <param name="item">The item being dragged.</param>
    public void StartDrag(object item)
    {
        ActiveDragItem = item;
    }

    /// <summary>
    /// Clears the active drag state.
    /// </summary>
    public void EndDrag()
    {
        ActiveDragItem = null;
    }

    /// <summary>
    /// Gets a value indicating whether an item is currently being dragged.
    /// </summary>
    public bool IsDragging => ActiveDragItem != null;
}
