// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using Vek.UnitTests.TestComponents.Hotkey;
using Vek.Utilities;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

#nullable enable
[TestFixture]
public class VekHotkeyTests : BunitTest
{
    [Test]
    public async Task Hotkey_ShouldShowChildContent()
    {
        // Arrange
        var comp = Context.Render<VekHotkeyTest>(p => p.Add(x => x.HideChildContentOnRepress, false));
        var hotKeyComponent = comp.FindComponent<VekHotkey>();

        // Act
        await comp.InvokeAsync(hotKeyComponent.Instance.VekHotkeyProviderJsCallback);

        // Assert
        comp.Instance.PressedCount.Should().Be(1);
        comp.Find("#hotkey-child").TextContent.Trim().Should().Be("Child Content");
    }

    [Test]
    public async Task Hotkey_ShouldNotShowChildContent()
    {
        // Arrange
        var comp = Context.Render<VekHotkeyTest>(p => p.Add(x => x.HideChildContentOnRepress, true));
        var hotKeyComponent = comp.FindComponent<VekHotkey>();
        var hotKeyChildContent = () => comp.Find("#hotkey-child");

        // Act
        await comp.InvokeAsync(hotKeyComponent.Instance.VekHotkeyProviderJsCallback);
        await comp.InvokeAsync(hotKeyComponent.Instance.VekHotkeyProviderJsCallback);

        // Assert
        comp.Instance.PressedCount.Should().Be(2);
        hotKeyChildContent.Should().Throw<ElementNotFoundException>();
    }

    [Test]
    public async Task Hotkey_ShouldPassEventArgsIdentifyingWhichHotkeyFired()
    {
        // Arrange: several hotkeys share a single handler that tells them apart via HotkeyEventArgs.
        var comp = Context.Render<VekHotkeyEventArgsTest>();
        var hotkeys = comp.FindComponents<VekHotkey>();

        // The first hotkey: KeyA, no modifiers.
        await comp.InvokeAsync(hotkeys[0].Instance.VekHotkeyProviderJsCallback);
        comp.Instance.LastArgs!.Key.Should().Be(JsKey.KeyA);
        comp.Instance.LastArgs!.KeyModifiers.Should().BeEmpty();
        comp.Instance.LastArgs!.Sender.Should().BeSameAs(hotkeys[0].Instance);

        // The second hotkey: KeyB + ShiftLeft.
        await comp.InvokeAsync(hotkeys[1].Instance.VekHotkeyProviderJsCallback);
        comp.Instance.LastArgs!.Key.Should().Be(JsKey.KeyB);
        comp.Instance.LastArgs!.KeyModifiers.Should().ContainSingle().Which.Should().Be(JsKeyModifier.ShiftLeft);
        comp.Instance.LastArgs!.Sender.Should().BeSameAs(hotkeys[1].Instance);

        // The third hotkey: SAME Key as the second, distinguished only by its (multiple) modifiers.
        await comp.InvokeAsync(hotkeys[2].Instance.VekHotkeyProviderJsCallback);
        comp.Instance.LastArgs!.Key.Should().Be(JsKey.KeyB);
        comp.Instance.LastArgs!.KeyModifiers.Should().BeEquivalentTo([JsKeyModifier.ControlLeft, JsKeyModifier.ShiftLeft]);
        comp.Instance.LastArgs!.Sender.Should().BeSameAs(hotkeys[2].Instance);

        comp.Instance.PressedCount.Should().Be(3);
    }

    [Test]
    public async Task Hotkey_JsTestComponentLifetimeCycle()
    {
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()));
        jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()));
        Context.Services.AddSingleton(jsRuntimeMock.Object);

        Context.Render<VekHotkeyTest>();
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()), Times.Exactly(1));
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()), Times.Never);

        await Context.DisposeComponentsAsync();
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()), Times.Exactly(1));
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()), Times.Exactly(1));
    }

    [Test]
    public async Task Hotkey_JsTestParameters()
    {
        var jsRuntimeMock = new Mock<IJSRuntime>();
        jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()));
        jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()));
        Context.Services.AddSingleton(jsRuntimeMock.Object);

        var comp = Context.Render<VekHotkeyTest>();

        // Enabled by default
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()), Times.Once);
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()), Times.Never);

        await comp.SetParametersAndRenderAsync(p => p
            .Add(x => x.Key, JsKey.KeyB)
            .Add(x => x.KeyModifiers, [JsKeyModifier.ShiftLeft])
            .Add(x => x.PreventEventPropagation, false));

        // Shared handler, so only one update called
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()), Times.Exactly(2));
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()), Times.Never);

        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.Disabled, true));

        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()), Times.Exactly(2));
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()), Times.Exactly(1));

        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.Disabled, false));

        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()), Times.Exactly(3));
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()), Times.Exactly(1));

        // Changing ChildContent / HideChildContentOnRepress should not unregister/register hotkey
        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.ChildContent, builder => builder.AddContent(0, "New Child Content")));
        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.HideChildContentOnRepress, true));
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.registerOrUpdateHotkey", It.IsAny<object[]>()), Times.Exactly(3));
        jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("vekHotkeyListener.unregisterHotkey", It.IsAny<object[]>()), Times.Exactly(1));
    }
}
