import { afterEach, beforeEach, expect, test } from "bun:test";
import VekDialogInteraction from "./vekDialogInteraction";

const originalWidth = window.innerWidth;
const originalHeight = window.innerHeight;
const dialogs: HTMLElement[] = [];
const instances: VekDialogInteraction[] = [];

function createDialog() {
    const dialog = document.createElement("div");
    dialog.innerHTML = `
        <div data-vek-dialog-drag-handle="true"><button type="button" class="close">Close</button></div>
        <button type="button" data-vek-dialog-resize="e" aria-label="Resize dialog from right edge"></button>`;
    document.body.append(dialog);
    const initial = { left: 100, top: 120, width: 300, height: 220 };
    dialog.getBoundingClientRect = () => {
        const read = (property: "left" | "top" | "width" | "height", fallback: number) => {
            const value = Number.parseFloat(dialog.style[property]);
            return Number.isFinite(value) ? value : fallback;
        };
        const left = read("left", initial.left);
        const top = read("top", initial.top);
        const width = read("width", initial.width);
        const height = read("height", initial.height);
        return { left, top, width, height, right: left + width, bottom: top + height, x: left, y: top, toJSON: () => ({}) } as DOMRect;
    };
    dialogs.push(dialog);
    return dialog;
}

function pointer(type: string, pointerId: number, clientX: number, clientY: number) {
    const event = new Event(type, { bubbles: true, cancelable: true });
    Object.defineProperties(event, {
        button: { value: 0 },
        isPrimary: { value: true },
        pointerId: { value: pointerId },
        clientX: { value: clientX },
        clientY: { value: clientY },
    });
    return event as PointerEvent;
}

function attach(dialog: HTMLElement) {
    const calls: number[][] = [];
    const dotNetReference = {
        invokeMethodAsync: async (_method: string, ...args: number[]) => calls.push(args),
    };
    const interaction = new VekDialogInteraction();
    interaction.initialize(dialog, dotNetReference, true, true, null);
    instances.push(interaction);
    return calls;
}

beforeEach(() => {
    Object.defineProperty(window, "innerWidth", { configurable: true, value: 800 });
    Object.defineProperty(window, "innerHeight", { configurable: true, value: 600 });
    document.body.style.cursor = "";
    document.body.style.userSelect = "";
});

afterEach(() => {
    for (const interaction of instances.splice(0)) {
        for (const dialog of dialogs) interaction.destroy(dialog);
    }
    for (const dialog of dialogs.splice(0)) dialog.remove();
    Object.defineProperty(window, "innerWidth", { configurable: true, value: originalWidth });
    Object.defineProperty(window, "innerHeight", { configurable: true, value: originalHeight });
});

test("pointer movement stays within the viewport and reports final bounds", () => {
    const dialog = createDialog();
    const calls = attach(dialog);

    dialog.querySelector(".close")!.dispatchEvent(pointer("pointerdown", 1, 100, 120));
    dialog.dispatchEvent(pointer("pointermove", 1, 1000, 120));
    dialog.dispatchEvent(pointer("pointerup", 1, 1000, 120));
    expect(dialog.style.left).toBe("");
    expect(calls).toHaveLength(0);

    dialog.querySelector("[data-vek-dialog-drag-handle]")!.dispatchEvent(pointer("pointerdown", 2, 100, 120));
    dialog.dispatchEvent(pointer("pointermove", 2, 1000, 120));
    dialog.dispatchEvent(pointer("pointerup", 2, 1000, 120));

    expect(dialog.style.left).toBe("484px");
    expect(dialog.style.top).toBe("120px");
    expect(calls).toHaveLength(1);
    expect(calls[0]).toEqual([484, 120, 300, 220]);
    expect(document.body.style.cursor).toBe("");
    expect(document.body.style.userSelect).toBe("");
});

test("arrow keys move the dialog and resize it from a focused edge handle", () => {
    const dialog = createDialog();
    const calls = attach(dialog);

    dialog.querySelector("[data-vek-dialog-drag-handle]")!.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true, cancelable: true }));
    expect(dialog.style.left).toBe("110px");

    dialog.querySelector("[data-vek-dialog-resize='e']")!.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true, cancelable: true }));
    expect(dialog.style.width).toBe("310px");
    expect(calls).toHaveLength(2);
    expect(calls[0]).toEqual([110, 120, 300, 220]);
    expect(calls[1]).toEqual([110, 120, 310, 220]);
});
