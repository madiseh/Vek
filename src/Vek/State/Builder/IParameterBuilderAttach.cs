// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.State.Builder;

/// <summary>
/// Represents an interface for non-generic builder to attach <see cref="ParameterState{T}"/>.
/// </summary>
internal interface IParameterBuilderAttach
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="ParameterState{T}"/> is attached.
    /// </summary>
    bool IsAttached { get; }

    /// <summary>
    /// Attaches the <see cref="ParameterState{T}"/>.
    /// </summary>
    IParameterComponentLifeCycle Attach();
}
