// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Interfaces;

/// <summary>
/// Exposes a component's <c>StateHasChanged</c> method so a parent or sibling can request a re-render without a concrete component reference.
/// </summary>
public interface IVekStateHasChanged
{
    /// <summary>
    /// Notifies the component that its state has changed. When applicable, this will
    /// cause the component to be re-rendered.
    /// </summary>
    void StateHasChanged();
}
