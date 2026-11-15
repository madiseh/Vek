// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Vek.Analyzers.TestComponents;
using Vek.UnitTests.Analyzers.Internal;
using NUnit.Framework;

namespace Vek.UnitTests.Analyzers;

extern alias VekAnalyzer;

#nullable enable
/// <summary>
/// Runs VEK0002 over <c>MigrationHints.razor</c> as the Razor compiler actually lowers it, so the hints are verified against real markup rather than against hand-written render-tree calls.
/// </summary>
[TestFixture]
public class RazorMigrationHintTests
{
    private const string FixtureClassName = "Vek.Analyzers.TestComponents.MigrationHints";

    private const string CatalogNamespace = "Vek.Analyzers.TestComponents.MigrationHintCatalog.";

    private static IReadOnlyList<Diagnostic> LowerCaseDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> NoneDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> AnyDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> CatalogDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> ReplacementDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> LegacyBindingDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> BoundaryDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> AttributeTestLowerCaseDiagnostics { get; set; } = null!;

    private static IReadOnlyList<Diagnostic> AttributeTestNoneDiagnostics { get; set; } = null!;

    private static IEnumerable<string> CaseNames => MigrationHintCases.All.Select(x => x.ToString());

    [OneTimeSetUp]
    public static async Task OneTimeSetup()
    {
        using var projectCompilation = await ProjectCompilation.CreateAsync(Util.ProjectPath());

        var lowerCase = await GetDiagnosticsAsync(VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.LowerCase);
        var none = await GetDiagnosticsAsync(VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.None);

        LowerCaseDiagnostics = lowerCase.FilterToClass(FixtureClassName);
        NoneDiagnostics = none.FilterToClass(FixtureClassName);
        AnyDiagnostics = (await GetDiagnosticsAsync(VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.Any)).FilterToClass(FixtureClassName);

        CatalogDiagnostics = [.. lowerCase.Where(x => ClassName(x).StartsWith($"{CatalogNamespace}Removed", StringComparison.Ordinal))];
        ReplacementDiagnostics = none.FilterToClass($"{CatalogNamespace}Replacements");
        LegacyBindingDiagnostics = lowerCase.FilterToClass($"{CatalogNamespace}LegacyBindings");
        BoundaryDiagnostics = lowerCase.FilterToClass($"{CatalogNamespace}Boundaries");
        AttributeTestLowerCaseDiagnostics = lowerCase.FilterToClass("Vek.Analyzers.TestComponents.AttributeTest");
        AttributeTestNoneDiagnostics = none.FilterToClass("Vek.Analyzers.TestComponents.AttributeTest");

        async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(VekAnalyzer::Vek.Analyzers.AllowedAttributePattern pattern)
        {
            var analyzer = new VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer();
            var options = TestAnalyzerOptions.Create(pattern, projectCompilation.AdditionalTexts);

            return await projectCompilation.GetDiagnosticsAsync(ImmutableArray.Create<DiagnosticAnalyzer>(analyzer), options);
        }

        static string ClassName(Diagnostic diagnostic) =>
            diagnostic.Properties.TryGetValue(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.ClassNamePropertyKey, out var name) ? name ?? string.Empty : string.Empty;
    }

    /// <summary>
    /// Every removed parameter in the fixture reports once and carries its own component-specific guidance.
    /// </summary>
    [Test]
    public void RemovedParametersCarryTheirOwnGuidance()
    {
        var links = LowerCaseDiagnostics.All("Link", "VekButton").WithHint();

        links.Should().HaveCount(2, "the single-line and multiline usages are both explained");

        foreach (var diagnostic in links)
        {
            diagnostic.ShouldCarryHint("Link", "VekButton", MigrationHintExpectations.Link);
        }

        foreach (var diagnostic in LowerCaseDiagnostics.All("DisableRipple", "VekButton"))
        {
            diagnostic.ShouldCarryHint("DisableRipple", "VekButton", MigrationHintExpectations.DisableRipple);
        }

        foreach (var diagnostic in LowerCaseDiagnostics.All("AutoGrow", "VekTextField"))
        {
            diagnostic.ShouldCarryHint("AutoGrow", "VekTextField", MigrationHintExpectations.AutoGrow);
        }
    }

