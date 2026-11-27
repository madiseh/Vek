// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Vek.UnitTests.Components.Hotkey;

#nullable enable
[TestFixture]
public class HotkeyManagerTests
{
    [Test]
    public void Parse_ShouldSupportSingleKeysModifiersAndChords()
    {
        var ctrlS = HotkeySequence.Parse("Ctrl+S");
        ctrlS.IsChord.Should().BeFalse();
        ctrlS.Steps.Should().ContainSingle();
        ctrlS.Steps[0].Code.Should().Be("KeyS");
        ctrlS.Steps[0].RequireCtrl.Should().BeTrue();
        ctrlS.Steps[0].RequireShift.Should().BeFalse();
        ctrlS.Steps[0].RequireAlt.Should().BeFalse();
        ctrlS.Steps[0].RequireMeta.Should().BeFalse();

        var altShiftF = HotkeySequence.Parse("Alt+Shift+F");
        altShiftF.Steps.Should().ContainSingle();
        altShiftF.Steps[0].Code.Should().Be("KeyF");
        altShiftF.Steps[0].RequireAlt.Should().BeTrue();
        altShiftF.Steps[0].RequireShift.Should().BeTrue();

        var cmdK = HotkeySequence.Parse("Cmd+K");
        cmdK.Steps.Should().ContainSingle();
        cmdK.Steps[0].Code.Should().Be("KeyK");
        cmdK.Steps[0].RequireMeta.Should().BeTrue();

        var escape = HotkeySequence.Parse("Escape");
        escape.Steps.Should().ContainSingle();
        escape.Steps[0].Code.Should().Be("Escape");

        var chord = HotkeySequence.Parse("g then d");
        chord.IsChord.Should().BeTrue();
        chord.Steps.Should().HaveCount(2);
        chord.Steps[0].Code.Should().Be("KeyG");
        chord.Steps[1].Code.Should().Be("KeyD");
        chord.DisplayText.Should().Be("G then D");

        var multiChord = HotkeySequence.Parse("Ctrl+K Ctrl+S");
        multiChord.IsChord.Should().BeTrue();
        multiChord.Steps.Should().HaveCount(2);
        multiChord.Steps[0].RequireCtrl.Should().BeTrue();
        multiChord.Steps[0].Code.Should().Be("KeyK");
        multiChord.Steps[1].RequireCtrl.Should().BeTrue();
        multiChord.Steps[1].Code.Should().Be("KeyS");
    }

