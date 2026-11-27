// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// Allows registering global or container-scoped keyboard shortcuts, including multi-key chord sequences.
/// </summary>
public partial class VekHotkey : VekComponentBase, IAsyncDisposable
{
    private readonly string _hotkeyId = Identifier.Create("hotkey");
    private readonly ParameterState<JsKey> _keyState;
    private readonly ParameterState<IEnumerable<JsKeyModifier>> _keyModifiersState;
    private readonly ParameterState<string?> _keyCombinationState;
    private readonly ParameterState<bool> _preventEventPropagationState;
    private readonly ParameterState<bool?> _preventDefaultState;
    private readonly ParameterState<bool> _stopPropagationState;
    private readonly ParameterState<bool> _ignoreInInputsState;
    private readonly ParameterState<HotkeyScope> _scopeState;
    private readonly ParameterState<string?> _scopeTargetIdState;
    private readonly ParameterState<TimeSpan> _chordTimeoutState;
    private readonly ParameterState<bool> _disabledState;

    private bool _childContentIsVisible;
    private bool _jsListenerActive;
    private DotNetObjectReference<VekHotkey>? _dotNetObjectReference;
    private HotkeyManager? _fallbackHotkeyManager;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    private HotkeyManager EffectiveHotkeyManager =>
        _fallbackHotkeyManager ??= ServiceProvider?.GetService(typeof(HotkeyManager)) as HotkeyManager ?? new HotkeyManager();

