using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.ExpansionPanel;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class ExpansionPanelTests : BunitTest
    {

        /// <summary>
        /// Single mode keeps one available panel open and prevents collapsing the active panel.
        /// </summary>
        [Test]
        public async Task AccordionSingleModeKeepsOnePanelExpanded()
        {
            var comp = Context.Render<VekExpansionPanels>(parameters => parameters
                .Add(p => p.ExpandMode, AccordionExpandMode.Single)
                .Add(p => p.ChildContent, builder =>
                {
                    builder.OpenComponent<VekExpansionPanel>(0);
                    builder.AddAttribute(1, nameof(VekExpansionPanel.Text), "Account details");
                    builder.CloseComponent();
                    builder.OpenComponent<VekExpansionPanel>(2);
                    builder.AddAttribute(3, nameof(VekExpansionPanel.Text), "Notification settings");
                    builder.CloseComponent();
                }));

            comp.FindAll(".vek-panel-expanded").Count.Should().Be(1);
            await comp.FindAll(".vek-expand-panel-header")[1].ClickAsync();
            comp.FindAll(".vek-panel-expanded").Count.Should().Be(1);
            await comp.FindAll(".vek-expand-panel-header")[1].ClickAsync();
            comp.FindAll(".vek-panel-expanded").Count.Should().Be(1);
            comp.FindAll(".vek-panel-expanded")[0].TextContent.Should().Contain("Notification settings");
        }

        /// <summary>
        /// Single-or-none mode lets the active panel close and multiple mode keeps independent panels open.
        /// </summary>
        [Test]
        public async Task AccordionExpandModesAllowTheirConfiguredStates()
        {
            var singleOrNone = Context.Render<VekExpansionPanels>(parameters => parameters
                .Add(p => p.ExpandMode, AccordionExpandMode.SingleOrNone)
                .Add(p => p.ChildContent, builder =>
                {
                    builder.OpenComponent<VekExpansionPanel>(0);
                    builder.AddAttribute(1, nameof(VekExpansionPanel.Text), "Shipping address");
                    builder.AddAttribute(2, nameof(VekExpansionPanel.Expanded), true);
                    builder.CloseComponent();
                }));

            await singleOrNone.Find(".vek-expand-panel-header").ClickAsync();
            singleOrNone.FindAll(".vek-panel-expanded").Should().BeEmpty();

            var multiple = Context.Render<VekExpansionPanels>(parameters => parameters
                .Add(p => p.ExpandMode, AccordionExpandMode.Multiple)
                .Add(p => p.ChildContent, builder =>
                {
                    builder.OpenComponent<VekExpansionPanel>(0);
                    builder.AddAttribute(1, nameof(VekExpansionPanel.Text), "Delivery options");
                    builder.CloseComponent();
                    builder.OpenComponent<VekExpansionPanel>(2);
                    builder.AddAttribute(3, nameof(VekExpansionPanel.Text), "Payment methods");
                    builder.CloseComponent();
                }));

            foreach (var header in multiple.FindAll(".vek-expand-panel-header"))
            {
                await header.ClickAsync();
            }
            multiple.FindAll(".vek-panel-expanded").Count.Should().Be(2);
        }

        /// <summary>
        /// The expansion icon can be rendered before the panel title.
        /// </summary>
        [Test]
        public void ExpansionPanelIconCanBePlacedAtStart()
        {
            var comp = Context.Render<VekExpansionPanel>(parameters => parameters
                .Add(p => p.IconPosition, AccordionExpandIconPosition.Start)
                .Add(p => p.Text, "Billing history"));

            var header = comp.Find(".vek-expand-panel-header");
            header.Children[0].ClassList.Should().Contain("vek-expand-panel-icon");
            header.Children[1].ClassList.Should().Contain("vek-expand-panel-text");
        }

        [OneTimeSetUp]
        public static void Init()
        {
            AssertionConfiguration.Current.Formatting.MaxDepth = 100;
            AssertionConfiguration.Current.Formatting.MaxLines = 5000;
        }

        [OneTimeTearDown]
        public static void Cleanup()
        {
            AssertionConfiguration.Current.Formatting.MaxDepth = 5;
            AssertionConfiguration.Current.Formatting.MaxLines = 100;
        }

        /// <summary>
        /// Expansion panel must expand and collapse in the right order
        /// Here we are open the first, then the third and then the second
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_Respects_Collapsing_Order()
        {
            var comp = Context.Render<ExpansionPanelExpansionsTest>();
            //the order in which the panels are going to be clicked
            //First, the first; then, the third, and then the second
            var sequence = new List<int> { 0, 2, 1 };
            foreach (var item in sequence)
            {
                await comp.InvokeAsync(() => comp.FindAll(".vek-expand-panel-header")[item].Click());

                var panels = comp.FindAll(".vek-expand-panel").ToList();

                //just the panel that was clicked has the expanded class
                panels[item].OuterHtml.Should().Contain("vek-panel-expanded");
                foreach (var other in sequence.Where(it => it != item))
                {
                    //the other panels haven't the class expanded
                    panels[other].OuterHtml.Should().NotContain("vek-panel-expanded");
                }
            }
        }

        /// <summary>
        /// Multiple expanded expansion panels should not enter an infinite loop 
        /// when MultiExpansionPanel is false
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_Without_MultiExpansion_Doesnt_Crash_With_Multiple_Expanded_Tabs()
        {
            var comp = Context.Render<ExpansionPanelMultiUnsetTest>();

            //click in the three headers
            //foreach (var header in comp.FindAll(".vek-expand-panel-header"))
            //{
            //    await header.ClickAsync();
            //}

            //Only one panel should be expanded
            var allPanels = comp.FindAll(".vek-expand-panel").ToList();

            var expandedPanels = comp.FindAll(".vek-panel-expanded").ToList();
            expandedPanels.Count.Should().Be(1);
            expandedPanels.First().Should().Be(allPanels.First());
        }

        /// <summary>
        /// MultiExpansion panel should not collapse other panels
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_MultiExpansion_Doesnt_Collapse_Others()
        {
            var comp = Context.Render<ExpansionPanelMultiExpansionTest>();

            //click in the three headers
            foreach (var header in comp.FindAll(".vek-expand-panel-header"))
            {
                await comp.InvokeAsync(() => header.Click());
            }

            //the three panels must be expanded
            var panels = comp.FindAll(".vek-panel-expanded").ToList();
            panels.Count.Should().Be(3);
        }

        /// <summary>
        /// Start expanded should expand panel
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_IsInitiallyExpanded_Expands()
        {
            var comp = Context.Render<ExpansionPanelStartExpandedTest>();

            // one panel is expanded initially
            var panels = comp.FindAll(".vek-panel-expanded").ToList();
            panels.Count.Should().Be(1);

            var header = comp.FindAll(".panel-two > .vek-expand-panel-header").First();
            await header.ClickAsync();

            //we could close the panel
            panels = comp.FindAll(".vek-panel-expanded").ToList();
            panels.Count.Should().Be(0);
        }

        /// <summary>
        /// Start expanded should work with multi expansion
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_IsInitiallyExpanded_Works_With_Multi_Expanded()
        {
            var comp = Context.Render<ExpansionPanelStartExpandedMultipleTest>();

            // three panels is expanded initially
            var panels = comp.FindAll(".vek-panel-expanded").ToList();
            panels.Count.Should().Be(3);

            //click in the three headers
            foreach (var header in comp.FindAll(".vek-expand-panel-header"))
            {
                await comp.InvokeAsync(() => header.Click());
            }

            //we could close them all
            panels = comp.FindAll(".vek-panel-expanded").ToList();
            panels.Count.Should().Be(0);
        }

        [Test]
        public async Task VekExpansionPanel_Other()
        {
            var comp = Context.Render<ExpansionPanelStartExpandedTest>();
            var panel = comp.FindComponent<VekExpansionPanel>();
            await panel.SetParametersAndRenderAsync(parameters => parameters.Add(parameter => parameter.Disabled, true));

            await comp.WaitForAssertionAsync(() => comp.Instance.Panel1Expanded.Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel2Expanded.Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel3Expanded.Should().BeFalse());
            await comp.InvokeAsync(panel.Instance.ToggleExpansionAsync);
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel1Expanded.Should().BeFalse()); // ToggleExpansionAsync checks for Disabled, so nothing happens
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel2Expanded.Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel3Expanded.Should().BeFalse());
            await comp.InvokeAsync(panel.Instance.ExpandAsync);
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel1Expanded.Should().BeTrue()); // ExpandAsync ignores Disabled
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel2Expanded.Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel3Expanded.Should().BeFalse());
            await comp.InvokeAsync(panel.Instance.CollapseAsync);
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel1Expanded.Should().BeFalse()); // ExpandAsync ignores Disabled
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel2Expanded.Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Panel3Expanded.Should().BeFalse());
        }

        /// <summary>
        /// Tests that ExpandAll method expands all panels.
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_ExpandAll()
        {
            var panels = Context.Render<VekExpansionPanels>();
            var panel1 = Context.Render<VekExpansionPanel>().Instance;
            var panel2 = Context.Render<VekExpansionPanel>().Instance;
            var panel3 = Context.Render<VekExpansionPanel>().Instance;
            await panels.Instance.AddPanelAsync(panel1);
            await panels.Instance.AddPanelAsync(panel2);
            await panels.Instance.AddPanelAsync(panel3);
            // We check _expandedState because we do not modify Expanded directly, therefore the parameter doesn't change.
            // For parameter to change you need to bind panels Expansion it via razor syntax, so that parent would update it.
            panels.Instance.Panels.Count.Should().Be(3);
            panel1._expandedState.Value.Should().BeFalse();
            panel2._expandedState.Value.Should().BeFalse();
            panel3._expandedState.Value.Should().BeFalse();
            await panels.Instance.ExpandAllAsync();
            panel1._expandedState.Value.Should().BeTrue();
            panel2._expandedState.Value.Should().BeTrue();
            panel3._expandedState.Value.Should().BeTrue();
        }

        /// <summary>
        /// Tests that CollapseAll method collapses all panels.
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_CollapseAll()
        {
            var panels = Context.Render<VekExpansionPanels>();
            var panel1 = Context.Render<VekExpansionPanel>().Instance;
            var panel2 = Context.Render<VekExpansionPanel>().Instance;
            var panel3 = Context.Render<VekExpansionPanel>().Instance;
            await panels.Instance.AddPanelAsync(panel1);
            await panels.Instance.AddPanelAsync(panel2);
            await panels.Instance.AddPanelAsync(panel3);
            await panel1.ExpandAsync();
            await panel2.ExpandAsync();
            await panel3.ExpandAsync();
            // We check _expandedState because we do not modify Expanded directly, therefore the parameter doesn't change.
            // For parameter to change you need to bind panels Expansion it via razor syntax, so that parent would update it.
            panels.Instance.Panels.Count.Should().Be(3);
            panel1._expandedState.Value.Should().BeTrue();
            panel2._expandedState.Value.Should().BeTrue();
            panel3._expandedState.Value.Should().BeTrue();
            await panels.Instance.CollapseAllAsync();
            panel1._expandedState.Value.Should().BeFalse();
            panel2._expandedState.Value.Should().BeFalse();
            panel3._expandedState.Value.Should().BeFalse();
        }

        /// <summary>
        /// Tests that CollapseAllExcept method collapses all panels except one.
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_CollapseAllExcept()
        {
            var panels = Context.Render<VekExpansionPanels>();
            var panel1 = Context.Render<VekExpansionPanel>().Instance;
            var panel2 = Context.Render<VekExpansionPanel>().Instance;
            var panel3 = Context.Render<VekExpansionPanel>().Instance;
            await panels.Instance.AddPanelAsync(panel1);
            await panels.Instance.AddPanelAsync(panel2);
            await panels.Instance.AddPanelAsync(panel3);
            await panel1.ExpandAsync();
            await panel2.ExpandAsync();
            await panel3.ExpandAsync();
            // We check _expandedState because we do not modify Expanded directly, therefore the parameter doesn't change.
            // For parameter to change you need to bind panels Expansion it via razor syntax, so that parent would update it.
            panels.Instance.Panels.Count.Should().Be(3);
            panel1._expandedState.Value.Should().BeTrue();
            panel2._expandedState.Value.Should().BeTrue();
            panel3._expandedState.Value.Should().BeTrue();
            await panels.Instance.CollapseAllExceptAsync(panel2);
            panel1._expandedState.Value.Should().BeFalse();
            panel2._expandedState.Value.Should().BeTrue();
            panel3._expandedState.Value.Should().BeFalse();
        }

        /// <summary>
        /// Test for https://github.com/madiseh/vek/issues/8429
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_TwoWayBinding()
        {
            var comp = Context.Render<ExpansionPanelTwoWayBIndingTest>();

            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[0].Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[1].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[2].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[3].Should().BeFalse());

            await comp.InvokeAsync(comp.Instance.ToggleExpansion2);

            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[0].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[1].Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[2].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[3].Should().BeFalse());

            await comp.InvokeAsync(comp.Instance.ToggleExpansion3);

            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[0].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[1].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[2].Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[3].Should().BeFalse());

            await comp.InvokeAsync(comp.Instance.ToggleExpansion4);

            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[0].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[1].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[2].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[3].Should().BeTrue());

            await comp.InvokeAsync(comp.Instance.ToggleExpansion3);

            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[0].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[1].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[2].Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[3].Should().BeFalse());

            await comp.InvokeAsync(comp.Instance.ToggleExpansion2);

            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[0].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[1].Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[2].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[3].Should().BeFalse());

            await comp.InvokeAsync(comp.Instance.ToggleExpansion1);

            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[0].Should().BeTrue());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[1].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[2].Should().BeFalse());
            await comp.WaitForAssertionAsync(() => comp.Instance.Expansion[3].Should().BeFalse());
        }

        /// <summary>
        /// Tests that the panel toggles expansion state when the Enter and Space keys are pressed.
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_Handles_KeyDown_For_Toggle()
        {
            var comp = Context.Render<ExpansionPanelKeyboardTest>();
            var header = comp.Find(".vek-expand-panel-header");

            comp.Markup.Should().NotContain("vek-panel-expanded");
            await header.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            comp.Markup.Should().Contain("vek-panel-expanded");
            await header.KeyDownAsync(new KeyboardEventArgs { Key = " " });
            comp.Markup.Should().NotContain("vek-panel-expanded");
        }

        /// <summary>
        /// Tests that buttons and other interactive content within a collapsed expansion panel 
        /// are properly hidden from accessibility tools and keyboard navigation by being placed 
        /// in a container with the visibility:hidden attribute.
        /// </summary>
        [Test]
        public void VekExpansionPanel_Button_IsInHiddenContainer_When_Panel_Collapsed()
        {
            var comp = Context.Render<ExpansionPanelKeyboardTest>();
            var button = comp.Find("button");
            var hiddenParent = button
                .GetAncestors()
                .OfType<IElement>()
                .FirstOrDefault(e => e.ClassList.Contains("invisible"));

            hiddenParent.Should().NotBeNull("button should not be accessible when the panel is collapsed");
        }

        /// <summary>
        /// Tests that HandleKeyDownAsync ignores key presses when the panel is disabled.
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_HandleKeyDownAsync_IgnoresKeys_When_Disabled()
        {
            var comp = Context.Render<VekExpansionPanel>(parameters => parameters
                .Add(p => p.Disabled, true)
                .Add(p => p.Text, "Disabled Panel"));

            var header = comp.Find(".vek-expand-panel-header");
            comp.Markup.Should().NotContain("vek-panel-expanded");

            // Try to expand with Enter key - should be ignored
            await header.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
            comp.Markup.Should().NotContain("vek-panel-expanded");

            // Try to expand with Space key - should be ignored
            await header.KeyDownAsync(new KeyboardEventArgs { Key = " " });
            comp.Markup.Should().NotContain("vek-panel-expanded");
        }

        /// <summary>
        /// Tests that a disabled panel puts "vek-disabled" on the header only, so the global ".vek-disabled .vek-icon-root" rule cannot grey out icons the user nests in the panel body (#11453).
        /// </summary>
        [Test]
        public async Task VekExpansionPanel_Disabled_AppliesDisabledClassToHeaderOnly()
        {
            var comp = Context.Render<VekExpansionPanel>(parameters => parameters
                .Add(p => p.Text, "Panel"));

            comp.Find(".vek-expand-panel").ClassList.Should().NotContain("vek-disabled");
            comp.Find(".vek-expand-panel-header").ClassList.Should().NotContain("vek-disabled");

            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.Disabled, true));

            comp.Find(".vek-expand-panel").ClassList.Should().NotContain("vek-disabled");
            comp.Find(".vek-expand-panel-header").ClassList.Should().Contain("vek-disabled");
        }

        /// <summary>
        /// Tests that content is rendered even when collapsed when KeepContentAlive is true.
        /// </summary>
        [Test]
        public void VekExpansionPanel_KeepContentAlive_True_RendersContentWhenCollapsed()
        {
            var comp = Context.Render<VekExpansionPanel>(parameters => parameters
                .Add(p => p.Text, "Test Panel")
                .Add(p => p.KeepContentAlive, true)
                .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<button>Test Button</button>")));

            comp.Markup.Should().NotContain("vek-panel-expanded");
            comp.Find("button").Should().NotBeNull();

            var contentDiv = comp.Find(".vek-collapse-container");
            contentDiv.ClassList.Contains("invisible").Should().BeTrue();
        }

        /// <summary>
        /// Tests that content is not rendered when collapsed when KeepContentAlive is false.
        /// </summary>
        [Test]
        public void VekExpansionPanel_KeepContentAlive_False_DoesNotRenderContentWhenCollapsed()
        {
            var comp = Context.Render<VekExpansionPanel>(parameters => parameters
                .Add(p => p.Text, "Test Panel")
                .Add(p => p.KeepContentAlive, false)
                .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<button>Test Button</button>")));

            comp.Markup.Should().NotContain("vek-panel-expanded");
            comp.FindAll("button").Should().BeEmpty();
        }

        /// <summary>
        /// Tests that when Gutters is true (default), both header and content have gutters classes.
        /// </summary>
        [Test]
        public void VekExpansionPanel_Gutters_True_AppliesGuttersToHeaderAndContent()
        {
            var comp = Context.Render<ExpansionPanelGuttersTrueTest>();

            var header = comp.Find(".vek-expand-panel-header");
            header.ClassList.Should().Contain("vek-expand-panel-header-gutters");

            var content = comp.Find(".vek-expand-panel-content");
            content.ClassList.Should().Contain("vek-expand-panel-gutters");
        }

        /// <summary>
        /// Tests that when Gutters is false, neither header nor content have gutters classes.
        /// </summary>
        [Test]
        public void VekExpansionPanel_Gutters_False_RemovesGuttersFromHeaderAndContent()
        {
            var comp = Context.Render<ExpansionPanelGuttersFalseTest>();

            var header = comp.Find(".vek-expand-panel-header");
            header.ClassList.Should().NotContain("vek-expand-panel-header-gutters");

            var content = comp.Find(".vek-expand-panel-content");
            content.ClassList.Should().NotContain("vek-expand-panel-gutters");
        }

        /// <summary>
        /// Changing the appearance parameters of the panels after the first render reaches a panel that has no content of its own.
        /// </summary>
        [Test]
        public async Task VekExpansionPanels_AppearanceChanged_ShouldReachPanelsWithoutContent()
        {
            var comp = Context.Render<VekExpansionPanels>(parameters => parameters
                .Add(x => x.Dense, false)
                .Add(x => x.Outlined, true)
                .Add(x => x.Gutters, true)
                .Add(x => x.Elevation, 1)
                .Add(x => x.ChildContent, builder =>
                {
                    builder.OpenComponent<VekExpansionPanel>(0);
                    builder.AddAttribute(1, nameof(VekExpansionPanel.Text), "Panel");
                    builder.CloseComponent();
                }));

            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(x => x.Dense, true)
                .Add(x => x.Outlined, false)
                .Add(x => x.Gutters, false)
                .Add(x => x.Elevation, 4));

            var panel = comp.Find(".vek-expand-panel");
            panel.ClassList.Should().Contain("vek-elevation-4").And.NotContain("vek-elevation-1").And.NotContain("vek-expand-panel-border");
            comp.Find(".vek-expand-panel-header").ClassList.Should().NotContain("vek-expand-panel-header-gutters");
            comp.Find(".vek-expand-panel-content").ClassList.Should().Contain("vek-expand-panel-dense");
        }

        /// <summary>
        /// Tests that when Parent Gutters is false, it overrides panel's Gutters setting.
        /// </summary>
        [Test]
        public void VekExpansionPanel_ParentGuttersFalse_OverridesChildGutters()
        {
            var comp = Context.Render<ExpansionPanelParentGuttersFalseTest>();

            var header = comp.Find(".vek-expand-panel-header");
            header.ClassList.Should().NotContain("vek-expand-panel-header-gutters");

            var content = comp.Find(".vek-expand-panel-content");
            content.ClassList.Should().NotContain("vek-expand-panel-gutters");
        }
    }
}