    [Test]
    public async Task HandleKeyDownAsync_ShouldMatchExactModifiersAndRejectExtras()
    {
        var manager = new HotkeyManager();
        var saveCount = 0;

        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "save",
            Sequence = HotkeySequence.Parse("Ctrl+S"),
            Handler = _ =>
            {
                saveCount++;
                return Task.CompletedTask;
            }
        });

        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "s", Code = "KeyS" });
        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "s", Code = "KeyS", CtrlKey = true, ShiftKey = true });
        saveCount.Should().Be(0);

        var result = await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "s", Code = "KeyS", CtrlKey = true });
        result.Matched.Should().BeTrue();
        result.PreventDefault.Should().BeTrue();
        saveCount.Should().Be(1);
    }

    [Test]
    public async Task HandleKeyDownAsync_ShouldSupportChordSequenceAndResetOnMismatch()
    {
        var manager = new HotkeyManager();
        var navCount = 0;
        var escCount = 0;

        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "go-dashboard",
            Sequence = HotkeySequence.Parse("g then d"),
            Handler = _ =>
            {
                navCount++;
                return Task.CompletedTask;
            }
        });
        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "cancel",
            Sequence = HotkeySequence.Parse("Escape"),
            Handler = _ =>
            {
                escCount++;
                return Task.CompletedTask;
            }
        });

        var step1 = await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "g", Code = "KeyG" });
        step1.Matched.Should().BeFalse();
        step1.IsChordPrefix.Should().BeTrue();
        step1.PendingChordDisplay.Should().Be("G");
        manager.HasPendingChord().Should().BeTrue();

        // Non-matching Escape breaks the pending chord AND still triggers the single-key Escape shortcut.
        var escResult = await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "Escape", Code = "Escape" });
        escResult.Matched.Should().BeTrue();
        escCount.Should().Be(1);
        navCount.Should().Be(0);
        manager.HasPendingChord().Should().BeFalse();

        // Now complete the full "g then d" chord.
        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "g", Code = "KeyG" });
        var step2 = await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "d", Code = "KeyD" });
        step2.Matched.Should().BeTrue();
        navCount.Should().Be(1);
        manager.HasPendingChord().Should().BeFalse();
    }

    [Test]
    public async Task HandleKeyDownAsync_ShouldExpirePendingChordAfterTimeout()
    {
        var fakeTime = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var manager = new HotkeyManager(fakeTime);
        var triggered = false;

        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "chord",
            Sequence = HotkeySequence.Parse("g then d"),
            ChordTimeout = TimeSpan.FromMilliseconds(500),
            Handler = _ =>
            {
                triggered = true;
                return Task.CompletedTask;
            }
        });

        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "g", Code = "KeyG" });
        manager.HasPendingChord().Should().BeTrue();

        fakeTime.Advance(TimeSpan.FromMilliseconds(600));
        manager.HasPendingChord().Should().BeFalse();

        var secondResult = await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "d", Code = "KeyD" });
        secondResult.Matched.Should().BeFalse();
        triggered.Should().BeFalse();
    }

    [Test]
    public async Task HandleKeyDownAsync_ShouldSuppressInsideInputsWhenIgnoreInInputsIsTrue()
    {
        var manager = new HotkeyManager();
        var suppressedCount = 0;
        var allowedCount = 0;

        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "suppressed",
            Sequence = HotkeySequence.Parse("Ctrl+S"),
            IgnoreInInputs = true,
            Handler = _ =>
            {
                suppressedCount++;
                return Task.CompletedTask;
            }
        });
        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "allowed",
            Sequence = HotkeySequence.Parse("Escape"),
            IgnoreInInputs = false,
            Handler = _ =>
            {
                allowedCount++;
                return Task.CompletedTask;
            }
        });

        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "s", Code = "KeyS", CtrlKey = true }, isInputFocused: true);
        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "Escape", Code = "Escape" }, isInputFocused: true);

        suppressedCount.Should().Be(0);
        allowedCount.Should().Be(1);
    }

    [Test]
    public async Task HandleKeyDownAsync_ScopedRegistrationShouldOverrideGlobalWithinScope()
    {
        var manager = new HotkeyManager();
        var globalCount = 0;
        var scopedCount = 0;

        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "global-save",
            Sequence = HotkeySequence.Parse("Ctrl+S"),
            Handler = _ =>
            {
                globalCount++;
                return Task.CompletedTask;
            }
        });
        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "scoped-save",
            ScopeId = "editor-scope",
            Sequence = HotkeySequence.Parse("Ctrl+S"),
            StopPropagation = true,
            Handler = _ =>
            {
                scopedCount++;
                return Task.CompletedTask;
            }
        });

        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "s", Code = "KeyS", CtrlKey = true }, scopeId: "editor-scope");
        scopedCount.Should().Be(1);
        globalCount.Should().Be(0);

        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "s", Code = "KeyS", CtrlKey = true }, scopeId: "other-scope");
        scopedCount.Should().Be(1);
        globalCount.Should().Be(1);
    }

    [Test]
    public async Task HandleKeyDownAsync_ShouldIgnoreStandaloneModifierPressBetweenChordSteps()
    {
        var manager = new HotkeyManager();
        var fired = false;

        manager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = "chord-mod",
            Sequence = HotkeySequence.Parse("Ctrl+K then Ctrl+S"),
            Handler = _ =>
            {
                fired = true;
                return Task.CompletedTask;
            }
        });

        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "k", Code = "KeyK", CtrlKey = true });
        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "Control", Code = "ControlLeft", CtrlKey = true });
        await manager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "s", Code = "KeyS", CtrlKey = true });

        fired.Should().BeTrue();
    }
}
