// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

import type { DotNet } from "@microsoft/dotnet-js-interop";

type NumericInput = HTMLInputElement & { destroyNumericField?: () => void };

type ActiveHold = {
    timeout: number;
    interval?: number;
    count: number;
    repeated: boolean;
};

/**
 * Adds focused mouse-wheel and press-and-hold interactions to numeric fields.
 */
export default class VekNumericField {
    /**
     * Attaches interactions to the input and its adjacent stepper buttons.
     */
    init(inputId: string, dotNet: DotNet.DotNetObject, allowMouseWheel: boolean) {
        const input = document.getElementById(inputId) as NumericInput | null;
        const root = input?.closest(".vek-input-number-control");
        if (!input || !root) {
            return;
        }

        const buttons = Array.from(root.querySelectorAll<HTMLButtonElement>(".vek-input-numeric-spin button"));
        const holds = new Map<HTMLButtonElement, ActiveHold>();
        const suppressClick = new WeakSet<HTMLButtonElement>();

        const updateBoundaries = () => {
            const value = Number(input.value);
            const min = input.min === "" ? Number.NEGATIVE_INFINITY : Number(input.min);
            const max = input.max === "" ? Number.POSITIVE_INFINITY : Number(input.max);
            const blocked = input.disabled || input.readOnly;

            if (buttons[0]) {
                buttons[0].disabled = blocked || (!Number.isNaN(value) && value >= max);
            }
            if (buttons[1]) {
                buttons[1].disabled = blocked || (!Number.isNaN(value) && value <= min);
            }
        };

        const stopHold = (button: HTMLButtonElement) => {
            const hold = holds.get(button);
            if (!hold) {
                return;
            }
            window.clearTimeout(hold.timeout);
            if (hold.interval !== undefined) {
                window.clearInterval(hold.interval);
            }
            holds.delete(button);
        };

        const startHold = (button: HTMLButtonElement, direction: number, event: PointerEvent) => {
            if (event.button !== 0 || button.disabled || input.disabled || input.readOnly) {
                return;
            }

            const hold: ActiveHold = {
                timeout: window.setTimeout(() => {
                    const repeat = () => {
                        hold.repeated = true;
                        hold.count += 1;
                        const multiplier = hold.count >= 12 ? 10 : hold.count >= 5 ? 5 : 2;
                        void dotNet.invokeMethodAsync("ChangeByInputInteraction", direction, multiplier);
                        const interval = hold.count >= 12 ? 80 : hold.count >= 5 ? 120 : 200;
                        if (hold.interval !== undefined) {
                            window.clearInterval(hold.interval);
                        }
                        hold.interval = window.setInterval(repeat, interval);
                    };
                    repeat();
                }, 400),
                count: 0,
                repeated: false,
            };
            holds.set(button, hold);
        };

        const endAllHolds = () => {
            for (const button of holds.keys()) {
                stopHold(button);
            }
        };

        const wheelHandler = (event: WheelEvent) => {
            if (input.dataset["vekNumericFieldWheel"] !== "true" || document.activeElement !== input || input.disabled || input.readOnly || event.deltaY === 0) {
                return;
            }

            event.preventDefault();
            void dotNet.invokeMethodAsync("ChangeByInputInteraction", event.deltaY < 0 ? 1 : -1);
        };

        const valueHandler = () => updateBoundaries();
        const startHandlers = buttons.map((button, index) => {
            const direction = index === 0 ? 1 : -1;
            const start = (event: PointerEvent) => startHold(button, direction, event);
            const end = () => {
                if (holds.get(button)?.repeated) {
                    suppressClick.add(button);
                }
                stopHold(button);
            };
            const click = (event: MouseEvent) => {
                if (suppressClick.has(button)) {
                    suppressClick.delete(button);
                    event.preventDefault();
                    event.stopImmediatePropagation();
                }
                stopHold(button);
            };

            button.addEventListener("pointerdown", start);
            button.addEventListener("pointerup", end);
            button.addEventListener("pointercancel", end);
            button.addEventListener("click", click, true);
            return { button, start, end, click };
        });

        input.addEventListener("wheel", wheelHandler, { passive: false });
        input.addEventListener("input", valueHandler);
        input.addEventListener("change", valueHandler);
        window.addEventListener("pointerup", endAllHolds);
        window.addEventListener("blur", endAllHolds);
        input.destroyNumericField = () => {
            endAllHolds();
            input.removeEventListener("wheel", wheelHandler);
            input.removeEventListener("input", valueHandler);
            input.removeEventListener("change", valueHandler);
            window.removeEventListener("pointerup", endAllHolds);
            window.removeEventListener("blur", endAllHolds);
            for (const { button, start, end, click } of startHandlers) {
                button.removeEventListener("pointerdown", start);
                button.removeEventListener("pointerup", end);
                button.removeEventListener("pointercancel", end);
                button.removeEventListener("click", click, true);
            }
            delete input.destroyNumericField;
        };
        input.dataset["vekNumericFieldWheel"] = String(allowMouseWheel);
        updateBoundaries();
    }

    /**
     * Updates the enabled state of the wheel listener and boundary controls.
     */
    update(inputId: string, allowMouseWheel: boolean) {
        const input = document.getElementById(inputId) as NumericInput | null;
        if (input) {
            input.dataset["vekNumericFieldWheel"] = String(allowMouseWheel);
            input.dispatchEvent(new Event("change"));
        }
    }

    /**
     * Removes interactions previously attached to the input.
     */
    destroy(inputId: string) {
        const input = document.getElementById(inputId) as NumericInput | null;
        input?.destroyNumericField?.();
    }
}
