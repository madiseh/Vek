// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// Indicates the sorting mode for a <see cref="VekDataGrid{T}"/>.
/// </summary>
[EnumExtensions]
public enum SortMode
{
    /// <summary>
    /// Sorting is not allowed.
    /// </summary>
    [Description("none")]
    None,

    /// <summary>
    /// Sorting can occur for one column at a time.
    /// </summary>
    [Description("single")]
    Single,

    /// <summary>
    /// Sorting can be done for multiple columns.
    /// </summary>
    [Description("multiple")]
    Multiple
}
