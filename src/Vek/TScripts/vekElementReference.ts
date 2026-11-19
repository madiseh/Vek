// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

import type { DotNet } from "@microsoft/dotnet-js-interop";

// Legacy Internet Explorer text range API, still feature-detected before the standard selection APIs.
type TextRange = {
    collapse(start: boolean): void;
    moveStart(unit: string, count: number): void;
    moveEnd(unit: string, count: number): void;
    select(): void;
};

// The interop passes arbitrary elements, so the text APIs are optional and feature-detected.
type ReferenceElement = HTMLElement & {
    type?: string;
    selectionStart?: number | null;
    selectionEnd?: number | null;
    setSelectionRange?: (start: number, end: number) => void;
    createTextRange?: () => TextRange;
    select?: () => void;
    // Expandos the helpers store on the element across interop calls.
    vek_savedFocus?: Element | null;
    _vekBlurHandler?: (e: FocusEvent) => void;
};

/**
 * Shared ElementReference interop surface for focus, selection, and DOM metrics.
 * Centralizes browser-specific behavior used by components and extension helpers.
 */
export default class VekElementReference {
    listenerId = 0;
    eventListeners: Record<number, EventListener> = {};
    private contextMenuListeners: Record<number, { cleanup: () => void; setExpanded: (open: boolean) => void }> = {};

    /**
     * Some input types (e.g., "email" and "number") can surface caret APIs but still throw at runtime when selecting ranges.
     */
    _supportsTextSelection(element: ReferenceElement | null | undefined) {
        if (!element) {
            return false;
        }

        const tagName = element.tagName?.toUpperCase();
        if (tagName === 'TEXTAREA') {
            return true;
        }

        if (tagName !== 'INPUT') {
            return typeof element.setSelectionRange === 'function'
                || !!element.createTextRange
                || typeof element.selectionStart === 'number';
        }

        const type = (element.type || 'text').toLowerCase();
        return type === 'text'
            || type === 'search'
            || type === 'tel'
            || type === 'url'
            || type === 'password';
    }

    /**
     * Moves focus to the provided element.
     */
    focus (element: HTMLElement | null | undefined) {
        if (element)
        {
            element.focus();
        }
    }

    /**
     * Removes focus from the provided element.
     */
    blur(element: HTMLElement | null | undefined) {
        if (element) {
            element.blur();
        }
    }

    /**
     * Focuses the first tabbable descendant, with optional skipping.
     * Falls back to the container when not enough tabbable elements exist.
     */
    focusFirst (element: HTMLElement | null | undefined, skip = 0, min = 0) {
        if (element)
        {
            // Looked up on window at call time, so an application that replaces the global still affects focus trapping.
            const tabbables = window.getTabbableElements(element);
            // Fallback avoids trapping focus when a dialog/container has too few tabbable children.
            if (tabbables.length <= min)
                element.focus();
            else
                tabbables[skip]!.focus();
        }
    }

    /**
     * Focuses the last tabbable descendant, with optional reverse skipping.
     * Falls back to the container when not enough tabbable elements exist.
     */
    focusLast (element: HTMLElement | null | undefined, skip = 0, min = 0) {
        if (element)
        {
            const tabbables = window.getTabbableElements(element);
            if (tabbables.length <= min)
                element.focus();
            else
                tabbables[tabbables.length - skip - 1]!.focus();
        }
    }

    /**
     * Stores the currently active element on the container for later restoration.
     */
    saveFocus (element: ReferenceElement | null | undefined) {
        if (element)
        {
            // Store on the container instance so restoration survives intermediate re-renders.
            element['vek_savedFocus'] = document.activeElement;
        }
    }

    /**
     * Restores focus previously captured by saveFocus.
     */
    restoreFocus (element: ReferenceElement | null | undefined) {
        if (element)
        {
            const previous = element['vek_savedFocus'] as HTMLElement | null | undefined;
            delete element['vek_savedFocus'];
            if (previous)
                previous.focus();
        }
    }

