// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.State;

/// <summary>
/// Handles a Vek parameter's value change, receiving the old and new values through <see cref="ParameterChangedEventArgs{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the component's property value.</typeparam>
public interface IParameterChangedHandler<T>
{
    /// <summary>
    /// Handles parameter changes asynchronously.
    /// </summary>
    /// <param name="parameterChangedEventArgs">The <see cref="ParameterChangedEventArgs{T}"/> containing the information about the last and current values of a parameter.</param>
    /// <param name="context">The parameter changed context containing parameter view and state values.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task HandleAsync(ParameterChangedEventArgs<T> parameterChangedEventArgs, ParameterChangedContext context);
}
