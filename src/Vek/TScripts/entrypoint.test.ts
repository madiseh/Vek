import { test, expect, describe } from "bun:test";
import "./entrypoint";

test("window is defined", () => {
    expect(window).toBeDefined();
});

// Every global exposed on `window` by the legacy side-effect scripts imported above.
// When a script is ported to TypeScript, keep (or update) its entry here so a missing
// export is caught immediately instead of surfacing as a runtime interop failure.
const expectedWindowGlobals: [name: string, type: "object" | "function"][] = [
    // vekDataGridExport.ts
    ["vekDataGridExport", "object"],
    // vekDragAndDrop.ts
    ["vekDragAndDrop", "object"],
    // vekElementReference.ts
    ["vekElementRef", "object"],
    // vekExitPrompt.ts
    ["vekExitPrompt", "object"],
    // vekFileUpload.ts
    ["vekFileUpload", "object"],
    // vekHelpers.ts
    ["getTabbableElements", "function"],
    ["serializeParameter", "function"],
    ["vekGetSvgBBox", "function"],
    ["hasDefinedParentHeight", "function"],
    ["vekObserveElementSize", "function"],
    // vekHotkeyListener.ts
    ["vekHotkeyListener", "object"],
    // vekInput.ts
    ["vekInput", "object"],
    // vekInputSizing.ts
    ["vekInputSizing", "object"],
    // vekJsEvent.ts
    ["vekJsEvent", "object"],
    // vekKeyInterceptor.ts
    ["vekKeyInterceptor", "object"],
    // vekPointerEventsNone.ts
    ["vekPointerEventsNone", "object"],
    // vekPointerCapture.ts
    ["vekPointerCapture", "object"],
    // vekPopover.ts
    ["vekpopoverHelper", "object"],
    ["vekPopover", "object"],
    // vekResizeListener.ts
    ["vekResizeListener", "object"],
    ["vekResizeListenerFactory", "object"],
    // vekResizeObserver.ts
    ["vekResizeObserver", "object"],
    // vekScrollListener.ts
    ["vekScrollListener", "object"],
    // vekScrollManager.ts
    ["vekScrollManager", "object"],
    // vekScrollSpy.ts
    ["vekScrollSpy", "object"],
    // vekSplitPanel.ts
    ["vekSplitPanel", "function"],
    ["splitPanels", "object"],
    ["vekSplitPanel_update", "function"],
    ["vekSplitPanel_resetDividerPosition", "function"],
    ["vekSplitPanel_getDividerPosition", "function"],
    ["vekSplitPanel_setDividerPosition", "function"],
    ["vekSplitPanel_destroy", "function"],
    // vekTableCell.ts
    ["vekTableCell", "object"],
    // vekThemeProvider.ts
    ["vekThemeProvider", "object"],
    // vekTimePicker.ts
    ["vekTimePicker", "object"],
    // VekWindow.ts
    ["vekWindow", "object"],
];

describe("legacy script globals", () => {
    test.each(expectedWindowGlobals)("window.%s is a %s", (name, type) => {
        const globalValue = (window as unknown as Record<string, unknown>)[name];
        expect(globalValue).not.toBeNull();
        expect(typeof globalValue).toBe(type);
    });
});

// Scripts without a window global register document listeners from entrypoint.ts instead.
test("ripple handlers are registered", () => {
    document.body.innerHTML = `<button class="vek-ripple"></button>`;
    const button = document.querySelector("button")!;

    button.dispatchEvent(new PointerEvent("pointerdown", { button: 0, pointerId: 1, bubbles: true }));

    expect(button.querySelector(".vek-ripple-effect")).not.toBeNull();
    document.body.replaceChildren();
});
