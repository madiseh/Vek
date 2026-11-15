// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Central place to read runtime library metadata such as the Vek version.
/// </summary>
/// <remarks>
/// Useful for diagnostics, bug reports, and displaying version info in app footers or about dialogs without hard-coding values.
/// </remarks>
public static class Metadata
{
    /// <summary>
    /// The current version number of Vek.
    /// </summary>
    public static string Version { get; } = typeof(Metadata).Assembly.GetName().Version?.ToString(3) ?? "unknown";
}
