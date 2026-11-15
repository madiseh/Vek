// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/**
 * Generic browser/window operations exposed to the JS API service.
 * Centralizes direct browser API dependencies behind one interop surface.
 */
export default class VekWindow {
    /**
     * Copies text to the system clipboard.
     */
    copyToClipboard (text: string) {
        navigator.clipboard.writeText(text);
    }

    /**
     * Replaces an element className by element ID.
     */
    changeCssById (id: string, css: string) {
        const element = document.getElementById(id);
        if (element) {
            element.className = css;
        }
    }

    /**
     * Updates a CSS style property for an element by ID.
     * The C# side declares the value as object, and the only caller passes an int, so a number arrives here as a number.
     */
    updateStyleProperty (elementId: string, propertyName: string, value: string | number) {
        const element = document.getElementById(elementId);
        if (element) {
            element.style.setProperty(propertyName, String(value));
        }
    }

    /**
     * Updates a CSS variable on the document root.
     */
    changeGlobalCssVariable (name: string, newValue: string) {
        document.documentElement.style.setProperty(name, newValue);
    }

    /**
     * Opens a new browser window/tab with the provided URL.
     */
    open (url: string) {
        window.open(url);
    }

    /**
     * Starts a named browser download from a .NET stream reference.
     */
    async downloadFileFromStream (fileName: string, contentType: string, streamReference: { arrayBuffer: () => Promise<ArrayBuffer> }): Promise<void> {
        const buffer = await streamReference.arrayBuffer();
        const blob = new Blob([buffer], { type: contentType });
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        document.body.append(anchor);
        anchor.click();
        anchor.remove();
        window.setTimeout(() => URL.revokeObjectURL(url), 0);
    }
}
