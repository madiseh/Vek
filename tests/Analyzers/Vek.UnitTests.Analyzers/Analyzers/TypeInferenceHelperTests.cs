// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Vek.UnitTests.Analyzers.Internal;
using NUnit.Framework;

namespace Vek.UnitTests.Analyzers;

extern alias VekAnalyzer;

#nullable enable
/// <summary>
/// Covers how VEK0002 follows the TypeInference helpers Razor generates for generic components whose type argument is inferred.
/// </summary>
/// <remarks>
/// Real Razor output is covered by <see cref="ValidAttributeTests"/> and <see cref="RazorMigrationHintTests"/>.
/// These hand-written shapes pin the boundaries around which helper declaration is followed.
/// </remarks>
[TestFixture]
public class TypeInferenceHelperTests
{
    private const string Usings = """
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;
        using Vek;
        """;

    /// <summary>
    /// Every call is followed into its helper, including repeated calls, explicit type arguments, and helpers split across a partial class, and each diagnostic names the calling component.
    /// </summary>
    [Test]
    public async Task FollowsEveryHelperCallAndAttributesItToTheCaller()
    {
        var source = Usings + """

            namespace Consumer
            {
                public class GenericCaller : ComponentBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        global::__Blazor.Consumer.GenericCaller.TypeInference.CreateVekTextField_0(builder, 0, 1, "inferred", 2, "first");
                        global::__Blazor.Consumer.GenericCaller.TypeInference.CreateVekTextField_0<int>(builder, 3, 4, 5, 6, "explicit");
                        global::__Blazor.Consumer.GenericCaller.TypeInference.CreateVekSelect_1(builder, 7, 8, "inferred", 9, "select");
                        builder.OpenComponent<VekButton>(10);
                        builder.AddAttribute(11, "Direct", "direct");
                        builder.CloseComponent();
                    }
                }

                public class OtherCaller : ComponentBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        global::__Blazor.Consumer.GenericCaller.TypeInference.CreateVekSelect_1(builder, 0, 1, 2, 3, "other");
                    }
                }
            }

            namespace __Blazor.Consumer.GenericCaller
            {
                internal static partial class TypeInference
                {
                    public static void CreateVekTextField_0<T>(RenderTreeBuilder __builder, int seq, int __seq0, T __arg0, int __seq1, string __arg1)
                    {
                        __builder.OpenComponent<global::Vek.VekTextField<T>>(seq);
                        __builder.AddComponentParameter(__seq0, "Value", __arg0);
                        __builder.AddAttribute(__seq1, "TextFieldUnknown", __arg1);
                        __builder.CloseComponent();
                    }
                }

                internal static partial class TypeInference
                {
                    public static void CreateVekSelect_1<T>(RenderTreeBuilder __builder, int seq, int __seq0, T __arg0, int __seq1, string __arg1)
                    {
                        __builder.OpenComponent<global::Vek.VekSelect<T>>(seq);
                        __builder.AddComponentParameter(__seq0, "Value", __arg0);
                        __builder.AddAttribute(__seq1, "SelectUnknown", __arg1);
                        __builder.CloseComponent();
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        diagnostics.Should().BeEquivalentTo(
            "TextFieldUnknown on VekTextField in Consumer.GenericCaller",
            "TextFieldUnknown on VekTextField in Consumer.GenericCaller",
            "SelectUnknown on VekSelect in Consumer.GenericCaller",
            "Direct on VekButton in Consumer.GenericCaller",
            "SelectUnknown on VekSelect in Consumer.OtherCaller");
    }

    /// <summary>
    /// A helper class nested in the component, and a local function sharing the helper's name, do not change which body is followed.
    /// </summary>
    [Test]
    public async Task NestedHelperClassAndLocalFunctionWithTheSameName()
    {
        var source = Usings + """

            namespace Consumer
            {
                public class NestedCaller : ComponentBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        TypeInference.CreateVekTextField_0(builder, 0, "nested");

                        void CreateVekSelect_1()
                        {
                        }
                    }

                    private static class TypeInference
                    {
                        public static void CreateVekTextField_0<T>(RenderTreeBuilder __builder, int seq, T __arg0)
                        {
                            __builder.OpenComponent<global::Vek.VekTextField<T>>(seq);
                            __builder.AddAttribute(1, "NestedUnknown", __arg0);
                            __builder.CloseComponent();
                        }
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        diagnostics.Should().BeEquivalentTo("NestedUnknown on VekTextField in Consumer.NestedCaller");
    }

    /// <summary>
    /// Constructions of one generic component over different type arguments, including each helper's own type parameter, all get its migration hint, while a subclass that declares the parameter again stays silent.
    /// </summary>
    [Test]
    public async Task EveryConstructionOfAGenericComponentGetsItsHint()
    {
        var source = Usings + """

            namespace Consumer
            {
                public class ReintroducedAutoGrow<T> : VekTextField<T>
                {
                    [Parameter]
                    public bool AutoGrow { get; set; }
                }

                public class GenericHints : ComponentBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        TypeInference.CreateVekTextField_0(builder, 0, "text");
                        TypeInference.CreateVekTextField_1(builder, 1, 42);
                        TypeInference.CreateReintroduced_2(builder, 2, "text");
                        builder.OpenComponent<VekTextField<decimal>>(3);
                        builder.AddAttribute(4, "AutoGrow", true);
                        builder.CloseComponent();
                        builder.OpenComponent<ReintroducedAutoGrow<int>>(5);
                        builder.AddAttribute(6, "AutoGrow", true);
                        builder.CloseComponent();
                    }
                }

                internal static class TypeInference
                {
                    public static void CreateVekTextField_0<T>(RenderTreeBuilder __builder, int seq, T __arg0)
                    {
                        __builder.OpenComponent<global::Vek.VekTextField<T>>(seq);
                        __builder.AddAttribute(1, "AutoGrow", __arg0);
                        __builder.CloseComponent();
                    }

                    public static void CreateVekTextField_1<TValue>(RenderTreeBuilder __builder, int seq, TValue __arg0)
                    {
                        __builder.OpenComponent<global::Vek.VekTextField<TValue>>(seq);
                        __builder.AddAttribute(1, "AutoGrow", __arg0);
                        __builder.CloseComponent();
                    }

                    public static void CreateReintroduced_2<T>(RenderTreeBuilder __builder, int seq, T __arg0)
                    {
                        __builder.OpenComponent<global::Consumer.ReintroducedAutoGrow<T>>(seq);
                        __builder.AddAttribute(1, "AutoGrow", __arg0);
                        __builder.CloseComponent();
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source, includeHint: true);

        diagnostics.Should().BeEquivalentTo(
            "AutoGrow on VekTextField in Consumer.GenericHints with hint",
            "AutoGrow on VekTextField in Consumer.GenericHints with hint",
            "AutoGrow on VekTextField in Consumer.GenericHints with hint");
    }

    /// <summary>
    /// A helper declared in a different file is not followed, which is also how a missing declaration behaves.
    /// </summary>
    [Test]
    public async Task HelperDeclaredInAnotherFileIsNotFollowed()
    {
        var caller = Usings + """

            namespace Consumer
            {
                public class SplitCaller : ComponentBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        global::Helpers.TypeInference.CreateVekTextField_0(builder, 0, "split");
                    }
                }
            }
            """;
        var helper = Usings + """

