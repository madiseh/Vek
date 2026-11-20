// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vek.Services;
using Vek.UnitTests.TestComponents.Tabs;

namespace Vek.UnitTests.Components.Tabs
{
    [TestFixture]
    public class TabPositionTests : BunitTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Context.Services.Add(new ServiceDescriptor(typeof(IResizeObserverFactory), new MockResizeObserverFactory()));
        }

        [Test]
        public void TabPosition_Default_IsTop()
        {
            var comp = Context.Render<TabPositionTestComponent>();

            comp.Instance.Tabs.TabPosition.Should().Be(TabPosition.Top);
            comp.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Top);

            var tabsRoot = comp.Find("div.vek-tabs");
            tabsRoot.ClassList.Should().NotContain("vek-tabs-vertical");
            tabsRoot.ClassList.Should().NotContain("vek-tabs-reverse");

            var tablist = comp.Find("div.vek-tabs-tabbar-wrapper");
            tablist.GetAttribute("aria-orientation").Should().Be("horizontal");
        }

        [Test]
        public void TabPosition_Top_AppliesExpectedClassesAndAttributes()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Top)
                .Add(p => p.Border, true));

            var tabsRoot = comp.Find("div.vek-tabs");
            tabsRoot.ClassList.Should().NotContain("vek-tabs-vertical");
            tabsRoot.ClassList.Should().NotContain("vek-tabs-reverse");

            var tabbar = comp.Find("div.vek-tabs-tabbar");
            tabbar.ClassList.Should().Contain("vek-tabs-border-top");

            var tablist = comp.Find("div.vek-tabs-tabbar-wrapper");
            tablist.GetAttribute("aria-orientation").Should().Be("horizontal");

            var slider = comp.Find("div.vek-tab-slider");
            slider.ClassList.Should().Contain("vek-tab-slider-horizontal");
            slider.ClassList.Should().NotContain("vek-tab-slider-horizontal-reverse");
            slider.ClassList.Should().NotContain("vek-tab-slider-vertical");
        }

        [Test]
        public void TabPosition_Bottom_AppliesExpectedClassesAndAttributes()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Bottom)
                .Add(p => p.Border, true));

            var tabsRoot = comp.Find("div.vek-tabs");
            tabsRoot.ClassList.Should().NotContain("vek-tabs-vertical");
            tabsRoot.ClassList.Should().Contain("vek-tabs-reverse");

            var tabbar = comp.Find("div.vek-tabs-tabbar");
            tabbar.ClassList.Should().Contain("vek-tabs-border-bottom");

            var tablist = comp.Find("div.vek-tabs-tabbar-wrapper");
            tablist.GetAttribute("aria-orientation").Should().Be("horizontal");

            var slider = comp.Find("div.vek-tab-slider");
            slider.ClassList.Should().Contain("vek-tab-slider-horizontal");
            slider.ClassList.Should().Contain("vek-tab-slider-horizontal-reverse");
            slider.ClassList.Should().NotContain("vek-tab-slider-vertical");
        }

        [Test]
        public void TabPosition_Left_AppliesVerticalClassesAndAttributes()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Left)
                .Add(p => p.Border, true));

            var tabsRoot = comp.Find("div.vek-tabs");
            tabsRoot.ClassList.Should().Contain("vek-tabs-vertical");
            tabsRoot.ClassList.Should().NotContain("vek-tabs-vertical-reverse");

            var tabbar = comp.Find("div.vek-tabs-tabbar");
            tabbar.ClassList.Should().Contain("vek-tabs-vertical");
            tabbar.ClassList.Should().Contain("vek-tabs-border-left");

            var tablist = comp.Find("div.vek-tabs-tabbar-wrapper");
            tablist.ClassList.Should().Contain("vek-tabs-vertical");
            tablist.GetAttribute("aria-orientation").Should().Be("vertical");

            var panels = comp.Find("div.vek-tabs-panels");
            panels.ClassList.Should().Contain("vek-tabs-vertical");

            var slider = comp.Find("div.vek-tab-slider");
            slider.ClassList.Should().Contain("vek-tab-slider-vertical");
            slider.ClassList.Should().NotContain("vek-tab-slider-vertical-reverse");
        }

        [Test]
        public void TabPosition_Right_AppliesVerticalAndReverseClassesAndAttributes()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Right)
                .Add(p => p.Border, true));

            var tabsRoot = comp.Find("div.vek-tabs");
            tabsRoot.ClassList.Should().Contain("vek-tabs-vertical");
            tabsRoot.ClassList.Should().Contain("vek-tabs-vertical-reverse");

            var tabbar = comp.Find("div.vek-tabs-tabbar");
            tabbar.ClassList.Should().Contain("vek-tabs-vertical");
            tabbar.ClassList.Should().Contain("vek-tabs-border-right");

            var tablist = comp.Find("div.vek-tabs-tabbar-wrapper");
            tablist.ClassList.Should().Contain("vek-tabs-vertical");
            tablist.GetAttribute("aria-orientation").Should().Be("vertical");

            var panels = comp.Find("div.vek-tabs-panels");
            panels.ClassList.Should().Contain("vek-tabs-vertical");

            var slider = comp.Find("div.vek-tab-slider");
            slider.ClassList.Should().Contain("vek-tab-slider-vertical");
            slider.ClassList.Should().Contain("vek-tab-slider-vertical-reverse");
        }

        [Test]
        public async Task TabPosition_DynamicUpdate_RecalculatesLayoutAndClasses()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Top));

            comp.Find("div.vek-tabs").ClassList.Should().NotContain("vek-tabs-vertical");

            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Left));

            comp.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Left);
            comp.Find("div.vek-tabs").ClassList.Should().Contain("vek-tabs-vertical");
            comp.Find("div.vek-tabs-tabbar-wrapper").GetAttribute("aria-orientation").Should().Be("vertical");

            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Right));

            comp.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Right);
            comp.Find("div.vek-tabs").ClassList.Should().Contain("vek-tabs-vertical-reverse");

            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Bottom));

            comp.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Bottom);
            comp.Find("div.vek-tabs").ClassList.Should().Contain("vek-tabs-reverse");
            comp.Find("div.vek-tabs").ClassList.Should().NotContain("vek-tabs-vertical");
        }

        [Test]
        public async Task TabPosition_Vertical_ArrowUpDownNavigation_SelectsTabs()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Left));

            // Initial active tab is 0
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 1");

            // ArrowDown from tab 0 to tab 1
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[0].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
            });

            // Enter to activate tab 1
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[1].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 2");

            // ArrowDown from tab 1 to tab 2
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[1].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
            });

            // Space to activate tab 2
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[2].KeyDownAsync(new KeyboardEventArgs { Key = " " });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 3");

            // ArrowUp from tab 2 to tab 1
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[2].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
            });

            // Enter to activate tab 1
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[1].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 2");
        }

        [Test]
        public async Task TabPosition_Vertical_ArrowUpDown_SkipsDisabledTab()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Left)
                .Add(p => p.DisableTab2, true));

            // ArrowDown from tab 0 should skip disabled tab 1 and focus tab 2
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[0].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
            });

            // Activating tab 2 (index 2 in DOM)
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[2].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 3");

            // ArrowUp from tab 2 should skip disabled tab 1 and focus tab 0
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[2].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
            });

            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[0].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 1");
        }

        [Test]
        public async Task TabPosition_Vertical_ArrowKeys_WrapAround()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Left));

            // ArrowUp from tab 0 wraps to last enabled tab (tab 3)
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[0].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
            });

            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[3].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 4");

            // ArrowDown from tab 3 wraps back to tab 0
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[3].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
            });

            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[0].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 1");
        }

        [Test]
        public async Task TabPosition_HomeAndEndKeys_NavigateToFirstAndLastTab()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Right));

            // End key focuses last tab
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[0].KeyDownAsync(new KeyboardEventArgs { Key = "End" });
            });

            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[3].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 4");

            // Home key focuses first tab
            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[3].KeyDownAsync(new KeyboardEventArgs { Key = "Home" });
            });

            await comp.InvokeAsync(async () =>
            {
                var tabs = comp.FindAll("div.vek-tab");
                await tabs[0].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            });
            comp.Find("div.vek-tabs-panels").InnerHtml.Should().Contain("Content 1");
        }

        [Test]
        public void TabPosition_RTL_AppliesCorrectReversal()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Left)
                .Add(p => p.RightToLeft, true)
                .AddCascadingValue("RightToLeft", true));

            // In RTL, TabPosition.Left requires row-reverse to put tab bar on visual left
            var tabsRoot = comp.Find("div.vek-tabs");
            tabsRoot.ClassList.Should().Contain("vek-tabs-vertical");
            tabsRoot.ClassList.Should().Contain("vek-tabs-vertical-reverse");

            var compRight = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Right)
                .Add(p => p.RightToLeft, true)
                .AddCascadingValue("RightToLeft", true));

            // In RTL, TabPosition.Right uses normal row because default row flows right-to-left
            var tabsRightRoot = compRight.Find("div.vek-tabs");
            tabsRightRoot.ClassList.Should().Contain("vek-tabs-vertical");
            tabsRightRoot.ClassList.Should().NotContain("vek-tabs-vertical-reverse");
        }

        [Test]
        public void TabPosition_LegacyPositionMapping_ResolvesCorrectly()
        {
            var compLeft = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.Position, Position.Left));
            compLeft.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Left);

            var compRight = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.Position, Position.Right));
            compRight.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Right);

            var compBottom = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.Position, Position.Bottom));
            compBottom.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Bottom);

            var compStart = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.Position, Position.Start)
                .Add(p => p.RightToLeft, false));
            compStart.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Left);

            var compStartRtl = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.Position, Position.Start)
                .Add(p => p.RightToLeft, true)
                .AddCascadingValue("RightToLeft", true));
            compStartRtl.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Right);
        }

        [Test]
        public void TabPosition_ExplicitTabPosition_OverridesLegacyPosition()
        {
            var comp = Context.Render<TabPositionTestComponent>(parameters => parameters
                .Add(p => p.ExplicitTabPosition, TabPosition.Right)
                .Add(p => p.Position, Position.Left));

            comp.Instance.Tabs.EffectiveTabPosition.Should().Be(TabPosition.Right);
            comp.Find("div.vek-tabs").ClassList.Should().Contain("vek-tabs-vertical-reverse");
        }
    }
}