    /// <summary>
    /// The optional content to display when a global hotkey is pressed, or the scoped container content when <see cref="Scope"/> is <see cref="HotkeyScope.Scoped"/>.
    /// </summary>
    [Parameter, Category(CategoryTypes.Hotkey.Appearance)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The hotkey to register when <see cref="KeyCombination"/> is not specified.
    /// </summary>
    /// <remarks>
    /// If you would like to use a modifier key here, you also have to add it to <see cref="KeyModifiers"/>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public JsKey Key { get; set; }

    /// <summary>
    /// The modifiers the user has to press together with <see cref="Key"/> to trigger the hotkey.
    /// </summary>
    /// <remarks>
    /// If left empty the hotkey will be triggered by pressing <see cref="Key"/> alone.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public IEnumerable<JsKeyModifier> KeyModifiers { get; set; } = [];

    /// <summary>
    /// A declarative key combination or chord sequence string such as <c>"Ctrl+S"</c>, <c>"Alt+Shift+F"</c>, <c>"Escape"</c>, or <c>"g then d"</c>.
    /// </summary>
    /// <remarks>
    /// When set to a non-empty value, takes precedence over <see cref="Key"/> and <see cref="KeyModifiers"/>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public string? KeyCombination { get; set; }

    /// <summary>
    /// Occurs when the registered hotkey or chord sequence is triggered.
    /// </summary>
    [Parameter, Category(CategoryTypes.Hotkey.Behavior)]
    public EventCallback<HotkeyEventArgs> OnHotkeyPressed { get; set; }

    /// <summary>
    /// Occurs when the registered hotkey or chord sequence is matched on keydown.
    /// </summary>
    [Parameter, Category(CategoryTypes.Hotkey.Behavior)]
    public EventCallback<HotkeyEventArgs> OnHotkeyDown { get; set; }

    /// <summary>
    /// Whether to hide the child content when a global hotkey is pressed again, allowing for toggle behavior.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.Hotkey.Behavior)]
    public bool HideChildContentOnRepress { get; set; }

    /// <summary>
    /// Whether to prevent the browser's default action when the hotkey is triggered, unless overridden by <see cref="PreventDefault"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public bool PreventEventPropagation { get; set; } = true;

    /// <summary>
    /// Whether to call <c>preventDefault()</c> on matching keyboard events.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="PreventEventPropagation"/> when <c>null</c>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public bool? PreventDefault { get; set; }

    /// <summary>
    /// Whether to call <c>stopPropagation()</c> on matching keyboard events.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public bool StopPropagation { get; set; }

    /// <summary>
    /// Suppresses the hotkey while focus is inside editable input elements such as text fields or text areas.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public bool IgnoreInInputs { get; set; } = true;

    /// <summary>
    /// Determines whether the shortcut listens globally across the document or only within a scoped container.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="HotkeyScope.Global"/>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public HotkeyScope Scope { get; set; } = HotkeyScope.Global;

    /// <summary>
    /// Optional external element identifier to scope this hotkey to when <see cref="Scope"/> is <see cref="HotkeyScope.Scoped"/>.
    /// </summary>
    /// <remarks>
    /// When omitted and <see cref="Scope"/> is <see cref="HotkeyScope.Scoped"/>, a wrapper container element is rendered around <see cref="ChildContent"/>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public string? ScopeTargetId { get; set; }

    /// <summary>
    /// Maximum allowed delay between consecutive key presses in a chord sequence.
    /// </summary>
    /// <remarks>
    /// Defaults to 1 second.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public TimeSpan ChordTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Ignores the hotkey when set to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, ParameterState, Category(CategoryTypes.Hotkey.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// Accessible label applied to the scoped container element when <see cref="Scope"/> is <see cref="HotkeyScope.Scoped"/>.
    /// </summary>
    [Parameter, Category(CategoryTypes.Hotkey.Behavior)]
    public string? AriaLabel { get; set; }

    private bool IsScopedContainer =>
        _scopeState.Value == HotkeyScope.Scoped && string.IsNullOrWhiteSpace(_scopeTargetIdState.Value);

    private string EffectiveScopeId => GetEffectiveElementId(_hotkeyId);

    private string? EffectiveScopeTargetId => _scopeState.Value == HotkeyScope.Scoped
        ? (!string.IsNullOrWhiteSpace(_scopeTargetIdState.Value) ? _scopeTargetIdState.Value : EffectiveScopeId)
        : null;

    private string? ScopeRole => !string.IsNullOrWhiteSpace(AriaLabel) ? "region" : null;

    private string ScopeTabIndex => "-1";

    private string Classname =>
        new CssBuilder("vek-hotkey-scope")
            .AddClass(Class)
            .Build();

    [DynamicDependency(nameof(VekHotkeyProviderJsCallback))]
    [DynamicDependency(nameof(OnJsHotkeyTriggeredAsync))]
    public VekHotkey()
    {
        _dotNetObjectReference = DotNetObjectReference.Create(this);
        using var registerScope = CreateRegisterScope();
        _keyState = registerScope.RegisterParameter<JsKey>(nameof(Key))
            .WithParameter(() => Key)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _keyModifiersState = registerScope.RegisterParameter<IEnumerable<JsKeyModifier>>(nameof(KeyModifiers))
            .WithParameter(() => KeyModifiers)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _keyCombinationState = registerScope.RegisterParameter<string?>(nameof(KeyCombination))
            .WithParameter(() => KeyCombination)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _preventEventPropagationState = registerScope.RegisterParameter<bool>(nameof(PreventEventPropagation))
            .WithParameter(() => PreventEventPropagation)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _preventDefaultState = registerScope.RegisterParameter<bool?>(nameof(PreventDefault))
            .WithParameter(() => PreventDefault)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _stopPropagationState = registerScope.RegisterParameter<bool>(nameof(StopPropagation))
            .WithParameter(() => StopPropagation)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _ignoreInInputsState = registerScope.RegisterParameter<bool>(nameof(IgnoreInInputs))
            .WithParameter(() => IgnoreInInputs)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _scopeState = registerScope.RegisterParameter<HotkeyScope>(nameof(Scope))
            .WithParameter(() => Scope)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _scopeTargetIdState = registerScope.RegisterParameter<string?>(nameof(ScopeTargetId))
            .WithParameter(() => ScopeTargetId)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _chordTimeoutState = registerScope.RegisterParameter<TimeSpan>(nameof(ChordTimeout))
            .WithParameter(() => ChordTimeout)
            .WithChangeHandler(RegisterOrUpdateHotkeyAsync);
        _disabledState = registerScope.RegisterParameter<bool>(nameof(Disabled))
            .WithParameter(() => Disabled)
            .WithChangeHandler(OnDisabledChangedAsync);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            if (IsScopedContainer)
            {
                var (_, isActive) = await JsRuntime.InvokeAsyncWithErrorHandling(false, "vekHotkeyListener.isActive");
                _jsListenerActive = isActive;
            }

            if (!_disabledState.Value)
            {
                await RegisterOrUpdateHotkeyAsync();
            }
        }
    }

    private HotkeySequence BuildSequence()
    {
        var keyCombination = _keyCombinationState.Value;
        if (!string.IsNullOrWhiteSpace(keyCombination))
        {
            return HotkeySequence.Parse(keyCombination);
        }

        return HotkeySequence.FromKey(_keyState.Value, _keyModifiersState.Value);
    }

