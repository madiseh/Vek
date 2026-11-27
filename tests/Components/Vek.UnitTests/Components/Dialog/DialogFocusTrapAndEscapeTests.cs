using System.Linq;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.Dialog;

namespace Vek.UnitTests.Components.Dialog;

[TestFixture]
public class DialogFocusTrapAndEscapeTests : BunitTest
{
    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task VekDialog_CloseOnEscapeKey_ShouldControlEscapeDismissal(bool closeOnEscapeKey, bool expectedClosed)
    {
        var keyInterceptorService = Context.AddKeyInterceptorService();
        var comp = Context.Render<VekDialogProvider>();
        var service = Context.Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<DialogFocusTrapAndEscapeParameterTest>
        {
            { x => x.CloseOnEscapeKey, closeOnEscapeKey }
        };

        var reference = await comp.InvokeAsync(() => service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Escape Test", parameters));
        var container = comp.FindComponent<VekDialogContainer>();

        await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(container.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));

        if (expectedClosed)
        {
            comp.FindAll("div.vek-dialog-container").Should().BeEmpty();
            var result = await reference.Result;
            result.Canceled.Should().BeTrue();
        }
        else
        {
            comp.FindAll("div.vek-dialog-container").Should().ContainSingle();
            reference.Result.IsCompleted.Should().BeFalse();
        }
    }

