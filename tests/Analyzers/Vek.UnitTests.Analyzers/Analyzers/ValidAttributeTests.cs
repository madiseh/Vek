using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Vek.Analyzers.TestComponents;
using Vek.UnitTests.Analyzers.Internal;
using NUnit.Framework;

namespace Vek.UnitTests.Analyzers;

extern alias VekAnalyzer;

#nullable enable
[TestFixture]
//[Ignore("Until a solution for matching SDK/roslyn package reference is found see https://github.com/dotnet/roslyn/issues/77979")]
public class ValidAttributeTests : BunitTest
{
    private static IEnumerable<Diagnostic> LowerCaseAttributesDiagnostics { get; set; } = null!;

    private static IEnumerable<Diagnostic> DefaultAttributesListDiagnostics { get; set; } = null!;

    private static IEnumerable<Diagnostic> CustomAttributesListDiagnostics { get; set; } = null!;

    private static IEnumerable<Diagnostic> DataAndAriaAttributesDiagnostics { get; set; } = null!;

    private static IEnumerable<Diagnostic> NoAttributesDiagnostics { get; set; } = null!;
    private static IEnumerable<Diagnostic> AnyAttributesDiagnostics { get; set; } = null!;

    private static ExpectedDiagnostic IllegalAttributeOffsetXOnVekAutocomplete { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(63, 12), new LinePosition(63, 62)),
        "Illegal Attribute 'OffsetX' on 'VekAutocomplete'");

    private static ExpectedDiagnostic IllegalAttributeiconOnVekFab { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(67, 12), new LinePosition(67, 60)),
        "Illegal Attribute 'icon' on 'VekFab'");

    private static ExpectedDiagnostic IllegalAttributeTextOnVekSlider { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(71, 12), new LinePosition(71, 60)),
        "Illegal Attribute 'Text' on 'VekSlider'");

    private static ExpectedDiagnostic IllegalAttributeAvatarOnInheritedVekChip { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(84, 12), new LinePosition(84, 61)),
        "Illegal Attribute 'Avatar' on 'InheritedVekChip'");

    private static ExpectedDiagnostic IllegalAttributeImageOnVekAvatar { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(107, 16), new LinePosition(107, 66)),
        "Illegal Attribute 'Image' on 'VekAvatar'");

    private static ExpectedDiagnostic IllegalAttributeMinimumOnVekProgressLinear { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(113, 12), new LinePosition(113, 63)),
        "Illegal Attribute 'Minimum' on 'VekProgressLinear'");

    private static ExpectedDiagnostic IllegalAttributeDenseOnVekToggleGroup { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(125, 12), new LinePosition(125, 64)),
        "Illegal Attribute 'Dense' on 'VekToggleGroup'");

    private static ExpectedDiagnostic IllegalAttributebindOnVekChip { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(180, 12), new LinePosition(180, 13)),
        "Illegal Attribute '@bind' on 'VekChip'");

    private static ExpectedDiagnostic IllegalAttributebindafterOnVekChip { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(198, 12), new LinePosition(198, 70)),
        "Illegal Attribute '@bind:after' on 'VekChip'");

    private static ExpectedDiagnostic IllegalAttributelowerCaseOnVekProgressCircular { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(228, 12), new LinePosition(228, 66)),
        "Illegal Attribute 'lowerCase' on 'VekProgressCircular'");

    private static ExpectedDiagnostic IllegalAttributeUpperCaseOnVekProgressCircular { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(229, 12), new LinePosition(229, 66)),
        "Illegal Attribute 'UpperCase' on 'VekProgressCircular'");

    private static ExpectedDiagnostic IllegalAttributedataanimationOnVekRadio { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(233, 12), new LinePosition(233, 70)),
        "Illegal Attribute 'data-animation' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributeariadisabledOnVekRadio { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(234, 12), new LinePosition(234, 73)),
        "Illegal Attribute 'aria-disabled' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributeroleOnVekRadio { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(235, 12), new LinePosition(235, 63)),
        "Illegal Attribute 'role' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributeunknownOnVekRadio { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(239, 12), new LinePosition(239, 76)),
        "Illegal Attribute 'unknownAttribute' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributehiddenOnVekRadio { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(243, 12), new LinePosition(243, 62)),
        "Illegal Attribute 'hidden' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributeInertOnVekRadio { get; set; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(247, 12), new LinePosition(247, 62)),
        "Illegal Attribute 'Inert' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributecustomattributeOnVekRadio { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(251, 12), new LinePosition(251, 72)),
        "Illegal Attribute 'customattribute' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributecustomAttribute2OnVekRadio { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(255, 14), new LinePosition(255, 73)),
        "Illegal Attribute 'customAttribute2' on 'VekRadio'");

    private static ExpectedDiagnostic IllegalAttributeErrorTextChangedOnVekCheckBox { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(303, 8), new LinePosition(303, 75)),
        "Illegal Attribute 'RequiredErrorChanged' on 'VekCheckBox'");

    private static ExpectedDiagnostic IllegalAttributeAvatarClassOnVekChip { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(318, 8), new LinePosition(318, 70)),
        "Illegal Attribute 'AvatarClass' on 'VekChip'");

    private static ExpectedDiagnostic IllegalAttributeValueChangedOnVekChip { get; } = new(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.AttributeDescriptor,
        new FileLinePositionSpan($"{nameof(AttributeTest)}_razor.g.cs", new LinePosition(328, 8), new LinePosition(328, 71)),
        "Illegal Attribute 'ValueChanged' on 'VekChip'");

    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        var source = CreateGeneratedSource();

        LowerCaseAttributesDiagnostics = await AnalyzerCompilationFactory.GetDiagnosticsAsync(source, VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.LowerCase);
        DefaultAttributesListDiagnostics = await AnalyzerCompilationFactory.GetDiagnosticsAsync(source, VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.HTMLAttributes);
        CustomAttributesListDiagnostics = await AnalyzerCompilationFactory.GetDiagnosticsAsync(source, VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.HTMLAttributes, "customattribute,customAttribute2");
        DataAndAriaAttributesDiagnostics = await AnalyzerCompilationFactory.GetDiagnosticsAsync(source, VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.DataAndAria);
        NoAttributesDiagnostics = await AnalyzerCompilationFactory.GetDiagnosticsAsync(source, VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.None);
        AnyAttributesDiagnostics = await AnalyzerCompilationFactory.GetDiagnosticsAsync(source, VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.Any);
    }

    [Test]
    public void AllowLowerCaseAttributes()
    {
        var diagnostics = LowerCaseAttributesDiagnostics.FilterToClass(typeof(AttributeTest).FullName);

        var expectedDiagnostics = new List<ExpectedDiagnostic>([
            IllegalAttributeOffsetXOnVekAutocomplete,
            IllegalAttributeTextOnVekSlider,
            IllegalAttributeAvatarOnInheritedVekChip,
            IllegalAttributeImageOnVekAvatar,
            IllegalAttributeMinimumOnVekProgressLinear,
            IllegalAttributeDenseOnVekToggleGroup,
            IllegalAttributebindOnVekChip,
            IllegalAttributebindafterOnVekChip,
            IllegalAttributeUpperCaseOnVekProgressCircular,
            IllegalAttributeInertOnVekRadio,
            IllegalAttributeErrorTextChangedOnVekCheckBox,
            IllegalAttributeAvatarClassOnVekChip,
            IllegalAttributeValueChangedOnVekChip
        ]);

        ExpectedDiagnostic.Compare(diagnostics, expectedDiagnostics);
    }

    [Test]
    public void AllowDefaultListAttributes()
    {
        var diagnostics = DefaultAttributesListDiagnostics.FilterToClass(typeof(AttributeTest).FullName);

        var expectedDiagnostics = new List<ExpectedDiagnostic>([
            IllegalAttributeOffsetXOnVekAutocomplete,
            IllegalAttributeiconOnVekFab,
            IllegalAttributeTextOnVekSlider,
            IllegalAttributeAvatarOnInheritedVekChip,
            IllegalAttributeImageOnVekAvatar,
            IllegalAttributeMinimumOnVekProgressLinear,
            IllegalAttributeDenseOnVekToggleGroup,
            IllegalAttributebindOnVekChip,
            IllegalAttributebindafterOnVekChip,
            IllegalAttributelowerCaseOnVekProgressCircular,
            IllegalAttributeUpperCaseOnVekProgressCircular,
            IllegalAttributeunknownOnVekRadio,
            IllegalAttributeInertOnVekRadio,
            IllegalAttributecustomattributeOnVekRadio,
            IllegalAttributecustomAttribute2OnVekRadio,
            IllegalAttributeErrorTextChangedOnVekCheckBox,
            IllegalAttributeAvatarClassOnVekChip,
            IllegalAttributeValueChangedOnVekChip]);

        ExpectedDiagnostic.Compare(diagnostics, expectedDiagnostics);
    }

    [Test]
    public void AllowCustomListAttributes()
    {
        var diagnostics = CustomAttributesListDiagnostics.FilterToClass(typeof(AttributeTest).FullName);

        var expectedDiagnostics = new List<ExpectedDiagnostic>([
            IllegalAttributeOffsetXOnVekAutocomplete,
            IllegalAttributeiconOnVekFab,
            IllegalAttributeTextOnVekSlider,
            IllegalAttributeAvatarOnInheritedVekChip,
            IllegalAttributeImageOnVekAvatar,
            IllegalAttributeMinimumOnVekProgressLinear,
            IllegalAttributeDenseOnVekToggleGroup,
            IllegalAttributebindOnVekChip,
            IllegalAttributebindafterOnVekChip,
            IllegalAttributelowerCaseOnVekProgressCircular,
            IllegalAttributeUpperCaseOnVekProgressCircular,
            IllegalAttributeunknownOnVekRadio,
            IllegalAttributehiddenOnVekRadio,
            IllegalAttributeInertOnVekRadio,
            IllegalAttributeErrorTextChangedOnVekCheckBox,
            IllegalAttributeAvatarClassOnVekChip,
            IllegalAttributeValueChangedOnVekChip]);

        ExpectedDiagnostic.Compare(diagnostics, expectedDiagnostics);
    }

    [Test]
    public void AllowDataAndAriaAttributes()
    {
        var diagnostics = DataAndAriaAttributesDiagnostics.FilterToClass(typeof(AttributeTest).FullName);

        var expectedDiagnostics = new List<ExpectedDiagnostic>([
            IllegalAttributeOffsetXOnVekAutocomplete,
            IllegalAttributeiconOnVekFab,
            IllegalAttributeTextOnVekSlider,
            IllegalAttributeAvatarOnInheritedVekChip,
            IllegalAttributeImageOnVekAvatar,
            IllegalAttributeMinimumOnVekProgressLinear,
            IllegalAttributeDenseOnVekToggleGroup,
            IllegalAttributebindOnVekChip,
            IllegalAttributebindafterOnVekChip,
            IllegalAttributelowerCaseOnVekProgressCircular,
            IllegalAttributeUpperCaseOnVekProgressCircular,
            IllegalAttributeunknownOnVekRadio,
            IllegalAttributehiddenOnVekRadio,
            IllegalAttributeInertOnVekRadio,
            IllegalAttributecustomattributeOnVekRadio,
            IllegalAttributecustomAttribute2OnVekRadio,
            IllegalAttributeErrorTextChangedOnVekCheckBox,
            IllegalAttributeAvatarClassOnVekChip,
            IllegalAttributeValueChangedOnVekChip]);

        ExpectedDiagnostic.Compare(diagnostics, expectedDiagnostics);
    }

    [Test]
    public void AllowNoAttributes()
    {
        var diagnostics = NoAttributesDiagnostics.FilterToClass(typeof(AttributeTest).FullName);

        var expectedDiagnostics = new List<ExpectedDiagnostic>([
            IllegalAttributeOffsetXOnVekAutocomplete,
            IllegalAttributeiconOnVekFab,
            IllegalAttributeTextOnVekSlider,
            IllegalAttributeAvatarOnInheritedVekChip,
            IllegalAttributeImageOnVekAvatar,
            IllegalAttributeMinimumOnVekProgressLinear,
            IllegalAttributeDenseOnVekToggleGroup,
            IllegalAttributebindOnVekChip,
            IllegalAttributebindafterOnVekChip,
            IllegalAttributelowerCaseOnVekProgressCircular,
            IllegalAttributeUpperCaseOnVekProgressCircular,
            IllegalAttributedataanimationOnVekRadio,
            IllegalAttributeariadisabledOnVekRadio,
            IllegalAttributeroleOnVekRadio,
            IllegalAttributeunknownOnVekRadio,
            IllegalAttributehiddenOnVekRadio,
            IllegalAttributeInertOnVekRadio,
            IllegalAttributecustomattributeOnVekRadio,
            IllegalAttributecustomAttribute2OnVekRadio,
            IllegalAttributeErrorTextChangedOnVekCheckBox,
            IllegalAttributeAvatarClassOnVekChip,
            IllegalAttributeValueChangedOnVekChip]);

        ExpectedDiagnostic.Compare(diagnostics, expectedDiagnostics);
    }

    [Test]
    public void AllowAnyAttributes()
    {
        var diagnostics = AnyAttributesDiagnostics.FilterToClass(typeof(AttributeTest).FullName);

        var expectedDiagnostics = new List<ExpectedDiagnostic>([]);

        ExpectedDiagnostic.Compare(diagnostics, expectedDiagnostics);
    }

    private static string CreateGeneratedSource() =>
        """
        using System;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;
        using Vek;

        namespace Vek.Analyzers.TestComponents;

        public class InheritedVekChip<T> : VekChip<T>
        {
            [Parameter]
            public string? AvatarClass { get; set; }
        }

        public class AttributeTest : ComponentBase
        {
            private readonly string _bindValue = "y";

            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                builder.OpenComponent<VekAutocomplete<string>>(0);
                builder.AddAttribute(1, "Value", _bindValue);
                builder.AddAttribute(2, "OffsetX", "5");
                builder.CloseComponent();

                builder.OpenComponent<VekFab>(3);
                builder.AddAttribute(4, "icon", "dd");
                builder.CloseComponent();

                builder.OpenComponent<VekSlider<int>>(5);
                builder.AddAttribute(6, "Text", true);
                builder.CloseComponent();

                builder.OpenComponent<InheritedVekChip<string>>(7);
                builder.AddAttribute(8, "Text", "Href set");
                builder.AddAttribute(9, "AvatarClass", _bindValue);
                builder.AddAttribute(10, "Avatar", string.Empty);
                builder.CloseComponent();

                builder.OpenComponent<VekAvatar>(11);
                builder.AddAttribute(12, "Image", "avatar.png");
                builder.CloseComponent();

                builder.OpenComponent<VekProgressLinear>(13);
                builder.AddAttribute(14, "Minimum", 0);
                builder.CloseComponent();

                builder.OpenComponent<VekToggleGroup<string>>(15);
                builder.AddAttribute(16, "Dense", true);
                builder.CloseComponent();

                builder.OpenComponent<VekChip<string>>(17);
                builder.AddAttribute(18, "@bind", _bindValue);
                builder.CloseComponent();

                builder.OpenComponent<VekChip<string>>(19);
                builder.AddAttribute(20, "@bind:after", nameof(After));
                builder.CloseComponent();

                builder.OpenComponent<VekProgressCircular>(21);
                builder.AddAttribute(22, "lowerCase", true);
                builder.AddAttribute(23, "UpperCase", true);
                builder.CloseComponent();

                builder.OpenComponent<VekRadio<string>>(24);
                builder.AddAttribute(25, "data-animation", "a");
                builder.AddAttribute(26, "aria-disabled", "false");
                builder.AddAttribute(27, "role", "test");
                builder.AddAttribute(28, "unknownAttribute", "false");
                builder.AddAttribute(29, "hidden", true);
                builder.AddAttribute(30, "Inert", true);
                builder.AddAttribute(31, "customattribute", true);
                builder.AddAttribute(32, "customAttribute2", true);
                builder.CloseComponent();

                builder.OpenComponent<VekCheckBox<string>>(33);
                builder.AddAttribute(34, "RequiredError", _bindValue);
                builder.AddComponentParameter(35, "RequiredErrorChanged", _bindValue);
                builder.CloseComponent();

                builder.OpenComponent<VekChip<string>>(36);
                builder.AddAttribute(37, "Text", "Href set");
                builder.AddAttribute(38, "AvatarClass", _bindValue);
                builder.CloseComponent();

                TypeInference.CreateVekChip_0(builder, 39, _bindValue, After);
            }

            private void After()
            {
            }
        }

        public static class TypeInference
        {
            public static void CreateVekChip_0(RenderTreeBuilder builder, int sequence, string value, Action after)
            {
                builder.OpenComponent<VekChip<string>>(sequence);
                builder.AddAttribute(sequence + 1, "Value", value);
                builder.AddAttribute(sequence + 2, "ValueChanged", after);
                builder.CloseComponent();
            }
        }
        """;
}
#nullable restore
