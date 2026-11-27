// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Docs.Extensions;
using Vek.Docs.Models;
using Vek.Docs.Services;

namespace Vek.Docs.Components;

/// <summary>
/// A title and description for a docs page.
/// </summary>
public sealed partial class DocsPageHeader
{
    /// <summary>
    /// The service for navigating to other pages.
    /// </summary>
    [Inject]
    public NavigationManager NavigationManager { get; set; }

    /// <summary>
    /// The service for building menus.
    /// </summary>
    [Inject]
    public IMenuService MenuService { get; set; }

    /// <summary>
    /// The name of the component associated with this page.
    /// </summary>
    /// <remarks>
    /// Should be the name of a component, such as <c>nameof(VekAlert)</c>.  When set, the
    /// <see cref="DocumentedType"/> property will contain all the documentation for this 
    /// component.
    /// </remarks>
    [Parameter]
    public string Component { get; set; }

    /// <summary>
    /// Whether this page shows API documentation.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    public bool IsApi { get; set; }

    /// <summary>
    /// The title of this page.
    /// </summary>
    [Parameter]
    public string Title { get; set; }

    /// <summary>
    /// The subtitle of this page.
    /// </summary>
    [Parameter]
    public string SubTitle { get; set; }
    private string ComputedSubTitle { get; set; }

    /// <summary>
    /// The description of this page.
    /// </summary>
    [Parameter]
    public RenderFragment Description { get; set; }

    /// <summary>
    /// The special headers, if any, for this page.
    /// </summary>
    [Parameter]
    public RenderFragment SpecialHeaderContent { get; set; }

    /// <summary>
    /// The documentation for this page's component.
    /// </summary>
    public DocumentedType DocumentedType { get; set; }

    /// <summary>
    /// The example page for this type.
    /// </summary>
    public VekComponent Example { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Has the page changed?  Or is this the first render?
        if (Component != null && (DocumentedType == null || DocumentedType.Name != Component))
        {
            // Get the documentation for this component
            DocumentedType = ApiDocumentation.GetType(Component);
            // Look for an example page for this type
            Example = DocumentedType == null ? null : MenuService.GetExample(DocumentedType);
        }

        if (!string.IsNullOrWhiteSpace(SubTitle))
        {
            ComputedSubTitle = SubTitle;
        }
        else if (DocumentedType != null && !string.IsNullOrWhiteSpace(DocumentedType.Summary))
        {
            ComputedSubTitle = DocumentedType.Summary;
        }
        else
        {
            ComputedSubTitle = "";
        }
    }

    /// <summary>
    /// Gets the title for this page.
    /// </summary>
    private string GetTitle() => $"{Title} - Vek";

    /// <summary>
    /// Gets the page description for search and social previews.
    /// </summary>
    private string GetMetaDescription()
    {
        return string.IsNullOrWhiteSpace(ComputedSubTitle)
            ? "Vek is an open-source Blazor component library with accessible, customizable UI components for .NET 8–11."
            : ComputedSubTitle.TrimEnd('.');
    }

    /// <summary>
    /// Gets the subtitle for this page.
    /// </summary>
    /// <returns></returns>
    private string GetSubTitle()
    {
        if (string.IsNullOrEmpty(ComputedSubTitle))
            return "";
        return ComputedSubTitle.TrimEnd('.') + ".";
    }

    /// <summary>
    /// Gets the canonical URL for this page.
    /// </summary>
    /// <returns></returns>
    private string GetCanonicalUri()
    {
        var canonicalUri = new UriBuilder(NavigationManager.Uri)
        {
            Scheme = Uri.UriSchemeHttps,
            Host = "vekui.dev",
            Port = -1,
            Query = string.Empty,
            Fragment = string.Empty
        };

        var canonicalPath = canonicalUri.Uri.GetLeftPart(UriPartial.Path);
        return IsApi ? canonicalPath.ToLowerInvariant() : canonicalPath;
    }
}
