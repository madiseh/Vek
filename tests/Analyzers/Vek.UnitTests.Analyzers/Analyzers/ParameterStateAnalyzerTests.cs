// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using Vek.UnitTests.Analyzers.Verifiers;
using NUnit.Framework;

namespace Vek.UnitTests.Analyzers;

extern alias VekAnalyzer;
using VerifyCS = CSharpAnalyzerVerifier<VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer>;

/// <summary>
/// Tests for ParameterStateAnalyzer following Microsoft's analyzer testing patterns.
/// </summary>
[TestFixture]
public class ParameterStateAnalyzerTests
{
    [Test]
    public void AnalyzerShouldReportSupportedDiagnostics()
    {
        var analyzer = new VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer();
        var supportedDiagnostics = analyzer.SupportedDiagnostics;

        Assert.That(supportedDiagnostics, Has.Length.EqualTo(3));
        Assert.That(supportedDiagnostics, Has.Some.Matches<DiagnosticDescriptor>(d => d.Id == "VEK0010"));
        Assert.That(supportedDiagnostics, Has.Some.Matches<DiagnosticDescriptor>(d => d.Id == "VEK0011"));
        Assert.That(supportedDiagnostics, Has.Some.Matches<DiagnosticDescriptor>(d => d.Id == "VEK0012"));
    }

