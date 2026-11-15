using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

/// <summary>
/// Covers what <see cref="VekIconButton"/> renders on top of the shared behavior in <see cref="BaseButtonTests{TButton}"/>.
/// </summary>
[TestFixture]
public class IconButtonTests : BunitTest
{
    /// <summary>
    /// Icon renders as the only content, sized by the button's Size, and ChildContent is ignored.
    /// </summary>
    [Test]
    public void IconRendersInsteadOfChildContent()
    {
        var comp = Context.Render<VekIconButton>(parameters => parameters
            .Add(p => p.Icon, Icons.Material.Filled.Delete)
            .Add(p => p.Size, Size.Small)
            .AddChildContent("Delete"));

        comp.Find(".vek-icon-button-label svg").ClassList.Should().Contain("vek-icon-size-small");
        comp.Find("button").TextContent.Should().BeEmpty();
    }

    /// <summary>
    /// Without an Icon, ChildContent is rendered as text so the button can show a short label.
    /// </summary>
    [Test]
    public void ChildContentRendersWithoutIcon()
    {
        var comp = Context.Render<VekIconButton>(parameters => parameters
            .AddChildContent("AB"));

        comp.FindAll(".vek-icon-button-label").Should().BeEmpty();
        comp.Find("button .vek-typography").TextContent.Should().Be("AB");
    }

    /// <summary>
    /// The default text variant renders a round icon button colored through its text color, without the regular button classes.
    /// </summary>
    [Test]
    public void TextVariantRendersAsIconButton()
    {
        var comp = Context.Render<VekIconButton>(parameters => parameters
            .Add(p => p.Icon, Icons.Material.Filled.Delete)
            .Add(p => p.Color, Color.Primary));

        var classes = comp.Find("button").ClassList;
        classes.Should().Contain(["vek-icon-button", "vek-primary-text", "hover:vek-primary-hover", "vek-ripple", "vek-ripple-icon"]);
        classes.Should().NotContain(["vek-button", "vek-button-text"]);
    }

    /// <summary>
    /// A filled or outlined icon button takes the regular button classes for its variant and color instead of a text color.
    /// </summary>
    [TestCase(Variant.Filled, "vek-button-filled", "vek-button-filled-primary")]
    [TestCase(Variant.Outlined, "vek-button-outlined", "vek-button-outlined-primary")]
    public void NonTextVariantRendersAsButton(Variant variant, string variantClass, string colorClass)
    {
        var comp = Context.Render<VekIconButton>(parameters => parameters
            .Add(p => p.Icon, Icons.Material.Filled.Delete)
            .Add(p => p.Variant, variant)
            .Add(p => p.Color, Color.Primary));

        var classes = comp.Find("button").ClassList;
        classes.Should().Contain(["vek-icon-button", "vek-button", variantClass, colorClass]);
        classes.Should().NotContain(["vek-primary-text", "vek-ripple-icon"]);
    }

    /// <summary>
    /// A non-medium Size and a non-default Edge each add a class, while the defaults add none.
    /// </summary>
    [TestCase(Size.Medium, Edge.False, null, null)]
    [TestCase(Size.Small, Edge.Start, "vek-icon-button-size-small", "vek-icon-button-edge-start")]
    [TestCase(Size.Large, Edge.End, "vek-icon-button-size-large", "vek-icon-button-edge-end")]
    public void SizeAndEdgeSetClasses(Size size, Edge edge, string sizeClass, string edgeClass)
    {
        var comp = Context.Render<VekIconButton>(parameters => parameters
            .Add(p => p.Icon, Icons.Material.Filled.Delete)
            .Add(p => p.Size, size)
            .Add(p => p.Edge, edge));

        var classes = comp.Find("button").ClassList;
        classes.SingleOrDefault(c => c.StartsWith("vek-icon-button-size-")).Should().Be(sizeClass);
        classes.SingleOrDefault(c => c.StartsWith("vek-icon-button-edge-")).Should().Be(edgeClass);
    }
}