    /**
     * Selects a text range and focuses the element.
     * Uses legacy-compatible APIs when needed.
     */
    selectRange(element: ReferenceElement | null | undefined, pos1: number, pos2: number) {
        if (element)
        {
            if (this._supportsTextSelection(element)) {
                if (element.createTextRange) {
                    const selRange = element.createTextRange();
                    selRange.collapse(true);
                    selRange.moveStart('character', pos1);
                    selRange.moveEnd('character', pos2);
                    selRange.select();
                } else if (element.setSelectionRange) {
                    element.setSelectionRange(pos1, pos2);
                } else if (element.selectionStart) {
                    element.selectionStart = pos1;
                    element.selectionEnd = pos2;
                }
            }
            element.focus();
        }
    }

    /**
     * Selects the element's full text content.
     */
    select(element: ReferenceElement | null | undefined) {
        if (element)
        {
            element.select!();
        }
    }

    /**
     * Returns a serializable bounding rectangle enriched with scroll and viewport data.
     */
    getBoundingClientRect(element: Element | null | undefined) {
        if (!element) return;

        const rect = JSON.parse(JSON.stringify(element.getBoundingClientRect()));

        rect.scrollY = window.scrollY || document.documentElement.scrollTop;
        rect.scrollX = window.scrollX || document.documentElement.scrollLeft;

        rect.windowHeight = window.innerHeight;
        rect.windowWidth = window.innerWidth;
        return rect;
    }

    /**
     * Replaces the element className in one operation.
     */
    changeCss (element: Element | null | undefined, css: string) {
        if (element)
        {
            element.className = css;
        }
    }

    /**
     * Removes a tracked event listener by event name and listener ID.
     */
    removeEventListener (element: Element, event: string, eventId: number) {
        element.removeEventListener(event, this.eventListeners[eventId]!);
        delete this.eventListeners[eventId];
    }

    /**
     * Adds an event listener that always prevents default behavior.
     * Returns a generated listener ID for later removal.
     */
    addDefaultPreventingHandler(element: Element, eventName: string) {
        const listener = function (e: Event) {
            // Only prevent default if not already prevented
            if (!e.defaultPrevented) {
                e.preventDefault();
            }
        };

        element.addEventListener(eventName, listener, { passive: false });
        this.eventListeners[++this.listenerId] = listener;
        return this.listenerId;
    }

    /**
     * Removes a default-preventing listener by its generated listener ID.
     */
    removeDefaultPreventingHandler(element: Element, eventName: string, listenerId: number) {
        this.removeEventListener(element, eventName, listenerId);
    }

    /**
     * Adds default-preventing listeners for multiple event names.
     * Returns listener IDs aligned with the provided event list.
     */
    addDefaultPreventingHandlers(element: Element, eventNames: string[]) {
        const listeners: number[] = [];

        for (const eventName of eventNames) {
            const listenerId = this.addDefaultPreventingHandler(element, eventName);
            listeners.push(listenerId);
        }

        return listeners;
    }

    /**
     * Removes default-preventing listeners for multiple event names.
     */
    removeDefaultPreventingHandlers(element: Element, eventNames: string[], listenerIds: number[]) {
        for (let index = 0; index < eventNames.length; ++index) {
            const eventName = eventNames[index]!;
            const listenerId = listenerIds[index]!;
            this.removeDefaultPreventingHandler(element, eventName, listenerId);
        }
    }

