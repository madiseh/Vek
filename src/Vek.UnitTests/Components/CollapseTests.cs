// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Vek.UnitTests.TestComponents.Collapse;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class CollapseTests : BunitTest
    {
        [Test]
        public async Task Collapse_TwoWayBinding_Test1()
        {
            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<CollapseBindingTest>();
            var collapse = comp.FindComponent<VekCollapse>();

            collapse.Markup.Should().Contain("vek-collapse-entered");

            IElement Button() => comp.Find("#outside_btn");

            IRenderedComponent<VekSwitch<bool>> VekSwitch() => comp.FindComponent<VekSwitch<bool>>();
            // Initial state is expanded
            VekSwitch().Find("input").HasAttribute("checked").Should().BeTrue();

            // Collapse via button
            await Button().ClickAsync();
            VekSwitch().Find("input").HasAttribute("checked").Should().BeFalse();

            // Expand via button
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await Button().ClickAsync();
            VekSwitch().Find("input").HasAttribute("checked").Should().BeTrue();

            // Collapse via switch
            await VekSwitch().Find("input").ChangeAsync(false);
            VekSwitch().Find("input").HasAttribute("checked").Should().BeFalse();

            // Expand via switch
            await VekSwitch().Find("input").ChangeAsync(true);
            VekSwitch().Find("input").HasAttribute("checked").Should().BeTrue();
        }

        [Test]
        public async Task Collapse_OnAnimationEnd_ShouldIgnoreChildTransitionEnd()
        {
            var comp = Context.Render<CollapseAnimationEndChildTransitionTest>();
            comp.Find("#animation_end_count").TextContent.Should().Be("0");

            await comp.Find("#inner").TriggerEventAsync("ontransitionend", EventArgs.Empty);

            comp.Find("#animation_end_count").TextContent.Should().Be("0");
        }
    }
}