    private bool GetEffectivePreventDefault() => _preventDefaultState.Value ?? _preventEventPropagationState.Value;

    private void SyncHotkeyManagerRegistration(HotkeySequence sequence)
    {
        EffectiveHotkeyManager.RegisterOrUpdate(new HotkeyRegistration
        {
            Id = _hotkeyId,
            Sequence = sequence,
            ScopeId = EffectiveScopeTargetId,
            PreventDefault = GetEffectivePreventDefault(),
            StopPropagation = _stopPropagationState.Value,
            IgnoreInInputs = _ignoreInInputsState.Value,
            Disabled = _disabledState.Value,
            ChordTimeout = _chordTimeoutState.Value > TimeSpan.Zero ? _chordTimeoutState.Value : TimeSpan.FromSeconds(1),
            Handler = context => TriggerHotkeyAsync(context.KeyboardEventArgs, context.IsChord)
        });
    }

    private async Task RegisterOrUpdateHotkeyAsync()
    {
        var sequence = BuildSequence();
        SyncHotkeyManagerRegistration(sequence);

        if (!IsJSRuntimeAvailable || _disabledState.Value)
        {
            return;
        }

        var effectivePreventDefault = GetEffectivePreventDefault();
        var useSimpleLegacyCall = string.IsNullOrWhiteSpace(_keyCombinationState.Value)
            && _scopeState.Value == HotkeyScope.Global
            && string.IsNullOrWhiteSpace(_scopeTargetIdState.Value)
            && !_stopPropagationState.Value
            && _ignoreInInputsState.Value;

        if (useSimpleLegacyCall)
        {
            await JsRuntime.InvokeVoidAsyncWithErrorHandling(
                "vekHotkeyListener.registerOrUpdateHotkey",
                _dotNetObjectReference,
                nameof(VekHotkeyProviderJsCallback),
                _hotkeyId,
                _keyState.Value.ToString(),
                _keyModifiersState.Value.Select(m => m.ToString()).ToArray(),
                effectivePreventDefault,
                new HotkeyJsOptions(
                    Steps: null,
                    ScopeElementId: null,
                    StopPropagation: false,
                    IgnoreInInputs: _ignoreInInputsState.Value,
                    ChordTimeoutMs: (int)Math.Max(1, _chordTimeoutState.Value.TotalMilliseconds)));
            return;
        }

        var stepSpecs = sequence.Steps
            .Select(s => new HotkeyJsStepSpec(
                s.Key,
                s.Code,
                s.Modifiers.ToArray(),
                s.UseExactSideModifiers,
                s.RequireCtrl,
                s.RequireShift,
                s.RequireAlt,
                s.RequireMeta))
            .ToArray();

        var primaryKey = sequence.Steps.Count > 0 ? sequence.Steps[^1].Code : _keyState.Value.ToString();
        var primaryMods = sequence.Steps.Count > 0 ? sequence.Steps[^1].Modifiers.ToArray() : _keyModifiersState.Value.Select(m => m.ToString()).ToArray();

        await JsRuntime.InvokeVoidAsyncWithErrorHandling(
            "vekHotkeyListener.registerOrUpdateHotkey",
            _dotNetObjectReference,
            nameof(OnJsHotkeyTriggeredAsync),
            _hotkeyId,
            primaryKey,
            primaryMods,
            effectivePreventDefault,
            new HotkeyJsOptions(
                Steps: stepSpecs,
                ScopeElementId: EffectiveScopeTargetId,
                StopPropagation: _stopPropagationState.Value,
                IgnoreInInputs: _ignoreInInputsState.Value,
                ChordTimeoutMs: (int)Math.Max(1, _chordTimeoutState.Value.TotalMilliseconds)));
    }

    private async Task UnregisterHotkeyAsync()
    {
        EffectiveHotkeyManager.Unregister(_hotkeyId);

        if (!IsJSRuntimeAvailable)
        {
            return;
        }

        await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekHotkeyListener.unregisterHotkey", _hotkeyId);
    }

    private Task OnDisabledChangedAsync(ParameterChangedEventArgs<bool> args)
    {
        return !args.Value
            ? RegisterOrUpdateHotkeyAsync()
            : UnregisterHotkeyAsync();
    }