    // Some virtual keyboard flows can dismiss focus without Blazor observing the blur.
    // Bridge only those fallback cases and keep normal focus-to-control transitions on Blazor's native path.
    /**
     * Attaches a blur bridge that calls back into .NET.
     * Used as a fallback when blur doesn't move focus to another interactive element.
     */
    addOnBlurEvent(element: ReferenceElement | null | undefined, dotNetReference: DotNet.DotNetObject | null | undefined) {
        if (!element) return;

        // The handler outlives this call, so it keeps a reference to the instance for its cleanup.
        // oxlint-disable-next-line typescript/no-this-alias
        const elementRef = this;
        element._vekBlurHandler = function (e) {
            const relatedTarget = e.relatedTarget as Node | null;

            window.setTimeout(() => {
                if (!element || !document.contains(element)) {
                    // Virtual keyboard flows can blur after disposal; clean up to prevent stale callbacks.
                    elementRef.removeOnBlurEvent(element);
                    return;
                }

                const activeElement = document.activeElement;
                const nextFocusedElement = relatedTarget && document.contains(relatedTarget)
                    ? relatedTarget
                    : activeElement;
                const movedFocusToAnotherElement = !!nextFocusedElement
                    && nextFocusedElement !== element
                    && nextFocusedElement !== document.body
                    && nextFocusedElement !== document.documentElement;

                if (movedFocusToAnotherElement) {
                    return;
                }

                if (dotNetReference) {
                    dotNetReference.invokeMethodAsync('CallOnBlurredAsync').catch(err => {
                        console.warn("Error invoking CallOnBlurredAsync, possibly disposed:", err);
                        elementRef.removeOnBlurEvent(element);
                    });
                } else {
                    console.error("No dotNetReference found for blur fallback");
                }
            }, 0);
        };

        element.addEventListener('blur', element._vekBlurHandler);
    }

    /**
     * Detaches the blur bridge previously installed by addOnBlurEvent.
     */
    removeOnBlurEvent(element: ReferenceElement | null | undefined) {
        if (!element) return;
        if (element._vekBlurHandler) {
            element.removeEventListener('blur', element._vekBlurHandler);
            delete element._vekBlurHandler;
        }
    }

    /**
     * Attaches context-menu, keyboard, and touch long-press listeners to all matching elements.
     */
    addContextMenuListenerForSelector(selector: string, dotNetReference: DotNet.DotNetObject, longPressDelay: number) {
        let elements: Element[];
        try {
            elements = [...document.querySelectorAll(selector)];
        } catch {
            return 0;
        }

        return this._attachContextMenuListeners(elements, dotNetReference, longPressDelay);
    }

    /**
     * Attaches context-menu, keyboard, and touch long-press listeners to an element reference.
     */
    addContextMenuListenerForElement(element: Element | null | undefined, dotNetReference: DotNet.DotNetObject, longPressDelay: number) {
        return this._attachContextMenuListeners(element ? [element] : [], dotNetReference, longPressDelay);
    }

    /**
     * Removes listeners previously attached by one of the addContextMenuListener methods.
     */
    removeContextMenuListener(listenerId: number) {
        this.contextMenuListeners[listenerId]?.cleanup();
        delete this.contextMenuListeners[listenerId];
    }

    /**
     * Updates the expanded state on targets that did not already provide their own state.
     */
    updateContextMenuExpanded(listenerId: number, open: boolean) {
        this.contextMenuListeners[listenerId]?.setExpanded(open);
    }