    /// <summary>
    /// Literal and expression-valued attributes, including multiline markup with nested content, all report.
    /// </summary>
    [Test]
    public void LiteralAndExpressionValuesBothReport()
    {
        LowerCaseDiagnostics.All("DisableRipple", "VekButton").Should().HaveCount(4,
            "the fixture uses true, false, a simple expression, and a compound expression");

        LowerCaseDiagnostics.All("Link", "VekButton").Should().HaveCount(3,
            "the fixture has a single-line, a multiline, and a shadow-type usage");
    }

    /// <summary>
    /// A generic VekTextField bound with @bind-Value lowers through a TypeInference helper and still reports.
    /// </summary>
    [Test]
    public void BoundGenericTextFieldReportsThroughTypeInference()
    {
        var diagnostics = LowerCaseDiagnostics.All("AutoGrow", "VekTextField");

        diagnostics.Should().HaveCount(4, "two bound usages lower through TypeInference and two are inline");
    }

    /// <summary>
    /// A component derived from VekButton inherits the hint.
    /// </summary>
    [Test]
    public void DerivedButtonInheritsHint()
    {
        LowerCaseDiagnostics.Single("Link", "DerivedVekButton").ShouldCarryHint("Link", "DerivedVekButton", MigrationHintExpectations.Link);
    }

    /// <summary>
    /// A derived component that declares its own Link parameter is not reported at all.
    /// </summary>
    [Test]
    public void DerivedButtonDeclaringLinkRaisesNoDiagnostic()
    {
        NoneDiagnostics.All("Link", "ReintroducedLinkButton").Should().BeEmpty();
    }

    /// <summary>
    /// A type that only shares VekButton's display name keeps the generic warning.
    /// </summary>
    [Test]
    public void ShadowTypeWithVekButtonNameKeepsGenericWarning()
    {
        var withoutHint = LowerCaseDiagnostics
            .All("Link", "VekButton")
            .Where(x => !x.GetMessage().Contains(MigrationHintExpectations.HintMarker))
            .ToList();

        withoutHint.Should().ContainSingle("only the shadow type's usage is left unexplained");
    }

    /// <summary>
    /// VekChip and VekIconButton dropped Link in the same release as VekButton, so they carry the same Href guidance.
    /// </summary>
    [Test]
    public void LinkOnVekChipAndVekIconButtonExplainsHref()
    {
        LowerCaseDiagnostics.Single("Link", "VekChip").ShouldCarryHint("Link", "VekChip", MigrationHintExpectations.Link);
        LowerCaseDiagnostics.Single("Link", "VekIconButton").ShouldCarryHint("Link", "VekIconButton", MigrationHintExpectations.Link);
    }

    /// <summary>
    /// Attribute names that merely look like removed parameters keep the generic warning.
    /// </summary>
    [Test]
    public void SimilarNamesKeepGenericWarning()
    {
        LowerCaseDiagnostics.Single("Linked", "VekButton").ShouldCarryNoHint("Linked", "VekButton");
        LowerCaseDiagnostics.Single("DisableRippleEffect", "VekButton").ShouldCarryNoHint("DisableRippleEffect", "VekButton");
        LowerCaseDiagnostics.Single("AutoGrowth", "VekTextField").ShouldCarryNoHint("AutoGrowth", "VekTextField");
    }

    /// <summary>
    /// Href, Ripple, and Sizing usages in the fixture compile and raise nothing.
    /// </summary>
    [Test]
    public void ReplacementMarkupRaisesNoDiagnostic()
    {
        NoneDiagnostics.All("Href", "VekButton").Should().BeEmpty();
        NoneDiagnostics.All("Ripple", "VekButton").Should().BeEmpty();
        NoneDiagnostics.All("Sizing", "VekTextField").Should().BeEmpty();
        NoneDiagnostics.All("Lines", "VekTextField").Should().BeEmpty();
    }

