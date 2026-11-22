using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.SplitPanel;

namespace Vek.UnitTests.Components;

[TestFixture]
public class SplitPanelTests : BunitTest
{
    [Test]
    public void RendersCorrectly()
    {
        var comp = Context.Render<SplitPanelTest>();
        comp.FindAll(".vek-split-panel").Count.Should().Be(1);

        var childPanels = comp.FindAll(".child-panel");
        childPanels.Count.Should().Be(2);
        childPanels[0].ToMarkup().Should().BeEquivalentTo(childPanels[1].ToMarkup());
    }

    /// <summary>
    /// Nested split panels preserve independent vertical and horizontal separators.
    /// </summary>
    [Test]
    public void NestedPanels_RenderVerticalParentAndHorizontalChild()
    {
        var comp = Context.Render<VekSplitPanel>(parameters => parameters
            .Add(p => p.FirstPanel, (RenderFragment)(builder =>
            {
                builder.OpenComponent<VekSplitPanel>(0);
                builder.AddAttribute(1, nameof(VekSplitPanel.Horizontal), true);
                builder.CloseComponent();
            }))
            .Add(p => p.SecondPanel, builder => builder.AddContent(0, "Right pane")));

        comp.FindAll(".vek-split-panel").Count.Should().Be(2);
        comp.FindAll("[role='separator']").Count.Should().Be(2);
        var panels = comp.FindAll(".vek-split-panel");
        panels[0].Children[1].ClassList.Should().NotContain("horizontal");
        panels[1].Children[1].ClassList.Should().Contain("horizontal");
    }

    [Test]
    public void VisualParametersRenderCorrectly()
    {
        var comp = Context.Render<VekSplitPanel>(parameters => parameters
            .Add(p => p.FirstPanel, builder => builder.AddContent(0, "First Panel"))
            .Add(p => p.SecondPanel, builder => builder.AddContent(0, "Second Panel"))
            .Add(p => p.Horizontal, true)
            .Add(p => p.UseAsOverlay, true)
            .Add(p => p.Elevation, 1)
            .Add(p => p.Padding, 2)
            .Add(p => p.Rounded, true)
            .Add(p => p.Transparent, true)
        );
        comp.FindAll(".vek-split-panel.flex-column.absolute").Count.Should().Be(1);

        var childPanels = comp.FindAll(".child-panel");
        foreach (var panel in childPanels)
        {
            panel.ClassList.Should().Contain("vek-elevation-1");
            panel.ClassList.Should().Contain("transparent");
            panel.ClassList.Should().Contain("pa-2");
            panel.ClassList.Should().Contain("rounded");
        }

        comp.FindAll(".divider.horizontal").Count.Should().Be(1);
    }

    [Test]
    public async Task ExecutesBuildDestroyJsCalls()
    {
        Context.Render<SplitPanelTest>();
        var invocation = Context.JSInterop.VerifyInvoke("vekSplitPanel.build");
        invocation.Arguments.Count.Should().Be(12);

        await Context.DisposeComponentsAsync();
        invocation = Context.JSInterop.VerifyInvoke("vekSplitPanel_destroy");
        invocation.Arguments.Count.Should().Be(1);
    }

    [Test]
    public async Task ExecutesUpdateJsCall()
    {
        var comp = Context.Render<SplitPanelTest>();
        await comp.SetParametersAndRenderAsync(parameters => parameters.Add(c => c.Horizontal, true));

        var invocation = Context.JSInterop.VerifyInvoke("vekSplitPanel_update");
        invocation.Arguments.Count.Should().Be(10);
    }

    [Test]
    public async Task ExecutesResetDividerPositionJsCall()
    {
        var comp = Context.Render<SplitPanelTest>();
        await comp.Instance.ResetDividerPositionAsync();

        var invocation = Context.JSInterop.VerifyInvoke("vekSplitPanel_resetDividerPosition");
        invocation.Arguments.Count.Should().Be(1);
    }

    [Test]
    public async Task ExecutesSetDividerPositionJsCall()
    {
        var comp = Context.Render<SplitPanelTest>();
        await comp.Instance.SetDividerPositionAsync(123);

        var invocation = Context.JSInterop.VerifyInvoke("vekSplitPanel_setDividerPosition");
        invocation.Arguments.Count.Should().Be(2);
    }

