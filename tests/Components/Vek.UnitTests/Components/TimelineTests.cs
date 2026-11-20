// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AngleSharp.Css.Dom;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Bunit;
using Vek.UnitTests.TestComponents.Timeline;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class TimelineTests : BunitTest
    {
        [Test]
        public void Timeline_DefaultValues()
        {
            var comp = Context.Render<VekTimeline>();

            comp.Instance.TimelineOrientation.Should().Be(TimelineOrientation.Vertical);
            comp.Instance.TimelinePosition.Should().Be(TimelinePosition.Alternate);
            comp.Instance.TimelineAlign.Should().Be(TimelineAlign.Default);
            comp.Instance.Reverse.Should().Be(false);
            comp.Instance.Modifiers.Should().Be(true);

        }

        /// <summary>
        /// Default Timeline, with five items.
        /// Testing if selection is sync with move commands.
        /// </summary>
        [Test]
        public async Task Timeline()
        {
            var comp = Context.Render<TimelineTest>();
            // print the generated html
            //// select elements needed for the test
            var timeline = comp.FindComponent<VekTimeline>().Instance;
            //// validating some renders
            timeline.Should().NotBeNull();
            await comp.WaitForAssertionAsync(() => comp.FindAll("div.vek-timeline").Count.Should().Be(1));
            comp.FindAll("div.vek-timeline-item").Count.Should().Be(5);
            var items = comp.FindComponents<VekTimelineItem>();
            items.Count.Should().Be(5);
            //// changing current index
            for (var i = 1; i <= 4; i++)
            {
                await comp.InvokeAsync(() => timeline.MoveTo(i));
                timeline.SelectedIndex.Should().Be(i);
                timeline.SelectedContainer.Should().Be(items[i].Instance);
            }
            await comp.InvokeAsync(() => timeline.MoveTo(0));
            timeline.SelectedIndex.Should().Be(0);
            timeline.SelectedContainer.Should().Be(items[0].Instance);
        }

        [Test]

        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Alternate, false, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Start, false, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Left, false, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Right, false, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.End, false, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]

        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Top, false, new[] { "vek-timeline-horizontal", "vek-timeline-position-top" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Bottom, false, new[] { "vek-timeline-horizontal", "vek-timeline-position-bottom" })]

        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Alternate, false, new[] { "vek-timeline-vertical", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Top, false, new[] { "vek-timeline-vertical", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Bottom, false, new[] { "vek-timeline-vertical", "vek-timeline-position-alternate" })]

        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Start, false, new[] { "vek-timeline-vertical", "vek-timeline-position-start" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.End, false, new[] { "vek-timeline-vertical", "vek-timeline-position-end" })]

        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Left, false, new[] { "vek-timeline-vertical", "vek-timeline-position-start" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Right, false, new[] { "vek-timeline-vertical", "vek-timeline-position-end" })]

        //RTL to true

        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Alternate, true, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Start, true, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Left, true, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Right, true, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.End, true, new[] { "vek-timeline-horizontal", "vek-timeline-position-alternate" })]

        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Top, true, new[] { "vek-timeline-horizontal", "vek-timeline-position-top" })]
        [TestCase(TimelineOrientation.Horizontal, TimelinePosition.Bottom, true, new[] { "vek-timeline-horizontal", "vek-timeline-position-bottom" })]

        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Alternate, true, new[] { "vek-timeline-vertical", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Top, true, new[] { "vek-timeline-vertical", "vek-timeline-position-alternate" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Bottom, true, new[] { "vek-timeline-vertical", "vek-timeline-position-alternate" })]

        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Start, true, new[] { "vek-timeline-vertical", "vek-timeline-position-start" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.End, true, new[] { "vek-timeline-vertical", "vek-timeline-position-end" })]

        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Left, true, new[] { "vek-timeline-vertical", "vek-timeline-position-end" })]
        [TestCase(TimelineOrientation.Vertical, TimelinePosition.Right, true, new[] { "vek-timeline-vertical", "vek-timeline-position-start" })]

        public async Task TimelineTest_Position(TimelineOrientation orientation, TimelinePosition position, bool rtl, string[] expectedClass)
        {
            var comp = Context.Render<TimelineTest>(p => p.AddCascadingValue("RightToLeft", rtl));

            var timeline = comp.FindComponent<VekTimeline>();

            await timeline.SetParametersAndRenderAsync(p =>
            {
                p.Add(x => x.TimelineOrientation, orientation);
                p.Add(x => x.TimelinePosition, position);
            });

            timeline.Nodes.Should().ContainSingle();
            timeline.Nodes[0].Should().BeAssignableTo<IHtmlDivElement>();

            (timeline.Nodes[0] as IHtmlDivElement).ClassList.Should().Contain(expectedClass);
        }

        [Test]
        public async Task Timeline_SelectItem()
        {
            var comp = Context.Render<TimelineTest>();

            var itemsDiv = comp.FindAll(".vek-timeline-item");

            itemsDiv.Should().HaveCount(5);

            for (var i = 0; i < 5; i++)
            {
                await itemsDiv[i].ClickAsync();

                comp.Instance.SelectedIndex.Should().Be(i);
            }
        }

        [Test]
        public async Task Timeline_DotClass()
        {
            var comp = Context.Render<TimelineTest>();
            var firstItem = comp.FindComponent<VekTimelineItem>();

            comp.Find("div.vek-timeline-item-dot-inner").ClassList.Should().NotContain("timeline-red-dot");

            await firstItem.SetParametersAndRenderAsync(p =>
            {
                p.Add(t => t.DotClass, "timeline-red-dot");
            });

            comp.Find("div.vek-timeline-item-dot-inner").ClassList.Should().Contain("timeline-red-dot");
        }

        [Test]
        public async Task Timeline_DotStyle()
        {
            var comp = Context.Render<TimelineTest>();
            var firstItem = comp.FindComponent<VekTimelineItem>();
            comp.Find("div.vek-timeline-item-dot-inner").GetStyle()["background-color"].Should().Be("");

#pragma warning disable CS0618 // Type or member is obsolete
            await firstItem.SetParametersAndRenderAsync(p =>
            {
                p.Add(t => t.DotStyle, "background-color: #ff0000");
            });
#pragma warning restore CS0618 // Type or member is obsolete

            comp.Find("div.vek-timeline-item-dot-inner").GetStyle()["background-color"].Should().Be("rgba(255, 0, 0, 1)");
        }

        [Test]
        public async Task Timeline_Icon()
        {
            var comp = Context.Render<TimelineTest>();
            var firstItem = comp.FindComponent<VekTimelineItem>();

            // No icon by default.
            comp.FindAll(".vek-timeline-item-dot-inner .vek-icon-root").Should().BeEmpty();

            await firstItem.SetParametersAndRenderAsync(p =>
            {
                p.Add(t => t.Icon, Icons.Material.Filled.Groups);
            });

            var icon = comp.Find(".vek-timeline-item-dot-inner .vek-icon-root");
            icon.ClassList.Should().Contain("vek-timeline-item-dot-icon");
            comp.Markup.Should().Contain(Icons.Material.Filled.Groups);
            // Only the item with an icon set renders one.
            comp.FindAll(".vek-icon-root").Count.Should().Be(1);
        }

        [Test]
        public async Task Timeline_Icon_EmptyRendersNoIcon()
        {
            var comp = Context.Render<TimelineTest>();
            var firstItem = comp.FindComponent<VekTimelineItem>();

            await firstItem.SetParametersAndRenderAsync(p =>
            {
                p.Add(t => t.Icon, string.Empty);
            });

            comp.FindAll(".vek-icon-root").Should().BeEmpty();
        }

        [Test]
        public async Task Timeline_ItemDotTakesPrecedenceOverIcon()
        {
            var comp = Context.Render<TimelineTest>();
            var firstItem = comp.FindComponent<VekTimelineItem>();

            await firstItem.SetParametersAndRenderAsync(p =>
            {
                p.Add(t => t.Icon, Icons.Material.Filled.Groups);
                p.Add(t => t.ItemDot, "<span class=\"custom-dot\">X</span>");
            });

            comp.FindAll(".vek-timeline-item-dot-inner .custom-dot").Should().ContainSingle();
            comp.FindAll(".vek-timeline-item-dot-icon").Should().BeEmpty();
        }

        [Test]
        public async Task Timeline_HideDotHidesIcon()
        {
            var comp = Context.Render<TimelineTest>();
            var firstItem = comp.FindComponent<VekTimelineItem>();

            await firstItem.SetParametersAndRenderAsync(p =>
            {
                p.Add(t => t.Icon, Icons.Material.Filled.Groups);
                p.Add(t => t.HideDot, true);
            });

            // The whole dot (and therefore the icon) is gone for that item.
            comp.FindAll(".vek-timeline-item").Count.Should().Be(5);
            comp.FindAll(".vek-timeline-item-dot").Count.Should().Be(4);
            comp.FindAll(".vek-icon-root").Should().BeEmpty();
        }

        /// <summary>
        /// Test horizontal timeline inside vertical timeline.
        /// </summary>
        [Test]
        public async Task HorizontalTimelineInsideVerticalTimeline()
        {
            var comp = Context.Render<HorizontalTimelineInsideVerticalTimelineTest>();
            // select elements needed for the test
            var timeline = comp.FindComponent<VekTimeline>().Instance;
            // validating some renders
            timeline.Should().NotBeNull();
            await comp.WaitForAssertionAsync(() => comp.FindAll("div.vek-timeline").Count.Should().Be(2));
            comp.FindAll("div.vek-timeline-item").Count.Should().Be(9);
            var items = comp.FindComponents<VekTimelineItem>();
            items.Count.Should().Be(9);
        }
    }
}
