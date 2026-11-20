using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.ButtonGroup;

namespace Vek.UnitTests.Components;

[TestFixture]
public class ButtonGroupTests : BunitTest
{
    /// <summary>
    /// Buttons keep their action behavior without exposing selection state when the group uses the default mode.
    /// </summary>
    [Test]
    public async Task NoneModeKeepsButtonsAsActions()
    {
        var clickCount = 0;
        var comp = Context.Render<VekButtonGroup>(parameters => parameters
            .Add(p => p.ChildContent, CreateSelectionButtons(() => clickCount++)));

        var buttons = comp.FindAll("button");
        buttons[0].HasAttribute("aria-pressed").Should().BeFalse();
        await buttons[0].ClickAsync();

        clickCount.Should().Be(1);
    }

    /// <summary>
    /// Single mode keeps one selected key and exposes its state on the corresponding button.
    /// </summary>
    [Test]
    public async Task SingleModeBindsAndKeepsOneButtonPressed()
    {
        object selectedValue = "left";
        IReadOnlyCollection<object> changedValues = null;
        var comp = Context.Render<VekButtonGroup>(parameters => parameters
            .Add(p => p.SelectionMode, ButtonGroupSelectionMode.Single)
            .Add(p => p.SelectedValue, selectedValue)
            .Add(p => p.SelectedValueChanged, EventCallback.Factory.Create<object>(this, value => selectedValue = value))
            .Add(p => p.OnSelectionChanged, EventCallback.Factory.Create<IReadOnlyCollection<object>>(this, values => changedValues = values))
            .Add(p => p.ChildContent, CreateSelectionButtons()));

        var buttons = comp.FindAll("button");
        buttons[0].GetAttribute("aria-pressed").Should().Be("true");
        buttons[1].GetAttribute("aria-pressed").Should().Be("false");

        await buttons[1].ClickAsync();

        selectedValue.Should().Be("right");
        changedValues.Should().ContainSingle().Which.Should().Be("right");
        comp.FindAll("button").Select(button => button.GetAttribute("aria-pressed")).Should().Equal("false", "true");
    }

    /// <summary>
    /// Single mode moves the pressed state when selection is managed by the group.
    /// </summary>
    [Test]
    public async Task SingleModeUpdatesPressedStateWhenUnbound()
    {
        var comp = Context.Render<VekButtonGroup>(parameters => parameters
            .Add(p => p.SelectionMode, ButtonGroupSelectionMode.Single)
            .Add(p => p.SelectedValue, "left")
            .Add(p => p.ChildContent, CreateSelectionButtons()));

        await comp.FindAll("button")[1].ClickAsync();

        comp.FindAll("button").Select(button => button.GetAttribute("aria-pressed")).Should().Equal("false", "true");
    }

    /// <summary>
    /// Multiple mode toggles only the activated key and reports the remaining selection.
    /// </summary>
    [Test]
    public async Task MultipleModeBindsAndReportsSelectedKeys()
    {
        IReadOnlyCollection<object> selectedValues = new object[] { "left" };
        IReadOnlyCollection<object> changedValues = null;
        var comp = Context.Render<VekButtonGroup>(parameters => parameters
            .Add(p => p.SelectionMode, ButtonGroupSelectionMode.Multiple)
            .Add(p => p.SelectedValues, selectedValues)
            .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<IReadOnlyCollection<object>>(this, values => selectedValues = values))
            .Add(p => p.OnSelectionChanged, EventCallback.Factory.Create<IReadOnlyCollection<object>>(this, values => changedValues = values))
            .Add(p => p.ChildContent, CreateSelectionButtons()));

        var buttons = comp.FindAll("button");
        buttons[0].GetAttribute("aria-pressed").Should().Be("true");
        buttons[1].GetAttribute("aria-pressed").Should().Be("false");

        await buttons[1].ClickAsync();

