// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vek.Utilities;

namespace Vek.UnitTests.Components.Hotkey;

#nullable enable
[TestFixture]
public class VekHotkeyScopedAndChordTests : BunitTest
{
    [Test]
    public async Task KeyCombination_ShouldTriggerOnHotkeyDownWithParsedEventArgs()
    {
        HotkeyEventArgs? receivedArgs = null;

        var comp = Context.Render<VekHotkey>(p => p
            .Add(x => x.KeyCombination, "Alt+Shift+F")
            .Add(x => x.PreventDefault, true)
            .Add(x => x.StopPropagation, true)
            .Add(x => x.OnHotkeyDown, EventCallback.Factory.Create<HotkeyEventArgs>(this, args => receivedArgs = args)));

        var manager = Context.Services.GetRequiredService<HotkeyManager>();
        var result = await comp.InvokeAsync(() =>
            manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "f", Code = "KeyF", AltKey = true, ShiftKey = true }));

        result.Matched.Should().BeTrue();
        result.PreventDefault.Should().BeTrue();
        result.StopPropagation.Should().BeTrue();
        receivedArgs.Should().NotBeNull();
        receivedArgs!.Key.Should().Be(JsKey.KeyF);
        receivedArgs.KeyCombination.Should().Be("Alt+Shift+F");
        receivedArgs.IsChord.Should().BeFalse();
        receivedArgs.Sender.Should().BeSameAs(comp.Instance);
    }

    [Test]
    public async Task ScopedContainer_ShouldRenderAriaAndHandleChordKeyDown()
    {
        HotkeyEventArgs? receivedArgs = null;

        var comp = Context.Render<VekHotkey>(p => p
            .Add(x => x.KeyCombination, "g then d")
            .Add(x => x.Scope, HotkeyScope.Scoped)
            .Add(x => x.AriaLabel, "Navigation shortcuts")
            .Add(x => x.Class, "custom-scope")
            .Add(x => x.Style, "padding: 8px")
            .AddUnmatched("data-testid", "scoped-hotkey")
            .Add(x => x.OnHotkeyDown, EventCallback.Factory.Create<HotkeyEventArgs>(this, args => receivedArgs = args))
            .AddChildContent("<button type=\"button\">Inside Scope</button>"));

        var scopeEl = comp.Find("[data-testid='scoped-hotkey']");
        scopeEl.GetAttribute("role").Should().Be("region");
        scopeEl.GetAttribute("aria-label").Should().Be("Navigation shortcuts");
        scopeEl.GetAttribute("tabindex").Should().Be("-1");
        scopeEl.ClassList.Should().Contain("vek-hotkey-scope");
        scopeEl.ClassList.Should().Contain("custom-scope");

        await scopeEl.KeyDownAsync(new KeyboardEventArgs { Key = "g", Code = "KeyG" });
        receivedArgs.Should().BeNull();

        await scopeEl.KeyDownAsync(new KeyboardEventArgs { Key = "d", Code = "KeyD" });
        receivedArgs.Should().NotBeNull();
        receivedArgs!.IsChord.Should().BeTrue();
        receivedArgs.KeyCombination.Should().Be("g then d");
        receivedArgs.ScopeId.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ScopedContainer_ShouldExpireChordWhenFakeTimeAdvances()
    {
        var fakeTime = Context.AddFakeTimeProvider();
        var firedCount = 0;

        var comp = Context.Render<VekHotkey>(p => p
            .Add(x => x.KeyCombination, "g then d")
            .Add(x => x.Scope, HotkeyScope.Scoped)
            .Add(x => x.ChordTimeout, TimeSpan.FromMilliseconds(400))
            .Add(x => x.OnHotkeyDown, EventCallback.Factory.Create<HotkeyEventArgs>(this, _ => firedCount++))
            .AddChildContent("<span>Scoped Area</span>"));

        var scopeEl = comp.Find(".vek-hotkey-scope");
        await scopeEl.KeyDownAsync(new KeyboardEventArgs { Key = "g", Code = "KeyG" });
        fakeTime.Advance(TimeSpan.FromMilliseconds(500));
        await scopeEl.KeyDownAsync(new KeyboardEventArgs { Key = "d", Code = "KeyD" });

        firedCount.Should().Be(0);
    }

    [Test]
    public async Task ScopeTargetId_ShouldRegisterAgainstExternalTargetWithoutWrapperMarkup()
    {
        var triggered = false;

        var comp = Context.Render<VekHotkey>(p => p
            .Add(x => x.KeyCombination, "Ctrl+K")
            .Add(x => x.Scope, HotkeyScope.Scoped)
            .Add(x => x.ScopeTargetId, "external-panel")
            .Add(x => x.OnHotkeyDown, EventCallback.Factory.Create<HotkeyEventArgs>(this, _ => triggered = true)));

        comp.Markup.Trim().Should().BeEmpty();

        var manager = Context.Services.GetRequiredService<HotkeyManager>();
        await comp.InvokeAsync(() => manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "k", Code = "KeyK", CtrlKey = true }));
        triggered.Should().BeFalse();

        await comp.InvokeAsync(() => manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "k", Code = "KeyK", CtrlKey = true }, scopeId: "external-panel"));
        triggered.Should().BeTrue();
    }

    [Test]
    public async Task DisabledAndDispose_ShouldSyncWithHotkeyManager()
    {
        var comp = Context.Render<VekHotkey>(p => p
            .Add(x => x.KeyCombination, "Escape")
            .Add(x => x.Disabled, false));

        var manager = Context.Services.GetRequiredService<HotkeyManager>();
        manager.GetRegistrations().Should().ContainSingle(r => !r.Disabled);

        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.Disabled, true));
        manager.GetRegistrations().Should().BeEmpty();

        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.Disabled, false));
        manager.GetRegistrations().Should().ContainSingle(r => !r.Disabled);

        await Context.DisposeComponentsAsync();
        manager.GetRegistrations().Should().BeEmpty();
    }
}
