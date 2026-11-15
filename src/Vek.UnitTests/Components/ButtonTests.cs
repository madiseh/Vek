using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

/// <summary>
/// Covers what <see cref="VekButton"/> renders on top of the shared behavior in <see cref="BaseButtonTests{TButton}"/>.
/// </summary>
[TestFixture]
public class ButtonTests : BunitTest
{
    /// <summary>
    /// A button with no appearance parameters renders as a medium, default-colored text button with a ripple and elevation.
    /// </summary>
    [Test]
    public void DefaultAppearance()
    {
        var comp = Context.Render<VekButton>();

        var classes = comp.Find("button").ClassList;
        classes.Should().Contain(["vek-button-root", "vek-button", "vek-button-text", "vek-button-text-default", "vek-button-text-size-medium", "vek-ripple"]);
        classes.Should().NotContain(["vek-width-full", "vek-button-disable-elevation"]);
    }

    /// <summary>
    /// Variant, Color, and Size combine into the variant-scoped classes the stylesheet targets.
    /// </summary>
    [TestCase(Variant.Filled, Color.Primary, Size.Small, "vek-button-filled", "vek-button-filled-primary", "vek-button-filled-size-small")]
    [TestCase(Variant.Outlined, Color.Secondary, Size.Large, "vek-button-outlined", "vek-button-outlined-secondary", "vek-button-outlined-size-large")]
    public void AppearanceParametersSetVariantClasses(Variant variant, Color color, Size size, string variantClass, string colorClass, string sizeClass)
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.Variant, variant)
            .Add(p => p.Color, color)
            .Add(p => p.Size, size));

        var classes = comp.Find("button").ClassList;
        classes.Should().Contain([variantClass, colorClass, sizeClass]);
        classes.Should().NotContain("vek-button-text");
    }

    /// <summary>
    /// FullWidth stretches the button, and turning off DropShadow and Ripple removes elevation and the ripple.
    /// </summary>
    [Test]
    public void FullWidthDropShadowAndRippleSetClasses()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.FullWidth, true)
            .Add(p => p.DropShadow, false)
            .Add(p => p.Ripple, false));

        var classes = comp.Find("button").ClassList;
        classes.Should().Contain(["vek-width-full", "vek-button-disable-elevation"]);
        classes.Should().NotContain("vek-ripple");
    }

    /// <summary>
    /// StartIcon and EndIcon render on either side of the content, both carrying IconClass and IconColor.
    /// </summary>
    [Test]
    public void StartAndEndIconsRenderAroundContent()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Material.Filled.Save)
            .Add(p => p.EndIcon, Icons.Material.Filled.ArrowDropDown)
            .Add(p => p.IconClass, "my-icon")
            .Add(p => p.IconColor, Color.Primary)
            .AddChildContent("Save"));

        var label = comp.Find(".vek-button-label");
        label.TextContent.Should().Be("Save");
        label.Children.Should().HaveCount(2);
        label.Children[0].ClassList.Should().Contain(["vek-button-icon-start", "my-icon"]);
        label.Children[1].ClassList.Should().Contain(["vek-button-icon-end", "my-icon"]);
        comp.FindAll(".vek-button-label svg").Should().AllSatisfy(icon => icon.ClassList.Should().Contain("vek-primary-text"));
    }

    /// <summary>
    /// Without StartIcon or EndIcon, only the content is rendered.
    /// </summary>
    [Test]
    public void RendersNoIconsWhenNoneAreSet()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .AddChildContent("Save"));

        comp.Find(".vek-button-label").TextContent.Should().Be("Save");
        comp.FindAll("svg").Should().BeEmpty();
    }

    /// <summary>
    /// Icons follow the button's Size unless IconSize is set.
    /// </summary>
    [TestCase(Size.Small, null, "small")]
    [TestCase(Size.Large, null, "large")]
    [TestCase(Size.Small, Size.Large, "large")]
    [TestCase(Size.Large, Size.Small, "small")]
    public void IconSizeFallsBackToButtonSize(Size size, Size? iconSize, string expectedSize)
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.Size, size)
            .Add(p => p.IconSize, iconSize)
            .Add(p => p.StartIcon, Icons.Material.Filled.Delete)
            .Add(p => p.EndIcon, Icons.Material.Filled.Delete));

        comp.Find("button").ClassList.Should().Contain($"vek-button-text-size-{size.ToString().ToLowerInvariant()}");
        foreach (var selector in new[] { ".vek-button-icon-start", ".vek-button-icon-end" })
        {
            var iconSpan = comp.Find(selector);
            iconSpan.ClassList.Should().Contain($"vek-button-icon-size-{expectedSize}");
            iconSpan.QuerySelector("svg")!.ClassList.Should().Contain($"vek-icon-size-{expectedSize}");
        }
    }
}