    private _attachContextMenuListeners(elements: Element[], dotNetReference: DotNet.DotNetObject, longPressDelay: number) {
        if (elements.length === 0) {
            return 0;
        }

        const cleanupFunctions: Array<() => void> = [];
        const expandedStateFunctions: Array<(open: boolean) => void> = [];
        const listenerId = ++this.listenerId;

        for (const element of elements) {
            const originalHasPopup = element.getAttribute('aria-haspopup');
            const originalExpanded = element.getAttribute('aria-expanded');
            if (originalHasPopup === null) {
                element.setAttribute('aria-haspopup', 'menu');
            }
            if (originalExpanded === null) {
                element.setAttribute('aria-expanded', 'false');
            }

            let longPressTimer: number | undefined;
            let ignoreContextMenuUntil = 0;
            let pointerStart: { pointerId: number; x: number; y: number } | undefined;

            const openAt = (pageX: number, pageY: number, clientX: number, clientY: number) => {
                void dotNetReference.invokeMethodAsync('OpenAtAsync', pageX, pageY, clientX, clientY)
                    .catch(() => this.removeContextMenuListener(listenerId));
            };

            const clearLongPress = () => {
                if (longPressTimer !== undefined) {
                    window.clearTimeout(longPressTimer);
                    longPressTimer = undefined;
                }
                pointerStart = undefined;
            };

            const onContextMenu = (event: Event) => {
                event.preventDefault();
                event.stopPropagation();

                if (Date.now() < ignoreContextMenuUntil) {
                    return;
                }

                const mouseEvent = event as MouseEvent;
                openAt(mouseEvent.pageX, mouseEvent.pageY, mouseEvent.clientX, mouseEvent.clientY);
            };

            const onPointerDown = (event: PointerEvent) => {
                if (event.pointerType !== 'touch' || !event.isPrimary || event.button !== 0) {
                    return;
                }

                clearLongPress();
                pointerStart = { pointerId: event.pointerId, x: event.clientX, y: event.clientY };
                longPressTimer = window.setTimeout(() => {
                    longPressTimer = undefined;
                    ignoreContextMenuUntil = Date.now() + 1000;
                    openAt(event.pageX, event.pageY, event.clientX, event.clientY);
                }, Math.max(0, longPressDelay));
            };

            const onPointerMove = (event: PointerEvent) => {
                if (!pointerStart || pointerStart.pointerId !== event.pointerId) {
                    return;
                }

                if (Math.hypot(event.clientX - pointerStart.x, event.clientY - pointerStart.y) > 8) {
                    clearLongPress();
                }
            };

            const onPointerEnd = (event: PointerEvent) => {
                if (!pointerStart || pointerStart.pointerId === event.pointerId) {
                    clearLongPress();
                }
            };

            const onKeyDown = (event: KeyboardEvent) => {
                if (event.key !== 'ContextMenu' && !(event.key === 'F10' && event.shiftKey)) {
                    return;
                }

                event.preventDefault();
                const rect = element.getBoundingClientRect();
                const clientX = rect.left + rect.width / 2;
                const clientY = rect.top + rect.height / 2;
                openAt(clientX + window.scrollX, clientY + window.scrollY, clientX, clientY);
            };

            element.addEventListener('contextmenu', onContextMenu);
            element.addEventListener('pointerdown', onPointerDown as EventListener);
            element.addEventListener('pointermove', onPointerMove as EventListener);
            element.addEventListener('pointerup', onPointerEnd as EventListener);
            element.addEventListener('pointercancel', onPointerEnd as EventListener);
            element.addEventListener('lostpointercapture', onPointerEnd as EventListener);
            element.addEventListener('keydown', onKeyDown as EventListener);

            cleanupFunctions.push(() => {
                clearLongPress();
                element.removeEventListener('contextmenu', onContextMenu);
                element.removeEventListener('pointerdown', onPointerDown as EventListener);
                element.removeEventListener('pointermove', onPointerMove as EventListener);
                element.removeEventListener('pointerup', onPointerEnd as EventListener);
                element.removeEventListener('pointercancel', onPointerEnd as EventListener);
                element.removeEventListener('lostpointercapture', onPointerEnd as EventListener);
                element.removeEventListener('keydown', onKeyDown as EventListener);
                if (originalHasPopup === null) {
                    element.removeAttribute('aria-haspopup');
                }
                if (originalExpanded === null) {
                    element.removeAttribute('aria-expanded');
                }
            });

            if (originalExpanded === null) {
                expandedStateFunctions.push(open => element.setAttribute('aria-expanded', String(open)));
            }
        }

        this.contextMenuListeners[listenerId] = {
            cleanup: () => cleanupFunctions.forEach(cleanup => cleanup()),
            setExpanded: open => expandedStateFunctions.forEach(setExpanded => setExpanded(open))
        };
        return listenerId;
    }
}
