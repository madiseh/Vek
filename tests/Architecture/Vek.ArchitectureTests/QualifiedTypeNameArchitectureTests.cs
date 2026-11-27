using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vek.ArchitectureTests;

/// <summary>
/// Architecture harness verifying that C# source files do not contain qualified type names (names with namespace prefixes)
/// in their code definitions, and instead use using directives or using aliases.
/// </summary>
[TestFixture]
public sealed class QualifiedTypeNameArchitectureTests
{
    private static readonly string[] MonitoredProjectDirectories =
    [
        "src/Vek"
    ];

    private static readonly string[] DisallowedNamespacePrefixes =
    [
        "System.",
        "Microsoft."
    ];

    /// <summary>
    /// Verifies that source files in monitored projects do not use qualified type names with namespace prefixes,
    /// requiring using directives or aliases instead.
    /// </summary>
    [Test]
    public void MonitoredProjectsDoNotUseQualifiedTypeNamesInDeclarations()
    {
        var repoRoot = FindRepositoryRoot();
        var violations = new List<string>();

        foreach (var relativeDir in MonitoredProjectDirectories)
        {
            var dirPath = Path.Combine(repoRoot, relativeDir.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(dirPath))
            {
                continue;
            }

            var csFiles = Directory.GetFiles(dirPath, "*.cs", SearchOption.AllDirectories)
                .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                               !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                               !file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) &&
                               !file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) &&
                               !Path.GetFileName(file).Equals("GlobalUsings.cs", StringComparison.OrdinalIgnoreCase) &&
                               !Path.GetFileName(file).Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in csFiles)
            {
                var code = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetRoot();

                // 1. Qualified type names (e.g. System.Collections.IEnumerable, Microsoft.AspNetCore.Components.Web.KeyboardEventArgs)
                var qualifiedTypes = root.DescendantNodes()
                    .OfType<QualifiedNameSyntax>()
                    .Where(q => q.Ancestors().All(a => a is not UsingDirectiveSyntax) &&
                                q.Parent is not BaseNamespaceDeclarationSyntax &&
                                q.Parent is not QualifiedNameSyntax);

                foreach (var qn in qualifiedTypes)
                {
                    var nameText = qn.ToString();
                    if (DisallowedNamespacePrefixes.Any(prefix => nameText.StartsWith(prefix, StringComparison.Ordinal)))
                    {
                        var lineSpan = qn.GetLocation().GetLineSpan();
                        var line = lineSpan.StartLinePosition.Line + 1;
                        var relFilePath = Path.GetRelativePath(repoRoot, file);
                        violations.Add($"{relFilePath}:{line} - Qualified type '{nameText}'");
                    }
                }

                // 2. MemberAccessExpressionSyntax starting with namespaces (e.g. System.Diagnostics.Process)
                var memberAccesses = root.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Where(m => m.Ancestors().All(a => a is not UsingDirectiveSyntax &&
                                                       a is not BaseNamespaceDeclarationSyntax));

                foreach (var ma in memberAccesses)
                {
                    var text = ma.ToString();
                    if (DisallowedNamespacePrefixes.Any(prefix => text.StartsWith(prefix, StringComparison.Ordinal)))
                    {
                        if (ma.Parent is MemberAccessExpressionSyntax parentMa &&
                            DisallowedNamespacePrefixes.Any(prefix => parentMa.ToString().StartsWith(prefix, StringComparison.Ordinal)))
                        {
                            continue;
                        }

                        var lineSpan = ma.GetLocation().GetLineSpan();
                        var line = lineSpan.StartLinePosition.Line + 1;
                        var relFilePath = Path.GetRelativePath(repoRoot, file);
                        violations.Add($"{relFilePath}:{line} - Qualified member access '{text}'");
                    }
                }
            }
        }

        violations.Should().BeEmpty(
            $"Found {violations.Count} disallowed qualified type names in source files. Types must be imported via using/global using or aliased (e.g., using MyAlias = Namespace.Type):\n" +
            string.Join("\n", violations.Take(50)));
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "Vek.slnx")) ||
                File.Exists(Path.Combine(current, "Directory.Build.props")) ||
                File.Exists(Path.Combine(current, ".git")) ||
                Directory.Exists(Path.Combine(current, ".git")))
            {
                return current;
            }
            current = Path.GetDirectoryName(current);
        }
        throw new InvalidOperationException("Could not find repository root containing Vek.slnx or Directory.Build.props");
    }
}
