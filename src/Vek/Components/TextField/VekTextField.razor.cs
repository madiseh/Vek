using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// An input for collecting text values.
    /// </summary>
    /// <typeparam name="T">The type of object managed by this input.</typeparam>
    public partial class VekTextField<T> : VekDebouncedInput<T>
    {
        private IMask? _mask;
        private VekMask? _maskReference;
        private readonly ParameterState<global::Vek.BindValueMode?> _bindValueModeState;
        private readonly ParameterState<double> _inputDelayState;
        private readonly ParameterState<bool> _showPasswordToggleState;
        private readonly ParameterState<bool> _passwordVisibleState;

        /// <summary>
        /// Initializes a new instance of the <see cref="VekTextField{T}"/> class.
        /// </summary>
        public VekTextField()
        {
            using var registerScope = CreateRegisterScope();
            _bindValueModeState = registerScope.RegisterParameter<global::Vek.BindValueMode?>(nameof(BindValueMode))
                .WithParameter(() => BindValueMode)
                .WithChangeHandler(OnBindValueModeChangedAsync);
            _inputDelayState = registerScope.RegisterParameter<double>(nameof(InputDelay))
                .WithParameter(() => InputDelay)
                .WithChangeHandler(OnInputDelayChangedAsync);
            _showPasswordToggleState = registerScope.RegisterParameter<bool>(nameof(ShowPasswordToggle))
                .WithParameter(() => ShowPasswordToggle);
            _passwordVisibleState = registerScope.RegisterParameter<bool>(nameof(PasswordVisible))
                .WithParameter(() => PasswordVisible)
                .WithEventCallback(() => PasswordVisibleChanged);
        }

        protected override double EffectiveDebounceInterval => _bindValueModeState.Value switch
        {
            null => base.EffectiveDebounceInterval,
            global::Vek.BindValueMode.OnDelayedInput => Math.Max(0, _inputDelayState.Value),
            _ => 0
        };

        protected internal override bool EffectiveImmediate => _bindValueModeState.Value is null
            ? base.EffectiveImmediate
            : _bindValueModeState.Value switch
            {
                global::Vek.BindValueMode.OnInput or global::Vek.BindValueMode.OnChange or global::Vek.BindValueMode.OnDelayedInput => true,
                _ => false
            };

        protected string Classname =>
            new CssBuilder("vek-input-input-control")
                .AddClass($"vek-input-sizing-{Sizing.ToStringFast(true)}")
                .AddClass(Class)
                .Build();

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// The reference to the underlying <see cref="VekInput{T}"/> component.
        /// </summary>
        public VekInput<string>? InputReference { get; private set; }

        /// <summary>
        /// The type of input collected by this component.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="InputType.Text"/>.  Represents a valid HTML5 input type.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public InputType InputType { get; set; } = InputType.Text;

        /// <summary>
        /// Controls when user-entered text updates the bound value.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, which preserves the legacy <see cref="VekBaseInput{T}.Immediate"/> and <see cref="VekDebouncedInput{T}.DebounceInterval"/> behavior.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public global::Vek.BindValueMode? BindValueMode { get; set; }

        /// <summary>
        /// The delay before an <see cref="global::Vek.BindValueMode.OnDelayedInput"/> value is committed.
        /// </summary>
        /// <remarks>
        /// Defaults to 300 milliseconds. Non-positive values commit immediately.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public double InputDelay { get; set; } = 300;

        /// <summary>
        /// Shows a button to clear this input's value.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Clearable { get; set; }

        /// <summary>
        /// Controls when the clear button is displayed.
        /// </summary>
        /// <remarks>
        /// Defaults to the legacy <see cref="Clearable"/> setting. When omitted, a clearable field shows its button only while it has a value.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public ClearButtonDisplayMode? ClearButtonDisplayMode { get; set; }

        /// <summary>
        /// The icon to display when <see cref="Clearable"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.Clear"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string ClearIcon { get; set; } = Icons.Material.Filled.Clear;

        /// <summary>
        /// Occurs when the clear button is clicked.
        /// </summary>
        /// <remarks>
        /// When clicked, the <see cref="VekBaseInput{T}.Text"/> and <see cref="VekBaseInput{T}.Value"/> properties are reset.
        /// </remarks>
        [Parameter]
        public EventCallback<MouseEventArgs> OnClearButtonClick { get; set; }

        /// <summary>
        /// Shows an interactive button to toggle password visibility between masked and plain text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. Only applies when <see cref="InputType"/> is <see cref="InputType.Password"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool ShowPasswordToggle { get; set; }

        /// <summary>
        /// Indicates whether the password is currently visible in plain text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. Supports two-way binding.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool PasswordVisible { get; set; }

        /// <summary>
        /// Occurs when the password visibility changes.
        /// </summary>
        [Parameter]
        public EventCallback<bool> PasswordVisibleChanged { get; set; }

        /// <summary>
        /// Occurs when the password toggle button is clicked.
        /// </summary>
        [Parameter]
        public EventCallback<MouseEventArgs> OnPasswordToggleClick { get; set; }

        /// <summary>
        /// The icon to display when the password is hidden, allowing the user to reveal it.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.Visibility"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string PasswordHiddenIcon { get; set; } = Icons.Material.Filled.Visibility;

        /// <summary>
        /// The icon to display when the password is visible, allowing the user to hide it.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.VisibilityOff"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string PasswordVisibleIcon { get; set; } = Icons.Material.Filled.VisibilityOff;

        /// <summary>
        /// The <c>aria-label</c> for the password toggle button when the password is hidden.
        /// </summary>
        /// <remarks>
        /// Defaults to the localized equivalent of "Show password".
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string? PasswordHiddenAriaLabel { get; set; }

        /// <summary>
        /// The <c>aria-label</c> for the password toggle button when the password is visible.
        /// </summary>
        /// <remarks>
        /// Defaults to the localized equivalent of "Hide password".
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string? PasswordVisibleAriaLabel { get; set; }

        /// <summary>
        /// The mask to apply to text values.
        /// </summary>
        /// <remarks>
        /// Typically set to common masks such as <see cref="PatternMask"/>, <see cref="MultiMask"/>, <see cref="RegexMask"/>, and <see cref="BlockMask"/>.
        /// When set, some properties will be ignored such as <see cref="VekInput{T}.MaxLines"/>, <see cref="VekInput{T}.Sizing"/>, and <see cref="VekInput{T}.HideSpinButtons"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.General.Data)]
        public IMask? Mask
        {
            get => _maskReference?.Mask ?? _mask; // this might look strange, but it is absolutely necessary due to how VekMask works.
            set => _mask = value;
        }

        /// <summary>
        /// Defines the resizing behavior of this input.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="InputSizing.Fixed"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.General.Behavior)]
        public InputSizing Sizing { get; set; } = InputSizing.Fixed;

        /// <summary>
        /// The maximum vertical lines to display when <see cref="Sizing"/> is <see cref="InputSizing.Auto"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>0</c>.  When <c>0</c>. this property is ignored.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.General.Behavior)]
        public int MaxLines { get; set; }

        [MemberNotNullWhen(false, nameof(InputReference))]
        [MemberNotNullWhen(true, nameof(_mask), nameof(Mask), nameof(_maskReference))]
        private bool HasMask => _mask is not null;

        /// <inheritdoc />
        public override ValueTask FocusAsync() => FocusAsync(preventScroll: false);

        /// <inheritdoc />
        internal override ValueTask FocusAsync(bool preventScroll)
        {
            if (!HasMask)
            {
                return InputReference.FocusAsync(preventScroll);
            }

            return _maskReference.FocusAsync(preventScroll);
        }

        /// <inheritdoc />
        public override ValueTask BlurAsync()
        {
            if (!HasMask)
            {
                return InputReference.BlurAsync();
            }

            return _maskReference.BlurAsync();
        }

        /// <inheritdoc />
        public override ValueTask SelectAsync()
        {
            if (!HasMask)
            {
                return InputReference.SelectAsync();
            }

            return _maskReference.SelectAsync();
        }

        /// <inheritdoc />
        public override ValueTask SelectRangeAsync(int pos1, int pos2)
        {
            if (!HasMask)
            {
                return InputReference.SelectRangeAsync(pos1, pos2);
            }

            return _maskReference.SelectRangeAsync(pos1, pos2);
        }

        /// <inheritdoc />
        protected override async Task ResetValueAsync()
        {
            if (!HasMask)
            {
                await InputReference.ResetAsync();
            }
            else
            {
                await _maskReference.ResetAsync();
            }

            await base.ResetValueAsync();
        }

        /// <summary>
        /// Clears the <see cref="VekBaseInput{T}.Text"/> and sets <see cref="VekBaseInput{T}.Value"/> to <c>default(T)</c>.
        /// </summary>
        public Task ClearAsync()
        {
            if (!HasMask)
            {
                return InputReference.SetText(null);
            }

            return _maskReference.Clear();
        }

        /// <summary>
        /// Sets the <see cref="VekBaseInput{T}.Text"/> to the specified value.
        /// </summary>
        /// <param name="text">The new text value to use.</param>
        public async Task SetTextAsync(string? text)
        {
            // A programmatic text set is not a user interaction, so suppress the touched/FieldChanged side effects during the value sync: otherwise the change validates and updates the form while Touched is momentarily true (#12997).
            var previousValue = ReadValue;
            await SuppressInteractionEffectsWhileAsync(async () =>
            {
                if (HasMask)
                {
                    await _maskReference.Clear();
                    await _maskReference.OnPasteAsync(text);
                }
                else
                {
                    await InputReference.SetText(text);
                }
            });

            // Notify the form explicitly to keep the SetTextAsync-fires-FieldChanged contract, but only on a real change so a no-op set (same value) is not a spurious notification, matching the suppressed path's early-returns.
            if (!EqualityComparer<T?>.Default.Equals(previousValue, ReadValue))
            {
                FieldChanged(ReadValue);
            }
        }

        /// <summary>
        /// Returns the current caret position.
        /// </summary>
        /// <remarks>
        /// Returns the text length if called and this field hasn't been focused yet.
        /// Returns <c>-1</c> if called before this component has been rendered.
        /// </remarks>
        public async Task<int> GetCurrentCaretPositionAsync()
        {
            if (IsJSRuntimeAvailable && InputReference != null)
            {
                return await JsRuntime.InvokeAsync<int>("vekInput.getCaretPosition", InputReference.ElementReference);
            }

            return -1;
        }

        /// <summary>
        /// Inserts the given text at the given caret position.
        /// </summary>
        /// <param name="text">The text to insert.</param>
        /// <param name="position">The position to insert the text at. Set to <c>0</c> to insert the text before and to <c>int.MaxValue</c> after the existing text.</param>
        /// <remarks>
        /// If <c>position</c> is greater than the current text length, the text will be inserted at the end.<br/>
        /// If <c>position</c> is less than <c>0</c>, the text will be inserted at the beginning.<br/>
        /// Note that this function doesn't support <see cref="VekMask"/>.
        /// </remarks>
        public async Task InsertTextAsync(string text, int position = int.MaxValue)
        {
            if (HasMask)
            {
                throw new InvalidOperationException("Cannot insert text into masked input.");
            }

            if (IsJSRuntimeAvailable && InputReference != null)
            {
                await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekInput.insertAtPosition", InputReference.ElementReference, text, position);
            }
        }

        /// <summary>
        /// Inserts the given text at the current caret position.
        /// </summary>
        /// <param name="text">The text to insert.</param>
        public async Task InsertTextAtCurrentCaretPositionAsync(string text)
        {
            if (!HasMask && IsJSRuntimeAvailable && InputReference != null)
            {
                await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekInput.insertAtCurrentCaretPosition", InputReference.ElementReference, text);
                return;
            }

            if (HasMask)
            {
                await _maskReference.OnPasteAsync(text);
            }
        }

        /// <inheritdoc />
        protected override Task SetValueAndUpdateTextAsync(T? value, bool updateText = true, bool force = false)
        {
            if (HasMask)
            {
                var textValue = ConvertSet(value);
                _mask.SetText(textValue);
                textValue = Mask.GetCleanText();
                value = ConvertGet(textValue);
            }

            return base.SetValueAndUpdateTextAsync(value, updateText, force);
        }

        /// <inheritdoc />
        protected override Task SetTextAndUpdateValueAsync(string? text, bool updateValue = true)
        {
            if (HasMask)
            {
                _mask.SetText(text);
                text = _mask.Text;
            }

            return base.SetTextAndUpdateValueAsync(text, updateValue);
        }

        protected internal InputType EffectiveInputType =>
            (_showPasswordToggleState.Value && InputType == InputType.Password && _passwordVisibleState.Value)
                ? InputType.Text
                : InputType;

        /// <inheritdoc />
        protected internal override InputType GetInputType() => EffectiveInputType;

        /// <summary>
        /// Toggles the visibility of the password between masked and plain text.
        /// </summary>
        public async Task TogglePasswordVisibilityAsync()
        {
            var caret = -1;
            if (_isFocused && IsJSRuntimeAvailable && InputReference != null)
            {
                var (success, pos) = await JsRuntime.InvokeAsyncWithErrorHandling(-1, "vekInput.getCaretPosition", InputReference.ElementReference);
                if (success)
                {
                    caret = pos;
                }
            }

            var next = !_passwordVisibleState.Value;
            await _passwordVisibleState.SetValueAsync(next);
            StateHasChanged();

            if (_isFocused && IsJSRuntimeAvailable && InputReference != null)
            {
                if (caret >= 0)
                {
                    await SelectRangeAsync(caret, caret);
                }
                else
                {
                    await FocusAsync();
                }
            }
        }

        /// <summary>
        /// Sets the visibility of the password.
        /// </summary>
        /// <param name="visible">Whether the password should be displayed as plain text.</param>
        public async Task SetPasswordVisibleAsync(bool visible)
        {
            if (_passwordVisibleState.Value == visible)
            {
                return;
            }

            var caret = -1;
            if (_isFocused && IsJSRuntimeAvailable && InputReference != null)
            {
                var (success, pos) = await JsRuntime.InvokeAsyncWithErrorHandling(-1, "vekInput.getCaretPosition", InputReference.ElementReference);
                if (success)
                {
                    caret = pos;
                }
            }

            await _passwordVisibleState.SetValueAsync(visible);
            StateHasChanged();

            if (_isFocused && IsJSRuntimeAvailable && InputReference != null)
            {
                if (caret >= 0)
                {
                    await SelectRangeAsync(caret, caret);
                }
                else
                {
                    await FocusAsync();
                }
            }
        }

        private Task OnInnerPasswordVisibleChangedAsync(bool visible)
        {
            StateHasChanged();
            return _passwordVisibleState.SetValueAsync(visible);
        }

        private bool ShowClearButton()
        {
            var displayMode = ClearButtonDisplayMode ?? (Clearable ? Vek.ClearButtonDisplayMode.WhileEditing : Vek.ClearButtonDisplayMode.Never);
            if (displayMode == Vek.ClearButtonDisplayMode.Never || GetDisabledState())
            {
                return false;
            }

            return !SubscribeToParentForm || !GetReadOnlyState();
        }

        private async Task OnInnerValueChangedAsync(string? text)
        {
            if (_bindValueModeState.Value == global::Vek.BindValueMode.OnChange)
            {
                await SetTextAndUpdateValueAsync(text, updateValue: false);
                return;
            }

            var valueBefore = ReadValue;
            await SetTextAndUpdateValueAsync(text);

            // When the entered text parses to the SAME Value, SetValueAndUpdateTextAsync returns early and
            // the value->text round-trip that VekTextField relies on for formatting never runs, so a
            // Converter or Format is not re-applied and the raw text is left on screen (#13096). Only in
            // that value-unchanged case, resync the displayed text to the value's formatted representation.
            // Gated to committed changes (not live oninput typing, not while debouncing) so we never
            // reformat mid-keystroke and jump the caret, matching VekNumericField.
            if (_bindValueModeState.Value is null && !Immediate && DebounceInterval <= 0 && !ConversionError && !HasMask
              && EqualityComparer<T?>.Default.Equals(valueBefore, ReadValue))
            {
                var formatted = ConvertSet(ReadValue);
                if (!string.Equals(ReadText, formatted, StringComparison.Ordinal))
                {
                    await SetTextCoreAsync(formatted);
                    await InputReference.SetText(formatted, updateValue: false);
                }
            }
        }

        /// <inheritdoc />
        protected internal override async Task OnBlurredAsync(FocusEventArgs obj)
        {
            if (_bindValueModeState.Value is not null)
            {
                await CommitTextValueAsync();
            }

            await base.OnBlurredAsync(obj);

            // Apply the format/converter to the displayed text on blur. While the user edits an Immediate
            // input the value->text echo is suppressed so their raw text is preserved (#13002), so we
            // re-derive the formatted text here to restore the pre-v9 "format on LostFocus" behavior.
            // No-op for plain text fields (ConvertSet(Value) == Text); skipped for masks and conversion errors.
            if (!HasMask && !ConversionError)
            {
                await UpdateTextPropertyAsync(false);
                await InputReference.SetText(ReadText, updateValue: false);
            }
        }

        private Task OnMaskedValueChangedAsync(string s) =>
            SetTextAndUpdateValueAsync(s, updateValue: _bindValueModeState.Value != global::Vek.BindValueMode.OnChange);

        protected override async Task InvokeKeyDownAsync(KeyboardEventArgs obj)
        {
            if (obj.Key == "Enter" && _bindValueModeState.Value is not null)
            {
                await CommitTextValueAsync();
            }

            await base.InvokeKeyDownAsync(obj);
        }

        private async Task OnBindValueModeChangedAsync(ParameterChangedEventArgs<global::Vek.BindValueMode?> args)
        {
            if (args.LastValue == global::Vek.BindValueMode.OnDelayedInput)
            {
                await CommitTextValueAsync();
            }

            await UpdateDebouncerIntervalAsync(EffectiveDebounceInterval);
            if (args.Value == global::Vek.BindValueMode.OnDelayedInput && args.LastValue == global::Vek.BindValueMode.OnChange)
            {
                await base.UpdateValuePropertyAsync(updateText: false);
            }
        }

        private async Task OnInputDelayChangedAsync(ParameterChangedEventArgs<double> args)
        {
            if (_bindValueModeState.Value == global::Vek.BindValueMode.OnDelayedInput && args.Value <= 0)
            {
                await CommitTextValueAsync();
            }

            await UpdateDebouncerIntervalAsync(EffectiveDebounceInterval);
        }

        private string GetCounterText() => Counter switch
        {
            null => string.Empty,
            0 => string.IsNullOrEmpty(ReadText) ? "0" : $"{ReadText.Length}",
            _ => (string.IsNullOrEmpty(ReadText) ? "0" : $"{ReadText.Length}") + $" / {Counter}"
        };

        protected async Task HandleContainerClickAsync()
        {
            if (!_isFocused && IsJSRuntimeAvailable && InputReference != null)
            {
                await InputReference.FocusAsync();
            }
        }
    }
}
