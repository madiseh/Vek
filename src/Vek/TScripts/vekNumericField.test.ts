import { afterEach, expect, jest, mock, test } from "bun:test";
import type { DotNet } from "@microsoft/dotnet-js-interop";
import VekNumericField from "./vekNumericField";

const numericField = new VekNumericField();

function renderField(allowMouseWheel = true) {
    document.body.innerHTML = `
        <div class="vek-input-number-control">
            <input id="quantity" value="1" min="0" max="2">
            <div class="vek-input-numeric-spin">
                <button type="button" aria-label="Increment"></button>
                <button type="button" aria-label="Decrement"></button>
            </div>
        </div>`;
    const input = document.querySelector<HTMLInputElement>("#quantity")!;
    const [increment, decrement] = Array.from(document.querySelectorAll<HTMLButtonElement>("button"));
    const invokeMethodAsync = mock(async (_method: string, _direction: number, _multiplier?: number) => { });
    numericField.init("quantity", { invokeMethodAsync } as unknown as DotNet.DotNetObject, allowMouseWheel);
    return { input, increment: increment!, decrement: decrement!, invokeMethodAsync };
}

afterEach(() => {
    numericField.destroy("quantity");
    document.body.replaceChildren();
    mock.restore();
    jest.useRealTimers();
});

test("wheel changes only the focused input when enabled", () => {
    const { input, invokeMethodAsync } = renderField();
    input.focus();

    const wheel = new WheelEvent("wheel", { deltaY: -100, cancelable: true });
    input.dispatchEvent(wheel);

    expect(wheel.defaultPrevented).toBe(true);
    expect(invokeMethodAsync.mock.calls).toEqual([["ChangeByInputInteraction", 1]]);

    const other = document.createElement("input");
    document.body.append(other);
    other.focus();
    const unfocusedWheel = new WheelEvent("wheel", { deltaY: 100, cancelable: true });
    input.dispatchEvent(unfocusedWheel);

    expect(unfocusedWheel.defaultPrevented).toBe(false);
    expect(invokeMethodAsync).toHaveBeenCalledTimes(1);
});

test("stepper buttons disable at their configured numeric boundaries", () => {
    const { input, increment, decrement } = renderField();

    expect(increment.disabled).toBe(false);
    expect(decrement.disabled).toBe(false);

    input.value = "2";
    input.dispatchEvent(new Event("input"));
    expect(increment.disabled).toBe(true);
    expect(decrement.disabled).toBe(false);

    input.value = "0";
    input.dispatchEvent(new Event("input"));
    expect(increment.disabled).toBe(false);
    expect(decrement.disabled).toBe(true);
});

test("holding a stepper repeats with accelerating increments", () => {
    jest.useFakeTimers();
    const { increment, invokeMethodAsync } = renderField(false);
    increment.dispatchEvent(new PointerEvent("pointerdown", { button: 0, pointerId: 1, bubbles: true }));

    jest.advanceTimersByTime(400);
    jest.advanceTimersByTime(200 * 5);

    expect(invokeMethodAsync.mock.calls.map(([, , multiplier]) => multiplier)).toEqual([2, 2, 2, 2, 5, 5]);

    increment.dispatchEvent(new PointerEvent("pointerup", { button: 0, pointerId: 1, bubbles: true }));
    const click = new MouseEvent("click", { bubbles: true, cancelable: true });
    increment.dispatchEvent(click);
    expect(click.defaultPrevented).toBe(true);
});