    private async Task OnScopedKeyDownAsync(KeyboardEventArgs args)
    {
        if (_jsListenerActive || _disabledState.Value)
        {
            return;
        }

        await EffectiveHotkeyManager.HandleKeyDownAsync(args, EffectiveScopeTargetId);
    }

    /// <summary>
    /// Invoked from JavaScript interop when a registered hotkey fires.
    /// </summary>
    [JSInvokable]
    public Task VekHotkeyProviderJsCallback() => TriggerHotkeyAsync(keyboardEventArgs: null, isChordOverride: null);

    /// <summary>
    /// Invoked from JavaScript interop with event details when an extended hotkey or chord sequence fires.
    /// </summary>
    [JSInvokable]
    public Task OnJsHotkeyTriggeredAsync(HotkeyJsTriggerPayload? payload)
    {
        KeyboardEventArgs? keyboardEventArgs = payload is null
            ? null
            : new KeyboardEventArgs
            {
                Key = payload.Key ?? string.Empty,
                Code = payload.Code ?? string.Empty,
                CtrlKey = payload.CtrlKey,
                ShiftKey = payload.ShiftKey,
                AltKey = payload.AltKey,
                MetaKey = payload.MetaKey,
                Repeat = payload.Repeat
            };

        return TriggerHotkeyAsync(keyboardEventArgs, payload?.IsChord);
    }

    private async Task TriggerHotkeyAsync(KeyboardEventArgs? keyboardEventArgs, bool? isChordOverride)
    {
        if (_disabledState.Value)
        {
            return;
        }

        if (_scopeState.Value == HotkeyScope.Global)
        {
            if (!_childContentIsVisible)
            {
                _childContentIsVisible = true;
                await InvokeAsync(StateHasChanged);
            }
            else if (HideChildContentOnRepress)
            {
                _childContentIsVisible = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        var sequence = BuildSequence();
        var resolvedKey = sequence.ResolvePrimaryJsKey(_keyState.Value);
        var resolvedModifiers = !string.IsNullOrWhiteSpace(_keyCombinationState.Value)
            ? sequence.ResolveModifiers(_keyModifiersState.Value)
            : _keyModifiersState.Value.ToArray();

        var eventArgs = new HotkeyEventArgs(
            resolvedKey,
            resolvedModifiers,
            this,
            keyCombination: string.IsNullOrWhiteSpace(_keyCombinationState.Value)
                ? sequence.DisplayText
                : _keyCombinationState.Value,
            scopeId: EffectiveScopeTargetId,
            isChord: isChordOverride ?? sequence.IsChord,
            keyboardEventArgs: keyboardEventArgs);

        if (OnHotkeyPressed.HasDelegate)
        {
            await OnHotkeyPressed.InvokeAsync(eventArgs);
        }

        if (OnHotkeyDown.HasDelegate)
        {
            await OnHotkeyDown.InvokeAsync(eventArgs);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_dotNetObjectReference != null)
        {
            _dotNetObjectReference.Dispose();
            _dotNetObjectReference = null;

            await UnregisterHotkeyAsync();
        }
    }

    private sealed record HotkeyJsStepSpec(
        string Key,
        string Code,
        string[] Modifiers,
        bool ExactModifiers,
        bool RequireCtrl,
        bool RequireShift,
        bool RequireAlt,
        bool RequireMeta);

    private sealed record HotkeyJsOptions(
        HotkeyJsStepSpec[]? Steps,
        string? ScopeElementId,
        bool StopPropagation,
        bool IgnoreInInputs,
        int ChordTimeoutMs);

    /// <summary>
    /// Payload deserialized from JavaScript when a hotkey triggers.
    /// </summary>
    public sealed class HotkeyJsTriggerPayload
    {
        /// <summary>
        /// The DOM <c>KeyboardEvent.key</c> value.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The DOM <c>KeyboardEvent.code</c> value.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Whether Control was held.
        /// </summary>
        public bool CtrlKey { get; set; }

        /// <summary>
        /// Whether Shift was held.
        /// </summary>
        public bool ShiftKey { get; set; }

        /// <summary>
        /// Whether Alt was held.
        /// </summary>
        public bool AltKey { get; set; }

        /// <summary>
        /// Whether Meta was held.
        /// </summary>
        public bool MetaKey { get; set; }

        /// <summary>
        /// Whether the event was a key repeat.
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// Whether the trigger completed a chord sequence.
        /// </summary>
        public bool IsChord { get; set; }
    }
}
