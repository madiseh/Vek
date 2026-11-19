using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.Badge;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class BadgeTests : BunitTest
    {
        /// <summary>
        /// A badge without an OnClick subscriber must not register a DOM click listener, so it cannot cost a round-trip.
        /// </summary>
        [Test]
        public async Task Badge_WithoutOnClick_RegistersNoClickListener()
        {
            var comp = Context.Render<VekBadge>(parameters => parameters.Add(x => x.Visible, true));
            var badge = comp.Find("span.vek-badge");

            var click = async () => await badge.ClickAsync(new MouseEventArgs());

            await click.Should().ThrowAsync<MissingEventHandlerException>();
        }

        /// <summary>
        /// A badge with an OnClick subscriber still registers the DOM click listener.
        /// </summary>
        [Test]
        public async Task Badge_WithOnClick_RegistersClickListener()
        {
            var clicked = false;
            var comp = Context.Render<VekBadge>(parameters => parameters
                .Add(x => x.Visible, true)
                .Add(x => x.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true)));

            await comp.Find("span.vek-badge").ClickAsync(new MouseEventArgs());

            clicked.Should().BeTrue();
        }

        [Test]
        public async Task Badge_Renders_Using_Default_Values()
        {
            var comp = Context.Render<VekBadge>();
            comp.FindAll("span").Should().HaveCount(3, "Default behavior of badge is to render 3 spans");

            await comp.InvokeAsync(() => comp.Instance.HandleBadgeClick(new MouseEventArgs()));
        }

        [Test]
        public async Task Badge_Renders_When_VisibleIsTrue()
        {
            var comp = Context.Render<VekBadge>();
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.Visible, true));
            comp.FindAll("span").Should().HaveCount(3, "Visible badge renders 3 spans");
        }

        [Test]
        public async Task Badge_Does_Not_Render_When_VisibleIsFalse()
        {
            var comp = Context.Render<VekBadge>();
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.Visible, false));
            comp.FindAll("span").Should().HaveCount(1, "Hidden badge renders 1 span");
        }

        /// <summary>
        /// A numeric badge shows a plus sign after its configured maximum.
        /// </summary>
        [Test]
        public void Badge_MaxNumber_CapsNumericContent()
        {
            var comp = Context.Render<VekBadge>(parameters => parameters
                .Add(x => x.Content, 150)
                .Add(x => x.MaxNumber, 99));

            comp.Find(".vek-badge").TextContent.Should().Be("99+");
        }

        /// <summary>
        /// The legacy maximum remains effective when the new maximum is not supplied.
        /// </summary>
        [Test]
        public void Badge_MaxNumber_UsesLegacyMaxWhenUnset()
        {
            var comp = Context.Render<VekBadge>(parameters => parameters
                .Add(x => x.Content, 42)
                .Add(x => x.Max, 20));

            comp.Find(".vek-badge").TextContent.Should().Be("20+");
        }

        /// <summary>
        /// A badge's logical position maps to the corresponding physical corner in the current reading direction.
        /// </summary>
        [TestCase(BadgePosition.TopEnd, false, "vek-badge-top right")]
        [TestCase(BadgePosition.TopStart, false, "vek-badge-top left")]
        [TestCase(BadgePosition.BottomEnd, false, "vek-badge-bottom right")]
        [TestCase(BadgePosition.BottomStart, false, "vek-badge-bottom left")]
        [TestCase(BadgePosition.TopEnd, true, "vek-badge-top left")]
        [TestCase(BadgePosition.TopStart, true, "vek-badge-top right")]
        [TestCase(BadgePosition.BottomEnd, true, "vek-badge-bottom left")]
        [TestCase(BadgePosition.BottomStart, true, "vek-badge-bottom right")]
        public void Badge_Position_UsesLogicalCorners(BadgePosition position, bool rightToLeft, string expectedClasses)
        {
            var comp = Context.Render<VekBadge>(parameters =>
            {
                parameters.AddCascadingValue("RightToLeft", rightToLeft);
                parameters.Add(x => x.Position, position);
            });

            comp.Find(".vek-badge-wrapper").ClassName.Should().Be("vek-badge-wrapper " + expectedClasses);
            comp.Find(".vek-badge").ClassName.Should().Contain(expectedClasses);
        }

        /// <summary>
        /// A status dot renders a compact indicator without numeric badge content.
        /// </summary>
        [Test]
        public void Badge_Dot_RendersCompactIndicator()
        {
            var comp = Context.Render<VekBadge>(parameters => parameters
                .Add(x => x.Dot, true)
                .Add(x => x.Content, 4));

            var badge = comp.Find(".vek-badge");
            badge.ClassList.Should().Contain("vek-badge-dot");
            badge.TextContent.Should().BeEmpty();
        }

        [Test]
        public async Task Badge_Click()
        {
            var comp = Context.Render<BadgeClickTest>();
            var badge = comp.FindComponent<VekBadge>();
            var numeric = comp.FindComponent<VekNumericField<int>>();
            await comp.WaitForAssertionAsync(() => numeric.Instance.Value.Should().Be(0));
            await comp.InvokeAsync(() => badge.Instance.HandleBadgeClick(new MouseEventArgs()));
            await comp.WaitForAssertionAsync(() => numeric.Instance.Value.Should().Be(1));
        }

        [Test]
        public void Badge_AccessibilityAttributes()
        {
            // Arrange
            const string BadgeAriaLabel = "New notifications";

            // Act
            var cut = Context.Render<VekBadge>(parameters => parameters
                .Add(p => p.BadgeAriaLabel, BadgeAriaLabel)
                .Add(p => p.Visible, true)
                .AddChildContent("Test Content")
            );

            // Assert
            var badge = cut.Find(".vek-badge");
            badge.GetAttribute("role").Should().Be("status");
            badge.GetAttribute("aria-live").Should().Be("polite");
            badge.GetAttribute("aria-label").Should().Be(BadgeAriaLabel);
        }
    }
}
