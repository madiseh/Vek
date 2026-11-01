// Copyright (c) 2026 AMIN SABAGHI
// Licensed under the MIT License. See the LICENSE file in the project root for more information.
// Created: 2026-11-01

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vek.WPF.Demo.Tests;

/// <summary>
/// Architecture harness verifying that C# source files do not contain qualified type names (names with dots) in their code definitions,
/// and instead use using directives or using aliases.
/// </summary>
public sealed class QualifiedTypeNameArchitectureTests
{
    private static readonly string[] MonitoredProjectDirectories =
    [
        "src/Components/WPF",
        "src/Components/Core",
        "src/Demo/WPF",
        "src/Demo/Vek.Demo",
        "src/Adapters",
        "src/Ports"
    ];

    /// <summary>
    /// Verifies that source files in monitored projects do not use qualified type names in member declarations,
    /// base types, or generic arguments, requiring using directives or aliases instead.
    /// </summary>
    [Fact]
    public void MonitoredProjectsDoNotUseQualifiedTypeNamesInDeclarations()
    {
        var repoRoot = FindRepositoryRoot();
        var violations = new List<string>();

        foreach (var relativeDir in MonitoredProjectDirectories)
        {
            var dirPath = Path.Combine(repoRoot, relativeDir.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(Directory.Exists(dirPath), $"Directory '{dirPath}' does not exist");

            var csFiles = Directory.GetFiles(dirPath, "*.cs", SearchOption.AllDirectories)
                .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                               !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                               !Path.GetFileName(file).Equals("GlobalUsings.cs", StringComparison.OrdinalIgnoreCase) &&
                               !Path.GetFileName(file).Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.NotEmpty(csFiles);

            foreach (var file in csFiles)
            {
                var code = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetRoot();

                // 1. Qualified type names (e.g. System.Windows.Controls.DataGrid or System.Windows.DataTemplate)
                // Filter out the namespace declaration itself:
                var qualifiedTypes = root.DescendantNodes()
                    .OfType<QualifiedNameSyntax>()
                    .Where(q => q.Ancestors().All(a => a is not UsingDirectiveSyntax) &&
                                q.Parent is not BaseNamespaceDeclarationSyntax &&
                                q.Parent is not QualifiedNameSyntax);

                foreach (var qn in qualifiedTypes)
                {
                    var lineSpan = qn.GetLocation().GetLineSpan();
                    var line = lineSpan.StartLinePosition.Line + 1;
                    var relFilePath = Path.GetRelativePath(repoRoot, file);
                    violations.Add($"{relFilePath}:{line} - Qualified type '{qn}'");
                }

                // 2. MemberAccessExpressionSyntax starting with namespaces (e.g. System.Diagnostics.Process)
                var memberAccesses = root.DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Where(m => m.Ancestors().All(a => a is not UsingDirectiveSyntax &&
                                                       a is not BaseNamespaceDeclarationSyntax));

                foreach (var ma in memberAccesses)
                {
                    var text = ma.ToString();
                    if (text.StartsWith("System.", StringComparison.Ordinal) ||
                        text.StartsWith("Microsoft.", StringComparison.Ordinal) ||
                        text.StartsWith("Vek.", StringComparison.Ordinal))
                    {
                        if (ma.Parent is MemberAccessExpressionSyntax parentMa &&
                            (parentMa.ToString().StartsWith("System.", StringComparison.Ordinal) ||
                             parentMa.ToString().StartsWith("Microsoft.", StringComparison.Ordinal) ||
                             parentMa.ToString().StartsWith("Vek.", StringComparison.Ordinal)))
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

        Assert.True(
            violations.Count == 0,
            $"Found {violations.Count} disallowed qualified type names in source files. Types must be imported via using/global using or aliased (e.g., using MyAlias = Namespace.Type):\n" +
            string.Join("\n", violations));
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
