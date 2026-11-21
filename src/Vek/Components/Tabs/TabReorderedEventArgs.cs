// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Describes a tab panel's completed reorder operation.
/// </summary>
public sealed class TabReorderedEventArgs(int sourceIndex, int targetIndex, VekTabPanel panel)
{
    /// <summary>
    /// The panel's index before it was moved.
    /// </summary>
    public int SourceIndex { get; } = sourceIndex;

    /// <summary>
    /// The panel's index after it was moved.
    /// </summary>
    public int TargetIndex { get; } = targetIndex;

    /// <summary>
    /// The panel that was moved.
    /// </summary>
    public VekTabPanel Panel { get; } = panel;
}
