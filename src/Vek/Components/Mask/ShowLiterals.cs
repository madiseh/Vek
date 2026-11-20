// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Controls when literal formatting characters and prompt characters are displayed in a masked input.
/// </summary>
public enum ShowLiterals
{
    /// <summary>
    /// Literals and prompt characters are always visible regardless of input focus.
    /// </summary>
    Always,

    /// <summary>
    /// Literals and prompt characters are visible while the input has focus or is being edited.
    /// When blurred, unfilled prompt characters are hidden.
    /// </summary>
    WhileEditing,

    /// <summary>
    /// Literals are never displayed in the input, displaying only raw user input while enforcing pattern rules.
    /// </summary>
    Never
}
