using Microsoft.AspNetCore.Components;
using Vek.State;
using Vek.Utilities;

namespace Vek.UnitTests;

public partial class ParameterStateDependencyComp2 : VekComponentBase
{
    private readonly ParameterState<string?> _textState;
    private readonly ParameterState<VekColor?> _valueState;

    [Parameter, ParameterState]
    public string? Text { get; set; }

    [Parameter]
    public EventCallback<string?> TextChanged { get; set; }

    [Parameter, ParameterState]
    public VekColor? Value { get; set; }

    [Parameter]
    public EventCallback<VekColor?> ValueChanged { get; set; }

    public List<ParameterChangedEventArgs<string?>> TextChanges { get; } = [];

    public List<ParameterChangedEventArgs<VekColor?>> ValueChanges { get; } = [];

    public ParameterStateDependencyComp2()
    {
        using var registerScope = CreateRegisterScope();
        _textState = registerScope.RegisterParameter<string?>(nameof(Text))
            .WithParameter(() => Text)
            .WithEventCallback(() => TextChanged)
            .WithChangeHandler(OnTextChangedHandlerAsync);
        _valueState = registerScope.RegisterParameter<VekColor?>(nameof(Value))
            .WithParameter(() => Value)
            .WithEventCallback(() => ValueChanged)
            .WithChangeHandler(OnValuerChangedHandlerAsync);
    }

    private async Task OnTextChangedHandlerAsync(ParameterChangedEventArgs<string?> args)
    {
        TextChanges.Add(args);
        if (string.IsNullOrWhiteSpace(args.Value))
        {
            return;
        }

        if (args.ParameterView.TryGetValue<VekColor?>(nameof(Value), out var incomingValue))
        {
            if (incomingValue is null)
            {
                await _valueState.SetValueAsync(VekColor.Parse(args.Value));
                return;
            }

            var valueText = incomingValue.ToString(VekColorOutputFormats.Hex);

            // Conflict: both changed and they disagree
            if (!string.Equals(
                    args.Value,
                    valueText,
                    StringComparison.OrdinalIgnoreCase))
            {
                await _textState.SetValueAsync(valueText);
                return;
            }
        }

        await _valueState.SetValueAsync(VekColor.Parse(args.Value));
    }

    private Task OnValuerChangedHandlerAsync(ParameterChangedEventArgs<VekColor?> args)
    {
        ValueChanges.Add(args);
        return _textState.SetValueAsync(args.Value?.ToString(VekColorOutputFormats.Hex));
    }
}
