// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Specifies the display mode for close buttons on tab headers.
/// </summary>
public enum CloseButtonDisplayMode
{
    /// <summary>
    /// The close button is never shown.
    /// </summary>
    Never,

    /// <summary>
    /// The close button is always shown for closable tabs.
    /// </summary>
    Always,

    /// <summary>
    /// The close button is shown only when the mouse hovers over the tab or when focused.
    /// </summary>
    OnHover,

    /// <summary>
    /// The close button is shown only on the currently active tab.
    /// </summary>
    ActiveTabOnly
}
