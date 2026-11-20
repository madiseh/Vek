// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

namespace Vek.UnitTests.Analyzers.Internal;

extern alias VekAnalyzer;

#nullable enable
internal static class DiagnosticHelper
{
    internal static IReadOnlyList<Diagnostic> FilterToClass(this IEnumerable<Diagnostic> diagnostics, string? className)
    {
        var results = new List<Diagnostic>();
        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Properties.TryGetValue(VekAnalyzer::Vek.Analyzers.VekComponentUnknownParametersAnalyzer.ClassNamePropertyKey, out var cn)
                && string.Equals(cn, className))
            {
                results.Add(diagnostic);
            }
        }

        return results;
    }
}
