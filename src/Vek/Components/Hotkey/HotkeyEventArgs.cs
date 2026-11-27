// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components.Web;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// The information passed when a <see cref="VekHotkey"/> is triggered.
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
    /// The normalized key combination or chord sequence string associated with the hotkey.
    /// </summary>
    public string? KeyCombination { get; }

    /// <summary>
    /// The container scope identifier if the hotkey was scoped, or <c>null</c> for global hotkeys.
    /// </summary>
    public string? ScopeId { get; }

    /// <summary>
    /// Indicates whether the triggered hotkey was a multi-step chord sequence.
    /// </summary>
    public bool IsChord { get; }

    /// <summary>
    /// The keyboard event arguments associated with the trigger, when available.
    /// </summary>
    public KeyboardEventArgs? KeyboardEventArgs { get; }

    public HotkeyEventArgs(JsKey key, IReadOnlyCollection<JsKeyModifier> keyModifiers, VekHotkey sender)
        : this(key, keyModifiers, sender, keyCombination: null, scopeId: null, isChord: false, keyboardEventArgs: null)
    {
    }

    public HotkeyEventArgs(
        JsKey key,
        IReadOnlyCollection<JsKeyModifier> keyModifiers,
        VekHotkey sender,
        string? keyCombination,
        string? scopeId = null,
        bool isChord = false,
        KeyboardEventArgs? keyboardEventArgs = null)
    {
        Key = key;
        KeyModifiers = keyModifiers;
        Sender = sender;
        KeyCombination = keyCombination;
        ScopeId = scopeId;
        IsChord = isChord;
        KeyboardEventArgs = keyboardEventArgs;
    }
}
