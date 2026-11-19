// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.ToolBar;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class ToolBarTests : BunitTest
    {
        [Test]
        public void ToolBarWrapContent()
        {
            var component = Context.Render<ToolBarWrapContentTest>();
            var vekToolBar = component.Find(".vek-toolbar");

            vekToolBar.ClassList.Should().Contain("vek-toolbar-wrap-content");
        }

        /// <summary>
        /// ToolBar's WrapContent should be false by default
        /// </summary>
        [Test]
        public void ToolBar_WrapContent_ShouldBeFalseByDefault()
        {
            var comp = Context.Render<VekToolBar>();
            comp.Instance.WrapContent.Should().Be(false);
        }

        /// <summary>
        /// Adaptive overflow is disabled unless it is explicitly enabled.
        /// </summary>
        [Test]
        public void ToolBar_AdaptiveOverflow_ShouldBeFalseByDefault()
        {
            var comp = Context.Render<VekToolBar>();

            comp.Instance.AdaptiveOverflow.Should().BeFalse();
            comp.FindAll(".vek-toolbar-content-measure").Should().BeEmpty();
        }

        /// <summary>
        /// Items with lower collapse priority move into the overflow menu first.
        /// </summary>
        [Test]
        public void ToolBar_AdaptiveOverflow_CollapsesLowerPriorityItemsFirst()
        {
            var overflowedItems = VekToolBar.GetOverflowIndices(
                availableWidth: 220,
                overflowActivatorWidth: 48,
                items: [(80, 10), (80, 1), (80, 5)]);

            overflowedItems.Should().BeEquivalentTo([1]);
        }

        /// <summary>
        /// Items with equal collapse priority move in reverse toolbar order.
        /// </summary>
        [Test]
        public void ToolBar_AdaptiveOverflow_CollapsesTrailingItemsFirstWhenPrioritiesMatch()
        {
            var overflowedItems = VekToolBar.GetOverflowIndices(
                availableWidth: 220,
                overflowActivatorWidth: 48,
                items: [(80, 1), (80, 1), (80, 1)]);

            overflowedItems.Should().BeEquivalentTo([2]);
        }

        /// <summary>
        /// Items that exactly fit remain in the toolbar without reserving overflow space.
        /// </summary>
        [Test]
        public void ToolBar_AdaptiveOverflow_DoesNotCollapseItemsThatExactlyFit()
        {
            var overflowedItems = VekToolBar.GetOverflowIndices(
                availableWidth: 160,
                overflowActivatorWidth: 48,
                items: [(80, 0), (80, 0)]);

            overflowedItems.Should().BeEmpty();
        }
    }
}
