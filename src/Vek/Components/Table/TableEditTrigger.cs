// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;


/// <summary>
/// Indicates the behavior which begins inline editing for a <see cref="VekTable{T}"/>.
/// </summary>
public enum TableEditTrigger
{
    /// <summary>
    /// Editing begins when a row is clicked.
    /// </summary>
    RowClick,

    /// <summary>
    /// Editing begins when the edit button is clicked.
    /// </summary>
    EditButton,
}
