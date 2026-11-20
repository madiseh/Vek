using System.Threading.Tasks;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek.Extensions;
using Vek.UnitTests.Shared;

#nullable enable
namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class TextFieldPasswordToggleTests : BunitTest
    {
        [Test]
        public void PasswordToggle_NotRendered_ByDefault()
        {
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password));

            comp.FindAll(".vek-input-password-toggle-button").Should().BeEmpty();
            comp.Find("input").GetAttribute("type").Should().Be("password");
        }

        [Test]
        public void PasswordToggle_NotRendered_WhenInputTypeIsNotPassword()
        {
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Text)
                .Add(p => p.ShowPasswordToggle, true));

            comp.FindAll(".vek-input-password-toggle-button").Should().BeEmpty();
            comp.Find("input").GetAttribute("type").Should().Be("text");
        }

        [Test]
        public void PasswordToggle_Rendered_WithInitialPasswordMaskAndAriaLabel()
        {
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.Value, "Secret123"));

            var button = comp.Find(".vek-input-password-toggle-button");
            button.Should().NotBeNull();
            button.GetAttribute("aria-label").Should().Be("Show password");
            comp.Find("input").GetAttribute("type").Should().Be("password");
            comp.Instance.GetState(x => x.PasswordVisible).Should().BeFalse();
            comp.Instance.GetInputType().Should().Be(InputType.Password);
        }

        [Test]
        public async Task PasswordToggle_Click_TogglesMaskAndAriaLabel()
        {
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.Value, "Secret123"));

            // Click to reveal password
            await comp.Find(".vek-input-password-toggle-button").ClickAsync();

            comp.Find("input").GetAttribute("type").Should().Be("text");
            comp.Find(".vek-input-password-toggle-button").GetAttribute("aria-label").Should().Be("Hide password");
            comp.Instance.GetState(x => x.PasswordVisible).Should().BeTrue();
            comp.Instance.GetInputType().Should().Be(InputType.Text);

            // Click again to hide password
            await comp.Find(".vek-input-password-toggle-button").ClickAsync();

            comp.Find("input").GetAttribute("type").Should().Be("password");
            comp.Find(".vek-input-password-toggle-button").GetAttribute("aria-label").Should().Be("Show password");
            comp.Instance.GetState(x => x.PasswordVisible).Should().BeFalse();
            comp.Instance.GetInputType().Should().Be(InputType.Password);
        }

        [Test]
        public async Task PasswordToggle_TwoWayBinding_UpdatesPasswordVisible()
        {
            var passwordVisible = true;
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.PasswordVisible, passwordVisible)
                .Add(p => p.PasswordVisibleChanged, EventCallback.Factory.Create<bool>(this, val => passwordVisible = val)));

            comp.Find("input").GetAttribute("type").Should().Be("text");
            comp.Find(".vek-input-password-toggle-button").GetAttribute("aria-label").Should().Be("Hide password");

            await comp.Find(".vek-input-password-toggle-button").ClickAsync();

            passwordVisible.Should().BeFalse();
            comp.Find("input").GetAttribute("type").Should().Be("password");
            comp.Find(".vek-input-password-toggle-button").GetAttribute("aria-label").Should().Be("Show password");
        }

        [Test]
        public async Task PasswordToggle_CustomIconsAndAriaLabels()
        {
            const string customHiddenIcon = "<path d=\"custom-hidden\"/>";
            const string customVisibleIcon = "<path d=\"custom-visible\"/>";

            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.PasswordHiddenIcon, customHiddenIcon)
                .Add(p => p.PasswordVisibleIcon, customVisibleIcon)
                .Add(p => p.PasswordHiddenAriaLabel, "Reveal my password")
                .Add(p => p.PasswordVisibleAriaLabel, "Conceal my password"));

            var button = comp.Find(".vek-input-password-toggle-button");
            button.GetAttribute("aria-label").Should().Be("Reveal my password");
            button.InnerHtml.Should().Contain("custom-hidden");

            await button.ClickAsync();

            var updatedButton = comp.Find(".vek-input-password-toggle-button");
            updatedButton.GetAttribute("aria-label").Should().Be("Conceal my password");
            updatedButton.InnerHtml.Should().Contain("custom-visible");
        }

        [Test]
        public void PasswordToggle_Disabled_DisablesToggleButton()
        {
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.Disabled, true));

            var button = comp.Find(".vek-input-password-toggle-button");
            button.Should().NotBeNull();
            button.GetAttribute("disabled").Should().NotBeNull();
        }

        [Test]
        public async Task PasswordToggle_IntegratesWithClearButton()
        {
            var value = "InitialPassword";
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.Clearable, true)
                .Add(p => p.Value, value)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, val => value = val!)));

            // Both clear button and password toggle button are visible
            comp.Find(".vek-input-clear-button").Should().NotBeNull();
            comp.Find(".vek-input-password-toggle-button").Should().NotBeNull();

            // Clear button clears text
            await comp.Find(".vek-input-clear-button").ClickAsync();
            value.Should().BeNullOrEmpty();

            // Password toggle remains available
            comp.Find(".vek-input-password-toggle-button").Should().NotBeNull();

            // Toggle works even when empty
            await comp.Find(".vek-input-password-toggle-button").ClickAsync();
            comp.Find("input").GetAttribute("type").Should().Be("text");
        }

        [Test]
        public void PasswordToggle_IntegratesWithEndAdornment()
        {
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.Adornment, Adornment.End)
                .Add(p => p.AdornmentIcon, Icons.Material.Filled.Lock));

            comp.Find(".vek-input-password-toggle-button").Should().NotBeNull();
            comp.Find(".vek-input-adornment-end").Should().NotBeNull();
        }

        [Test]
        public async Task PasswordToggle_ProgrammaticMethods()
        {
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true));

            comp.Instance.GetState(x => x.PasswordVisible).Should().BeFalse();

            await comp.InvokeAsync(async () => await comp.Instance.TogglePasswordVisibilityAsync());
            comp.Instance.GetState(x => x.PasswordVisible).Should().BeTrue();
            comp.Find("input").GetAttribute("type").Should().Be("text");

            await comp.InvokeAsync(async () => await comp.Instance.SetPasswordVisibleAsync(false));
            comp.Instance.GetState(x => x.PasswordVisible).Should().BeFalse();
            comp.Find("input").GetAttribute("type").Should().Be("password");
        }

        [Test]
        public async Task PasswordToggle_ClickCallback_Fires()
        {
            var clicked = false;
            var comp = Context.Render<VekTextField<string>>(parameters => parameters
                .Add(p => p.InputType, InputType.Password)
                .Add(p => p.ShowPasswordToggle, true)
                .Add(p => p.OnPasswordToggleClick, EventCallback.Factory.Create<MouseEventArgs>(this, _ => clicked = true)));

            await comp.Find(".vek-input-password-toggle-button").ClickAsync();
            clicked.Should().BeTrue();
        }
    }
}
