// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components.Web;

namespace Vek;

/// <summary>
/// Describes an active sort rule for a <see cref="VekTreeList{T}"/> column.
/// </summary>
/// <typeparam name="T">The type of data item displayed in the tree list.</typeparam>
public sealed class TreeListSortDescriptor<T>
{
    /// <summary>
    /// The column being sorted.
    /// </summary>
    public required VekTreeListColumn<T> Column { get; init; }

    /// <summary>
    /// The active sort direction for the column.
    /// </summary>
    public required SortDirection SortDirection { get; init; }
}

/// <summary>
/// Provides event data when sorting changes in a <see cref="VekTreeList{T}"/>.
/// </summary>
/// <typeparam name="T">The type of data item displayed in the tree list.</typeparam>
public sealed class TreeListSortEventArgs<T>
{
    /// <summary>
    /// The column whose sort direction changed.
    /// </summary>
    public required VekTreeListColumn<T> Column { get; init; }

    /// <summary>
    /// The new sort direction applied to <see cref="Column"/>.
    /// </summary>
    public required SortDirection SortDirection { get; init; }

    /// <summary>
    /// The ordered collection of currently active sort descriptors across all columns.
    /// </summary>
    public required IReadOnlyList<TreeListSortDescriptor<T>> ActiveSorts { get; init; }
}

/// <summary>
/// Provides event data when a column is resized in a <see cref="VekTreeList{T}"/>.
/// </summary>
/// <typeparam name="T">The type of data item displayed in the tree list.</typeparam>
public sealed class TreeListColumnResizeEventArgs<T>
{
    /// <summary>
    /// The column that was resized.
    /// </summary>
    public required VekTreeListColumn<T> Column { get; init; }

    /// <summary>
    /// The previous width of the column in pixels.
    /// </summary>
    public double OldWidth { get; init; }

    /// <summary>
    /// The new width of the column in pixels, or <c>null</c> when reset to default width.
    /// </summary>
    public double? NewWidth { get; init; }
}

/// <summary>
/// Provides event data when a row is clicked in a <see cref="VekTreeList{T}"/>.
/// </summary>
/// <typeparam name="T">The type of data item displayed in the tree list.</typeparam>
public sealed class TreeListRowClickEventArgs<T>
{
    /// <summary>
    /// The hierarchical item data associated with the clicked row.
    /// </summary>
    public required ITreeItemData<T> Item { get; init; }

    /// <summary>
    /// The zero-based nesting depth of the clicked row.
    /// </summary>
    public int Depth { get; init; }

    /// <summary>
    /// The underlying mouse event arguments.
    /// </summary>
    public required MouseEventArgs MouseEventArgs { get; init; }
}

internal sealed record TreeListRow<T>(
    ITreeItemData<T> Item,
    ITreeItemData<T>? Parent,
    int Depth,
    int PosInSet,
    int SetSize);
