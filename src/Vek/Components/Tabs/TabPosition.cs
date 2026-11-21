// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// Specifies the orientation and position of the tab strip relative to the tab panels.
/// </summary>
[EnumExtensions]
public enum TabPosition
{
    /// <summary>
    /// The tab strip is positioned along the top edge with horizontal orientation.
    /// </summary>
    [Description("top")]
    Top,

    /// <summary>
    /// The tab strip is positioned along the bottom edge with horizontal orientation.
    /// </summary>
    [Description("bottom")]
    Bottom,

    /// <summary>
    /// The tab strip is positioned along the left edge with vertical orientation.
    /// </summary>
    [Description("left")]
    Left,

    /// <summary>
    /// The tab strip is positioned along the right edge with vertical orientation.
    /// </summary>
    [Description("right")]
    Right,
}
