// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek;


/// <summary>
/// The clickable link rendered for each <see cref="BreadcrumbItem"/> in a <see cref="VekBreadcrumbs"/> trail.
/// </summary>
/// <seealso cref="BreadcrumbItem" />
/// <seealso cref="BreadcrumbSeparator" />
/// <seealso cref="VekBreadcrumbs" />
public partial class BreadcrumbLink
{
    /// <summary>
    /// The item to display.
    /// </summary>
    [Parameter]
    public BreadcrumbItem? Item { get; set; }

    /// <summary>
    /// The parent breadcrumb component.
    /// </summary>
    [CascadingParameter]
    public VekBreadcrumbs? Parent { get; set; }

    /// <summary>
    /// Whether this item is the last one in the trail and therefore represents the current page.
    /// </summary>
    private bool IsCurrentPage => Parent?.IsLastItem(Item) == true;

    private string Classname => new CssBuilder("vek-breadcrumb-item")
        .AddClass("vek-disabled", Item?.Disabled)
        .Build();
}
