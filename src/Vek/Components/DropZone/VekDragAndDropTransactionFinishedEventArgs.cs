// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Vek;


/// <summary>
/// The information related to a <see cref="VekDropZone{T}"/> completed drag-and-drop transaction.
/// </summary>
/// <typeparam name="T">The type of item being dragged and dropped.</typeparam>
public class VekDragAndDropTransactionFinishedEventArgs<T> : EventArgs
{
    /// <summary>
    /// The item which was dropped.
    /// </summary>
    public T? Item { get; }

    /// <summary>
    /// Whether the drag-and-drop completed successfully.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    public bool Success { get; }

    /// <summary>
    /// The unique ID of the zone where the drag-and-drop started.
    /// </summary>
    public string OriginatedDropzoneIdentifier { get; }

    /// <summary>
    /// The unique ID of the zone where the drag-and-drop finished.
    /// </summary>
    public string DestinationDropzoneIdentifier { get; }

    /// <summary>
    /// The index of the zone where the drag-and-drop started.
    /// </summary>
    public int OriginIndex { get; }

    /// <summary>
    /// The index of the zone where the drag-and-drop finished.
    /// </summary>
    public int DestinationIndex { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="destinationDropZoneIdentifier">The unique ID of the zone where the drag-and-drop finished.</param>
    /// <param name="success">Whether the drag-and-drop completed successfully.</param>
    /// <param name="transaction">The transaction related to this event.</param>
    public VekDragAndDropTransactionFinishedEventArgs(string destinationDropZoneIdentifier, bool success, VekDragAndDropItemTransaction<T> transaction)
    {
        Item = transaction.Item;
        Success = success;
        OriginatedDropzoneIdentifier = transaction.SourceZoneIdentifier;
        DestinationDropzoneIdentifier = destinationDropZoneIdentifier;
        OriginIndex = transaction.SourceIndex;
        DestinationIndex = transaction.Index;
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="transaction">The transaction related to this event.</param>
    public VekDragAndDropTransactionFinishedEventArgs(VekDragAndDropItemTransaction<T> transaction) :
        this(string.Empty, false, transaction)
    {
    }
}
