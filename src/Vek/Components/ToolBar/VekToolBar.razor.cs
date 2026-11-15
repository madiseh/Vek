// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek;


/// <summary>
/// A set of action buttons.  
/// </summary>
/// <seealso cref="VekIconButton" />
public partial class VekToolBar : VekComponentBase
{
    protected string Classname =>
        new CssBuilder("vek-toolbar")
            .AddClass("vek-toolbar-dense", Dense)
            .AddClass("vek-toolbar-gutters", Gutters)
            .AddClass("vek-toolbar-wrap-content", WrapContent)
            .AddClass(Class)
            .Build();

    /// <summary>
    /// Uses compact vertical padding.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Appearance)]
    public bool Dense { get; set; }

    /// <summary>
    /// Adds left and right padding.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Appearance)]
    public bool Gutters { get; set; } = true;

    /// <summary>
    /// The content of the toolbar.
    /// </summary>
    /// <remarks>
    /// Typically a set of <see cref="VekIconButton"/> components.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Allows the toolbar's content to wrap.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public bool WrapContent { get; set; }
}