    /// <summary>
    /// Removed parameters written inside a Razor comment, an HTML comment, or a C# string are never analyzed.
    /// </summary>
    [Test]
    public void MarkupInCommentsAndStringsIsNotAnalyzed()
    {
        NoneDiagnostics.Should().NotContain(x => x.GetMessage().Contains("/razor-comment"));
        NoneDiagnostics.Should().NotContain(x => x.GetMessage().Contains("/string-literal"));
        NoneDiagnostics.All("Link", "VekButton").Should().HaveCount(3, "the commented-out usages add nothing");
    }

    /// <summary>
    /// A removed parameter supplied through an @attributes dictionary is a known blind spot, not a hint.
    /// </summary>
    /// <remarks>
    /// The analyzer only sees the splat call, never the dictionary's keys, so this usage is undiagnosed rather than migration-clean.
    /// </remarks>
    [Test]
    public void DynamicAttributeSplatIsNotDiagnosed()
    {
        NoneDiagnostics.Should().NotContain(x => x.GetMessage().Contains("/splatted"));
        NoneDiagnostics.All("Link", "VekButton").Should().HaveCount(3, "the splatted Link is invisible to the analyzer");
    }

    /// <summary>
    /// Diagnostics map back to the .razor file, which the migration hint does not change.
    /// </summary>
    [Test]
    public void DiagnosticsStillMapToTheRazorFile()
    {
        var enriched = LowerCaseDiagnostics.Single("Link", "DerivedVekButton");

        enriched.Location.GetLineSpan().Path.Should().EndWith("MigrationHints.razor");
        enriched.AdditionalLocations.Should().ContainSingle();
        enriched.AdditionalLocations[0].GetLineSpan().Path.Should().EndWith(".g.cs");
    }

    /// <summary>
    /// The fixture's diagnostic population does not change with the hints, and Any still reports nothing.
    /// </summary>
    [Test]
    public void DiagnosticPopulationIsUnchanged()
    {
        LowerCaseDiagnostics.Should().HaveCount(17);
        LowerCaseDiagnostics.Should().OnlyContain(x => x.Id == "VEK0002");
        LowerCaseDiagnostics.Count(x => x.GetMessage().Contains(MigrationHintExpectations.HintMarker)).Should().Be(13);
        AnyDiagnostics.Should().BeEmpty();
    }

    /// <summary>
    /// Every catalog case, written in real markup on the component a consumer uses, carries its own guidance.
    /// </summary>
    [TestCaseSource(nameof(CaseNames))]
    public void CatalogCaseCarriesItsHint(string caseName)
    {
        var hintCase = MigrationHintCases.All.Single(x => x.ToString() == caseName);

        CatalogDiagnostics.Single(hintCase.Removed, hintCase.TagName).ShouldCarryHint(hintCase.Removed, hintCase.TagName, [.. hintCase.ExpectedHint]);
    }

    /// <summary>
    /// The catalog fixtures raise exactly one diagnostic per case, every one explained, and nothing else.
    /// </summary>
    [Test]
    public void CatalogFixturesRaiseOneExplainedDiagnosticPerCase()
    {
        CatalogDiagnostics.Should().HaveCount(MigrationHintCases.All.Count);
        CatalogDiagnostics.Should().OnlyContain(x => x.Id == "VEK0002" && x.GetMessage().Contains(MigrationHintExpectations.HintMarker));
    }

    /// <summary>
    /// The markup every hint and documented example points at compiles and raises nothing, even with every attribute checked.
    /// </summary>
    [Test]
    public void ReplacementMarkupRaisesNothingUnderTheStrictestPattern()
    {
        ReplacementDiagnostics.Select(x => x.GetMessage()).Should().BeEmpty();
    }

