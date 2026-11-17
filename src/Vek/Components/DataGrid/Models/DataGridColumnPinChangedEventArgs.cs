// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Vek;

/// <summary>
/// Event data for the <see cref="VekDataGrid{T}.ColumnPinChanged"/> event, carrying the affected column and its new fixed pinning style.
/// </summary>
/// <typeparam name="T">The item type managed by the <see cref="VekDataGrid{T}"/>.</typeparam>
public class DataGridColumnPinChangedEventArgs<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : EventArgs
{
    /// <summary>
    /// The column whose pinning state was changed.
    /// </summary>
    public Column<T> Column { get; }

    /// <summary>
    /// The new fixed pinning style applied to the column.
    /// </summary>
    public DataGridFixedStyle FixedStyle { get; }

    public DataGridColumnPinChangedEventArgs(Column<T> column, DataGridFixedStyle fixedStyle)
    {
        Column = column;
        FixedStyle = fixedStyle;
    }
}
