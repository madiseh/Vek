// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Docs.Models;

#nullable enable
public class VekVirtualizedIcons
{
    public VekIcons[] RowIcons { get; }

    public VekVirtualizedIcons(VekIcons[] rowIcons)
    {
        RowIcons = rowIcons;
    }
}
