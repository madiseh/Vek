using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek;


/// <summary>
/// A header displayed as part of a <see cref="VekList{T}"/>.
/// </summary>
/// <remarks>
/// Typically used to describe a list.
/// </remarks>
/// <seealso cref="VekList{T}"/>
/// <seealso cref="VekListItem{T}"/>
public partial class VekListSubheader : VekComponentBase
{
    protected string Classname =>
        new CssBuilder("vek-list-subheader")
            .AddClass("vek-list-subheader-gutters", Gutters)
            .AddClass("vek-list-subheader-inset", Inset)
            .AddClass(Class)
            .Build();

    /// <summary>
    /// The content within this header.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.List.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Applies left and right padding to all list items.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.List.Appearance)]
    public bool Gutters { get; set; } = true;

    /// <summary>
    /// Applies an indent to this header.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.List.Appearance)]
    public bool Inset { get; set; }
}
