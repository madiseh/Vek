// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Specifies the fixed pinning style for a <see cref="Column{T}"/> within a <see cref="VekDataGrid{T}"/>.
/// </summary>
public enum DataGridFixedStyle
{
    /// <summary>
    /// The column is not pinned and scrolls horizontally with the grid content.
    /// </summary>
    None,

    /// <summary>
    /// The column is pinned to the left edge of the grid viewport.
    /// </summary>
    Left,

    /// <summary>
    /// The column is pinned to the right edge of the grid viewport.
    /// </summary>
    Right
}
