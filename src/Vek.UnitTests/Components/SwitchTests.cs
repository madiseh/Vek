using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Vek.UnitTests.TestComponents.Switch;
using Vek.UnitTests.Utilities;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class SwitchTest : BunitTest
    {
        [Test]
        public async Task Switch_KeyboardNavigation()
        {
            var comp = Context.Render<VekSwitchBasicTest>();
            var switchInstance = comp.FindComponent<VekSwitch<bool>>().Instance;
            IElement VekSwitch() => comp.Find("#switch");

            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = "Enter", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(true));

            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = "Delete", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(false));

            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(true));

            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(false));

            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = "NumpadEnter", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(true));

            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = " ", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(false));

            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = " ", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(true));

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.Disabled, true));
            await comp.InvokeAsync(() => VekSwitch().KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => switchInstance.ReadValue.Should().Be(true));
        }

        /// <summary>
        /// The label and child content use body1 typography and take the error color while the switch has errors.
        /// </summary>
        [Test]
        public async Task Switch_LabelAndChildContent_TakeErrorColor()
        {
            var comp = Context.Render<VekSwitch<bool>>(parameters => parameters
                .Add(p => p.Label, "Notifications")
                .AddChildContent("Email"));

            comp.FindAll("label.vek-switch span.vek-typography").Should().HaveCount(2)
                .And.OnlyContain(text => text.ClassName == "vek-typography vek-typography-body1");

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Error, true));

            comp.FindAll("label.vek-switch span.vek-typography").Should().HaveCount(2)
                .And.OnlyContain(text => text.ClassName == "vek-typography vek-typography-body1 vek-error-text");

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Error, false));

            comp.FindAll("label.vek-switch span.vek-typography").Should().HaveCount(2)
                .And.OnlyContain(text => text.ClassName == "vek-typography vek-typography-body1");
        }

        [Test]
        public void Switch_AriaLabel()
        {
            var comp = Context.Render<SwitchAriaLabelTest>();
            var switches = comp.FindAll(".vek-input-control-boolean-input");

            // verify switch one maintains it's original structure, no aria class used, label with a span element
            switches[0].GetElementsByClassName("vek-sr-only").Length.Should().Be(0);
            var element0 = comp.Find(".s1 label.vek-switch span.vek-typography");
            element0.HasAttribute("aria-hidden").Should().BeFalse();

            // switch two should have both a valid label with aria-hidden, an input with arialabelledby and the labelledby element
            switches[1].GetElementsByClassName("vek-sr-only").Length.Should().Be(1);
            var element1 = comp.Find(".s2 label.vek-switch span.vek-typography");
            element1.GetAttribute("aria-hidden").Should().Be("true");
            var input1 = comp.Find(".s2 label.vek-switch input");
            var input1ForId = input1.GetAttribute("aria-labelledby");
            comp.Find($".s2 label.vek-switch #{input1ForId}").Should().NotBeNull();

            // switch three should have original structure intact, no aria class used, label with a span element for child content
            switches[2].GetElementsByClassName("vek-sr-only").Length.Should().Be(0);
            var element2 = comp.Find(".s3 label.vek-switch span.vek-typography");
            element2.HasAttribute("aria-hidden").Should().BeFalse();

            // switch four should look identical to two except this time it's with ChildContent
            switches[3].GetElementsByClassName("vek-sr-only").Length.Should().Be(1);
            var element3 = comp.Find(".s4 label.vek-switch span.vek-typography");
            element3.GetAttribute("aria-hidden").Should().Be("true");
            var input3 = comp.Find(".s4 label.vek-switch input");
            var input3ForId = input3.GetAttribute("aria-labelledby");
            comp.Find($".s4 label.vek-switch #{input3ForId}").Should().NotBeNull();

            // switch five has no label, no child content, just arialabel
            switches[4].GetElementsByClassName("vek-sr-only").Length.Should().Be(1);
            comp.FindAll(".s5 label.vek-switch span.vek-typography").Count.Should().Be(0);
            var input4 = comp.Find(".s5 label.vek-switch input");
            var input4ForId = input4.GetAttribute("aria-labelledby");
            comp.Find($".s5 label.vek-switch #{input4ForId}").Should().NotBeNull();
        }

        [Test]
        [TestCase(Color.Default, Color.Primary)]
        [TestCase(Color.Primary, Color.Secondary)]
        [TestCase(Color.Secondary, Color.Info)]
        [TestCase(Color.Tertiary, Color.Success)]
        [TestCase(Color.Info, Color.Warning)]
        [TestCase(Color.Success, Color.Error)]
        [TestCase(Color.Warning, Color.Dark)]
        [TestCase(Color.Error, Color.Primary)]
        [TestCase(Color.Dark, Color.Primary)]
        public async Task SwitchColor(Color color, Color uncheckedcolor)
        {
            var comp = Context.Render<VekSwitch<bool>>(x => x.Add(c => c.Color, color).Add(b => b.UncheckedColor, uncheckedcolor));

            var box = comp.Instance;

            var checkboxClasses = comp.Find(".vek-button-root.vek-icon-button.vek-switch-base");
            // check initial state
            box.ReadValue.Should().Be(false);
            checkboxClasses.ClassList.Should().ContainInOrder(new[] { $"vek-{uncheckedcolor.ToStringFast(true)}-text", $"hover:vek-{uncheckedcolor.ToStringFast(true)}-hover" });

            // click and check if it has new color
            await comp.Find("input").ChangeAsync(true);
            box.ReadValue.Should().Be(true);
            checkboxClasses.ClassList.Should().ContainInOrder(new[] { $"vek-{color.ToStringFast(true)}-text", $"hover:vek-{color.ToStringFast(true)}-hover" });
        }

        [Test]
        [TestCase(Color.Success, Color.Error)]
        [TestCase(Color.Info, Color.Warning)]
        public void SwitchColor_ReadOnly_ShouldKeepColor(Color color, Color uncheckedColor)
        {
            // #9524: a read-only switch keeps Color/UncheckedColor (only Disabled greys out),
            // while the interactive hover class stays suppressed.
            var offComp = Context.Render<VekSwitch<bool>>(x => x
                .Add(c => c.Color, color)
                .Add(c => c.UncheckedColor, uncheckedColor)
                .Add(c => c.ReadOnly, true)
                .Add(c => c.Value, false));
            var offBase = offComp.Find(".vek-button-root.vek-icon-button.vek-switch-base");
            offBase.ClassList.Should().Contain($"vek-{uncheckedColor.ToStringFast(true)}-text");
            offBase.ClassList.Should().NotContain($"hover:vek-{uncheckedColor.ToStringFast(true)}-hover");

            var onComp = Context.Render<VekSwitch<bool>>(x => x
                .Add(c => c.Color, color)
                .Add(c => c.UncheckedColor, uncheckedColor)
                .Add(c => c.ReadOnly, true)
                .Add(c => c.Value, true));
            var onBase = onComp.Find(".vek-button-root.vek-icon-button.vek-switch-base");
            onBase.ClassList.Should().Contain($"vek-{color.ToStringFast(true)}-text");
            onBase.ClassList.Should().NotContain($"hover:vek-{color.ToStringFast(true)}-hover");
        }

        [Test]
        public void SwitchDisabled()
        {
            var comp = Context.Render<SwitchWithLabelTest>();
            comp.FindAll("label.vek-switch")[3].ClassList.Should().Contain("vek-disabled"); // 4rd switch
        }

        [Test]
        public void SwitchLabelPlacement()
        {
            var comp = Context.Render<SwitchWithLabelTest>();

            comp.FindAll("label.vek-switch")[0].ClassList.Should().Contain("vek-input-content-placement-end"); // 1st switch: (default) Placement.End
            comp.FindAll("label.vek-switch")[2].ClassList.Should().Contain("vek-input-content-placement-start"); // 3rd switch: Placement.Start
        }

        [Test]
        public void SwitchLabel()
        {
            var value = new DisplayNameLabelClass();

            var comp = Context.Render<VekSwitch<bool>>(x => x.Add(f => f.For, () => value.Boolean));
            comp.Instance.Label.Should().Be("Boolean LabelAttribute"); //label should be set by the attribute

            var comp2 = Context.Render<VekSwitch<bool>>(x => x.Add(f => f.For, () => value.Boolean).Add(l => l.Label, "Label Parameter"));
            comp2.Instance.Label.Should().Be("Label Parameter"); //existing label should remain
        }

        /// <summary>
        /// Optional Switch should not have required attribute should be false.
        /// </summary>
        [Test]
        public void OptionalSwitch_Should_NotHaveRequiredAttribute()
        {
            var comp = Context.Render<VekSwitch<bool>>();

            comp.Find("input").HasAttribute("required").Should().BeFalse();
        }

        /// <summary>
        /// Required Switch should have the required attribute.
        /// </summary>
        [Test]
        public void RequiredSwitch_Should_HaveRequiredAttribute()
        {
            var comp = Context.Render<VekSwitch<bool>>(parameters => parameters
                .Add(p => p.Required, true));

            comp.Find("input").HasAttribute("required").Should().BeTrue();
        }

        /// <summary>
        /// Required Switch attribute should be dynamic.
        /// </summary>
        [Test]
        public async Task RequiredSwitchAttribute_Should_BeDynamic()
        {
            var comp = Context.Render<VekSwitch<bool>>();

            comp.Find("input").HasAttribute("required").Should().BeFalse();

            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.Required, true));

            comp.Find("input").HasAttribute("required").Should().BeTrue();
        }

        [Test]
        public void Switch_Respects_Custom_TabIndex()
        {
            var comp = Context.Render<VekSwitch<bool>>(parameters => parameters.AddUnmatched("tabindex", "-1"));

            comp.Find("input").GetAttribute("tabindex").Should().Be("-1");
        }

        [Test]
        public void Switch_Uses_Default_TabIndex_When_Enabled()
        {
            var comp = Context.Render<VekSwitch<bool>>();

            comp.Find("input").GetAttribute("tabindex").Should().Be("0");
        }

        [Test]
        public void Switch_Uses_Default_TabIndex_When_Disabled()
        {
            var comp = Context.Render<VekSwitch<bool>>(parameters => parameters.Add(x => x.Disabled, true));

            comp.Find("input").GetAttribute("tabindex").Should().Be("-1");
        }

        [Test]
        public void Switch_Respects_Custom_TabIndex_CaseInsensitive()
        {
            var comp = Context.Render<VekSwitch<bool>>(parameters => parameters.AddUnmatched("TabIndex", "-1"));

            comp.Find("input").GetAttribute("tabindex").Should().Be("-1");
        }

        [Test]
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        [TestCase(null, "mixed")]
        public void Switch_AriaChecked_Reflects_Value(bool? value, string expectedAriaChecked)
        {
            var comp = Context.Render<VekSwitch<bool?>>(parameters => parameters.Add(x => x.Value, value));
            var input = comp.Find("input");

            input.GetAttribute("aria-checked").Should().Be(expectedAriaChecked);
        }

        [Test]
        public void ReadOnlyDisabled_ShouldNot_Hover()
        {
            Context.Render<VekSwitch<bool>>(self => self.Add(x => x.ReadOnly, false)).Find("span.vek-button-root").ClassList.Should().Contain("hover:vek-default-hover");
            Context.Render<VekSwitch<bool>>(self => self.Add(x => x.ReadOnly, true)).Find("span.vek-button-root").ClassList.Should().NotContain("hover:vek-default-hover");
            Context.Render<VekSwitch<bool>>(self => self.Add(x => x.ReadOnly, true).Add(x => x.Disabled, false)).Find("span.vek-button-root").ClassList.Should().NotContain("hover:vek-default-hover");
            Context.Render<VekSwitch<bool>>(self => self.Add(x => x.Disabled, false)).Find("span.vek-button-root").ClassList.Should().Contain("hover:vek-default-hover");
            Context.Render<VekSwitch<bool>>(self => self.Add(x => x.Disabled, true).Add(x => x.ReadOnly, false)).Find("span.vek-button-root").ClassList.Should().NotContain("hover:vek-default-hover");
            Context.Render<VekSwitch<bool>>(self => self.Add(x => x.Disabled, true).Add(x => x.ReadOnly, true)).Find("span.vek-button-root").ClassList.Should().NotContain("hover:vek-default-hover");
        }

        /// <summary>
        /// An invalid switch sets aria-invalid and links aria-describedby to its error text.
        /// </summary>
        [Test]
        public async Task Switch_ShouldLinkInputToErrorText()
        {
            var comp = Context.Render<VekSwitch<bool>>(parameters => parameters
                .Add(p => p.ErrorId, "field-error")
                .Add(p => p.ErrorText, "Required"));

            comp.Find("input").HasAttribute("aria-invalid").Should().BeFalse();
            comp.Find("input").HasAttribute("aria-describedby").Should().BeFalse();

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Error, true));

            comp.Find("input").GetAttribute("aria-invalid").Should().Be("true");
            comp.Find("input").GetAttribute("aria-describedby").Should().Be("field-error");
            comp.Find("#field-error").TextContent.Trim().Should().Be("Required");
        }

        /// <summary>
        /// A user-supplied aria-invalid wins over the generated one.
        /// </summary>
        [Test]
        public void Switch_ShouldAllowUserAriaInvalidOverride()
        {
            var comp = Context.Render<VekSwitch<bool>>(parameters => parameters
                .Add(p => p.Error, true)
                .AddUnmatched("aria-invalid", "false"));

            comp.Find("input").GetAttribute("aria-invalid").Should().Be("false");
        }
    }
}
