using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

/// <summary>
/// Covers what <see cref="VekFab"/> renders on top of the shared behavior in <see cref="BaseButtonTests{TButton}"/>.
/// </summary>
[TestFixture]
public class FabTests : BunitTest
{
    /// <summary>
    /// A FAB with no appearance parameters renders as a large, default-colored filled button with a ripple.
    /// </summary>
    [Test]
    public void DefaultAppearance()
    {
        var comp = Context.Render<VekFab>();

        var classes = comp.Find("button").ClassList;
        classes.Should().Contain(["vek-button-root", "vek-fab", "vek-fab-filled", "vek-fab-filled-default", "vek-fab-default", "vek-fab-size-large", "vek-ripple"]);
        classes.Should().NotContain(["vek-fab-extended", "vek-fab-disable-elevation"]);
    }

    /// <summary>
    /// Each variant renders its own variant and color classes and none of the others.
    /// Only the filled variant also emits the legacy vek-fab-{color} class, which older stylesheets target.
    /// </summary>
    [TestCase(Variant.Filled, "vek-fab-filled", "vek-fab-filled-primary", true)]
    [TestCase(Variant.Outlined, "vek-fab-outlined", "vek-fab-outlined-primary", false)]
    [TestCase(Variant.Text, "vek-fab-text", "vek-fab-text-primary", false)]
    public void VariantSetsClasses(Variant variant, string variantClass, string colorClass, bool hasLegacyColorClass)
    {
        var comp = Context.Render<VekFab>(parameters => parameters
            .Add(p => p.Variant, variant)
            .Add(p => p.Color, Color.Primary));

        var classes = comp.Find("button").ClassList;
        classes.Should().Contain([variantClass, colorClass]);
        classes.Where(c => c is "vek-fab-filled" or "vek-fab-outlined" or "vek-fab-text").Should().Equal(variantClass);
        classes.Contains("vek-fab-primary").Should().Be(hasLegacyColorClass);
    }

    /// <summary>
    /// A disabled FAB keeps its variant class, so variant-specific disabled styles such as the transparent text background still apply.
    /// </summary>
    [TestCase(Variant.Filled, "vek-fab-filled")]
    [TestCase(Variant.Outlined, "vek-fab-outlined")]
    [TestCase(Variant.Text, "vek-fab-text")]
    public void DisabledKeepsVariantClass(Variant variant, string variantClass)
    {
        var comp = Context.Render<VekFab>(parameters => parameters
            .Add(p => p.Variant, variant)
            .Add(p => p.Disabled, true));

        var button = comp.Find("button");
        button.HasAttribute("disabled").Should().BeTrue();
        button.ClassList.Should().Contain(variantClass);
    }

    /// <summary>
    /// Unlike other buttons, a disabled FAB drops its ripple.
    /// </summary>
    [Test]
    public void DisabledRemovesRipple()
    {
        var comp = Context.Render<VekFab>(parameters => parameters
            .Add(p => p.Disabled, true));

        comp.Find("button").ClassList.Should().NotContain("vek-ripple");
    }

    /// <summary>
    /// A Label extends the FAB and renders between the start and end icons, which take IconSize rather than the FAB's Size.
    /// </summary>
    [Test]
    public void LabelExtendsFabBetweenIcons()
    {
        var comp = Context.Render<VekFab>(parameters => parameters
            .Add(p => p.Label, "Compose")
            .Add(p => p.StartIcon, Icons.Material.Filled.Edit)
            .Add(p => p.EndIcon, Icons.Material.Filled.Send)
            .Add(p => p.IconSize, Size.Small));

        comp.Find("button").ClassList.Should().Contain("vek-fab-extended");
        var label = comp.Find(".vek-fab-label");
        label.TextContent.Should().Be("Compose");
        label.Children.Should().HaveCount(2).And.AllSatisfy(icon => icon.ClassList.Should().Contain("vek-icon-size-small"));
    }

    /// <summary>
    /// Without icons or a label, the FAB renders no icon.
    /// </summary>
    [Test]
    public void RendersNoIconWhenNoneIsSet()
    {
        var comp = Context.Render<VekFab>();

        comp.FindAll(".vek-icon-root").Should().BeEmpty();
    }

    /// <summary>
    /// Turning off DropShadow removes the FAB's elevation.
    /// </summary>
    [Test]
    public void DropShadowOffRemovesElevation()
    {
        var comp = Context.Render<VekFab>(parameters => parameters
            .Add(p => p.DropShadow, false));

        comp.Find("button").ClassList.Should().Contain("vek-fab-disable-elevation");
    }
}
