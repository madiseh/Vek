// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

import type { DotNet } from "@microsoft/dotnet-js-interop";

// The cleanup callback lives on the element so destroyPointerEvents can find it from the same .NET element reference.
type ClockElement = HTMLElement & { destroy?: () => void };

/**
 * Pointer interaction model for the analog clock UI in VekTimePicker.
 * Tracks pressed state so drag-over-stick selection remains continuous.
 */
export default class VekTimePicker {
    /**
     * Attaches pointer handlers for clock stick hover/select interactions.
     */
    initPointerEvents(clock: ClockElement, dotNetHelper: DotNet.DotNetObject) {
        let isPointerDown = false;

        const startHandler = (event: PointerEvent) => {
            if (event.button !== 0) {
                // Only handle main (left) pointer button.
                return;
            }

            isPointerDown = true;

            const target = event.target as Element;

            // Allow the pointerover event to trigger.
            target.releasePointerCapture(event.pointerId);

            // Set the selected value to the stick that the pointer went down on.
            if (target.classList.contains('vek-hour') || target.classList.contains('vek-minute')) {
                const attributeValue = target.getAttribute('data-stick-value');
                const stickValue = attributeValue ? Number.parseInt(attributeValue, 10) : -1; // Ensure an integer.

                dotNetHelper.invokeMethodAsync('SelectTimeFromStick', stickValue, false);
            }

            event.preventDefault();
        };

        const endHandler = (event: PointerEvent) => {
            if (event.button !== 0) {
                // Only handle main (left) pointer button.
                return;
            }

            isPointerDown = false;

            const target = event.target as Element;

            if (target.classList.contains('vek-hour') || target.classList.contains('vek-minute')) {
                const attributeValue = target.getAttribute('data-stick-value');
                const stickValue = attributeValue ? Number.parseInt(attributeValue, 10) : -1; // Ensure an integer.

                dotNetHelper.invokeMethodAsync('OnStickClick', stickValue);
            }

            event.preventDefault();
        };

        const moveHandler = (event: PointerEvent) => {
            const target = event.target as Element;

            if (!isPointerDown || (!target.classList.contains('vek-hour') && !target.classList.contains('vek-minute'))) {
                // Only update time from the stick if the pointer is down.
                return;
            }

            const attributeValue = target.getAttribute('data-stick-value');
            const stickValue = attributeValue ? Number.parseInt(attributeValue, 10) : -1; // Ensure an integer.

            dotNetHelper.invokeMethodAsync('SelectTimeFromStick', stickValue, true);

            event.preventDefault();
        };

        clock.addEventListener('pointerdown', startHandler);
        clock.addEventListener('pointerup', endHandler);
        clock.addEventListener('pointercancel', endHandler);
        clock.addEventListener('pointerover', moveHandler);

        clock.destroy = () => {
            clock.removeEventListener('pointerdown', startHandler);
            clock.removeEventListener('pointerup', endHandler);
            clock.removeEventListener('pointercancel', endHandler);
            clock.removeEventListener('pointerover', moveHandler);
        };
    }

    /**
     * Detaches pointer handlers previously registered on the clock container.
     */
    destroyPointerEvents(container: ClockElement | null | undefined) {
        if (container == null) {
            return;
        }
        // Clean up event listeners from the picker element
        if (typeof container.destroy === 'function') {
            container.destroy();
        }
    }
}