        selectedValues.Should().HaveCount(2).And.Contain("left").And.Contain("right");
        changedValues.Should().HaveCount(2).And.Contain("left").And.Contain("right");
        comp.FindAll("button").Select(button => button.GetAttribute("aria-pressed")).Should().Equal("true", "true");
    }

    /// <summary>
    /// Multiple mode toggles a pressed button off while preserving other selections.
    /// </summary>
    [Test]
    public async Task MultipleModeTogglesPressedStateWhenUnbound()
    {
        var comp = Context.Render<VekButtonGroup>(parameters => parameters
            .Add(p => p.SelectionMode, ButtonGroupSelectionMode.Multiple)
            .Add(p => p.SelectedValues, new object[] { "left", "right" })
            .Add(p => p.ChildContent, CreateSelectionButtons()));

        await comp.FindAll("button")[0].ClickAsync();

        comp.FindAll("button").Select(button => button.GetAttribute("aria-pressed")).Should().Equal("false", "true");
    }

    /// <summary>
    /// A group with no parameters renders a horizontal element with the group role, text variant classes, and overridden button styles.
    /// </summary>
    [Test]
    public void DefaultRendering()
    {
        var comp = Context.Render<VekButtonGroup>();

        var root = comp.Find("div");
        root.GetAttribute("role").Should().Be("group");
        root.ClassList.Should().Contain(["vek-button-group-root", "vek-button-group-horizontal", "vek-button-group-override-styles", "vek-button-group-text", "vek-button-group-text-default", "vek-button-group-text-size-medium"]);
        root.ClassList.Should().NotContain(["vek-button-group-vertical", "vek-button-group-rtl", "vek-button-group-disable-elevation", "vek-width-full"]);
    }

    /// <summary>
    /// Appearance parameters set the classes the stylesheet uses to style the group and, through OverrideStyles, the buttons inside it.
    /// </summary>
    [Test]
    public void AppearanceParametersSetClasses()
    {
        var comp = Context.Render<VekButtonGroup>(parameters => parameters
            .Add(p => p.Variant, Variant.Outlined)
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Size, Size.Small)
            .Add(p => p.Vertical, true)
            .Add(p => p.DropShadow, false)
            .Add(p => p.OverrideStyles, false));

        var classes = comp.Find("div").ClassList;
        classes.Should().Contain(["vek-button-group-outlined", "vek-button-group-outlined-primary", "vek-button-group-outlined-size-small", "vek-button-group-vertical", "vek-button-group-disable-elevation"]);
        classes.Should().NotContain(["vek-button-group-horizontal", "vek-button-group-override-styles"]);
    }

    /// <summary>
    /// A right-to-left layout flips the group so its rounded corners and dividers stay on the correct sides.
    /// </summary>
    [Test]
    public void RightToLeftSetsRtlClass()
    {
        var comp = Context.Render<CascadingValue<bool>>(parameters => parameters
            .Add(p => p.Name, "RightToLeft")
            .Add(p => p.Value, true)
            .AddChildContent<VekButtonGroup>());

        comp.Find(".vek-button-group-root").ClassList.Should().Contain("vek-button-group-rtl");
    }

    /// <summary>
    /// A caller's class, style, and attributes reach the group element.
    /// </summary>
    [Test]
    public void ForwardsClassStyleAndAttributes()
    {
        var comp = Context.Render<VekButtonGroup>(parameters => parameters
            .Add(p => p.Class, "my-group")
            .Add(p => p.Style, "gap:4px")
            .AddUnmatched("aria-label", "Text formatting"));

        var root = comp.Find("div");
        root.ClassList.Should().Contain(["vek-button-group-root", "my-group"]);
        root.GetAttribute("style").Should().Be("gap:4px");
        root.GetAttribute("aria-label").Should().Be("Text formatting");
    }

    /// <summary>
    /// A full-width group stretches every button when none of them is full width on its own (#9710).
    /// </summary>
    [Test]
    public void FullWidthGroupStretchesAllButtons()
    {
        var comp = Context.Render<ButtonGroupWithThreeButtons>(parameters => parameters
            .Add(p => p.ButtonGroupFullWidth, true));

        comp.Find(".vek-button-group-root").ClassList.Should().Contain("vek-width-full");
        StretchedButtons(comp).Should().Equal(true, true, true);
    }

    /// <summary>
    /// Once any button is full width, a full-width group stretches only that button.
    /// </summary>
    [Test]
    public void FullWidthGroupStretchesOnlyFullWidthButton()
    {
        var comp = Context.Render<ButtonGroupWithThreeButtons>(parameters => parameters
            .Add(p => p.ButtonGroupFullWidth, true)
            .Add(p => p.Button2FullWidth, true));

        StretchedButtons(comp).Should().Equal(false, true, false);
    }

    /// <summary>
    /// A group that is not full width leaves its buttons at their natural width.
    /// </summary>
    [Test]
    public void GroupWithoutFullWidthDoesNotStretchButtons()
    {
        var comp = Context.Render<ButtonGroupWithThreeButtons>(parameters => parameters
            .Add(p => p.ButtonGroupFullWidth, false));

        comp.Find(".vek-button-group-root").ClassList.Should().NotContain("vek-width-full");
        StretchedButtons(comp).Should().Equal(false, false, false);
    }

    /// <summary>
    /// Removing the only full-width button makes the group stretch the remaining buttons again.
    /// </summary>
    [Test]
    public async Task RemovingFullWidthButtonStretchesRemainingButtons()
    {
        var comp = Context.Render<ButtonGroupWithThreeButtons>(parameters => parameters
            .Add(p => p.ButtonGroupFullWidth, true)
            .Add(p => p.Button1FullWidth, true));

        await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Button1Displayed, false));

        StretchedButtons(comp).Should().Equal(true, true);
    }

    /// <summary>
    /// Adding a full-width button makes the group stop stretching the other buttons.
    /// </summary>
    [Test]
    public async Task AddingFullWidthButtonUnstretchesOtherButtons()
    {
        var comp = Context.Render<ButtonGroupWithThreeButtons>(parameters => parameters
            .Add(p => p.ButtonGroupFullWidth, true)
            .Add(p => p.Button3Displayed, false)
            .Add(p => p.Button3FullWidth, true));

        await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Button3Displayed, true));

        StretchedButtons(comp).Should().Equal(false, false, true);
    }

    /// <summary>
    /// Returns whether each rendered button in the group is stretched, in document order.
    /// </summary>
    private static IEnumerable<bool> StretchedButtons(IRenderedComponent<ButtonGroupWithThreeButtons> comp)
    {
        return comp.FindAll(".vek-button-root").Select(button => button.ClassList.Contains("vek-width-full")).ToList();
    }

    /// <summary>
    /// Creates keyed buttons for selection behavior tests.
    /// </summary>
    private static RenderFragment CreateSelectionButtons(Action onClick = null) => builder =>
    {
        builder.OpenComponent<VekButton>(0);
        builder.AddAttribute(1, nameof(VekButton.Value), "left");
        builder.AddAttribute(2, nameof(VekButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(builder, onClick ?? (() => { })));
        builder.AddAttribute(3, nameof(VekButton.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(0, "Left")));
        builder.CloseComponent();
        builder.OpenComponent<VekButton>(4);
        builder.AddAttribute(5, nameof(VekButton.Value), "right");
        builder.AddAttribute(6, nameof(VekButton.ChildContent), (RenderFragment)(contentBuilder => contentBuilder.AddContent(0, "Right")));
        builder.CloseComponent();
    };
}
