using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek.Extensions;

namespace Vek.UnitTests.Components;

[TestFixture]
public class SplitButtonTests : BunitTest
{
    [Test]
    public void SplitButton_DefaultAppearance()
    {
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Save"));

        var root = comp.Find("div.vek-split-button-root");
        root.GetAttribute("role").Should().Be("group");
        root.ClassList.Should().Contain([
            "vek-split-button-root",
            "vek-button-group-root",
            "vek-button-group-override-styles",
            "vek-button-group-text",
            "vek-button-group-text-default",
            "vek-button-group-text-size-medium",
            "vek-button-group-horizontal"
        ]);

        var primaryBtn = comp.Find("button.vek-split-button-primary");
        primaryBtn.TextContent.Should().Contain("Save");

        var dropdownTrigger = comp.Find("button.vek-menu-icon-button-activator");
        dropdownTrigger.Should().NotBeNull();
        dropdownTrigger.GetAttribute("aria-haspopup").Should().Be("menu");
        dropdownTrigger.GetAttribute("aria-expanded").Should().Be("false");
        dropdownTrigger.GetAttribute("aria-label").Should().Be("Open menu");
    }

    [TestCase(Variant.Filled, Color.Primary, Size.Small, "vek-button-group-filled", "vek-button-group-filled-primary", "vek-button-group-filled-size-small")]
    [TestCase(Variant.Outlined, Color.Secondary, Size.Large, "vek-button-group-outlined", "vek-button-group-outlined-secondary", "vek-button-group-outlined-size-large")]
    public void SplitButton_AppearanceParametersSetClasses(Variant variant, Color color, Size size, string variantClass, string colorClass, string sizeClass)
    {
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Variant, variant)
            .Add(p => p.Color, color)
            .Add(p => p.Size, size));

        var root = comp.Find("div.vek-split-button-root");
        root.ClassList.Should().Contain([variantClass, colorClass, sizeClass]);
    }

    [Test]
    public void SplitButton_FullWidthVerticalAndDropShadow()
    {
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.FullWidth, true)
            .Add(p => p.Vertical, true)
            .Add(p => p.DropShadow, false));

        var root = comp.Find("div.vek-split-button-root");
        root.ClassList.Should().Contain([
            "vek-width-full",
            "vek-button-group-vertical",
            "vek-button-group-disable-elevation"
        ]);
        root.ClassList.Should().NotContain("vek-button-group-horizontal");
    }

    [Test]
    public async Task SplitButton_PrimaryClick_InvokesOnClick()
    {
        var clicked = false;
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Submit")
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true)));

        var primaryBtn = comp.Find("button.vek-split-button-primary");
        await primaryBtn.ClickAsync();

        clicked.Should().BeTrue();
    }

    [Test]
    public async Task SplitButton_Disabled_DisablesBothButtons()
    {
        var clicked = false;
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Disabled Action")
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicked = true)));

        var primaryBtn = comp.Find("button.vek-split-button-primary");
        primaryBtn.HasAttribute("disabled").Should().BeTrue();

        var dropdownTrigger = comp.Find("button.vek-menu-icon-button-activator");
        dropdownTrigger.HasAttribute("disabled").Should().BeTrue();

        await primaryBtn.ClickAsync();
        clicked.Should().BeFalse();
    }

    [Test]
    public void SplitButton_PrimaryDisabled_OnlyDisablesPrimary()
    {
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Primary Only Disabled")
            .Add(p => p.PrimaryDisabled, true));

        var primaryBtn = comp.Find("button.vek-split-button-primary");
        primaryBtn.HasAttribute("disabled").Should().BeTrue();

        var dropdownTrigger = comp.Find("button.vek-menu-icon-button-activator");
        dropdownTrigger.HasAttribute("disabled").Should().BeFalse();
    }

    [Test]
    public void SplitButton_DropDownDisabled_OnlyDisablesDropdown()
    {
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Dropdown Only Disabled")
            .Add(p => p.DropDownDisabled, true));

        var primaryBtn = comp.Find("button.vek-split-button-primary");
        primaryBtn.HasAttribute("disabled").Should().BeFalse();

        var dropdownTrigger = comp.Find("button.vek-menu-icon-button-activator");
        dropdownTrigger.HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public async Task SplitButton_ToggleDropdown_OpensAndClosesMenu()
    {
        var provider = Context.Render<VekPopoverProvider>();

        var action1Clicked = false;
        RenderFragment menuItems = builder =>
        {
            builder.OpenComponent<VekMenuItem>(0);
            builder.AddAttribute(1, nameof(VekMenuItem.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => action1Clicked = true));
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Action 1")));
            builder.CloseComponent();
        };

        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Options")
            .Add(p => p.MenuItems, menuItems));

        comp.Instance.GetState(x => x.Open).Should().BeFalse();

        var dropdownTrigger = comp.Find("button.vek-menu-icon-button-activator");
        await dropdownTrigger.ClickAsync();

        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeTrue());
        dropdownTrigger.GetAttribute("aria-expanded").Should().Be("true");

        await provider.WaitForAssertionAsync(() => provider.FindAll("div.vek-menu-item").Count.Should().Be(1));
        var menuItem = provider.Find("div.vek-menu-item");
        menuItem.TextContent.Should().Contain("Action 1");

        await menuItem.ClickAsync();
        action1Clicked.Should().BeTrue();

        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeFalse());
    }

    [Test]
    public async Task SplitButton_KeyboardNavigation_AltArrowDown_And_Escape()
    {
        Context.Render<VekPopoverProvider>();

        RenderFragment menuItems = builder =>
        {
            builder.OpenComponent<VekMenuItem>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Option 1")));
            builder.CloseComponent();
        };

        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Keys")
            .Add(p => p.MenuItems, menuItems));

        var root = comp.Find("div.vek-split-button-root");

        // Alt + ArrowDown opens the menu
        await root.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown", AltKey = true });
        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeTrue());

        // Escape closes the menu
        await root.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });
        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeFalse());
    }

    [Test]
    public async Task SplitButton_TwoWayBinding_Open()
    {
        Context.Render<VekPopoverProvider>();

        var openValue = false;
        var comp = Context.Render<VekSplitButton>(parameters => parameters
            .Add(p => p.Text, "Binding")
            .Add(p => p.Open, openValue)
            .Add(p => p.OpenChanged, EventCallback.Factory.Create<bool>(this, val => openValue = val)));

        await comp.Instance.OpenMenuAsync();
        openValue.Should().BeTrue();

        await comp.Instance.CloseMenuAsync();
        openValue.Should().BeFalse();
    }

    [Test]
    public void DropDownButton_DefaultAppearance()
    {
        var comp = Context.Render<VekDropDownButton>(parameters => parameters
            .Add(p => p.Text, "Actions"));

        var root = comp.Find("div.vek-dropdown-button-root");
        root.Should().NotBeNull();

        var button = comp.Find("button.vek-button-root");
        button.TextContent.Should().Contain("Actions");
        button.GetAttribute("aria-haspopup").Should().Be("menu");
        button.GetAttribute("aria-expanded").Should().Be("false");
    }

    [TestCase(Variant.Filled, Color.Primary, Size.Small)]
    [TestCase(Variant.Outlined, Color.Secondary, Size.Large)]
    public void DropDownButton_AppearanceParameters(Variant variant, Color color, Size size)
    {
        var comp = Context.Render<VekDropDownButton>(parameters => parameters
            .Add(p => p.Text, "Actions")
            .Add(p => p.Variant, variant)
            .Add(p => p.Color, color)
            .Add(p => p.Size, size));

        var button = comp.Find("button.vek-button-root");
        button.ClassList.Should().Contain([
            $"vek-button-{variant.ToStringFast(true)}",
            $"vek-button-{variant.ToStringFast(true)}-{color.ToStringFast(true)}",
            $"vek-button-{variant.ToStringFast(true)}-size-{size.ToStringFast(true)}"
        ]);
    }

    [Test]
    public void DropDownButton_Disabled_DisablesButton()
    {
        var comp = Context.Render<VekDropDownButton>(parameters => parameters
            .Add(p => p.Text, "Actions")
            .Add(p => p.Disabled, true));

        var button = comp.Find("button.vek-button-root");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public async Task DropDownButton_ToggleDropdown_OpensAndClosesMenu()
    {
        var provider = Context.Render<VekPopoverProvider>();

        var actionClicked = false;
        RenderFragment menuItems = builder =>
        {
            builder.OpenComponent<VekMenuItem>(0);
            builder.AddAttribute(1, nameof(VekMenuItem.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => actionClicked = true));
            builder.AddAttribute(2, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Item 1")));
            builder.CloseComponent();
        };

        var comp = Context.Render<VekDropDownButton>(parameters => parameters
            .Add(p => p.Text, "Actions")
            .Add(p => p.MenuItems, menuItems));

        comp.Instance.GetState(x => x.Open).Should().BeFalse();

        var button = comp.Find("button.vek-button-root");
        await button.ClickAsync();

        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeTrue());
        button.GetAttribute("aria-expanded").Should().Be("true");

        await provider.WaitForAssertionAsync(() => provider.FindAll("div.vek-menu-item").Count.Should().Be(1));
        var menuItem = provider.Find("div.vek-menu-item");
        menuItem.TextContent.Should().Contain("Item 1");

        await menuItem.ClickAsync();
        actionClicked.Should().BeTrue();

        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeFalse());
    }

    [Test]
    public async Task DropDownButton_KeyboardNavigation_AltArrowDown_And_Escape()
    {
        Context.Render<VekPopoverProvider>();

        RenderFragment menuItems = builder =>
        {
            builder.OpenComponent<VekMenuItem>(0);
            builder.AddAttribute(1, "ChildContent", (RenderFragment)(b => b.AddContent(0, "Option 1")));
            builder.CloseComponent();
        };

        var comp = Context.Render<VekDropDownButton>(parameters => parameters
            .Add(p => p.Text, "Keys")
            .Add(p => p.MenuItems, menuItems));

        var root = comp.Find("div.vek-dropdown-button-root");

        // Alt + ArrowDown opens the menu
        await root.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown", AltKey = true });
        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeTrue());

        // Escape closes the menu
        await root.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });
        await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.Open).Should().BeFalse());
    }
}