    [Test]
    public async Task VekDialog_CloseOnEscapeKey_ShouldOverrideOptionsAndGlobalProvider()
    {
        var keyInterceptorService = Context.AddKeyInterceptorService();
        var comp = Context.Render<VekDialogProvider>(p => p.Add(x => x.CloseOnEscapeKey, false));
        var service = Context.Services.GetRequiredService<IDialogService>();

        var parametersAllow = new DialogParameters<DialogFocusTrapAndEscapeParameterTest>
        {
            { x => x.CloseOnEscapeKey, true }
        };
        var reference1 = await comp.InvokeAsync(() =>
            service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Override True", parametersAllow, new DialogOptions { CloseOnEscapeKey = false }));
        var container1 = comp.FindComponent<VekDialogContainer>();

        await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(container1.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));
        comp.FindAll("div.vek-dialog-container").Should().BeEmpty();
        (await reference1.Result).Canceled.Should().BeTrue();

        var comp2 = Context.Render<VekDialogProvider>(p => p.Add(x => x.CloseOnEscapeKey, true));
        var parametersBlock = new DialogParameters<DialogFocusTrapAndEscapeParameterTest>
        {
            { x => x.CloseOnEscapeKey, false }
        };
        var reference2 = await comp2.InvokeAsync(() =>
            service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Override False", parametersBlock, new DialogOptions { CloseOnEscapeKey = true }));
        var container2 = comp2.FindComponent<VekDialogContainer>();

        await comp2.InvokeAsync(() => keyInterceptorService.OnKeyDown(container2.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));
        comp2.FindAll("div.vek-dialog-container").Should().ContainSingle();
        reference2.Result.IsCompleted.Should().BeFalse();
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task VekDialog_CloseOnBackdropClick_ShouldControlBackdropDismissal(bool closeOnBackdropClick, bool expectedClosed)
    {
        var comp = Context.Render<VekDialogProvider>();
        var service = Context.Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<DialogFocusTrapAndEscapeParameterTest>
        {
            { x => x.CloseOnBackdropClick, closeOnBackdropClick }
        };

        var reference = await comp.InvokeAsync(() => service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Backdrop Test", parameters));

        await comp.Find("div.vek-overlay").ClickAsync();

        if (expectedClosed)
        {
            comp.FindAll("div.vek-dialog-container").Should().BeEmpty();
            var result = await reference.Result;
            result.Canceled.Should().BeTrue();
        }
        else
        {
            comp.FindAll("div.vek-dialog-container").Should().ContainSingle();
            reference.Result.IsCompleted.Should().BeFalse();
        }
    }

    [Test]
    public async Task VekDialog_CloseOnBackdropClick_FalseWithCloseOnEscapeKeyTrue_ShouldRefocusDialog()
    {
        var comp = Context.Render<VekDialogProvider>();
        var service = Context.Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<DialogFocusTrapAndEscapeParameterTest>
        {
            { x => x.CloseOnBackdropClick, false },
            { x => x.CloseOnEscapeKey, true }
        };

        await comp.InvokeAsync(() => service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Refocus Test", parameters));
        var focusCountBefore = Context.JSInterop.Invocations.Count(x => x.Identifier == "Blazor._internal.domWrapper.focus");

        await comp.Find("div.vek-overlay").ClickAsync();

        comp.FindAll("div.vek-dialog-container").Should().ContainSingle();
        var focusCountAfter = Context.JSInterop.Invocations.Count(x => x.Identifier == "Blazor._internal.domWrapper.focus");
        focusCountAfter.Should().BeGreaterThan(focusCountBefore);
    }

    [Test]
    public async Task InlineDialog_CloseOnEscapeKeyAndBackdropClick_ShouldSupportDynamicParameterUpdates()
    {
        var keyInterceptorService = Context.AddKeyInterceptorService();
        var provider = Context.Render<VekDialogProvider>();
        var host = Context.Render<InlineDialogFocusTrapEscapeTest>(p => p
            .Add(x => x.CloseOnEscapeKey, false)
            .Add(x => x.CloseOnBackdropClick, false));

        await host.Find(".open-inline-modal-btn").ClickAsync();
        await provider.WaitForAssertionAsync(() => provider.FindAll("div.vek-dialog-container").Should().ContainSingle());

        var container = provider.FindComponent<VekDialogContainer>();

        await provider.Find("div.vek-overlay").ClickAsync();
        provider.FindAll("div.vek-dialog-container").Should().ContainSingle();

        await provider.InvokeAsync(() => keyInterceptorService.OnKeyDown(container.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));
        provider.FindAll("div.vek-dialog-container").Should().ContainSingle();

        await host.SetParametersAndRenderAsync(p => p
            .Add(x => x.CloseOnEscapeKey, true)
            .Add(x => x.CloseOnBackdropClick, true));

        await provider.InvokeAsync(() => keyInterceptorService.OnKeyDown(container.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));
        await provider.WaitForAssertionAsync(() => provider.FindAll("div.vek-dialog-container").Should().BeEmpty());
    }

    [Test]
    public async Task DialogOptions_DefaultFocus_ShouldConfigureFocusTrap_WhenVekDialogDefaultFocusIsUnset()
    {
        var comp = Context.Render<VekDialogProvider>();
        var service = Context.Services.GetRequiredService<IDialogService>();

        await comp.InvokeAsync(() => service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Last Child Focus", new DialogOptions
        {
            DefaultFocus = DefaultFocus.LastChild
        }));

        var focusTrap = comp.FindComponent<VekFocusTrap>();
        focusTrap.Instance.DefaultFocus.Should().Be(DefaultFocus.LastChild);
        Context.JSInterop.VerifyInvoke("vekElementRef.focusLast", 1);
    }

    [Test]
    public async Task StackedDialogs_ShouldDisableUnderlyingFocusTrap_AndOnlyCloseTopmostOnEscape()
    {
        var keyInterceptorService = Context.AddKeyInterceptorService();
        var comp = Context.Render<VekDialogProvider>();
        var service = Context.Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<DialogFocusTrapAndEscapeParameterTest>
        {
            { x => x.CloseOnEscapeKey, true }
        };

        var firstRef = await comp.InvokeAsync(() => service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("First", parameters));
        var secondRef = await comp.InvokeAsync(() => service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Second", parameters));

        var traps = comp.FindComponents<VekFocusTrap>();
        traps.Should().HaveCount(2);
        traps[0].Instance.Disabled.Should().BeTrue();
        traps[1].Instance.Disabled.Should().BeFalse();

        var containers = comp.FindComponents<VekDialogContainer>();
        var firstContainer = containers[0];
        var secondContainer = containers[1];

        // Escape targeted at non-topmost dialog must be ignored.
        await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(firstContainer.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));
        comp.FindAll("div.vek-dialog-container").Should().HaveCount(2);

        // Escape targeted at topmost dialog closes only the second dialog and re-enables the first trap.
        await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(secondContainer.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));
        comp.FindAll("div.vek-dialog-container").Should().ContainSingle();
        (await secondRef.Result).Canceled.Should().BeTrue();
        firstRef.Result.IsCompleted.Should().BeFalse();

        comp.FindComponent<VekFocusTrap>().Instance.Disabled.Should().BeFalse();
    }

    [Test]
    public async Task Dialog_Close_ShouldSaveFocusOnOpen_AndRestoreFocusToTriggerOnClose()
    {
        var keyInterceptorService = Context.AddKeyInterceptorService();
        var comp = Context.Render<VekDialogProvider>();
        var service = Context.Services.GetRequiredService<IDialogService>();

        var parameters = new DialogParameters<DialogFocusTrapAndEscapeParameterTest>
        {
            { x => x.CloseOnEscapeKey, true }
        };

        await comp.InvokeAsync(() => service.ShowAsync<DialogFocusTrapAndEscapeParameterTest>("Restore Focus", parameters));
        Context.JSInterop.VerifyInvoke("vekElementRef.saveFocus", 1);

        var container = comp.FindComponent<VekDialogContainer>();
        await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(container.Instance.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown" }));

        await comp.WaitForAssertionAsync(() => Context.JSInterop.VerifyInvoke("vekElementRef.restoreFocus", 1));
    }
}
