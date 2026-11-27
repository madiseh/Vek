// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

import type { DotNet } from "@microsoft/dotnet-js-interop";

export type HotkeyStepSpec = {
    key: string;
    code: string;
    modifiers: string[];
    exactModifiers: boolean;
    requireCtrl: boolean;
    requireShift: boolean;
    requireAlt: boolean;
    requireMeta: boolean;
};

export type HotkeyBindingOptions = {
    steps?: HotkeyStepSpec[] | null;
    scopeElementId?: string | null;
    stopPropagation?: boolean;
    ignoreInInputs?: boolean;
    chordTimeoutMs?: number;
};

type Hotkey = {
    dotnetRef: DotNet.DotNetObject;
    dotnetMethodId: string;
    hotkeyId: string;
    key: string;
    modifiers: Set<string>;
    steps: HotkeyStepSpec[];
    scopeElementId: string | null;
    preventDefault: boolean;
    stopPropagation: boolean;
    ignoreInInputs: boolean;
    chordTimeoutMs: number;
    hasExtendedPayload: boolean;
};

type PendingChordState = {
    events: KeyboardEvent[];
    timestamp: number;
    timeoutMs: number;
};

const SHIFTED_SYMBOL_KEYS = new Set([
    "?", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "{", "}", "|", ":", "\"", "<", ">", "~"
]);

const MODIFIER_KEYS = new Set(["Control", "Shift", "Alt", "Meta"]);
const MODIFIER_CODES = new Set([
    "ControlLeft", "ControlRight", "ShiftLeft", "ShiftRight", "AltLeft", "AltRight", "MetaLeft", "MetaRight"
]);
const NON_TEXT_INPUT_TYPES = new Set(["checkbox", "radio", "button", "submit", "reset", "image", "file"]);

/**
 * Companion interop for the VekHotkey component.
 * Supports global and container-scoped shortcuts, chord key sequences, and input suppression.
 */
export default class VekHotkeyListener {
    _EVENT_TYPE = "keydown";
    _hotkeys = new Map<string, Hotkey>();
    _pendingChords = new Map<string, PendingChordState>();
    _handleKeyEventBound: (e: KeyboardEvent) => void;

    constructor() {
        this._handleKeyEventBound = this._handleKeyEvent.bind(this);
        document.addEventListener(this._EVENT_TYPE, this._handleKeyEventBound as EventListener, true);
    }

    /**
     * Reports that the browser hotkey listener is initialized and active.
     */
    isActive(): boolean {
        return true;
    }

    /**
     * Releases global keyboard listeners.
     */
    dispose() {
        document.removeEventListener(this._EVENT_TYPE, this._handleKeyEventBound as EventListener, true);
        this._pendingChords.clear();
    }

    /**
     * Registers a hotkey, or replaces an existing definition with the same hotkey ID.
     */
    registerOrUpdateHotkey(
        dotnetRef: DotNet.DotNetObject,
        dotnetMethodId: string,
        hotkeyId: string,
        key: string,
        modifiers: string[] | null,
        preventDefault: boolean,
        options: HotkeyBindingOptions | undefined = undefined
    ) {
        modifiers = modifiers || [];
        const newHotkey = this._createHotkey(dotnetRef, dotnetMethodId, hotkeyId, key, modifiers, preventDefault, options);
        this._hotkeys.set(hotkeyId, newHotkey);
    }

    /**
     * Removes a previously registered hotkey by ID.
     */
    unregisterHotkey(hotkeyId: string) {
        if (this._hotkeys.has(hotkeyId)) {
            this._hotkeys.delete(hotkeyId);
        } else {
            console.warn("[Vek] VekHotkey: No matching hotkey found to unregister");
        }
    }

    _createHotkey(
        dotnetRef: DotNet.DotNetObject,
        dotnetMethodId: string,
        hotkeyId: string,
        key: string,
        modifiers: string[],
        preventDefault: boolean,
        options: HotkeyBindingOptions | undefined = undefined
    ): Hotkey {
        const steps: HotkeyStepSpec[] = options?.steps && options.steps.length > 0
            ? options.steps
            : [{
                key: key,
                code: key,
                modifiers: modifiers,
                exactModifiers: true,
                requireCtrl: modifiers.includes("ControlLeft") || modifiers.includes("ControlRight"),
                requireShift: modifiers.includes("ShiftLeft") || modifiers.includes("ShiftRight"),
                requireAlt: modifiers.includes("AltLeft") || modifiers.includes("AltRight"),
                requireMeta: modifiers.includes("MetaLeft") || modifiers.includes("MetaRight")
            }];

        return {
            dotnetRef: dotnetRef,
            dotnetMethodId: dotnetMethodId,
            hotkeyId: hotkeyId,
            key: key,
            modifiers: new Set(modifiers),
            steps: steps,
            scopeElementId: options?.scopeElementId ?? null,
            preventDefault: preventDefault,
            stopPropagation: options?.stopPropagation ?? false,
            ignoreInInputs: options?.ignoreInInputs ?? false,
            chordTimeoutMs: options?.chordTimeoutMs && options.chordTimeoutMs > 0 ? options.chordTimeoutMs : 1000,
            hasExtendedPayload: Boolean(options) && dotnetMethodId !== "VekHotkeyProviderJsCallback"
        };
    }

