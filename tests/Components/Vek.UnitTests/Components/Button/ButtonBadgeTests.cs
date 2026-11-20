using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

/// <summary>
/// Verifies floating badge adornments, status indicator dots, and capped counter numbers rendered directly by <see cref="VekButton"/>.
/// </summary>
[TestFixture]
public class ButtonBadgeTests : BunitTest
{
    /// <summary>
    /// A button with no badge parameters does not render any badge markup.
    /// </summary>
    [Test]
    public void ButtonBadge_DoesNotRender_WhenBadgeUnset()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .AddChildContent("Click me"));

        comp.Find("button").ClassList.Should().NotContain("vek-button-has-badge");
        comp.FindAll(".vek-badge-wrapper").Should().BeEmpty();
        comp.FindAll(".vek-badge").Should().BeEmpty();
    }

    /// <summary>
    /// Setting badge content renders a notification badge with default appearance and ARIA status attributes.
    /// </summary>
    [Test]
    public void ButtonBadge_RendersNotificationContent()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, 5)
            .AddChildContent("Notifications"));

        comp.Find("button").ClassList.Should().Contain("vek-button-has-badge");

        var wrapper = comp.Find(".vek-badge-wrapper");
        wrapper.ClassList.Should().Contain(["vek-badge-top", "right"]);

        var badge = comp.Find(".vek-badge");
        badge.ClassList.Should().Contain(["vek-button-badge", "vek-badge-top", "right", "vek-theme-primary"]);
        badge.GetAttribute("role").Should().Be("status");
        badge.GetAttribute("aria-live").Should().Be("polite");
        badge.TextContent.Trim().Should().Be("5");
    }

    /// <summary>
    /// String badge content is rendered directly as text.
    /// </summary>
    [Test]
    public void ButtonBadge_RendersStringContent()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, "NEW")
            .AddChildContent("Feature"));

        var badge = comp.Find(".vek-badge");
        badge.TextContent.Trim().Should().Be("NEW");
    }

    /// <summary>
    /// A numeric badge exceeds the maximum value displays the capped number with a plus sign.
    /// </summary>
    [TestCase(100, 99, "99+")]
    [TestCase(150, 99, "99+")]
    [TestCase(42, 99, "42")]
    [TestCase(10, 9, "9+")]
    public void ButtonBadge_CapsNumericContent(int content, int max, string expectedText)
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, content)
            .Add(p => p.BadgeMax, max)
            .AddChildContent("Inbox"));

        var badge = comp.Find(".vek-badge");
        badge.TextContent.Trim().Should().Be(expectedText);
    }

    /// <summary>
    /// A string containing a parsable integer also respects the configured maximum.
    /// </summary>
    [Test]
    public void ButtonBadge_CapsNumericStringContent()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, "125")
            .Add(p => p.BadgeMax, 99));

        var badge = comp.Find(".vek-badge");
        badge.TextContent.Trim().Should().Be("99+");
    }

    /// <summary>
    /// A status dot renders as a compact circular indicator without textual content.
    /// </summary>
    [Test]
    public void ButtonBadge_RendersStatusDot()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeDot, true)
            .Add(p => p.BadgeColor, Color.Success)
            .Add(p => p.BadgeContent, "ShouldBeIgnoredInDotMode")
            .AddChildContent("Online User"));

        var badge = comp.Find(".vek-badge");
        badge.ClassList.Should().Contain(["vek-badge-dot", "vek-theme-success"]);
        badge.TextContent.Trim().Should().BeEmpty();
    }

    /// <summary>
    /// The badge adornment supports different theme colors.
    /// </summary>
    [TestCase(Color.Primary, "vek-theme-primary")]
    [TestCase(Color.Secondary, "vek-theme-secondary")]
    [TestCase(Color.Error, "vek-theme-error")]
    [TestCase(Color.Warning, "vek-theme-warning")]
    [TestCase(Color.Info, "vek-theme-info")]
    [TestCase(Color.Default, "vek-badge-default")]
    public void ButtonBadge_AppliesThemeColors(Color color, string expectedClass)
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, 1)
            .Add(p => p.BadgeColor, color));

        var badge = comp.Find(".vek-badge");
        badge.ClassList.Should().Contain(expectedClass);
    }

    /// <summary>
    /// Logical corner positions map properly based on right-to-left layout direction.
    /// </summary>
    [TestCase(BadgePosition.TopEnd, false, "vek-badge-top right")]
    [TestCase(BadgePosition.TopStart, false, "vek-badge-top left")]
    [TestCase(BadgePosition.BottomEnd, false, "vek-badge-bottom right")]
    [TestCase(BadgePosition.BottomStart, false, "vek-badge-bottom left")]
    [TestCase(BadgePosition.TopEnd, true, "vek-badge-top left")]
    [TestCase(BadgePosition.TopStart, true, "vek-badge-top right")]
    [TestCase(BadgePosition.BottomEnd, true, "vek-badge-bottom left")]
    [TestCase(BadgePosition.BottomStart, true, "vek-badge-bottom right")]
    public void ButtonBadge_LogicalPosition_RTL_Mapping(BadgePosition position, bool rightToLeft, string expectedClasses)
    {
        var comp = Context.Render<VekButton>(parameters =>
        {
            parameters.AddCascadingValue("RightToLeft", rightToLeft);
            parameters.Add(p => p.BadgeContent, "3");
            parameters.Add(p => p.BadgePosition, position);
        });

        comp.Find(".vek-badge-wrapper").ClassName.Should().Be("vek-badge-wrapper " + expectedClasses);
        comp.Find(".vek-badge").ClassName.Should().Contain(expectedClasses);
    }

    /// <summary>
    /// When BadgeVisible is set to false, the badge is hidden even if BadgeContent or BadgeDot is configured.
    /// </summary>
    [Test]
    public void ButtonBadge_HidesWhenVisibleIsFalse()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, 9)
            .Add(p => p.BadgeDot, true)
            .Add(p => p.BadgeVisible, false));

        comp.FindAll(".vek-badge-wrapper").Should().BeEmpty();
        comp.FindAll(".vek-badge").Should().BeEmpty();
        comp.Find("button").ClassList.Should().NotContain("vek-button-has-badge");
    }

    /// <summary>
    /// Overlap and Bordered parameters apply their respective CSS modifier classes.
    /// </summary>
    [Test]
    public void ButtonBadge_AppliesOverlapAndBordered()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, 1)
            .Add(p => p.BadgeOverlap, true)
            .Add(p => p.BadgeBordered, true)
            .Add(p => p.BadgeClass, "custom-badge-class"));

        var badge = comp.Find(".vek-badge");
        badge.ClassList.Should().Contain(["vek-badge-overlap", "vek-badge-bordered", "custom-badge-class"]);
    }

    /// <summary>
    /// BadgeAriaLabel is rendered when specified.
    /// </summary>
    [Test]
    public void ButtonBadge_RendersAriaLabel()
    {
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, 7)
            .Add(p => p.BadgeAriaLabel, "7 unread alerts"));

        var badge = comp.Find(".vek-badge");
        badge.GetAttribute("aria-label").Should().Be("7 unread alerts");
    }

    /// <summary>
    /// Button clicks function normally when a badge adornment is present.
    /// </summary>
    [Test]
    public async Task ButtonBadge_AllowsButtonClick()
    {
        var clicked = false;
        var comp = Context.Render<VekButton>(parameters => parameters
            .Add(p => p.BadgeContent, 3)
            .Add(p => p.OnClick, (MouseEventArgs _) => clicked = true)
            .AddChildContent("Click"));

        await comp.Find("button").ClickAsync();

        clicked.Should().BeTrue();
    }
}
