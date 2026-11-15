function getColumnContentWidths(gridElement: HTMLElement): number[] {
    const table = gridElement.querySelector<HTMLTableElement>("table.vek-table-root");
    if (!table) {
        return [];
    }

    const measurementTable = table.cloneNode(true) as HTMLTableElement;
    const measurementHost = document.createElement("div");
    measurementHost.style.position = "absolute";
    measurementHost.style.visibility = "hidden";
    measurementHost.style.left = "-10000px";
    measurementHost.style.top = "0";
    measurementHost.style.width = "max-content";
    measurementHost.append(measurementTable);
    measurementTable.style.width = "max-content";
    measurementTable.style.minWidth = "0";
    measurementTable.style.maxWidth = "none";
    measurementTable.style.tableLayout = "auto";

    for (const cell of measurementTable.querySelectorAll<HTMLElement>("col, th, td")) {
        cell.style.width = "max-content";
        cell.style.minWidth = "0";
        cell.style.maxWidth = "none";
        cell.style.whiteSpace = "nowrap";
        cell.removeAttribute("width");
    }

    gridElement.append(measurementHost);
    try {
        const headerRows = Array.from(measurementTable.querySelectorAll<HTMLTableRowElement>("thead tr"));
        const firstHeaderRow = headerRows[0];
        const widths = Array.from(firstHeaderRow?.cells ?? [])
            .filter((cell) => cell.colSpan === 1)
            .map((cell) => getCellWidth(cell));

        for (const row of headerRows.slice(1)) {
            updateColumnWidths(row, widths);
        }

        for (const row of measurementTable.querySelectorAll<HTMLTableRowElement>("tbody tr, tfoot tr")) {
            updateColumnWidths(row, widths);
        }

        return widths.map((width) => Math.ceil(width));
    } finally {
        measurementHost.remove();
    }
}

function updateColumnWidths(row: HTMLTableRowElement, widths: number[]): void {
    let columnIndex = 0;
    for (const cell of Array.from(row.cells)) {
        if (cell.colSpan !== 1 || cell.getClientRects().length === 0) {
            columnIndex += cell.colSpan;
            continue;
        }

        if (columnIndex < widths.length) {
            widths[columnIndex] = Math.max(widths[columnIndex]!, getCellWidth(cell));
        }

        columnIndex++;
    }
}

function getCellWidth(cell: HTMLTableCellElement): number {
    if (cell.getClientRects().length === 0) {
        return 0;
    }

    const styles = window.getComputedStyle(cell);
    const borders = (Number.parseFloat(styles.borderLeftWidth) || 0) + (Number.parseFloat(styles.borderRightWidth) || 0);
    return cell.scrollWidth + borders;
}

const vekDataGrid = { getColumnContentWidths };

export default vekDataGrid;
