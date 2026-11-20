// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// An advanced masked input component that formats and restricts user input based on pattern rules, prompt characters, and literals preservation options.
/// </summary>
public partial class VekMaskedInput : VekDebouncedInput<string>
{
    private VekMask? _maskReference;
    private PatternMask? _patternMask;
    private PatternMask? _defaultMask;

    private readonly ParameterState<string?> _maskState;
    private readonly ParameterState<IMask?> _maskInstanceState;
    private readonly ParameterState<char> _promptCharState;
    private readonly ParameterState<ShowLiterals> _showLiteralsState;
    private readonly ParameterState<bool> _saveLiteralRepresentationState;

    /// <summary>
    /// Initializes a new instance of the <see cref="VekMaskedInput"/> class.
    /// </summary>
    public VekMaskedInput()
    {
        using var registerScope = CreateRegisterScope();
        _maskState = registerScope.RegisterParameter<string?>(nameof(Mask))
            .WithParameter(() => Mask);
        _maskInstanceState = registerScope.RegisterParameter<IMask?>(nameof(MaskInstance))
            .WithParameter(() => MaskInstance);
        _promptCharState = registerScope.RegisterParameter<char>(nameof(PromptChar))
            .WithParameter(() => PromptChar);
        _showLiteralsState = registerScope.RegisterParameter<ShowLiterals>(nameof(ShowLiterals))
            .WithParameter(() => ShowLiterals);
        _saveLiteralRepresentationState = registerScope.RegisterParameter<bool>(nameof(SaveLiteralRepresentation))
            .WithParameter(() => SaveLiteralRepresentation);
    }

    protected string Classname =>
        new CssBuilder("vek-input-input-control")
            .AddClass("vek-masked-input")
            .AddClass(Class)
            .Build();

    /// <summary>
    /// The mask pattern string defining input formatting rules.
    /// </summary>
    /// <remarks>
    /// Standard mask tokens include <c>0</c> (digit required), <c>9</c> (digit optional), <c>#</c> (digit/sign),
    /// <c>L</c> (letter required), <c>?</c> (letter optional), <c>A</c> (alphanumeric required), and <c>a</c> (alphanumeric optional).
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public string? Mask { get; set; }

    /// <summary>
    /// A custom <see cref="IMask"/> implementation to apply.
    /// </summary>
    /// <remarks>
    /// When specified, takes precedence over the <see cref="Mask"/> string pattern.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public IMask? MaskInstance { get; set; }

    /// <summary>
    /// The prompt character displayed for unfilled positions.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>'_'</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public char PromptChar { get; set; } = '_';

    /// <summary>
    /// Controls when formatting literals and prompt characters are displayed.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="ShowLiterals.Always"/>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public ShowLiterals ShowLiterals { get; set; } = ShowLiterals.Always;

    /// <summary>
    /// Controls whether the bound <see cref="VekBaseInput{T}.Value"/> includes literal formatting characters or only raw data.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>. When <c>false</c>, only clean input characters without literals are saved to the value.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool SaveLiteralRepresentation { get; set; } = true;

    /// <summary>
    /// The type of the underlying input.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="InputType.Text"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListAppearance)]
    public InputType InputType { get; set; } = InputType.Text;

    /// <summary>
    /// Shows the clear button.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public bool Clearable { get; set; } = false;

    /// <summary>
    /// Controls when the clear button is displayed.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public ClearButtonDisplayMode? ClearButtonDisplayMode { get; set; }

    /// <summary>
    /// Occurs when the clear button is clicked.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListAppearance)]
    public EventCallback<MouseEventArgs> OnClearButtonClick { get; set; }

    /// <summary>
    /// The icon displayed when <see cref="Clearable" /> is <c>true</c>.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string ClearIcon { get; set; } = Icons.Material.Filled.Clear;

    /// <summary>
    /// Renders custom action buttons alongside the input.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public RenderFragment? Buttons { get; set; }

    /// <summary>
    /// Gets the actively resolved mask instance for the input.
    /// </summary>
    public IMask EffectiveMask
    {
        get
        {
            if (_maskInstanceState.Value is not null)
            {
                return _maskInstanceState.Value;
            }

            var pattern = _maskState.Value;
            if (!string.IsNullOrEmpty(pattern))
            {
                if (_patternMask is null || _patternMask.Mask != pattern)
                {
                    _patternMask = new PatternMask(pattern)
                    {
                        MaskChars = MaskTokens.Standard,
                        PromptChar = _promptCharState.Value
                    };
                }
                else if (_patternMask.PromptChar != _promptCharState.Value)
                {
                    _patternMask.PromptChar = _promptCharState.Value;
                }

                return _patternMask;
            }

            return _defaultMask ??= new PatternMask("** **-** **")
            {
                MaskChars = MaskTokens.Standard,
                PromptChar = _promptCharState.Value
            };
        }
    }

    /// <inheritdoc />
    public override ValueTask FocusAsync() => FocusAsync(preventScroll: false);

    /// <inheritdoc />
    internal override ValueTask FocusAsync(bool preventScroll)
    {
        if (_maskReference is not null)
        {
            return _maskReference.FocusAsync(preventScroll);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public override ValueTask BlurAsync()
    {
        if (_maskReference is not null)
        {
            return _maskReference.BlurAsync();
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public override ValueTask SelectAsync()
    {
        if (_maskReference is not null)
        {
            return _maskReference.SelectAsync();
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public override ValueTask SelectRangeAsync(int pos1, int pos2)
    {
        if (_maskReference is not null)
        {
            return _maskReference.SelectRangeAsync(pos1, pos2);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task ResetValueAsync()
    {
        if (_maskReference is not null)
        {
            await _maskReference.ResetAsync();
        }

        await base.ResetValueAsync();
    }

    /// <summary>
    /// Clears the input text and resets the value.
    /// </summary>
    public Task Clear()
    {
        if (_maskReference is not null)
        {
            return _maskReference.Clear();
        }

        return Task.CompletedTask;
    }

    private Task OnMaskedValueChangedAsync(string? s)
    {
        return SetTextAndUpdateValueAsync(s, updateValue: true);
    }

    protected async Task HandleContainerClickAsync()
    {
        if (_maskReference is not null)
        {
            await _maskReference.FocusAsync();
        }
    }

    private bool ShowClearButton()
    {
        if (!Clearable && ClearButtonDisplayMode is null)
            return false;
        return ClearButtonDisplayMode switch
        {
            global::Vek.ClearButtonDisplayMode.Always => true,
            global::Vek.ClearButtonDisplayMode.WhileEditing => !string.IsNullOrEmpty(ReadText),
            _ => Clearable && !string.IsNullOrEmpty(ReadText)
        };
    }

    private string GetCounterText() => Counter switch
    {
        null => string.Empty,
        0 => string.IsNullOrEmpty(ReadText) ? "0" : $"{ReadText.Length}",
        _ => (string.IsNullOrEmpty(ReadText) ? "0" : $"{ReadText.Length}") + $" / {Counter}"
    };
}