    /// <summary>
    /// A legacy @bind- lowers into the removed value and its removed callback, and both halves are explained, as are explicitly typed callbacks.
    /// </summary>
    [Test]
    public void LegacyBindingsExplainBothHalves()
    {
        LegacyBindingDiagnostics.Should().HaveCount(23, "ten legacy bindings report two halves each, plus three explicitly typed attributes");
        LegacyBindingDiagnostics.Should().OnlyContain(x => x.GetMessage().Contains(MigrationHintExpectations.HintMarker));

        AssertEach("Checked", "VekCheckBox", 3);
        AssertEach("CheckedChanged", "VekCheckBox", 3);
        AssertEach("Checked", "VekSwitch", 1);
        AssertEach("CheckedChanged", "VekSwitch", 2);
        AssertEach("SelectedOption", "VekRadioGroup", 1);
        AssertEach("SelectedOptionChanged", "VekRadioGroup", 1);
        AssertEach("IsVisible", "VekDialog", 1);
        AssertEach("IsVisibleChanged", "VekDialog", 1);
        AssertEach("IsVisible", "VekMessageBox", 1);
        AssertEach("IsVisibleChanged", "VekMessageBox", 1);
        AssertEach("IsVisible", "VekTooltip", 1);
        AssertEach("IsVisibleChanged", "VekTooltip", 1);
        AssertEach("IsHidden", "VekHidden", 1);
        AssertEach("IsHiddenChanged", "VekHidden", 1);
        AssertEach("IsExpanded", "VekExpansionPanel", 1);
        AssertEach("IsExpandedChanged", "VekExpansionPanel", 1);
        AssertEach("IsChecked", "VekTr", 1);
        AssertEach("IsCheckedChanged", "VekTr", 1);

        static void AssertEach(string attributeName, string tagName, int count)
        {
            var hintCase = MigrationHintCases.All.Single(x => x.ToString() == $"{tagName}.{attributeName}");
            var diagnostics = LegacyBindingDiagnostics.All(attributeName, tagName);

            diagnostics.Should().HaveCount(count);

            foreach (var diagnostic in diagnostics)
            {
                diagnostic.ShouldCarryHint(attributeName, tagName, [.. hintCase.ExpectedHint]);
            }
        }
    }

    /// <summary>
    /// Lookalikes keep the generic warning, and current parameters, a case-only rename, and a component VEK0002 does not inspect raise nothing.
    /// </summary>
    [Test]
    public void BoundariesKeepGenericWarningOrStaySilent()
    {
        BoundaryDiagnostics.Should().HaveCount(7);
        BoundaryDiagnostics.Single("Link", "VekMenu").ShouldCarryNoHint("Link", "VekMenu");
        BoundaryDiagnostics.Single("IsOpen", "VekMenu").ShouldCarryNoHint("IsOpen", "VekMenu");
        BoundaryDiagnostics.Single("DisableRipple", "VekSelectItem").ShouldCarryNoHint("DisableRipple", "VekSelectItem");
        BoundaryDiagnostics.Single("IsOpen", "VekAutocomplete").ShouldCarryNoHint("IsOpen", "VekAutocomplete");
        BoundaryDiagnostics.Single("Checked", "VekRadio").ShouldCarryNoHint("Checked", "VekRadio");
        BoundaryDiagnostics.Single("IsChecked", "VekTHeadRow").ShouldCarryNoHint("IsChecked", "VekTHeadRow");
        BoundaryDiagnostics.Single("Title", "VekIconButton").ShouldCarryNoHint("Title", "VekIconButton");
    }

    /// <summary>
    /// The existing attribute fixture keeps every warning it had, and only the removed parameters it already used gain a hint.
    /// </summary>
    [Test]
    public void ExistingAttributeFixtureOnlyGainsIntendedHints()
    {
        AttributeTestLowerCaseDiagnostics.Should().HaveCount(13);
        AttributeTestLowerCaseDiagnostics.WithHint().Should().ContainSingle();
        AttributeTestLowerCaseDiagnostics.Single("Minimum", "VekProgressLinear").ShouldCarryHint("Minimum", "VekProgressLinear", [.. MigrationHintCases.All.Single(x => x.ToString() == "VekProgressLinear.Minimum").ExpectedHint]);

        AttributeTestNoneDiagnostics.Should().HaveCount(22);
        AttributeTestNoneDiagnostics.WithHint().Should().HaveCount(2);
        AttributeTestNoneDiagnostics.Single("icon", "VekFab").ShouldCarryHint("icon", "VekFab", [.. MigrationHintCases.All.Single(x => x.ToString() == "VekFab.Icon").ExpectedHint]);
    }
}
#nullable restore
