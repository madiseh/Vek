using Microsoft.AspNetCore.Components;
using Vek.State;

namespace Vek.UnitTests;

public partial class ParameterStateComparerStaticTestComp : VekComponentBase
{
    private readonly List<ParameterChangedEventArgs<double>> _parameterChanges = new();

    [Parameter]
    public double DoubleParam { get; set; }

    public ParameterStateComparerStaticTestComp()
    {
        var comparer = new DoubleEpsilonEqualityComparer(0.00001f);
        using var registerScope = CreateRegisterScope();
        registerScope.RegisterParameter<double>(nameof(DoubleParam))
            .WithParameter(() => DoubleParam)
            .WithChangeHandler(OnParameterChanged)
            .WithComparer(comparer);
    }

    private void OnParameterChanged(ParameterChangedEventArgs<double> args)
    {
        _parameterChanges.Add(args);
    }
}
