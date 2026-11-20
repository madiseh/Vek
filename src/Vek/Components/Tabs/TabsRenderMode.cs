// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Specifies the rendering and DOM caching mode for tab panels.
/// </summary>
public enum TabsRenderMode
{
    /// <summary>
    /// Renders only the currently active tab panel. Inactive tab panels are removed from the DOM and recreated upon activation.
    /// </summary>
    ActiveTab,

    /// <summary>
    /// Renders all tab panels immediately upon initialization and preserves them in the DOM across tab switches.
    /// </summary>
    AllTabs,

    /// <summary>
    /// Renders tab panels on demand upon first activation and preserves their state in the DOM across tab switches.
    /// </summary>
    ActiveTabLazily,
}
