namespace Vek;

/// <summary>
/// Describes a row move requested by a data grid reorder interaction.
/// </summary>
/// <typeparam name="T">The type of the row item.</typeparam>
public sealed class DataGridRowReorderedEventArgs<T> : EventArgs
{
    /// <summary>
    /// Initializes a row reorder request.
    /// </summary>
    /// <param name="sourceItem">The row item moved by the user.</param>
    /// <param name="targetItem">The row item used as the drop target.</param>
    /// <param name="insertPosition">The zero-based destination index after the source item is removed.</param>
    public DataGridRowReorderedEventArgs(T sourceItem, T targetItem, int insertPosition)
    {
        SourceItem = sourceItem;
        TargetItem = targetItem;
        InsertPosition = insertPosition;
    }

    /// <summary>
    /// The row item moved by the user.
    /// </summary>
    public T SourceItem { get; }

    /// <summary>
    /// The row item used as the drop target.
    /// </summary>
    public T TargetItem { get; }

    /// <summary>
    /// The zero-based destination index after the source item is removed.
    /// </summary>
    public int InsertPosition { get; }
}
