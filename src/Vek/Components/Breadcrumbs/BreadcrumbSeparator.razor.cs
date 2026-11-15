// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;

namespace Vek;


/// <summary>
/// The divider rendered between items in a <see cref="VekBreadcrumbs"/> trail, showing the separator character or a custom template.
/// </summary>
/// <seealso cref="BreadcrumbItem" />
/// <seealso cref="BreadcrumbLink" />
/// <seealso cref="VekBreadcrumbs" />
public partial class BreadcrumbSeparator
{
    /// <summary>
    /// The parent breadcrumb component.
    /// </summary>
    [CascadingParameter]
    public VekBreadcrumbs? Parent { get; set; }
}
