// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Services;

/// <summary>
/// Creates <see cref="IJsEvent"/> instances that subscribe to an element's DOM events such as paste, text selection, and caret position changes.
/// </summary>
public interface IJsEventFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IJsEvent"/>.
    /// </summary>
    /// <remarks>
    /// If you are creating this <see cref="IJsEvent"/> instance yourself using this factory, then you need to manually call <see cref="JsEvent.DisposeAsync"/>; otherwise, you will get a memory leak.
    /// </remarks>
    /// <returns>A new instance of <see cref="IJsEvent"/>.</returns>
    IJsEvent Create();
}
