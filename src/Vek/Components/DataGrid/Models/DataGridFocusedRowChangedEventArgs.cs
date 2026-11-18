// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Vek;

/// <summary>
/// Provides data for the <see cref="VekDataGrid{T}.FocusedRowChanged"/> event.
/// </summary>
/// <typeparam name="T">The type of item represented in the data grid.</typeparam>
public class DataGridFocusedRowChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// Gets the data item associated with the newly focused row, or <c>default</c> if no row is focused.
    /// </summary>
    public T? Item { get; }

    /// <summary>
    /// Gets the zero-based index of the newly focused row, or <c>-1</c> if no row is focused.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// Gets the data item associated with the previously focused row, or <c>default</c> if no row was focused.
    /// </summary>
    public T? PreviousItem { get; }

    /// <summary>
    /// Gets the zero-based index of the previously focused row, or <c>-1</c> if no row was previously focused.
    /// </summary>
    public int PreviousRowIndex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridFocusedRowChangedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="item">The data item associated with the newly focused row.</param>
    /// <param name="rowIndex">The zero-based index of the newly focused row.</param>
    /// <param name="previousItem">The data item associated with the previously focused row.</param>
    /// <param name="previousRowIndex">The zero-based index of the previously focused row.</param>
    public DataGridFocusedRowChangedEventArgs(T? item, int rowIndex, T? previousItem = default, int previousRowIndex = -1)
    {
        Item = item;
        RowIndex = rowIndex;
        PreviousItem = previousItem;
        PreviousRowIndex = previousRowIndex;
    }
}