    [Test]
    public void DiagnosticDescriptors_ShouldHaveCorrectProperties()
    {
        // Assert VEK0010
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.ReadDescriptor.Id, Is.EqualTo("VEK0010"));
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.ReadDescriptor.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Warning));
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.ReadDescriptor.IsEnabledByDefault, Is.True);

        // Assert VEK0011
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.WriteDescriptor.Id, Is.EqualTo("VEK0011"));
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.WriteDescriptor.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Warning));
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.WriteDescriptor.IsEnabledByDefault, Is.True);

        // Assert VEK0012
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.ExternalAccessDescriptor.Id, Is.EqualTo("VEK0012"));
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.ExternalAccessDescriptor.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Warning));
        Assert.That(VekAnalyzer::Vek.Analyzers.ParameterStateAnalyzer.ExternalAccessDescriptor.IsEnabledByDefault, Is.True);
    }

    [Test]
    public void ParameterUsageOptions_MustMatch()
    {
        void AssertEnumsMatch(Type enumA, Type enumB)
        {
            var namesA = Enum.GetNames(enumA);
            var namesB = Enum.GetNames(enumB);
            Assert.That(namesA, Is.EqualTo(namesB));

            var valuesA = Enum.GetValues(enumA).Cast<object>().Select(v => (int)v);
            var valuesB = Enum.GetValues(enumB).Cast<object>().Select(v => (int)v);
            Assert.That(valuesA, Is.EqualTo(valuesB));
        }
        AssertEnumsMatch(
            typeof(Vek.State.ParameterUsageOptions),
            typeof(VekAnalyzer.Vek.State.ParameterUsageOptions)
        );
    }

    [Test]
    public async Task VEK0010_ReadInsideMethod_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public int GetCounter()
    {
        return {|#0:Counter|};
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0010")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0010_ReadInVariableAssignment_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void Method()
    {
        var x = {|#0:Counter|};
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0010")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0010_ReadAsMethodArgument_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void Method()
    {
        DoSomething({|#0:Counter|});
    }

    private void DoSomething(int value) { }
}";

        var expected = VerifyCS.Diagnostic("VEK0010")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0011_WriteInsideMethod_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void SetCounter(int value)
    {
        {|#0:Counter|} = value;
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0011")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0011_CompoundAssignment_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void Increment()
    {
        {|#0:Counter|} += 1;
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0011")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0011_Increment_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void Increment()
    {
        {|#0:Counter|}++;
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0011")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0011_Decrement_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void Decrement()
    {
        {|#0:Counter|}--;
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0011")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0011_ConstructorAssignment_ShouldNotReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public MyComponent()
    {
        Counter = 0;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task VEK0011_SetParametersAsyncAssignment_ShouldNotReportDiagnostic()
    {
        var source = @"
using System;
using System.Threading.Tasks;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public Task SetParametersAsync()
    {
        Counter = 5;
        return Task.CompletedTask;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task VEK0012_ExternalRead_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public int GetExternalCounter()
    {
        return {|#0:_componentA.Counter|};
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0012")
            .WithLocation(0)
            .WithArguments("Counter");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0010_ShouldNotReportForExternalAccess()
    {
        var source = @"
using System;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public int GetExternalCounter()
    {
        return {|#0:_componentA.Counter|};
    }
}";

        // External access should report VEK0012, not VEK0010
        var expected = VerifyCS.Diagnostic("VEK0012")
            .WithLocation(0)
            .WithArguments("Counter");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task NoDiagnostic_WhenPropertyDoesNotHaveParameterStateAttribute()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    public int Counter { get; set; }

    public int GetCounter()
    {
        return Counter;
    }

    public void SetCounter(int value)
    {
        Counter = value;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_WhenAttributeNotAvailable()
    {
        var source = @"
class MyComponent
{
    public int Counter { get; set; }

    public int GetCounter()
    {
        return Counter;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_WhenUsingNameofOnParameterStateProperty()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public string GetPropertyName()
    {
        return nameof(Counter);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_WhenUsingConstructorWithParameterStateFramework()
    {
        var source = @"
using System;
using Vek;
using Vek.State;

class MyComponent : ComponentBaseWithState
{
    private readonly ParameterState<int> _counterState;

    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public MyComponent()
    {
        Counter = 0;
        using var registerScope = base.CreateRegisterScope();
        _counterState = registerScope.RegisterParameter<int>(nameof(Counter))
            .WithParameter(() => Counter);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_ExternalAccessWhenUsingGetState()
    {
        var source = @"
using System;
using Vek;
using Vek.State;
using Vek.Extensions;

class ComponentA : ComponentBaseWithState
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public int GetExternalCounter()
    {
        return _componentA.GetState(x => x.Counter);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    // Tests for ParameterUsageOptions

    [Test]
    public async Task ParameterUsageAll_ShouldTriggerAllDiagnostics()
    {
        // ParameterUsageOptions.All is the default, so this should trigger all diagnostics
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.All)]
    public int Counter { get; set; }

    public void Method()
    {
        var x = {|#0:Counter|};
        {|#1:Counter|} = 5;
    }
}";

        var expectedRead = VerifyCS.Diagnostic("VEK0010")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        var expectedWrite = VerifyCS.Diagnostic("VEK0011")
            .WithLocation(1)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expectedRead, expectedWrite);
    }

    [Test]
    public async Task ParameterUsageRead_ShouldTriggerReadDiagnosticButNotWrite()
    {
        // ParameterUsageOptions.Read should trigger VEK0010 and VEK0012 but NOT VEK0011
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.Read)]
    public int Counter { get; set; }

    public void Method()
    {
        var x = {|#0:Counter|};
        Counter = 5; // Should NOT trigger VEK0011 when ParameterUsage = Read
    }
}";

        var expectedRead = VerifyCS.Diagnostic("VEK0010")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expectedRead);
    }

    [Test]
    public async Task ParameterUsageRead_ExternalAccess_ShouldTriggerVEK0012()
    {
        var source = @"
using System;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.Read)]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public int GetExternalCounter()
    {
        return {|#0:_componentA.Counter|};
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0012")
            .WithLocation(0)
            .WithArguments("Counter");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task ParameterUsageWrite_ShouldTriggerWriteDiagnosticButNotRead()
    {
        // ParameterUsageOptions.Write should trigger VEK0011 but NOT VEK0010 or VEK0012
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.Write)]
    public int Counter { get; set; }

    public void Method()
    {
        var x = Counter; // Should NOT trigger VEK0010 when ParameterUsage = Write
        {|#0:Counter|} = 5;
    }
}";

        var expectedWrite = VerifyCS.Diagnostic("VEK0011")
            .WithLocation(0)
            .WithArguments("Counter", "int");
        await VerifyCS.VerifyAnalyzerAsync(source, expectedWrite);
    }

    [Test]
    public async Task ParameterUsageWrite_ExternalAccess_ShouldNotTriggerVEK0012()
    {
        var source = @"
using System;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.Write)]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public int GetExternalCounter()
    {
        return _componentA.Counter; // Should NOT trigger VEK0012 when ParameterUsage = Write
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task ParameterUsageNone_ShouldNotTriggerAnyDiagnostics()
    {
        // ParameterUsageOptions.None should not trigger any diagnostics
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.None)]
    public int Counter { get; set; }

    public void Method()
    {
        var x = Counter; // Should NOT trigger VEK0010
        Counter = 5;     // Should NOT trigger VEK0011
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task ParameterUsageNone_ExternalAccess_ShouldNotTriggerVEK0012()
    {
        var source = @"
using System;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.None)]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public int GetExternalCounter()
    {
        return _componentA.Counter; // Should NOT trigger VEK0012 when ParameterUsage = None
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task VEK0010_GenericType_ShouldDisplayCorrectTypeName()
    {
        var source = @"
using System;
using System.Collections.Generic;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public List<string> Items { get; set; }

    public void Method()
    {
        var x = {|#0:Items|};
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0010")
            .WithLocation(0)
            .WithArguments("Items", "List<string>");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0011_StringType_ShouldDisplayCorrectTypeName()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public string Name { get; set; }

    public void Method()
    {
        {|#0:Name|} = ""test"";
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0011")
            .WithLocation(0)
            .WithArguments("Name", "string");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    // Tests for Expression<Func<>> - should NOT trigger VEK0012
    // Expression trees are used for scenarios like reflection, LINQ providers, and test frameworks (e.g., bUnit)
    // where the lambda is analyzed as data (expression tree) rather than executed as code.
    // In these cases, the property access is not a direct external access but part of metadata.

    [Test]
    public async Task NoDiagnostic_WhenUsedInExpressionParameter()
    {
        // Property access inside Expression<Func<>> should not trigger VEK0012
        // Expression trees are used for reflection-like scenarios
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public void Method(Expression<Func<ComponentA, int>> selector)
    {
        selector.Compile();
    }

    public void TestMethod()
    {
        Method(x => x.Counter); // Should NOT trigger VEK0012
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_WhenUsedInBUnitAddMethod()
    {
        // Simulates the bUnit ComponentParameterCollectionBuilder.Add pattern
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class VekProgressLinear
{
    [Vek.State.ParameterState]
    public double Min { get; set; }

    [Vek.State.ParameterState]
    public double Max { get; set; }

    [Vek.State.ParameterState]
    public double Value { get; set; }

    [Vek.State.ParameterState]
    public double BufferValue { get; set; }
}

class ComponentParameterCollectionBuilder<T>
{
    public ComponentParameterCollectionBuilder<T> Add<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        return this;
    }
}

class TestClass
{
    public void CheckingPercentageAndBufferValue(double min, double max, double value, double buffervalue)
    {
        var builder = new ComponentParameterCollectionBuilder<VekProgressLinear>();
        builder.Add(y => y.Min, min);
        builder.Add(y => y.Max, max);
        builder.Add(y => y.Value, value);
        builder.Add(y => y.BufferValue, buffervalue);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_WhenUsedInNestedExpressionParameter()
    {
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public void GenericMethod<T, TValue>(Expression<Func<T, TValue>> selector)
    {
    }

    public void TestMethod()
    {
        GenericMethod((ComponentA x) => x.Counter);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task VEK0012_NormalExternalAccess_ShouldStillTrigger()
    {
        // Normal external access (not in Expression) should still trigger VEK0012
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public void DirectAccess()
    {
        var x = {|#0:_componentA.Counter|}; // Should trigger VEK0012
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0012")
            .WithLocation(0)
            .WithArguments("Counter");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task VEK0012_MixedAccess_ShouldOnlyTriggerForDirectAccess()
    {
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public void Method(Expression<Func<ComponentA, int>> selector)
    {
    }

    public void TestMethod()
    {
        Method(x => x.Counter); // Should NOT trigger
        var y = {|#0:_componentA.Counter|}; // Should trigger VEK0012
    }
}";

        var expected = VerifyCS.Diagnostic("VEK0012")
            .WithLocation(0)
            .WithArguments("Counter");
        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Test]
    public async Task NoDiagnostic_WhenUsedInExpressionWithMultipleProperties()
    {
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    [Vek.State.ParameterState]
    public string Name { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public void Method<T, TValue>(Expression<Func<T, TValue>> selector)
    {
    }

    public void TestMethod()
    {
        Method((ComponentA x) => x.Counter);
        Method((ComponentA x) => x.Name);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_NestedLambdaInExpression()
    {
        // Test that nested lambdas work correctly - outer is Expression, inner is delegate
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }
}

class ComponentB
{
    private ComponentA _componentA = new ComponentA();

    public void Method(Expression<Func<ComponentA, Func<int>>> selector)
    {
    }

    public void TestMethod()
    {
        // Outer lambda is Expression, inner is a regular delegate
        Method(x => () => x.Counter);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_SetParamAsyncPattern()
    {
        // Test the SetParamAsync pattern from bUnit which was reported as not working
        var source = @"
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public bool AutoCycle { get; set; }
}

static class Extensions
{
    public static Task SetParamAsync<T>(this T self, Expression<Func<T, object?>> exp, object? value)
    {
        return Task.CompletedTask;
    }
}

class TestClass
{
    public async Task TestMethod()
    {
        var comp = new MyComponent();
        await comp.SetParamAsync(p => p.AutoCycle, true); // Should NOT trigger VEK0012
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Test]
    public async Task NoDiagnostic_RenderComponentWithNestedLambdas()
    {
        // Test the RenderComponent(x => { x.Add(y => y.Property, value); }) pattern
        // This is a common bUnit pattern where the outer lambda is Action<> and inner is Expression<>
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class VekProgressCircular
{
    [Vek.State.ParameterState]
    public double Value { get; set; }

    [Vek.State.ParameterState]
    public double Min { get; set; }

    [Vek.State.ParameterState]
    public double Max { get; set; }
}

class ComponentParameterCollectionBuilder<T>
{
    public ComponentParameterCollectionBuilder<T> Add<TValue>(Expression<Func<T, TValue>> selector, TValue value)
    {
        return this;
    }
}

class TestContext
{
    public object RenderComponent<T>(Action<ComponentParameterCollectionBuilder<T>> parameterBuilder)
    {
        return null;
    }
}

class TestClass
{
    private TestContext Context = new TestContext();

    public void TestMethod()
    {
        // Outer lambda is Action<>, inner lambdas are Expression<>
        var comp = Context.RenderComponent<VekProgressCircular>(x =>
        {
            x.Add(y => y.Value, 100.0);  // Should NOT trigger VEK0012
            x.Add(y => y.Min, 0.0);      // Should NOT trigger VEK0012
            x.Add(y => y.Max, 200.0);    // Should NOT trigger VEK0012
        });
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    /// <summary>
    /// A GetState method on a lookalike type or outside Vek.Extensions does not exempt the read inside its lambda.
    /// </summary>
    [Test]
    public async Task VEK0010_ImpostorGetState_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;
using Other.Extensions;

namespace Other.Extensions
{
    static class ComponentBaseWithStateExtensions
    {
        public static int GetState<T>(this T component, Func<T, int> selector) => 0;
    }
}

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public int Method()
    {
        return this.GetState(x => {|#0:x.Counter|}) + GetState(() => {|#1:Counter|});
    }

    private static int GetState(Func<int> selector) => 0;
}";

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic("VEK0010").WithLocation(0).WithArguments("Counter", "int"),
            VerifyCS.Diagnostic("VEK0010").WithLocation(1).WithArguments("Counter", "int"));
    }

    /// <summary>
    /// A read in an ordinary delegate lambda is reported, while the same lambda nested inside an expression-tree lambda is exempt.
    /// </summary>
    [Test]
    public async Task VEK0010_DelegateLambdaOnlyExemptInsideExpressionTree()
    {
        var source = @"
using System;
using System.Linq.Expressions;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void Method()
    {
        Func<int> plain = () => {|#0:Counter|};
        Func<Func<int>> nestedPlain = () => () => {|#1:Counter|};
        Expression<Func<Func<int>>> nestedInTree = () => () => Counter;
        Expression<Func<MyComponent, Func<Func<int>>>> deepInTree = x => () => () => x.Counter;
        Func<Expression<Func<int>>> treeInsidePlain = () => () => Counter;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic("VEK0010").WithLocation(0).WithArguments("Counter", "int"),
            VerifyCS.Diagnostic("VEK0010").WithLocation(1).WithArguments("Counter", "int"));
    }

    /// <summary>
    /// Reading another component's state in a constructor is still external access, and nameof on it is not.
    /// </summary>
    [Test]
    public async Task VEK0012_ExternalReadInConstructor_ShouldReportDiagnostic()
    {
        var source = @"
using System;
using Vek.State;

class ComponentA
{
    [Vek.State.ParameterState]
    public int Counter { get; set; }

    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.Write)]
    public int WriteOnly { get; set; }
}

class ComponentB
{
    public ComponentB(ComponentA other)
    {
        var counter = {|#0:other.Counter|};
        var writeOnly = other.WriteOnly;
        var name = nameof(other.Counter);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic("VEK0012").WithLocation(0).WithArguments("Counter"));
    }

    /// <summary>
    /// Assignment, compound assignment, increment, and decrement targets never get a read diagnostic, even when only reads are checked.
    /// </summary>
    [Test]
    public async Task NoReadDiagnostic_ForAssignmentTargets()
    {
        var source = @"
using System;
using Vek.State;

class MyComponent
{
    [Vek.State.ParameterState(ParameterUsage = ParameterUsageOptions.Read)]
    public int ReadOnly { get; set; }

    [Vek.State.ParameterState]
    public int Counter { get; set; }

    public void Method()
    {
        ReadOnly = 1;
        ReadOnly += 1;
        ReadOnly++;
        --ReadOnly;
        {|#0:Counter|} = {|#1:Counter|} + 1;
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(source,
            VerifyCS.Diagnostic("VEK0011").WithLocation(0).WithArguments("Counter", "int"),
            VerifyCS.Diagnostic("VEK0010").WithLocation(1).WithArguments("Counter", "int"));
    }
}
