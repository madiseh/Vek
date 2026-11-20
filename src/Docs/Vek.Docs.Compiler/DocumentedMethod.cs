// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Docs.Compiler;

/// <summary>
/// Represents documentation for a method.
/// </summary>
public class DocumentedMethod : DocumentedMember
{
    /// <summary>
    /// The parameters for this method.
    /// </summary>
    public List<DocumentedParameter> Parameters { get; set; } = [];

    /// <summary>
    /// The XML documentation for what this method returns.
    /// </summary>
    public string? Returns { get; set; }
}