            namespace Helpers
            {
                internal static class TypeInference
                {
                    public static void CreateVekTextField_0<T>(RenderTreeBuilder __builder, int seq, T __arg0)
                    {
                        __builder.OpenComponent<global::Vek.VekTextField<T>>(seq);
                        __builder.AddAttribute(1, "ElsewhereUnknown", __arg0);
                        __builder.CloseComponent();
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(caller, helper);

        diagnostics.Should().BeEmpty();
    }

    /// <summary>
    /// Another method with the helper's name in the same file no longer hides the helper, because the call's own declaration is followed instead of a name search.
    /// </summary>
    /// <remarks>
    /// Before, the name search found two declarations and threw, which surfaced as AD0001 instead of VEK0002.
    /// </remarks>
    [Test]
    public async Task SameNamedMethodInTheFileDoesNotHideTheHelper()
    {
        var source = Usings + """

            namespace Consumer
            {
                public class CollidingCaller : ComponentBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        TypeInference.CreateVekTextField_0(builder, 0, "colliding");
                    }
                }

                public class Unrelated
                {
                    public void CreateVekTextField_0()
                    {
                    }
                }

                internal static class TypeInference
                {
                    public static void CreateVekTextField_0<T>(RenderTreeBuilder __builder, int seq, T __arg0)
                    {
                        __builder.OpenComponent<global::Vek.VekTextField<T>>(seq);
                        __builder.AddAttribute(1, "CollidingUnknown", __arg0);
                        __builder.CloseComponent();
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        diagnostics.Should().BeEquivalentTo("CollidingUnknown on VekTextField in Consumer.CollidingCaller");
    }

    /// <summary>
    /// A same-named method in the calling file is not mistaken for a helper declared in another file.
    /// </summary>
    /// <remarks>
    /// Before, the name search followed the unrelated method and reported its attributes against the caller.
    /// </remarks>
    [Test]
    public async Task SameNamedMethodInTheFileIsNotMistakenForAHelperElsewhere()
    {
        var caller = Usings + """

            namespace Consumer
            {
                public class DecoyCaller : ComponentBase
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        global::Helpers.TypeInference.CreateVekTextField_0(builder, 0, "decoy");
                    }
                }

