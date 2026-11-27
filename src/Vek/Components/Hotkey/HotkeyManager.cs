// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// Represents a single keystroke and modifier state within a <see cref="HotkeySequence"/>.
/// </summary>
public sealed class HotkeyChordStep
{
    private static readonly HashSet<string> ShiftedSymbolKeys = new(StringComparer.Ordinal)
    {
        "?", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "{", "}", "|", ":", "\"", "<", ">", "~"
    };

    private static readonly Dictionary<string, (string Key, string Code)> KeyAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["esc"] = ("Escape", "Escape"),
        ["escape"] = ("Escape", "Escape"),
        ["enter"] = ("Enter", "Enter"),
        ["return"] = ("Enter", "Enter"),
        ["space"] = (" ", "Space"),
        ["spacebar"] = (" ", "Space"),
        ["tab"] = ("Tab", "Tab"),
        ["backspace"] = ("Backspace", "Backspace"),
        ["del"] = ("Delete", "Delete"),
        ["delete"] = ("Delete", "Delete"),
        ["ins"] = ("Insert", "Insert"),
        ["insert"] = ("Insert", "Insert"),
        ["home"] = ("Home", "Home"),
        ["end"] = ("End", "End"),
        ["pageup"] = ("PageUp", "PageUp"),
        ["pgup"] = ("PageUp", "PageUp"),
        ["pagedown"] = ("PageDown", "PageDown"),
        ["pgdn"] = ("PageDown", "PageDown"),
        ["up"] = ("ArrowUp", "ArrowUp"),
        ["arrowup"] = ("ArrowUp", "ArrowUp"),
        ["down"] = ("ArrowDown", "ArrowDown"),
        ["arrowdown"] = ("ArrowDown", "ArrowDown"),
        ["left"] = ("ArrowLeft", "ArrowLeft"),
        ["arrowleft"] = ("ArrowLeft", "ArrowLeft"),
        ["right"] = ("ArrowRight", "ArrowRight"),
        ["arrowright"] = ("ArrowRight", "ArrowRight"),
        ["plus"] = ("+", "Equal"),
        ["+"] = ("+", "Equal"),
        ["minus"] = ("-", "Minus"),
        ["-"] = ("-", "Minus"),
        ["equal"] = ("=", "Equal"),
        ["="] = ("=", "Equal"),
        ["/"] = ("/", "Slash"),
        ["slash"] = ("/", "Slash"),
        ["?"] = ("?", "Slash"),
        ["."] = (".", "Period"),
        ["period"] = (".", "Period"),
        [","] = (",", "Comma"),
        ["comma"] = (",", "Comma"),
        [";"] = (";", "Semicolon"),
        ["semicolon"] = (";", "Semicolon"),
        ["'"] = ("'", "Quote"),
        ["quote"] = ("'", "Quote"),
        ["["] = ("[", "BracketLeft"),
        ["bracketleft"] = ("[", "BracketLeft"),
        ["]"] = ("]", "BracketRight"),
        ["bracketright"] = ("]", "BracketRight"),
        ["\\"] = ("\\", "Backslash"),
        ["backslash"] = ("\\", "Backslash"),
        ["`"] = ("`", "Backquote"),
        ["backquote"] = ("`", "Backquote"),
    };

    /// <summary>
    /// The DOM <c>KeyboardEvent.key</c> value or normalized key name for this step.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The DOM <c>KeyboardEvent.code</c> value for this step, when applicable.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The generic or side-specific modifier names required for this step.
    /// </summary>
    public IReadOnlyList<string> Modifiers { get; }

    /// <summary>
    /// Indicates whether <see cref="Modifiers"/> uses exact left/right modifier matching.
    /// </summary>
    public bool UseExactSideModifiers { get; }

    /// <summary>
    /// Indicates whether the Control modifier is required.
    /// </summary>
    public bool RequireCtrl { get; }

    /// <summary>
    /// Indicates whether the Shift modifier is required.
    /// </summary>
    public bool RequireShift { get; }

    /// <summary>
    /// Indicates whether the Alt modifier is required.
    /// </summary>
    public bool RequireAlt { get; }

    /// <summary>
    /// Indicates whether the Meta (Command/Windows) modifier is required.
    /// </summary>
    public bool RequireMeta { get; }

    /// <summary>
    /// Human-readable representation of this chord step.
    /// </summary>
    public string DisplayText { get; }

    public HotkeyChordStep(
        string key,
        string code,
        IReadOnlyList<string> modifiers,
        bool useExactSideModifiers,
        bool requireCtrl,
        bool requireShift,
        bool requireAlt,
        bool requireMeta,
        string displayText)
    {
        Key = key;
        Code = code;
        Modifiers = modifiers;
        UseExactSideModifiers = useExactSideModifiers;
        RequireCtrl = requireCtrl;
        RequireShift = requireShift;
        RequireAlt = requireAlt;
        RequireMeta = requireMeta;
        DisplayText = displayText;
    }

    /// <summary>
    /// Parses a single shortcut step such as <c>"Ctrl+S"</c> or <c>"Escape"</c>.
    /// </summary>
    /// <param name="stepExpression">The raw step expression.</param>
    /// <returns>The parsed <see cref="HotkeyChordStep"/>.</returns>
    public static HotkeyChordStep Parse(string stepExpression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepExpression);

        var tokens = SplitStepTokens(stepExpression);
        var modifiers = new List<string>();
        var exactModifiers = false;
        var requireCtrl = false;
        var requireShift = false;
        var requireAlt = false;
        var requireMeta = false;
        string? rawKey = null;

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var isLast = i == tokens.Count - 1;

            if (!isLast && TryParseModifier(token, out var normalizedModifier, out var isExact))
            {
                if (!modifiers.Contains(normalizedModifier, StringComparer.Ordinal))
                {
                    modifiers.Add(normalizedModifier);
                }

                exactModifiers |= isExact;
                ApplyModifierFlags(normalizedModifier, ref requireCtrl, ref requireShift, ref requireAlt, ref requireMeta);
            }
            else if (isLast)
            {
                rawKey = token;
            }
            else
            {
                // Unknown non-final token: treat as part of key fallback if needed.
                rawKey = token;
            }
        }

        if (string.IsNullOrEmpty(rawKey))
        {
            throw new FormatException($"Invalid hotkey step expression '{stepExpression}'.");
        }

        var (normalizedKey, normalizedCode, keyLabel) = NormalizeKey(rawKey);
        var displayParts = new List<string>(modifiers.Count + 1);
        foreach (var mod in modifiers)
        {
            displayParts.Add(FormatModifierDisplay(mod));
        }

        displayParts.Add(keyLabel);

        return new HotkeyChordStep(
            normalizedKey,
            normalizedCode,
            modifiers,
            exactModifiers,
            requireCtrl,
            requireShift,
            requireAlt,
            requireMeta,
            string.Join("+", displayParts));
    }

    /// <summary>
    /// Creates a step from a strongly typed <see cref="JsKey"/> and modifier list.
    /// </summary>
    /// <param name="key">The primary key.</param>
    /// <param name="keyModifiers">Optional exact side-specific modifiers.</param>
    /// <returns>The constructed <see cref="HotkeyChordStep"/>.</returns>
    public static HotkeyChordStep FromJsKey(JsKey key, IEnumerable<JsKeyModifier>? keyModifiers)
    {
        var modifierList = keyModifiers?.Select(m => m.ToString()).Distinct(StringComparer.Ordinal).ToArray() ?? [];
        var requireCtrl = modifierList.Any(m => m is nameof(JsKeyModifier.ControlLeft) or nameof(JsKeyModifier.ControlRight));
        var requireShift = modifierList.Any(m => m is nameof(JsKeyModifier.ShiftLeft) or nameof(JsKeyModifier.ShiftRight));
        var requireAlt = modifierList.Any(m => m is nameof(JsKeyModifier.AltLeft) or nameof(JsKeyModifier.AltRight));
        var requireMeta = modifierList.Any(m => m is nameof(JsKeyModifier.MetaLeft) or nameof(JsKeyModifier.MetaRight));

        var code = key.ToString();
        var (normalizedKey, _, keyLabel) = NormalizeKey(code);
        var displayParts = modifierList.Select(FormatModifierDisplay).Append(keyLabel);

        return new HotkeyChordStep(
            normalizedKey,
            code,
            modifierList,
            useExactSideModifiers: true,
            requireCtrl,
            requireShift,
            requireAlt,
            requireMeta,
            string.Join("+", displayParts));
    }

    /// <summary>
    /// Evaluates whether a keyboard event matches this step's key and modifier requirements.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns><c>true</c> if the event matches; otherwise, <c>false</c>.</returns>
    public bool Matches(KeyboardEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!MatchesModifiers(args))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(args.Code) &&
            string.Equals(args.Code, Code, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(args.Key))
        {
            if (Key.Length == 1 && args.Key.Length == 1)
            {
                return string.Equals(args.Key, Key, StringComparison.OrdinalIgnoreCase);
            }

            if (string.Equals(args.Key, Key, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool MatchesModifiers(KeyboardEventArgs args)
    {
        if (UseExactSideModifiers)
        {
            var pressed = GetPressedSideModifiers(args);
            return Modifiers.Count == pressed.Count && Modifiers.All(pressed.Contains);
        }

        // Allow shifted punctuation symbols like '?' to match both synthetic { Key = "?" } and real browser { Key = "?", ShiftKey = true }.
        var shiftMatches = args.ShiftKey == RequireShift ||
                           (!RequireShift && ShiftedSymbolKeys.Contains(Key));

        return args.CtrlKey == RequireCtrl &&
               shiftMatches &&
               args.AltKey == RequireAlt &&
               args.MetaKey == RequireMeta;
    }

    internal static bool IsStandaloneModifierKey(KeyboardEventArgs args)
    {
        var key = args.Key;
        return key is "Control" or "Shift" or "Alt" or "Meta";
    }

    internal bool TargetsModifierKey()
    {
        return Code is "ControlLeft" or "ControlRight" or "ShiftLeft" or "ShiftRight" or "AltLeft" or "AltRight" or "MetaLeft" or "MetaRight";
    }

    private static HashSet<string> GetPressedSideModifiers(KeyboardEventArgs args)
    {
        var pressed = new HashSet<string>(StringComparer.Ordinal);
        const long RightLocation = 2;

        if (args.CtrlKey)
        {
            pressed.Add(args.Code == "ControlRight" || args.Location == RightLocation ? "ControlRight" : "ControlLeft");
        }

        if (args.ShiftKey)
        {
            pressed.Add(args.Code == "ShiftRight" || args.Location == RightLocation ? "ShiftRight" : "ShiftLeft");
        }

        if (args.AltKey)
        {
            pressed.Add(args.Code == "AltRight" || args.Location == RightLocation ? "AltRight" : "AltLeft");
        }

        if (args.MetaKey)
        {
            pressed.Add(args.Code == "MetaRight" || args.Location == RightLocation ? "MetaRight" : "MetaLeft");
        }

        return pressed;
    }

    private static List<string> SplitStepTokens(string stepExpression)
    {
        var trimmed = stepExpression.Trim();
        var tokens = new List<string>();
        var current = 0;

        for (var i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] == '+')
            {
                var segment = trimmed[current..i].Trim();
                if (segment.Length > 0)
                {
                    tokens.Add(segment);
                    current = i + 1;
                }
                else if (i == trimmed.Length - 1)
                {
                    // Trailing '+' represents the plus key itself (e.g. "Ctrl++" or "+").
                    tokens.Add("+");
                    current = trimmed.Length;
                }
            }
        }

        if (current < trimmed.Length)
        {
            var tail = trimmed[current..].Trim();
            if (tail.Length > 0)
            {
                tokens.Add(tail);
            }
        }

        if (tokens.Count == 0 && trimmed == "+")
        {
            tokens.Add("+");
        }

        return tokens;
    }

    private static bool TryParseModifier(string token, out string normalized, out bool isExact)
    {
        switch (token.ToLowerInvariant())
        {
            case "ctrl":
            case "control":
            case "mod":
                normalized = "Control";
                isExact = false;
                return true;
            case "shift":
                normalized = "Shift";
                isExact = false;
                return true;
            case "alt":
            case "option":
                normalized = "Alt";
                isExact = false;
                return true;
            case "meta":
            case "cmd":
            case "command":
            case "win":
            case "super":
                normalized = "Meta";
                isExact = false;
                return true;
            case "controlleft":
                normalized = "ControlLeft";
                isExact = true;
                return true;
            case "controlright":
                normalized = "ControlRight";
                isExact = true;
                return true;
            case "shiftleft":
                normalized = "ShiftLeft";
                isExact = true;
                return true;
            case "shiftright":
                normalized = "ShiftRight";
                isExact = true;
                return true;
            case "altleft":
                normalized = "AltLeft";
                isExact = true;
                return true;
            case "altright":
                normalized = "AltRight";
                isExact = true;
                return true;
            case "metaleft":
                normalized = "MetaLeft";
                isExact = true;
                return true;
            case "metaright":
                normalized = "MetaRight";
                isExact = true;
                return true;
            default:
                normalized = string.Empty;
                isExact = false;
                return false;
        }
    }

    private static void ApplyModifierFlags(string modifier, ref bool ctrl, ref bool shift, ref bool alt, ref bool meta)
    {
        switch (modifier)
        {
            case "Control":
            case "ControlLeft":
            case "ControlRight":
                ctrl = true;
                break;
            case "Shift":
            case "ShiftLeft":
            case "ShiftRight":
                shift = true;
                break;
            case "Alt":
            case "AltLeft":
            case "AltRight":
                alt = true;
                break;
            case "Meta":
            case "MetaLeft":
            case "MetaRight":
                meta = true;
                break;
        }
    }

    private static (string Key, string Code, string Label) NormalizeKey(string rawKey)
    {
        if (KeyAliases.TryGetValue(rawKey, out var alias))
        {
            var aliasLabel = alias.Key == " " ? "Space" : alias.Key;
            return (alias.Key, alias.Code, aliasLabel);
        }

        if (rawKey.Length == 1)
        {
            var ch = rawKey[0];
            if (ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z'))
            {
                var upper = char.ToUpperInvariant(ch);
                var lower = char.ToLowerInvariant(ch);
                return (lower.ToString(), $"Key{upper}", upper.ToString());
            }

            if (ch is >= '0' and <= '9')
            {
                return (ch.ToString(), $"Digit{ch}", ch.ToString());
            }

            return (rawKey, rawKey, rawKey);
        }

        if (rawKey.StartsWith("Key", StringComparison.OrdinalIgnoreCase) && rawKey.Length == 4)
        {
            var letter = char.ToUpperInvariant(rawKey[3]);
            return (char.ToLowerInvariant(letter).ToString(), $"Key{letter}", letter.ToString());
        }

        if (rawKey.StartsWith("Digit", StringComparison.OrdinalIgnoreCase) && rawKey.Length == 6 && char.IsDigit(rawKey[5]))
        {
            var digit = rawKey[5];
            return (digit.ToString(), $"Digit{digit}", digit.ToString());
        }

        if (Enum.TryParse<JsKey>(rawKey, ignoreCase: true, out var jsKey))
        {
            var name = jsKey.ToString();
            return (name, name, name);
        }

        if (rawKey.Length >= 2 && (rawKey[0] == 'f' || rawKey[0] == 'F') &&
            int.TryParse(rawKey[1..], NumberStyles.None, CultureInfo.InvariantCulture, out var fNum) &&
            fNum is >= 1 and <= 24)
        {
            var fKey = $"F{fNum}";
            return (fKey, fKey, fKey);
        }

        return (rawKey, rawKey, rawKey);
    }

    private static string FormatModifierDisplay(string modifier) => modifier switch
    {
        "Control" => "Ctrl",
        "ControlLeft" => "CtrlLeft",
        "ControlRight" => "CtrlRight",
        _ => modifier
    };
}

/// <summary>
/// Represents a parsed single-step or multi-step chord shortcut sequence.
/// </summary>
public sealed class HotkeySequence
{
    /// <summary>
    /// The ordered sequence of key steps required to trigger the shortcut.
    /// </summary>
    public IReadOnlyList<HotkeyChordStep> Steps { get; }

    /// <summary>
    /// Human-readable representation of the sequence (e.g. <c>"Ctrl+S"</c> or <c>"G then D"</c>).
    /// </summary>
    public string DisplayText { get; }

    /// <summary>
    /// Indicates whether this sequence requires more than one sequential key step.
    /// </summary>
    public bool IsChord => Steps.Count > 1;

    private HotkeySequence(IReadOnlyList<HotkeyChordStep> steps, string displayText)
    {
        Steps = steps;
        DisplayText = displayText;
    }

    /// <summary>
    /// Parses a key combination or chord string such as <c>"Ctrl+S"</c>, <c>"Alt+Shift+F"</c>, <c>"Escape"</c>, or <c>"g then d"</c>.
    /// </summary>
    /// <param name="keyCombination">The combination string to parse.</param>
    /// <returns>A parsed <see cref="HotkeySequence"/>, or an empty sequence when <paramref name="keyCombination"/> is blank.</returns>
    public static HotkeySequence Parse(string? keyCombination)
    {
        if (string.IsNullOrWhiteSpace(keyCombination))
        {
            return new HotkeySequence([], string.Empty);
        }

        var stepTokens = SplitChordSteps(keyCombination);
        if (stepTokens.Count == 0)
        {
            return new HotkeySequence([], string.Empty);
        }

        var steps = stepTokens.Select(HotkeyChordStep.Parse).ToArray();
        var display = string.Join(" then ", steps.Select(s => s.DisplayText));
        return new HotkeySequence(steps, display);
    }

    /// <summary>
    /// Creates a single-step sequence from a <see cref="JsKey"/> and modifier collection.
    /// </summary>
    /// <param name="key">The primary key.</param>
    /// <param name="modifiers">The optional modifiers.</param>
    /// <returns>A single-step <see cref="HotkeySequence"/>.</returns>
    public static HotkeySequence FromKey(JsKey key, IEnumerable<JsKeyModifier>? modifiers = null)
    {
        var step = HotkeyChordStep.FromJsKey(key, modifiers);
        return new HotkeySequence([step], step.DisplayText);
    }

    internal JsKey ResolvePrimaryJsKey(JsKey fallback)
    {
        if (Steps.Count == 0)
        {
            return fallback;
        }

        var lastStep = Steps[^1];
        return Enum.TryParse<JsKey>(lastStep.Code, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }

    internal JsKeyModifier[] ResolveModifiers(IEnumerable<JsKeyModifier>? fallback)
    {
        if (Steps.Count == 0)
        {
            return fallback?.ToArray() ?? [];
        }

        var lastStep = Steps[^1];
        if (lastStep.UseExactSideModifiers)
        {
            return lastStep.Modifiers
                .Select(m => Enum.TryParse<JsKeyModifier>(m, out var parsed) ? (JsKeyModifier?)parsed : null)
                .Where(m => m.HasValue)
                .Select(m => m!.Value)
                .ToArray();
        }

        var list = new List<JsKeyModifier>();
        if (lastStep.RequireCtrl)
        {
            list.Add(JsKeyModifier.ControlLeft);
        }

        if (lastStep.RequireShift)
        {
            list.Add(JsKeyModifier.ShiftLeft);
        }

        if (lastStep.RequireAlt)
        {
            list.Add(JsKeyModifier.AltLeft);
        }

        if (lastStep.RequireMeta)
        {
            list.Add(JsKeyModifier.MetaLeft);
        }

        return list.ToArray();
    }

    private static List<string> SplitChordSteps(string input)
    {
        // Normalize spaces around '+' so "Ctrl + S" stays one step while "g d" or "Ctrl+K Ctrl+S" splits cleanly.
        var normalized = Regex.Replace(input.Trim(), @"\s*\+\s*", "+");
        var segments = Regex.Split(normalized, @"(?:\s+then\s+|\s*,\s*|\s*>\s*|\s+)", RegexOptions.IgnoreCase);
        var result = new List<string>(segments.Length);

        foreach (var segment in segments)
        {
            var trimmed = segment.Trim();
            if (trimmed.Length > 0)
            {
                result.Add(trimmed);
            }
        }

        return result;
    }
}

/// <summary>
/// Represents a registered keyboard shortcut managed by <see cref="HotkeyManager"/>.
/// </summary>
public sealed class HotkeyRegistration
{
    /// <summary>
    /// The unique registration identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The parsed key or chord sequence for this registration.
    /// </summary>
    public HotkeySequence Sequence { get; init; } = HotkeySequence.Parse(null);

    /// <summary>
    /// The container element identifier for scoped shortcuts, or <c>null</c> for global shortcuts.
    /// </summary>
    public string? ScopeId { get; init; }

    /// <summary>
    /// Whether the browser default action should be prevented when matched.
    /// </summary>
    public bool PreventDefault { get; init; } = true;

    /// <summary>
    /// Whether DOM event propagation should be stopped when matched.
    /// </summary>
    public bool StopPropagation { get; init; }

    /// <summary>
    /// Whether the shortcut is suppressed while focus is inside an editable input element.
    /// </summary>
    public bool IgnoreInInputs { get; init; } = true;

    /// <summary>
    /// Whether this shortcut registration is currently disabled.
    /// </summary>
    public bool Disabled { get; init; }

    /// <summary>
    /// Maximum allowed interval between consecutive steps of a chord sequence.
    /// </summary>
    public TimeSpan ChordTimeout { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Optional asynchronous callback invoked when the shortcut sequence completes.
    /// </summary>
    public Func<HotkeyTriggerContext, Task>? Handler { get; init; }
}

/// <summary>
/// Context passed to a <see cref="HotkeyRegistration"/> handler when triggered.
/// </summary>
public sealed record HotkeyTriggerContext(
    HotkeyRegistration Registration,
    KeyboardEventArgs KeyboardEventArgs,
    bool IsChord);

/// <summary>
/// Result returned by <see cref="HotkeyManager.HandleKeyDownAsync"/> describing how an incoming key event was resolved.
/// </summary>
public sealed record HotkeyMatchResult(
    bool Matched,
    bool IsChordPrefix,
    string? PendingChordDisplay,
    HotkeyRegistration? Registration,
    bool PreventDefault,
    bool StopPropagation)
{
    /// <summary>
    /// Represents a key event that did not match any hotkey or chord prefix.
    /// </summary>
    public static HotkeyMatchResult None { get; } = new(false, false, null, null, false, false);
}

/// <summary>
/// Coordinates global and scoped keyboard shortcut registrations, chord key sequences, and input suppression.
/// </summary>
public sealed class HotkeyManager
{
    private const string GlobalScopeKey = "__global__";
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, HotkeyRegistration> _registrations = new(StringComparer.Ordinal);
    private readonly List<string> _registrationOrder = [];
    private readonly Dictionary<string, PendingChordState> _pendingChords = new(StringComparer.Ordinal);

    private sealed class PendingChordState
    {
        public List<KeyboardEventArgs> Events { get; } = [];
        public List<string> DisplayParts { get; } = [];
        public DateTimeOffset LastUpdatedAt { get; set; }
        public TimeSpan Timeout { get; set; }
    }

    public HotkeyManager()
        : this(TimeProvider.System)
    {
    }

    [ActivatorUtilitiesConstructor]
    public HotkeyManager(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>
    /// Raised when chord progress or registration state changes.
    /// </summary>
    public event Action? StateChanged;

    /// <summary>
    /// Registers a new shortcut or updates an existing shortcut with the same <see cref="HotkeyRegistration.Id"/>.
    /// </summary>
    /// <param name="registration">The shortcut registration definition.</param>
    public void RegisterOrUpdate(HotkeyRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.Id);

        if (!_registrations.ContainsKey(registration.Id))
        {
            _registrationOrder.Add(registration.Id);
        }

        _registrations[registration.Id] = registration;
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Unregisters a previously registered shortcut by identifier.
    /// </summary>
    /// <param name="id">The registration identifier.</param>
    /// <returns><c>true</c> if a registration was removed; otherwise, <c>false</c>.</returns>
    public bool Unregister(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !_registrations.Remove(id))
        {
            return false;
        }

        _registrationOrder.Remove(id);
        StateChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Returns all active registrations, optionally filtered to a specific scope identifier.
    /// </summary>
    /// <param name="scopeId">The scope identifier to filter by, or <c>null</c> to return all registrations.</param>
    /// <returns>A read-only collection of matching registrations.</returns>
    public IReadOnlyCollection<HotkeyRegistration> GetRegistrations(string? scopeId = null)
    {
        if (scopeId is null)
        {
            return _registrationOrder
                .Select(id => _registrations[id])
                .ToArray();
        }

        return _registrationOrder
            .Select(id => _registrations[id])
            .Where(r => string.Equals(r.ScopeId, scopeId, StringComparison.Ordinal))
            .ToArray();
    }

    /// <summary>
    /// Returns whether a multi-step chord sequence is currently awaiting its next keystroke.
    /// </summary>
    /// <param name="scopeId">The optional scope identifier, or <c>null</c> for global scope.</param>
    /// <returns><c>true</c> if a non-expired chord prefix is active; otherwise, <c>false</c>.</returns>
    public bool HasPendingChord(string? scopeId = null)
    {
        var scopeKey = NormalizeScopeKey(scopeId);
        if (!_pendingChords.TryGetValue(scopeKey, out var state))
        {
            return false;
        }

        if (IsExpired(state))
        {
            _pendingChords.Remove(scopeKey);
            return false;
        }

        return state.Events.Count > 0;
    }

    /// <summary>
    /// Gets the display text for an active pending chord prefix, or <c>null</c> when idle.
    /// </summary>
    /// <param name="scopeId">The optional scope identifier, or <c>null</c> for global scope.</param>
    /// <returns>The active chord prefix text, or <c>null</c>.</returns>
    public string? GetPendingChordDisplay(string? scopeId = null)
    {
        var scopeKey = NormalizeScopeKey(scopeId);
        if (!HasPendingChord(scopeId))
        {
            return null;
        }

        return string.Join(" then ", _pendingChords[scopeKey].DisplayParts);
    }

    /// <summary>
    /// Clears any pending chord sequence state for the given scope or all scopes when <paramref name="scopeId"/> is <c>null</c>.
    /// </summary>
    /// <param name="scopeId">The optional scope identifier to reset.</param>
    public void ResetChordState(string? scopeId = null)
    {
        if (scopeId is null)
        {
            if (_pendingChords.Count > 0)
            {
                _pendingChords.Clear();
                StateChanged?.Invoke();
            }

            return;
        }

        if (_pendingChords.Remove(NormalizeScopeKey(scopeId)))
        {
            StateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Evaluates a keyboard event against registered global or scoped shortcuts and invokes the matched handler.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <param name="scopeId">The scoped container ID when the event originates inside a scoped container, or <c>null</c> for global evaluation.</param>
    /// <param name="isInputFocused">Whether focus is currently inside an editable text input element.</param>
    /// <returns>A <see cref="HotkeyMatchResult"/> describing the match outcome.</returns>
    public async Task<HotkeyMatchResult> HandleKeyDownAsync(KeyboardEventArgs args, string? scopeId = null, bool isInputFocused = false)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!string.IsNullOrEmpty(scopeId))
        {
            var scopedResult = await EvaluateScopeAsync(args, scopeId, isInputFocused);
            if (scopedResult.Matched || scopedResult.IsChordPrefix)
            {
                return scopedResult;
            }

            if (scopedResult.StopPropagation)
            {
                return scopedResult;
            }
        }

        return await EvaluateScopeAsync(args, targetScopeId: null, isInputFocused);
    }

    private async Task<HotkeyMatchResult> EvaluateScopeAsync(KeyboardEventArgs args, string? targetScopeId, bool isInputFocused)
    {
        var candidates = GetEligibleRegistrations(targetScopeId, isInputFocused);
        if (candidates.Count == 0)
        {
            return HotkeyMatchResult.None;
        }

        var scopeKey = NormalizeScopeKey(targetScopeId);
        if (HotkeyChordStep.IsStandaloneModifierKey(args) &&
            !candidates.Any(r => r.Sequence.Steps.Any(s => s.TargetsModifierKey() && s.Matches(args))))
        {
            return HotkeyMatchResult.None;
        }

        _pendingChords.TryGetValue(scopeKey, out var pending);
        if (pending is not null && IsExpired(pending))
        {
            _pendingChords.Remove(scopeKey);
            pending = null;
        }

        var hadPending = pending is not null && pending.Events.Count > 0;
        var sequenceEvents = hadPending
            ? [.. pending!.Events, args]
            : new List<KeyboardEventArgs> { args };

        var evaluation = MatchCandidates(candidates, sequenceEvents);

        // If a pending chord did not match or extend on the incoming key, drop the stale prefix and retry the single key.
        if (!evaluation.Matched && !evaluation.IsChordPrefix && hadPending)
        {
            _pendingChords.Remove(scopeKey);
            sequenceEvents = [args];
            evaluation = MatchCandidates(candidates, sequenceEvents);
            if (!evaluation.Matched && !evaluation.IsChordPrefix)
            {
                StateChanged?.Invoke();
                return HotkeyMatchResult.None;
            }
        }

        if (evaluation.Matched && evaluation.Registration is not null)
        {
            _pendingChords.Remove(scopeKey);
            StateChanged?.Invoke();

            if (evaluation.Registration.Handler is not null)
            {
                await evaluation.Registration.Handler(new HotkeyTriggerContext(
                    evaluation.Registration,
                    args,
                    evaluation.Registration.Sequence.IsChord));
            }

            return evaluation;
        }

        if (evaluation.IsChordPrefix)
        {
            var nextState = new PendingChordState
            {
                LastUpdatedAt = _timeProvider.GetUtcNow(),
                Timeout = evaluation.Registration?.ChordTimeout ?? TimeSpan.FromSeconds(1)
            };
            nextState.Events.AddRange(sequenceEvents);
            for (var i = 0; i < sequenceEvents.Count; i++)
            {
                nextState.DisplayParts.Add(evaluation.Registration!.Sequence.Steps[i].DisplayText);
            }

            _pendingChords[scopeKey] = nextState;
            StateChanged?.Invoke();

            var display = string.Join(" then ", nextState.DisplayParts);
            return evaluation with { PendingChordDisplay = display };
        }

        return HotkeyMatchResult.None;
    }

    private static HotkeyMatchResult MatchCandidates(List<HotkeyRegistration> candidates, List<KeyboardEventArgs> sequenceEvents)
    {
        var count = sequenceEvents.Count;
        HotkeyRegistration? exactMatch = null;
        HotkeyRegistration? prefixMatch = null;
        var preventDefault = false;
        var stopPropagation = false;

        foreach (var reg in candidates)
        {
            var steps = reg.Sequence.Steps;
            if (steps.Count < count)
            {
                continue;
            }

            var allMatch = true;
            for (var i = 0; i < count; i++)
            {
                if (!steps[i].Matches(sequenceEvents[i]))
                {
                    allMatch = false;
                    break;
                }
            }

            if (!allMatch)
            {
                continue;
            }

            if (steps.Count == count)
            {
                if (exactMatch is null)
                {
                    exactMatch = reg;
                }
            }
            else
            {
                prefixMatch ??= reg;
                preventDefault |= reg.PreventDefault;
                stopPropagation |= reg.StopPropagation;
            }
        }

        if (exactMatch is not null)
        {
            return new HotkeyMatchResult(
                Matched: true,
                IsChordPrefix: false,
                PendingChordDisplay: null,
                Registration: exactMatch,
                PreventDefault: exactMatch.PreventDefault,
                StopPropagation: exactMatch.StopPropagation);
        }

        if (prefixMatch is not null)
        {
            return new HotkeyMatchResult(
                Matched: false,
                IsChordPrefix: true,
                PendingChordDisplay: null,
                Registration: prefixMatch,
                PreventDefault: preventDefault,
                StopPropagation: stopPropagation);
        }

        return HotkeyMatchResult.None;
    }

    private List<HotkeyRegistration> GetEligibleRegistrations(string? targetScopeId, bool isInputFocused)
    {
        var list = new List<HotkeyRegistration>();
        foreach (var id in _registrationOrder)
        {
            var reg = _registrations[id];
            if (reg.Disabled || reg.Sequence.Steps.Count == 0)
            {
                continue;
            }

            if (isInputFocused && reg.IgnoreInInputs)
            {
                continue;
            }

            if (targetScopeId is null)
            {
                if (reg.ScopeId is null)
                {
                    list.Add(reg);
                }
            }
            else if (string.Equals(reg.ScopeId, targetScopeId, StringComparison.Ordinal))
            {
                list.Add(reg);
            }
        }

        return list;
    }

    private bool IsExpired(PendingChordState state)
    {
        var timeout = state.Timeout > TimeSpan.Zero ? state.Timeout : TimeSpan.FromSeconds(1);
        return _timeProvider.GetUtcNow() - state.LastUpdatedAt > timeout;
    }

    private static string NormalizeScopeKey(string? scopeId) =>
        string.IsNullOrEmpty(scopeId) ? GlobalScopeKey : scopeId;
}
