type DialogBounds = {
    left: number;
    top: number;
    width: number;
    height: number;
};

type DotNetDialogReference = {
    invokeMethodAsync(method: string, ...args: number[]): Promise<unknown>;
};

type Interaction = {
    dotNetReference: DotNetDialogReference;
    draggable: boolean;
    resizable: boolean;
    activePointer: number | null;
    startX: number;
    startY: number;
    startBounds: DialogBounds | null;
    edge: string | null;
    captureTarget: HTMLElement | null;
    originalCursor: string;
    originalUserSelect: string;
    onPointerDown: (event: PointerEvent) => void;
    onPointerMove: (event: PointerEvent) => void;
    onPointerUp: (event: PointerEvent) => void;
    onKeyDown: (event: KeyboardEvent) => void;
};

const interactions = new WeakMap<HTMLElement, Interaction>();

function clamp(value: number, min: number, max: number) {
    return Math.min(Math.max(value, min), max);
}

function readBounds(dialog: HTMLElement): DialogBounds {
    const rect = dialog.getBoundingClientRect();
    return { left: rect.left, top: rect.top, width: rect.width, height: rect.height };
}

function normalizeBounds(dialog: HTMLElement, bounds: DialogBounds): DialogBounds {
    const margin = 16;
    const viewportWidth = Math.max(1, window.innerWidth);
    const viewportHeight = Math.max(1, window.innerHeight);
    const computed = window.getComputedStyle(dialog);
    const styleMaxWidth = Number.parseFloat(computed.maxWidth);
    const styleMaxHeight = Number.parseFloat(computed.maxHeight);
    const maxWidth = Math.max(1, Math.min(viewportWidth - margin * 2, Number.isFinite(styleMaxWidth) ? styleMaxWidth : viewportWidth - margin * 2));
    const maxHeight = Math.max(1, Math.min(viewportHeight - margin * 2, Number.isFinite(styleMaxHeight) ? styleMaxHeight : viewportHeight - margin * 2));
    const width = clamp(bounds.width, Math.min(280, maxWidth), maxWidth);
    const height = clamp(bounds.height, Math.min(160, maxHeight), maxHeight);

    return {
        left: clamp(bounds.left, margin, Math.max(margin, viewportWidth - width - margin)),
        top: clamp(bounds.top, margin, Math.max(margin, viewportHeight - height - margin)),
        width,
        height,
    };
}

function applyBounds(dialog: HTMLElement, bounds: DialogBounds) {
    const normalized = normalizeBounds(dialog, bounds);
    dialog.style.position = "absolute";
    dialog.style.left = `${normalized.left}px`;
    dialog.style.top = `${normalized.top}px`;
    dialog.style.width = `${normalized.width}px`;
    dialog.style.height = `${normalized.height}px`;
    dialog.style.transform = "none";
    dialog.style.margin = "0";
    return normalized;
}

function resizeBounds(start: DialogBounds, edge: string, deltaX: number, deltaY: number): DialogBounds {
    let { left, top, width, height } = start;
    if (edge.includes("w")) {
        left += deltaX;
        width -= deltaX;
    } else if (edge.includes("e")) {
        width += deltaX;
    }

    if (edge.includes("n")) {
        top += deltaY;
        height -= deltaY;
    } else if (edge.includes("s")) {
        height += deltaY;
    }

    return { left, top, width, height };
}

function resizeCursor(edge: string) {
    if (edge === "n" || edge === "s") return "ns-resize";
    if (edge === "e" || edge === "w") return "ew-resize";
    return edge === "ne" || edge === "sw" ? "nesw-resize" : "nwse-resize";
}

function finish(interaction: Interaction, bounds: DialogBounds) {
    if (interaction.activePointer !== null && interaction.captureTarget) {
        if (interaction.captureTarget.hasPointerCapture?.(interaction.activePointer)) {
            interaction.captureTarget.releasePointerCapture(interaction.activePointer);
        }
    }

    interaction.activePointer = null;
    interaction.startBounds = null;
    interaction.edge = null;
    interaction.captureTarget = null;
    document.body.style.cursor = interaction.originalCursor;
    document.body.style.userSelect = interaction.originalUserSelect;
    void interaction.dotNetReference.invokeMethodAsync("SetDialogBoundsAsync", bounds.left, bounds.top, bounds.width, bounds.height);
}

