import { expect, test } from "bun:test";
import VekDataGrid from "./vekDataGrid";

test("getColumnContentWidths returns the widest rendered header or cell for each column", () => {
    document.body.innerHTML = `
        <div id="grid">
            <table class="vek-table-root">
                <thead>
                    <tr><th data-test-width="60">Person</th><th data-test-width="72">City</th></tr>
                    <tr><th data-test-width="50">Search</th><th data-test-width="55">Filter</th></tr>
                </thead>
                <tbody>
                    <tr><td data-test-width="100">Alex</td><td data-test-width="90">Oslo</td></tr>
                    <tr><td data-test-width="80">Alexandria</td><td data-test-width="120">Reykjavik</td></tr>
                </tbody>
                <tfoot><tr><td data-test-width="130">Team total</td><td data-test-width="70">12</td></tr></tfoot>
            </table>
        </div>`;

    const scrollWidth = Object.getOwnPropertyDescriptor(HTMLTableCellElement.prototype, "scrollWidth");
    const getClientRects = HTMLTableCellElement.prototype.getClientRects;
    Object.defineProperty(HTMLTableCellElement.prototype, "scrollWidth", {
        configurable: true,
        get(this: HTMLTableCellElement) {
            return Number(this.dataset["testWidth"]);
        },
    });
    HTMLTableCellElement.prototype.getClientRects = () => [new DOMRect()] as unknown as DOMRectList;

    try {
        expect(VekDataGrid.getColumnContentWidths(document.getElementById("grid")!)).toEqual([130, 120]);
    } finally {
        if (scrollWidth) {
            Object.defineProperty(HTMLTableCellElement.prototype, "scrollWidth", scrollWidth);
        }
        HTMLTableCellElement.prototype.getClientRects = getClientRects;
    }
});

test("getColumnContentWidths returns an empty list when the grid table is missing", () => {
    const grid = document.createElement("div");

    expect(VekDataGrid.getColumnContentWidths(grid)).toEqual([]);
});
