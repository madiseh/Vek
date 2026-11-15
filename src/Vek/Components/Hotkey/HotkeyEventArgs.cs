// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// The information passed when a <see cref="VekHotkey"/> is pressed.
/// </summary>
public class HotkeyEventArgs : EventArgs
{
    /// <summary>
    /// The key which triggered the hotkey.
    /// </summary>
    public JsKey Key { get; }

    /// <summary>
    /// The modifiers which were pressed together with <see cref="Key"/>.
    /// </summary>
    public IReadOnlyCollection<JsKeyModifier> KeyModifiers { get; }

    /// <summary>
    /// The <see cref="VekHotkey"/> which raised the event.
    /// </summary>
    public VekHotkey Sender { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="key">The key which triggered the hotkey.</param>
    /// <param name="keyModifiers">The modifiers which were pressed together with <paramref name="key"/>.</param>
    /// <param name="sender">The <see cref="VekHotkey"/> which raised the event.</param>
    public HotkeyEventArgs(JsKey key, IReadOnlyCollection<JsKeyModifier> keyModifiers, VekHotkey sender)
    {
        Key = key;
        KeyModifiers = keyModifiers;
        Sender = sender;
    }
}