                internal static class Decoy
                {
                    public static void CreateVekTextField_0(RenderTreeBuilder __builder)
                    {
                        __builder.OpenComponent<VekButton>(0);
                        __builder.AddAttribute(1, "DecoyUnknown", "decoy");
                        __builder.CloseComponent();
                    }
                }
            }
            """;
        var helper = Usings + """

            namespace Helpers
            {
                internal static class TypeInference
                {
                    public static void CreateVekTextField_0<T>(RenderTreeBuilder __builder, int seq, T __arg0)
                    {
                        __builder.OpenComponent<global::Vek.VekTextField<T>>(seq);
                        __builder.AddAttribute(1, "ElsewhereUnknown", __arg0);
                        __builder.CloseComponent();
                    }
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(caller, helper);

        diagnostics.Should().BeEmpty();
    }

    /// <summary>
    /// Runs VEK0002 over one or more files and describes each diagnostic by attribute, component, and reporting class, or by ID when it is not VEK0002.
    /// </summary>
    private static Task<IReadOnlyList<string>> GetDiagnosticsAsync(string source, params string[] otherFiles) => GetDiagnosticsAsync(source, includeHint: false, otherFiles);

    private static async Task<IReadOnlyList<string>> GetDiagnosticsAsync(string source, bool includeHint, params string[] otherFiles)
    {
        var compilation = AnalyzerCompilationFactory.CreateCompilation(source, "Caller.razor.g.cs")
            .AddSyntaxTrees(otherFiles.Select((text, i) => CSharpSyntaxTree.ParseText(SourceText.From(text, Encoding.UTF8), path: $"Other{i}.cs")));
        compilation.GetDiagnostics().Where(x => x.Severity is DiagnosticSeverity.Error).Should().BeEmpty();

        var analyzer = new VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer();
        var options = TestAnalyzerOptions.Create(VekAnalyzer::Vek.Analyzers.AllowedAttributePattern.LowerCase, []);
        var diagnostics = await compilation.WithAnalyzers([analyzer], options).GetAnalyzerDiagnosticsAsync(CancellationToken.None);

        return diagnostics
            .Select(x => x.Id == VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.DiagnosticId
                ? $"{x.GetMessage().Split('\'')[1]} on {x.GetMessage().Split('\'')[3]} in {x.Properties[VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.ClassNamePropertyKey]}{(includeHint && x.GetMessage().Contains("replaced by 'Sizing'") ? " with hint" : "")}"
                : x.Id)
            .ToList();
    }
}