    [Test]
    public async Task ExecutesGetDividerPositionJsCall()
    {
        var comp = Context.Render<SplitPanelTest>();
        await comp.Instance.GetDividerPositionAsync();

        var invocation = Context.JSInterop.VerifyInvoke("vekSplitPanel_getDividerPosition");
        invocation.Arguments.Count.Should().Be(1);
    }

    [Test]
    public void NoUserId_JsInteropTargetsRenderedContainerId()
    {
        var comp = Context.Render<VekSplitPanel>(parameters => parameters
            .Add(p => p.FirstPanel, builder => builder.AddContent(0, "First Panel"))
            .Add(p => p.SecondPanel, builder => builder.AddContent(0, "Second Panel")));

        var renderedId = comp.Find(".vek-split-panel").GetAttribute("id");
        renderedId.Should().NotBeNullOrWhiteSpace();

        // The JS build must target the exact id rendered on the container, or document.getElementById fails.
        var build = Context.JSInterop.VerifyInvoke("vekSplitPanel.build");
        build.Arguments[0].Should().Be(renderedId);
    }

    [Test]
    public async Task UserProvidedId_OverridesContainerId_AndJsInteropFollowsIt()
    {
        var comp = Context.Render<VekSplitPanel>(parameters => parameters
            .Add(p => p.FirstPanel, builder => builder.AddContent(0, "First Panel"))
            .Add(p => p.SecondPanel, builder => builder.AddContent(0, "Second Panel"))
            .AddUnmatched("id", "custom-split"));

        // The consumer id wins on the rendered container (attribute splat comes after the explicit id).
        comp.Find(".vek-split-panel").GetAttribute("id").Should().Be("custom-split");

        // Regression for #13070: JS build/destroy must follow the effective rendered id, not the internal GUID,
        // otherwise document.getElementById('<guid>') is null and the divider drag/keyboard handlers never attach.
        Context.JSInterop.VerifyInvoke("vekSplitPanel.build").Arguments[0].Should().Be("custom-split");

        await comp.Instance.ResetDividerPositionAsync();
        Context.JSInterop.VerifyInvoke("vekSplitPanel_resetDividerPosition").Arguments[0].Should().Be("custom-split");

        await Context.DisposeComponentsAsync();
        Context.JSInterop.VerifyInvoke("vekSplitPanel_destroy").Arguments[0].Should().Be("custom-split");
    }

    /// <summary>
    /// Pane sizing options are forwarded to the interactive splitter.
    /// </summary>
    [Test]
    public void PaneSizingOptions_ArePassedToJavaScript()
    {
        Context.Render<VekSplitPanel>(parameters => parameters
            .Add(p => p.PaneSize, 35)
            .Add(p => p.PaneSizeUnit, SplitPanelSizeUnit.Percentage)
            .Add(p => p.MinSize, 20)
            .Add(p => p.MaxSize, 75)
            .Add(p => p.Collapsible, true));

        var build = Context.JSInterop.VerifyInvoke("vekSplitPanel.build");
        build.Arguments[6].Should().Be(35d);
        build.Arguments[7].Should().Be(SplitPanelSizeUnit.Percentage);
        build.Arguments[8].Should().Be(20d);
        build.Arguments[9].Should().Be(75d);
        build.Arguments[10].Should().Be(true);
        build.Arguments[11].Should().NotBeNull();
    }

    /// <summary>
    /// Interactive resizing updates the bound pane size.
    /// </summary>
    [Test]
    public async Task PaneSizeChanged_UpdatesTheBoundValue()
    {
        var values = new List<double?>();
        var comp = Context.Render<VekSplitPanel>(parameters => parameters
            .Add(p => p.PaneSize, 35)
            .Add(p => p.PaneSizeChanged, EventCallback.Factory.Create<double?>(this, value => values.Add(value))));

        var reference = (Microsoft.JSInterop.DotNetObjectReference<VekSplitPanel>)Context.JSInterop
            .VerifyInvoke("vekSplitPanel.build").Arguments[11]!;

        await comp.InvokeAsync(() => reference.Value.OnPaneSizeChangedAsync(42));

        values.Should().ContainSingle().Which.Should().Be(42d);
    }

    /// <summary>
    /// The divider's accessible name comes from the language resources so it can be translated.
    /// </summary>
    [Test]
    public void Divider_HasLocalizedAccessibleName()
    {
        var comp = Context.Render<VekSplitPanel>();

        var divider = comp.Find("[role='separator']");
        divider.GetAttribute("aria-label").Should().Be("Resize panels");
    }
}
