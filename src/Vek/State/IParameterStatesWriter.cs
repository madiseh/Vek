// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.State.Builder;

namespace Vek.State;

/// <summary>
/// Represents a writer for parameter states.
/// </summary>
internal interface IParameterStatesWriter
{
    /// <summary>
    /// Adds a parameter builder to the list for processing.
    /// </summary>
    /// <param name="builder">The parameter builder to add.</param>
    void WriteParameter(IParameterBuilderAttach builder);
}
