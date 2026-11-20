// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Contains information about a tab panel that is closing.
/// </summary>
public class TabClosingEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabClosingEventArgs"/> class.
    /// </summary>
    /// <param name="tab">The tab panel being closed.</param>
    /// <param name="panelIndex">The index of the tab panel being closed.</param>
    public TabClosingEventArgs(VekTabPanel tab, int panelIndex)
    {
        Tab = tab;
        PanelIndex = panelIndex;
    }

    /// <summary>
    /// The tab panel that is closing.
    /// </summary>
    public VekTabPanel Tab { get; }

    /// <summary>
    /// The index of the tab panel being closed.
    /// </summary>
    public int PanelIndex { get; }

    /// <summary>
    /// Prevents the tab from closing when set to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>. When set to <c>true</c>, the tab remains open and active tab selection is unchanged.
    /// </remarks>
    public bool Cancel { get; set; }
}