    _handleKeyEvent(e: KeyboardEvent) {
        const isInputTarget = this._isInputElement(e.target);

        // Collect scoped hotkeys whose container contains the event target first so they take precedence.
        const scopedGroups = new Map<string, Hotkey[]>();
        const globalHotkeys: Hotkey[] = [];

        for (const hotkey of this._hotkeys.values()) {
            if (isInputTarget && hotkey.ignoreInInputs) {
                continue;
            }

            if (hotkey.scopeElementId) {
                const scopeEl = document.getElementById(hotkey.scopeElementId);
                if (scopeEl && e.target instanceof Node && scopeEl.contains(e.target)) {
                    let group = scopedGroups.get(hotkey.scopeElementId);
                    if (!group) {
                        group = [];
                        scopedGroups.set(hotkey.scopeElementId, group);
                    }
                    group.push(hotkey);
                }
            } else {
                globalHotkeys.push(hotkey);
            }
        }

        for (const [scopeKey, candidates] of scopedGroups) {
            const handled = this._evaluateScope(scopeKey, candidates, e);
            if (handled) {
                return;
            }
        }

        if (globalHotkeys.length > 0) {
            this._evaluateScope("__global__", globalHotkeys, e);
        }
    }

    _evaluateScope(scopeKey: string, candidates: Hotkey[], e: KeyboardEvent): boolean {
        if (this._isStandaloneModifier(e) && !this._hasModifierOnlyCandidate(candidates, e)) {
            return false;
        }

        let pending = this._pendingChords.get(scopeKey);
        if (pending && Date.now() - pending.timestamp > pending.timeoutMs) {
            this._pendingChords.delete(scopeKey);
            pending = undefined;
        }

        const hadPending = Boolean(pending && pending.events.length > 0);
        let events = hadPending ? [...pending!.events, e] : [e];
        let result = this._matchCandidates(candidates, events);

        // If a pending chord fails to extend, clear it and try the current key as a fresh step.
        if (!result.matched && !result.prefix && hadPending) {
            this._pendingChords.delete(scopeKey);
            events = [e];
            result = this._matchCandidates(candidates, events);
            if (!result.matched && !result.prefix) {
                return false;
            }
        }

        if (result.matched) {
            this._pendingChords.delete(scopeKey);
            const hotkey = result.matched;

            if (hotkey.preventDefault) {
                e.preventDefault();
            }
            if (hotkey.stopPropagation) {
                e.stopPropagation();
            }

            this._invokeDotNet(hotkey, e);
            return true;
        }

        if (result.prefix) {
            this._pendingChords.set(scopeKey, {
                events: events,
                timestamp: Date.now(),
                timeoutMs: result.prefix.chordTimeoutMs
            });

            if (result.preventDefault) {
                e.preventDefault();
            }
            if (result.stopPropagation) {
                e.stopPropagation();
            }

            return true;
        }

        return false;
    }

    _matchCandidates(candidates: Hotkey[], events: KeyboardEvent[]) {
        const count = events.length;
        let matched: Hotkey | null = null;
        let prefix: Hotkey | null = null;
        let preventDefault = false;
        let stopPropagation = false;

        for (const hotkey of candidates) {
            if (hotkey.steps.length < count) {
                continue;
            }

            let allMatch = true;
            for (let i = 0; i < count; i++) {
                if (!this._matchesStep(events[i]!, hotkey.steps[i]!)) {
                    allMatch = false;
                    break;
                }
            }

            if (!allMatch) {
                continue;
            }

            if (hotkey.steps.length === count) {
                if (!matched) {
                    matched = hotkey;
                }
            } else {
                if (!prefix) {
                    prefix = hotkey;
                }
                preventDefault ||= hotkey.preventDefault;
                stopPropagation ||= hotkey.stopPropagation;
            }
        }

        return { matched, prefix, preventDefault, stopPropagation };
    }

