// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Specifies the listening scope for a <see cref="VekHotkey"/> registration.
/// </summary>
public enum HotkeyScope
{
    /// <summary>
    /// Listens for keyboard shortcuts globally across the document.
    /// </summary>
    Global,

    /// <summary>
    /// Listens for keyboard shortcuts only when the keyboard event originates inside the scoped container.
    /// </summary>
    Scoped
}
