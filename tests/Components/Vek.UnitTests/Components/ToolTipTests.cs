using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Vek.Extensions;
using Vek.UnitTests.TestComponents.Tooltip;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class ToolTipTests : BunitTest
    {
        [Test]
        public void DefaultValue()
        {
            var toolTip = new VekTooltip();

            toolTip.Color.Should().Be(Color.Default);
            toolTip.Text.Should().BeEmpty();
            toolTip.Arrow.Should().BeFalse();
            toolTip.Duration.Should().Be(251);
            toolTip.Delay.Should().Be(0);
            toolTip.Placement.Should().Be(Placement.Bottom);
            toolTip.Inline.Should().BeTrue();
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task RenderContent(bool usingFocusout)
        {
            var comp = Context.Render<TooltipWithTextTest>(p => p.Add(
                x => x.TooltipTextContent, "my tooltip content text"
                ));

            var tooltipComp = comp.FindComponent<VekTooltip>().Instance;

            // content should always be visible
            var button = comp.Find("#sample-button");
            button.TextContent.Should().Be("My Button");

            button.ParentElement.ClassList.Should().Contain("vek-tooltip-root");

            //the button [0] and [1] the popover npde
            button.ParentElement.Children.Should().HaveCount(2);

            var popoverNode = button.ParentElement.Children[1];
            popoverNode.Id.Should().StartWith("popover-");

            var popoverContentNode = () => comp.Find($"#popovercontent-{popoverNode.Id.Substring(8)}");

            //no content for the popover node
            popoverContentNode().Children.Should().BeEmpty();

            //not visible by default
            tooltipComp.GetState(x => x.Visible).Should().BeFalse();

            //trigger pointerover
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());

            popoverContentNode().TextContent.Should().Be("my tooltip content text");
            popoverContentNode().ClassList.Should().Contain("d-flex");

            tooltipComp.GetState(x => x.Visible).Should().BeTrue();

            //trigger pointerleave
            if (!usingFocusout)
            {
                await button.ParentElement.PointerLeaveAsync(new PointerEventArgs());
            }
            else
            {
                button.ParentElement.FocusOut();
            }
            //no content should be visible
            popoverContentNode().Children.Should().BeEmpty();

            tooltipComp.GetState(x => x.Visible).Should().BeFalse();
        }

        [Test]
        public void NoPopoverIfThereIsNoContent()
        {
            var comp = Context.Render<TooltipWithTextTest>(p => p.Add(
                x => x.TooltipTextContent, null
                ));

            // content should always be visible
            var button = comp.Find("#sample-button");
            button.TextContent.Should().Be("My Button");

            button.ParentElement.ClassList.Should().Contain("vek-tooltip-root");

            //popover should be but not having a content
            button.ParentElement.Children.Should().ContainSingle();
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task RenderTooltipFragment(bool usingFocusout)
        {
            var comp = Context.Render<TooltipWithRenderFragmentContentTest>();

            // content should always be visible
            var button = comp.Find("#sample-button");
            button.TextContent.Should().Be("My Button");

            button.ParentElement.ClassList.Should().Contain("vek-tooltip-root");

            //the button [0] and [1] the popover node
            button.ParentElement.Children.Should().HaveCount(2);

            var popoverNode = button.ParentElement.Children[1];
            popoverNode.Id.Should().StartWith("popover-");

            var popoverContentNode = comp.Find($"#popovercontent-{popoverNode.Id.Substring(8)}");

            //no content for the popover node
            popoverContentNode.Children.Should().BeEmpty();

            //trigger pointerover
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());

            //content should be visible
            popoverContentNode.ClassList.Should().Contain("vek-tooltip");
            popoverContentNode.ClassList.Should().Contain("d-flex");

            comp.Find(".my-customer-paper").Children[0].TextContent.Should().Be("My content");

            //trigger pointerleave
            if (!usingFocusout)
            {
                await button.ParentElement.PointerLeaveAsync(new PointerEventArgs());
            }
            else
            {
                button.ParentElement.FocusOut();
            }
            //no content should be visible
            popoverContentNode.Children.Should().BeEmpty();
        }

        [Test]
        [TestCase(false, new[] { "vek-tooltip-root" })]
        [TestCase(true, new[] { "vek-tooltip-root", "vek-tooltip-inline" })]
        public void ContainerClass_PropertyRelations(bool inlineValue, string[] expectedClasses)
        {
            var comp = Context.Render<TooltipContainerPropertyTest>(p =>
            p.Add(x => x.Inline, inlineValue));

            comp.Nodes.Last().Should().BeAssignableTo<IHtmlElement>();

            var container = comp.Nodes.Last() as IHtmlElement;

            container.ClassList.Should().BeEquivalentTo(expectedClasses);
        }

        [Test]
        public async Task InnerClass_ChildContentWrapper()
        {
            var comp = Context.Render<TooltipPopoverClassPropertyTest>();

            var button = comp.Find("button");
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());

            var wrapperDivNode = comp.Find("#my-tooltip-content").ParentElement;

            wrapperDivNode.ClassList.Should().BeEquivalentTo(new[] { "d-block" });
        }

        [Test]
        [TestCase(false, new[] { "vek-tooltip" })]
        [TestCase(true, new[] { "vek-tooltip", "vek-tooltip-arrow" })]
        public async Task PopoverClass_PropertyArrow(bool arrowValue, string[] expectedClasses)
        {
            var comp = Context.Render<TooltipPopoverClassPropertyTest>(p =>
            p.Add(x => x.Arrow, arrowValue));

            var button = comp.Find("button");
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());

            var popoverContentNode = comp.Find("#my-tooltip-content").ParentElement.ParentElement;

            popoverContentNode.ClassList.Should().Contain(expectedClasses);
        }

        [Test]
        [TestCase(Color.Default, new[] { "vek-tooltip", "vek-tooltip-default" })]
        [TestCase(Color.Tertiary, new[] { "vek-tooltip", "vek-theme-tertiary" })]
        [TestCase(Color.Success, new[] { "vek-tooltip", "vek-theme-success" })]
        [TestCase(Color.Dark, new[] { "vek-tooltip", "vek-theme-dark" })]
        public async Task PopoverClass_PropertyColor(Color colorValue, string[] expectedClasses)
        {
            var comp = Context.Render<TooltipPopoverClassPropertyTest>(p =>
            p.Add(x => x.Color, colorValue));

            var button = comp.Find("button");
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());

            var popoverContentNode = comp.Find("#my-tooltip-content").ParentElement.ParentElement;

            popoverContentNode.ClassList.Should().Contain(expectedClasses);
        }

        [Test]
        [TestCase(Color.Default, false, new[] { "vek-tooltip", "vek-tooltip-default", })]
        [TestCase(Color.Default, true, new[] { "vek-tooltip", "vek-tooltip-default", "vek-tooltip-arrow" })]
        [TestCase(Color.Success, true, new[] { "vek-tooltip", "vek-theme-success", "vek-tooltip-arrow", "vek-border-success" })]
        [TestCase(Color.Success, false, new[] { "vek-tooltip", "vek-theme-success" })]
        public async Task PopoverClass_PropertyColorAndArrow(Color colorValue, bool arrowValue, string[] expectedClasses)
        {
            var comp = Context.Render<TooltipPopoverClassPropertyTest>(p =>
            {
                p.Add(x => x.Color, colorValue);
                p.Add(x => x.Arrow, arrowValue);
            });

            var button = comp.Find("button");
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());

            var popoverContentNode = comp.Find("#my-tooltip-content").ParentElement.ParentElement;

            popoverContentNode.ClassList.Should().Contain(expectedClasses);
        }

        [Test]
        [TestCase(Placement.Bottom, false, new[] { "vek-tooltip", "vek-tooltip-bottom-center", "vek-popover-anchor-bottom-center", "vek-popover-top-center" })]
        [TestCase(Placement.Bottom, true, new[] { "vek-tooltip", "vek-tooltip-bottom-center", "vek-popover-anchor-bottom-center", "vek-popover-top-center" })]
        [TestCase(Placement.Top, false, new[] { "vek-tooltip", "vek-tooltip-top-center", "vek-popover-anchor-top-center", "vek-popover-bottom-center" })]
        [TestCase(Placement.Top, true, new[] { "vek-tooltip", "vek-tooltip-top-center", "vek-popover-anchor-top-center", "vek-popover-bottom-center" })]
        [TestCase(Placement.Left, false, new[] { "vek-tooltip", "vek-tooltip-center-left", "vek-popover-anchor-center-left", "vek-popover-center-right" })]
        [TestCase(Placement.Left, true, new[] { "vek-tooltip", "vek-tooltip-center-left", "vek-popover-anchor-center-left", "vek-popover-center-right" })]
        [TestCase(Placement.Start, false, new[] { "vek-tooltip", "vek-tooltip-center-left", "vek-popover-anchor-center-left", "vek-popover-center-right" })]
        [TestCase(Placement.Start, true, new[] { "vek-tooltip", "vek-tooltip-center-right", "vek-popover-anchor-center-right", "vek-popover-center-left" })]
        [TestCase(Placement.Right, false, new[] { "vek-tooltip", "vek-tooltip-center-right", "vek-popover-anchor-center-right", "vek-popover-center-left" })]
        [TestCase(Placement.Right, true, new[] { "vek-tooltip", "vek-tooltip-center-right", "vek-popover-anchor-center-right", "vek-popover-center-left" })]
        [TestCase(Placement.End, false, new[] { "vek-tooltip", "vek-tooltip-center-right", "vek-popover-anchor-center-right", "vek-popover-center-left" })]
        [TestCase(Placement.End, true, new[] { "vek-tooltip", "vek-tooltip-center-left", "vek-popover-anchor-center-left", "vek-popover-center-right" })]

        public async Task PopoverClass_Placement(Placement placementValue, bool rtlValue, string[] expectedClasses)
        {
            var comp = Context.Render<TooltipPlacementPropertyTest>(p =>
            {
                p.Add(x => x.Placement, placementValue);
                p.Add(x => x.RightToLeft, rtlValue);
            });

            var button = comp.Find("button");
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());

            var popoverContentNode = comp.Find("#my-tooltip-content").ParentElement.ParentElement;

            popoverContentNode.ClassList.Should().Contain(expectedClasses);
        }

        [Test]
        public async Task Tooltip_On_Focus()
        {
            var comp = Context.Render<TooltipPlacementPropertyTest>();

            var button = comp.Find("button");
            await button.ParentElement.FocusInAsync(new FocusEventArgs());

            var popoverContentNode = comp.Find("#my-tooltip-content").ParentElement;

            popoverContentNode.Should().NotBeNull();
        }

        [Test]
        public async Task Tooltip_On_Click()
        {
            var comp = Context.Render<TooltipClickTest>();
            var tooltipComp = comp.FindComponent<VekTooltip>().Instance;
            tooltipComp.Visible.Should().BeFalse();
            var button = comp.Find("button");
            await button.ParentElement.PointerUpAsync(new PointerEventArgs());

            var popoverContentNode = comp.Find("#my-tooltip-content").ParentElement;
            tooltipComp.GetState(x => x.Visible).Should().BeTrue();
            popoverContentNode.Should().NotBeNull();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Visible_ByDefault(bool usingFocusout)
        {
            var comp = Context.Render<TooltipVisiblePropTest>(p =>
            {
                p.Add(x => x.TooltipVisible, true);
            });
            var tooltipComp = comp.FindComponent<VekTooltip>().Instance;

            comp.Instance.TooltipVisible.Should().BeTrue();
            tooltipComp.Visible.Should().BeTrue(); //tooltip is visible by default in this case

            var button = comp.Find("button");

            if (!usingFocusout)
            {
                await button.ParentElement.PointerLeaveAsync(new PointerEventArgs());
            }
            else
            {
                button.ParentElement.FocusOut();
            }

            tooltipComp.Visible.Should().BeFalse();
            comp.Instance.TooltipVisible.Should().BeFalse();
        }

        [Test]
        public async Task Tooltip_Style_Respected()
        {
            var comp = Context.Render<TestComponents.Tooltip.TooltipStylingTest>();
            var tooltipComp = comp.FindComponent<VekTooltip>().Instance;
            var button = comp.Find("button");
            await button.ParentElement.PointerUpAsync(new PointerEventArgs());

            tooltipComp.Style.Should().Contain("background-color").And.Contain("orangered");
        }

        [Test]
        public void Tooltip_Disabled_Default_False()
        {
            var comp = Context.Render<TooltipDisabledPropertyTest>();
            var tooltipComp = comp.FindComponent<VekTooltip>().Instance;
            tooltipComp.Disabled.Should().BeFalse();
        }

        [Test]
        public async Task Tooltip_Disabled_Button_OnFocusIn_NoPopover()
        {
            var comp = Context.Render<TooltipDisabledPropertyTest>(p =>
            {
                p.Add(x => x.TooltipDisabled, true);
            });

            var button = comp.Find("button");
            await button.ParentElement.FocusInAsync(new FocusEventArgs());
            comp.FindAll("div.vek-popover-open").Count.Should().Be(0);
        }

        [Test]
        public async Task Tooltip_Disabled_Button_OnPointerEnter_NoPopover()
        {
            var comp = Context.Render<TooltipDisabledPropertyTest>(p =>
            {
                p.Add(x => x.TooltipDisabled, true);
            });

            var button = comp.Find("button");
            await button.ParentElement.PointerEnterAsync(new PointerEventArgs());
            comp.FindAll("div.vek-popover-open").Count.Should().Be(0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Tooltip_Handle_Pointer_Events(bool showOnHover)
        {
            var comp = Context.Render<VekTooltip>(parameters => parameters
                .Add(x => x.ShowOnHover, showOnHover)
                .Add(x => x.ShowOnClick, true)
                .Add(x => x.Text, "tooltip text")
            );

            var div = comp.Find(".vek-tooltip-root");
            div.Should().NotBeNull();

            var tooltip = comp.Instance;
            tooltip.Should().NotBeNull();

            await tooltip.HandlePointerEnterAsync();
            tooltip.GetState(x => x.Visible).Should().Be(showOnHover);

            if (showOnHover)
            {
                await tooltip.HandlePointerLeaveAsync();
                tooltip.GetState(x => x.Visible).Should().Be(!showOnHover);
            }

            await div.PointerEnterAsync(new PointerEventArgs());
            tooltip.GetState(x => x.Visible).Should().Be(showOnHover);

            if (showOnHover)
            {
                await div.PointerLeaveAsync(new PointerEventArgs());
                tooltip.GetState(x => x.Visible).Should().Be(!showOnHover);
            }
        }

        [Test]
        public async Task Tooltip_ShouldNotRerenderChildContent_WhenVisibleChangesInternally()
        {
            var complexData = new object();
            var comp = Context.Render<VekTooltip>(parameters => parameters
                .Add(p => p.Text, "Tooltip")
                .AddChildContent<ComplexComponent>(child => child.Add(c => c.Data, complexData))
            );

            var complexComp = comp.FindComponent<ComplexComponent>().Instance;
            var initialCount = complexComp.RenderCount;
            initialCount.Should().Be(1);

            // Simulate hover via bUnit's event trigger
            var div = comp.Find(".vek-tooltip-root");
            await div.PointerEnterAsync(new PointerEventArgs());

            // Verify that the tooltip is now visible
            comp.Instance.GetState(x => x.Visible).Should().BeTrue();

            // Check if ChildContent re-rendered
            complexComp.RenderCount.Should().Be(initialCount);
        }

        [Test]
        public async Task Tooltip_ShouldRerenderChildContent_WhenParentRerenders()
        {
            var complexData = new object();
            var comp = Context.Render<VekTooltip>(parameters => parameters
                .Add(p => p.Text, "Tooltip")
                .AddChildContent<ComplexComponent>(child => child.Add(c => c.Data, complexData))
            );

            var complexComp = comp.FindComponent<ComplexComponent>().Instance;
            var initialCount = complexComp.RenderCount;
            initialCount.Should().Be(1);

            // Simulate parent re-render by setting a parameter again
            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.Text, "Updated Tooltip")
            );

            // Should re-render because it should behave as if not encapsulated
            complexComp.RenderCount.Should().Be(initialCount + 1);
        }

        private class ComplexComponent : ComponentBase
        {
            [Parameter]
            public object Data { get; set; }

            public int RenderCount { get; private set; }

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                base.BuildRenderTree(builder);
                RenderCount++;
                builder.AddContent(0, "Complex Component Content");
            }
        }
    }
}
