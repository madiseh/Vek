// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// The main content region of a <see cref="VekLayout"/> where each page's content is rendered.
/// </summary>
/// <seealso cref="VekLayout" />
public partial class VekMainContent : VekComponentBase
{
    /// <summary>
    /// Gets the CSS class names for the component.
    /// </summary>
    protected string Classname =>
        new CssBuilder("vek-main-content")
            .AddClass(Class)
            .Build();

    /// <summary>
    /// Sets the content to be rendered inside the main content area.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.MainContent.Behavior)]
    public RenderFragment? ChildContent { get; set; }
}
