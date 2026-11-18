// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;

namespace Vek;

/// <summary>
/// The location of the <see cref="VekTabs.Header"/>
/// </summary>
public enum TabHeaderPosition
{
    /// <summary>
    /// Additional content is placed after the first tab.
    /// </summary>
    [Description("after")]
    After,

    /// <summary>
    /// Additional content is placed before the first tab.
    /// </summary>
    [Description("before")]
    Before,

    /// <summary>
    /// No additional content is shown.
    /// </summary>
    [Description("none")]
    None,
}
