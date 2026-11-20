// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vek.Services;
using Vek.UnitTests.TestComponents.Tabs;
using Vek.UnitTests.TestComponents.Tabs.KeepTabsAlive;

namespace Vek.UnitTests.Components.Tabs
{
    [TestFixture]
    public class TabsRenderModesTests : BunitTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Context.Services.Add(new ServiceDescriptor(typeof(IResizeObserverFactory), new MockResizeObserverFactory()));
        }

        [Test]
        public void DefaultRenderMode_IsActiveTab()
        {
            var comp = Context.Render<TabsRenderModesTest>();

            comp.Instance.Tabs.RenderMode.Should().Be(TabsRenderMode.ActiveTab);
            comp.Instance.Tabs.EffectiveRenderMode.Should().Be(TabsRenderMode.ActiveTab);
        }

        [Test]
        public async Task ActiveTab_OnlyRendersActivePanelInDom()
        {
            var comp = Context.Render<TabsRenderModesTest>(parameters =>
                parameters.Add(p => p.RenderMode, TabsRenderMode.ActiveTab));

            // Only 1 panel in DOM initially
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(1);
            comp.FindAll("button").Count.Should().Be(1);
            comp.Find("div.vek-tabs-panels > div.vek-tab-panel").ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("p")[^1].MarkupMatches("<p>Panel 1<br></p>");

            // Click button on first panel
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=0");
            await comp.FindAll("button")[0].ClickAsync();
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=1");

            // Switch to second tab
            await comp.FindAll("div.vek-tab")[1].ClickAsync();

            // Only panel 2 is in DOM, panel 1 was removed
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(1);
            comp.FindAll("button").Count.Should().Be(1);
            comp.Find("div.vek-tabs-panels > div.vek-tab-panel").ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 2=0");

            // Switch back to first tab - panel 1 is recreated and state was discarded
            await comp.FindAll("div.vek-tab")[0].ClickAsync();

            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(1);
            comp.FindAll("button").Count.Should().Be(1);
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=0");
        }

        [Test]
        public async Task AllTabs_RendersAllPanelsImmediately()
        {
            var comp = Context.Render<TabsRenderModesTest>(parameters =>
                parameters.Add(p => p.RenderMode, TabsRenderMode.AllTabs));

            // All 3 panels in DOM immediately
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(3);
            comp.FindAll("button").Count.Should().Be(3);
            comp.FindAll("p")[^1].MarkupMatches("<p>Panel 1<br>Panel 2<br>Panel 3<br></p>");

            // Only the first panel is active
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[0].ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[1].ClassList.Should().NotContain("vek-tab-panel-active");
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[2].ClassList.Should().NotContain("vek-tab-panel-active");

            // Click button on first panel
            await comp.FindAll("button")[0].ClickAsync();
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=1");
            comp.FindAll("button")[1].TrimmedText().Should().Be("Panel 2=0");
            comp.FindAll("button")[2].TrimmedText().Should().Be("Panel 3=0");

            // Switch to second tab
            await comp.FindAll("div.vek-tab")[1].ClickAsync();

            // All 3 panels still exist in DOM
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(3);
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[0].ClassList.Should().NotContain("vek-tab-panel-active");
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[1].ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[2].ClassList.Should().NotContain("vek-tab-panel-active");

            // Click button on second panel
            await comp.FindAll("button")[1].ClickAsync();
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=1");
            comp.FindAll("button")[1].TrimmedText().Should().Be("Panel 2=1");

            // Switch back to first tab - panel 1 state was preserved
            await comp.FindAll("div.vek-tab")[0].ClickAsync();
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=1");
            comp.FindAll("button")[1].TrimmedText().Should().Be("Panel 2=1");
        }

        [Test]
        public async Task ActiveTabLazily_RendersOnDemandAndPreservesInDom()
        {
            var comp = Context.Render<TabsRenderModesTest>(parameters =>
                parameters.Add(p => p.RenderMode, TabsRenderMode.ActiveTabLazily));

            // Only 1 panel in DOM initially
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(1);
            comp.FindAll("button").Count.Should().Be(1);
            comp.Find("div.vek-tabs-panels > div.vek-tab-panel").ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("p")[^1].MarkupMatches("<p>Panel 1<br/></p>");

            // Click button on first panel
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=0");
            await comp.FindAll("button")[0].ClickAsync();
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=1");

            // Switch to second tab
            await comp.FindAll("div.vek-tab")[1].ClickAsync();

            // Now 2 panels are in DOM! Panel 1 is preserved, panel 2 is active
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(2);
            comp.FindAll("button").Count.Should().Be(2);
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[0].ClassList.Should().NotContain("vek-tab-panel-active");
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[1].ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("p")[^1].MarkupMatches("<p>Panel 1<br>Panel 2<br></p>");

            // Click button on second panel
            comp.FindAll("button")[1].TrimmedText().Should().Be("Panel 2=0");
            await comp.FindAll("button")[1].ClickAsync();
            comp.FindAll("button")[1].TrimmedText().Should().Be("Panel 2=1");

            // First panel button preserved its state
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=1");

            // Switch to third tab
            await comp.FindAll("div.vek-tab")[2].ClickAsync();

            // Now all 3 panels are in DOM
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(3);
            comp.FindAll("button").Count.Should().Be(3);
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[2].ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("p")[^1].MarkupMatches("<p>Panel 1<br>Panel 2<br>Panel 3<br></p>");

            // Switch back to first tab
            await comp.FindAll("div.vek-tab")[0].ClickAsync();

            // All 3 remain in DOM and panel 1 is active with preserved state
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(3);
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel")[0].ClassList.Should().Contain("vek-tab-panel-active");
            comp.FindAll("button")[0].TrimmedText().Should().Be("Panel 1=1");
            comp.FindAll("button")[1].TrimmedText().Should().Be("Panel 2=1");
        }

        [Test]
        public void EffectiveRenderMode_ResolvesLegacyPropertiesCorrectly()
        {
            var comp1 = Context.Render<VekTabs>(parameters =>
                parameters.Add(p => p.KeepPanelsAlive, true)
                          .Add(p => p.LazyLoadPanels, false));
            comp1.Instance.EffectiveRenderMode.Should().Be(TabsRenderMode.AllTabs);

            var comp2 = Context.Render<VekTabs>(parameters =>
                parameters.Add(p => p.KeepPanelsAlive, true)
                          .Add(p => p.LazyLoadPanels, true));
            comp2.Instance.EffectiveRenderMode.Should().Be(TabsRenderMode.ActiveTabLazily);

            var comp3 = Context.Render<VekTabs>(parameters =>
                parameters.Add(p => p.KeepPanelsAlive, false));
            comp3.Instance.EffectiveRenderMode.Should().Be(TabsRenderMode.ActiveTab);
        }

        [Test]
        public async Task DynamicRenderModeSwitching_RemovesCachedPanelsWhenSwitchingToActiveTab()
        {
            var comp = Context.Render<TabsRenderModesTest>(parameters =>
                parameters.Add(p => p.RenderMode, TabsRenderMode.ActiveTabLazily));

            // Activate tab 2 so 2 panels are cached in DOM
            await comp.FindAll("div.vek-tab")[1].ClickAsync();
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(2);

            // Dynamically change RenderMode to ActiveTab
            await comp.SetParametersAndRenderAsync(parameters =>
                parameters.Add(p => p.RenderMode, TabsRenderMode.ActiveTab));

            // Only the currently active panel (panel 2) should remain in DOM
            comp.FindAll("div.vek-tabs-panels > div.vek-tab-panel").Count.Should().Be(1);
            comp.Find("div.vek-tabs-panels > div.vek-tab-panel").ClassList.Should().Contain("vek-tab-panel-active");
        }
    }
}