    _matchesStep(e: KeyboardEvent, step: HotkeyStepSpec): boolean {
        if (!this._matchesStepModifiers(e, step)) {
            return false;
        }

        const pressedCode = e.code || "";
        const pressedKey = e.key || "";

        if (pressedCode && step.code && pressedCode.toLowerCase() === step.code.toLowerCase()) {
            return true;
        }

        if (pressedKey && step.key) {
            if (step.key.length === 1 && pressedKey.length === 1) {
                return pressedKey.toLowerCase() === step.key.toLowerCase();
            }

            if (pressedKey.toLowerCase() === step.key.toLowerCase()) {
                return true;
            }
        }

        return (e.code || e.key) === step.code;
    }

    _matchesStepModifiers(e: KeyboardEvent, step: HotkeyStepSpec): boolean {
        if (step.exactModifiers) {
            const pressedModifiers = this._getPressedModifiers(e);
            const required = new Set(step.modifiers);
            return required.size === pressedModifiers.size &&
                [...required].every(m => pressedModifiers.has(m));
        }

        const shiftMatches = e.shiftKey === step.requireShift ||
            (!step.requireShift && SHIFTED_SYMBOL_KEYS.has(step.key));

        return e.ctrlKey === step.requireCtrl &&
            shiftMatches &&
            e.altKey === step.requireAlt &&
            e.metaKey === step.requireMeta;
    }

    _invokeDotNet(hotkey: Hotkey, e: KeyboardEvent) {
        try {
            if (hotkey.hasExtendedPayload) {
                // noinspection JSUnresolvedReference
                hotkey.dotnetRef.invokeMethodAsync(hotkey.dotnetMethodId, {
                    Key: e.key,
                    Code: e.code,
                    CtrlKey: e.ctrlKey,
                    ShiftKey: e.shiftKey,
                    AltKey: e.altKey,
                    MetaKey: e.metaKey,
                    Repeat: e.repeat,
                    IsChord: hotkey.steps.length > 1
                });
            } else {
                // noinspection JSUnresolvedReference
                hotkey.dotnetRef.invokeMethodAsync(hotkey.dotnetMethodId);
            }
        } catch (err) {
            console.error("[Vek] VekHotkey: DotNet invocation failed", {
                key: hotkey.key,
                modifiers: [...hotkey.modifiers],
                err: err
            });
        }
    }

    _isInputElement(target: EventTarget | null): boolean {
        if (!(target instanceof Element)) {
            return false;
        }

        const tagName = target.tagName;
        if (tagName === "INPUT") {
            const inputType = (target as HTMLInputElement).type?.toLowerCase() ?? "text";
            return !NON_TEXT_INPUT_TYPES.has(inputType);
        }

        if (tagName === "TEXTAREA" || tagName === "SELECT") {
            return true;
        }

        return (target as HTMLElement).isContentEditable ||
            target.closest("[contenteditable='true'], [contenteditable='']") !== null;
    }

    _isStandaloneModifier(e: KeyboardEvent): boolean {
        return MODIFIER_KEYS.has(e.key);
    }

    _hasModifierOnlyCandidate(candidates: Hotkey[], e: KeyboardEvent): boolean {
        return candidates.some(hotkey =>
            hotkey.steps.some(step => MODIFIER_CODES.has(step.code) && this._matchesStep(e, step)));
    }

    _getPressedModifiers(e: KeyboardEvent) {
        const pressedModifiers = new Set<string>();

        if (e.ctrlKey) {
            if (e.code === "ControlRight" || e.location === KeyboardEvent.DOM_KEY_LOCATION_RIGHT) {
                pressedModifiers.add("ControlRight");
            } else {
                pressedModifiers.add("ControlLeft");
            }
        }

        if (e.shiftKey) {
            if (e.code === "ShiftRight" || e.location === KeyboardEvent.DOM_KEY_LOCATION_RIGHT) {
                pressedModifiers.add("ShiftRight");
            } else {
                pressedModifiers.add("ShiftLeft");
            }
        }

        if (e.altKey) {
            if (e.code === "AltRight" || e.location === KeyboardEvent.DOM_KEY_LOCATION_RIGHT) {
                pressedModifiers.add("AltRight");
            } else {
                pressedModifiers.add("AltLeft");
            }
        }

        if (e.metaKey) {
            if (e.code === "MetaRight" || e.location === KeyboardEvent.DOM_KEY_LOCATION_RIGHT) {
                pressedModifiers.add("MetaRight");
            } else {
                pressedModifiers.add("MetaLeft");
            }
        }

        return pressedModifiers;
    }
}