/** Adds pointer and keyboard movement/resizing to a dialog without creating per-frame component renders. */
export default class VekDialogInteraction {
    /** Starts or updates the interaction listeners for one dialog element. */
    initialize(dialog: HTMLElement, dotNetReference: DotNetDialogReference, draggable: boolean, resizable: boolean, bounds: DialogBounds | null) {
        const existing = interactions.get(dialog);
        if (existing) {
            existing.dotNetReference = dotNetReference;
            existing.draggable = draggable;
            existing.resizable = resizable;
            if (bounds) applyBounds(dialog, bounds);
            return;
        }

        const interaction: Interaction = {
            dotNetReference,
            draggable,
            resizable,
            activePointer: null,
            startX: 0,
            startY: 0,
            startBounds: null,
            edge: null,
            captureTarget: null,
            originalCursor: "",
            originalUserSelect: "",
            onPointerDown: () => { },
            onPointerMove: () => { },
            onPointerUp: () => { },
            onKeyDown: () => { },
        };

        interaction.onPointerDown = (event: PointerEvent) => {
            if (event.button !== 0 || event.isPrimary === false || interaction.activePointer !== null) return;
            const target = event.target instanceof Element ? event.target : null;
            const resizeHandle = target?.closest<HTMLElement>("[data-vek-dialog-resize]");
            const dragHandle = target?.closest<HTMLElement>("[data-vek-dialog-drag-handle]");
            const edge = resizeHandle?.dataset["vekDialogResize"] ?? null;
            const canResize = !!edge && interaction.resizable;
            const canDrag = !!dragHandle && interaction.draggable && !target?.closest("button, a, input, textarea, select, [data-dialog-no-drag]");
            if (!canResize && !canDrag) return;

            event.preventDefault();
            interaction.activePointer = event.pointerId;
            interaction.startX = event.clientX;
            interaction.startY = event.clientY;
            interaction.startBounds = readBounds(dialog);
            interaction.edge = canResize ? edge : null;
            interaction.captureTarget = (canResize ? resizeHandle : dragHandle) ?? null;
            interaction.originalCursor = document.body.style.cursor;
            interaction.originalUserSelect = document.body.style.userSelect;
            document.body.style.cursor = canResize ? resizeCursor(edge!) : "move";
            document.body.style.userSelect = "none";
            (canResize ? resizeHandle : dragHandle)?.setPointerCapture?.(event.pointerId);
            applyBounds(dialog, interaction.startBounds);
        };

        interaction.onPointerMove = (event: PointerEvent) => {
            if (interaction.activePointer !== event.pointerId || !interaction.startBounds) return;
            const deltaX = event.clientX - interaction.startX;
            const deltaY = event.clientY - interaction.startY;
            const candidate = interaction.edge
                ? resizeBounds(interaction.startBounds, interaction.edge, deltaX, deltaY)
                : { ...interaction.startBounds, left: interaction.startBounds.left + deltaX, top: interaction.startBounds.top + deltaY };
            applyBounds(dialog, candidate);
        };

        interaction.onPointerUp = (event: PointerEvent) => {
            if (interaction.activePointer !== event.pointerId) return;
            finish(interaction, readBounds(dialog));
        };

        interaction.onKeyDown = (event: KeyboardEvent) => {
            const target = event.target instanceof Element ? event.target : null;
            const dragHandle = target?.closest<HTMLElement>("[data-vek-dialog-drag-handle]");
            const resizeHandle = target?.closest<HTMLElement>("[data-vek-dialog-resize]");
            const edge = resizeHandle?.dataset["vekDialogResize"];
            if ((!dragHandle || !interaction.draggable) && (!edge || !interaction.resizable)) return;
            if (!["ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown"].includes(event.key)) return;

            event.preventDefault();
            const step = event.shiftKey ? 50 : 10;
            const deltaX = event.key === "ArrowLeft" ? -step : event.key === "ArrowRight" ? step : 0;
            const deltaY = event.key === "ArrowUp" ? -step : event.key === "ArrowDown" ? step : 0;
            const current = readBounds(dialog);
            const candidate = edge ? resizeBounds(current, edge, deltaX, deltaY) : { ...current, left: current.left + deltaX, top: current.top + deltaY };
            const bounds = applyBounds(dialog, candidate);
            void interaction.dotNetReference.invokeMethodAsync("SetDialogBoundsAsync", bounds.left, bounds.top, bounds.width, bounds.height);
        };

        dialog.addEventListener("pointerdown", interaction.onPointerDown);
        dialog.addEventListener("pointermove", interaction.onPointerMove);
        dialog.addEventListener("pointerup", interaction.onPointerUp);
        dialog.addEventListener("pointercancel", interaction.onPointerUp);
        dialog.addEventListener("keydown", interaction.onKeyDown);
        interactions.set(dialog, interaction);
        if (bounds) applyBounds(dialog, bounds);
    }

    /** Removes listeners and restores temporary document interaction styles. */
    destroy(dialog: HTMLElement) {
        const interaction = interactions.get(dialog);
        if (!interaction) return;
        if (interaction.activePointer !== null) {
            document.body.style.cursor = interaction.originalCursor;
            document.body.style.userSelect = interaction.originalUserSelect;
        }
        dialog.removeEventListener("pointerdown", interaction.onPointerDown);
        dialog.removeEventListener("pointermove", interaction.onPointerMove);
        dialog.removeEventListener("pointerup", interaction.onPointerUp);
        dialog.removeEventListener("pointercancel", interaction.onPointerUp);
        dialog.removeEventListener("keydown", interaction.onKeyDown);
        interactions.delete(dialog);
    }
}
