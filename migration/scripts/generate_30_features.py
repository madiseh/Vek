#!/usr/bin/env python3
"""
Generates the 30 Feature Specifications (.md) and populates migration/backlog.json.
Ensures clean-room anonymity, Vek architectural alignment, and deterministic status tracking.
"""

import json
from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parents[2]
MIGRATION_DIR = ROOT_DIR / "migration"
SPECS_DIR = MIGRATION_DIR / "specifications"
BACKLOG_PATH = MIGRATION_DIR / "backlog.json"

SPECS_DIR.mkdir(parents=True, exist_ok=True)

FEATURES = [
    {
        "id": "001",
        "slug": "datagrid-search-panel",
        "title": "DataGrid Search Panel with Live Match Highlighting",
        "module": "DataGrid",
        "priority": 1,
        "status": "completed",
        "pr_url": "https://github.com/madiseh/Vek/pull/5",
        "completed_at": "2026-11-15T16:44:46Z",
        "dependencies": [],
        "summary": "Full-text search toolbar with configurable word parsing (GroupWordsByAnd, GroupWordsByOr, ExactMatch) and high-performance DOM mark highlighting.",
        "target_files": [
            "src/Vek/Components/DataGrid/Filtering/DataGridSearchEngine.cs",
            "src/Vek/Components/DataGrid/Filtering/DataGridSearchTextParseMode.cs",
            "src/Vek/Components/DataGrid/Models/DataGridSearchBoxContext.cs",
            "src/Vek/Components/DataGrid/Columns/Column.razor.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor",
            "src/Vek/Styles/components/_datagrid.scss"
        ],
        "requirements": [
            "ShowSearchBox toggle property.",
            "SearchText two-way binding.",
            "SearchTextParseMode (GroupWordsByAnd, GroupWordsByOr, ExactMatch).",
            "SearchBoxInputDelay debounce parameter (default: 300ms).",
            "Column.AllowSearch participation toggle.",
            "HighlightSearchResults live token highlighting."
        ]
    },
    {
        "id": "002",
        "slug": "datagrid-data-export",
        "title": "DataGrid Built-in Data Export (XLSX and CSV)",
        "module": "DataGrid",
        "priority": 2,
        "status": "completed",
        "pr_url": "https://github.com/madiseh/Vek/pull/7",
        "completed_at": "2026-11-15T15:32:07Z",
        "dependencies": [],
        "summary": "Built-in streaming export engine for CSV and OpenXML Excel (.xlsx) formats respecting active sorting, filtering, and column formats.",
        "target_files": [
            "src/Vek/Components/DataGrid/Export/DataGridExportOptions.cs",
            "src/Vek/Components/DataGrid/Export/DataGridCsvExporter.cs",
            "src/Vek/Components/DataGrid/Export/DataGridXlsxExporter.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "ExportToXlsxAsync(Stream, DataGridExportOptions) and file download overload.",
            "ExportToCsvAsync(Stream, DataGridExportOptions) and file download overload.",
            "Options: IncludeHeaders, OnlyFilteredItems, ExportHiddenColumns.",
            "Column formatting (dates, currencies, numbers) preserved in export."
        ]
    },
    {
        "id": "003",
        "slug": "datagrid-column-chooser",
        "title": "DataGrid Column Chooser and Visibility Customization Dialog",
        "module": "DataGrid",
        "priority": 3,
        "status": "ready",
        "dependencies": [],
        "summary": "Modal dialog or popup panel allowing users to toggle column visibility and reorder columns dynamically.",
        "target_files": [
            "src/Vek/Components/DataGrid/Columns/DataGridColumnChooser.razor",
            "src/Vek/Components/DataGrid/Models/DataGridColumnChooserContext.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs",
            "src/Vek/Styles/components/_datagrid.scss"
        ],
        "requirements": [
            "ShowColumnChooserAsync() and HideColumnChooserAsync() methods.",
            "ColumnChooserTemplate for custom customization UI.",
            "Column-level AllowHiding toggle to protect fixed columns.",
            "Search filter input inside chooser for grids with large column sets."
        ]
    },
    {
        "id": "004",
        "slug": "datagrid-advanced-filter-row",
        "title": "DataGrid Advanced Filter Row with Comparison Operators",
        "module": "DataGrid",
        "priority": 4,
        "status": "ready",
        "dependencies": [],
        "summary": "Dedicated filter row beneath column headers with operator selectors (Equals, Contains, StartsWith, Between, GreaterThan, LessThan) and debounced inputs.",
        "target_files": [
            "src/Vek/Components/DataGrid/Filtering/DataGridFilterRow.razor",
            "src/Vek/Components/DataGrid/Filtering/DataGridFilterOperator.cs",
            "src/Vek/Components/DataGrid/Filtering/DataGridFilterCell.razor",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "ShowFilterRow boolean toggle parameter.",
            "Operators: Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, Between, IsNull, IsNotNull.",
            "FilterRowInputDelay debounce (default 300ms).",
            "Column.AllowFiltering and Column.FilterOperator configuration."
        ]
    },
    {
        "id": "005",
        "slug": "datagrid-fixed-columns",
        "title": "DataGrid Fixed and Pinned Columns with Dynamic Drag Pinning",
        "module": "DataGrid",
        "priority": 5,
        "status": "ready",
        "dependencies": [],
        "summary": "Pin columns to the left or right edge of the viewport with sticky CSS positioning and horizontal scroll boundaries.",
        "target_files": [
            "src/Vek/Components/DataGrid/Columns/DataGridFixedStyle.cs",
            "src/Vek/Components/DataGrid/Columns/Column.razor.cs",
            "src/Vek/Styles/components/_datagrid.scss"
        ],
        "requirements": [
            "FixedStyle enum: None, Left, Right.",
            "Dynamic sticky offset calculation when multiple columns are pinned.",
            "Elevation drop shadow dividing fixed zones from scrollable content.",
            "Context menu pinning and unpinning actions."
        ]
    },
    {
        "id": "006",
        "slug": "datagrid-column-autofit",
        "title": "DataGrid Column Widths Auto-Fit to Content",
        "module": "DataGrid",
        "priority": 6,
        "status": "completed",
        "pr_url": "https://github.com/madiseh/Vek/pull/9",
        "completed_at": "2026-11-15T17:26:17Z",
        "dependencies": [],
        "summary": "Automated measurement of cell and header text dimensions to dynamically fit column widths to rendered content.",
        "target_files": [
            "src/Vek/Components/DataGrid/Columns/DataGridColumnWidthManager.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "AutoFitColumnWidths() and AutoFitColumnWidths(column) public methods.",
            "Fast DOM measurement avoiding full reflow.",
            "Graceful fallback when virtualized rows are enabled."
        ]
    },
    {
        "id": "007",
        "slug": "datagrid-batch-group-expand",
        "title": "DataGrid Batch Group Expand and Collapse Controls",
        "module": "DataGrid",
        "priority": 7,
        "status": "completed",
        "pr_url": "https://github.com/madiseh/Vek/pull/10",
        "completed_at": "2026-11-15T19:25:13Z",
        "dependencies": [],
        "summary": "Batch operations to expand or collapse all grouped hierarchy rows simultaneously across paging boundaries.",
        "target_files": [
            "src/Vek/Components/DataGrid/Grouping/DataGridGroupManager.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "ExpandAllGroupsAsync() expands every active grouping level.",
            "CollapseAllGroupsAsync() collapses every active grouping level.",
            "Preserves grouping definitions and custom group templates."
        ]
    },
    {
        "id": "008",
        "slug": "datagrid-cell-range-selection",
        "title": "DataGrid Cell Range Selection and Multi-Cell Selection",
        "module": "DataGrid",
        "priority": 8,
        "status": "backlog",
        "dependencies": ["009"],
        "summary": "Excel-style rectangular cell range selection with mouse drag marquee and Shift+Arrow navigation.",
        "target_files": [
            "src/Vek/Components/DataGrid/Selection/DataGridCellRange.cs",
            "src/Vek/Components/DataGrid/Selection/DataGridCellSelectionModel.cs",
            "src/Vek/Styles/components/_datagrid.scss"
        ],
        "requirements": [
            "SelectionMode='DataGridSelectionMode.CellRange'.",
            "SelectedCellRange bounding coordinates model.",
            "Shift+Click and Shift+Arrow selection expansion.",
            "Real-time status-bar aggregation (Sum, Average, Count) for selected cells."
        ]
    },
    {
        "id": "009",
        "slug": "datagrid-focused-row",
        "title": "DataGrid Focused Row and Keyboard Active Row Navigation",
        "module": "DataGrid",
        "priority": 9,
        "status": "ready",
        "dependencies": [],
        "summary": "Dedicated focused row state independent of selection, with smooth keyboard traversal and auto-scroll into view.",
        "target_files": [
            "src/Vek/Components/DataGrid/Selection/DataGridFocusManager.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "FocusedRowEnabled boolean parameter.",
            "FocusedRowIndex and FocusedItem two-way binding.",
            "FocusedRowChanged event callback.",
            "ArrowUp, ArrowDown, PageUp, PageDown, Home, End navigation."
        ]
    },
    {
        "id": "010",
        "slug": "datagrid-keyboard-navigation",
        "title": "DataGrid Comprehensive ARIA Keyboard Navigation Matrix",
        "module": "DataGrid",
        "priority": 10,
        "status": "backlog",
        "dependencies": ["009"],
        "summary": "Full W3C WAI-ARIA Grid pattern compliance with roving tabindex and keyboard-driven edit cycles.",
        "target_files": [
            "src/Vek/Components/DataGrid/Cells/DataGridAriaKeyboardHandler.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "Roving tabindex across table cells.",
            "Arrow keys navigate cells; Enter/F2 activates editing; Escape cancels edit; Space toggles selection.",
            "Tab key moves out of grid to adjacent interactive elements."
        ]
    },
    {
        "id": "011",
        "slug": "datagrid-custom-group-interval",
        "title": "DataGrid Custom Group Intervals (Date, Numeric, Alphabetic)",
        "module": "DataGrid",
        "priority": 11,
        "status": "ready",
        "dependencies": [],
        "summary": "Group rows by custom interval buckets (Year, Month, Alphabetical, NumericRange) or custom delegate calculators.",
        "target_files": [
            "src/Vek/Components/DataGrid/Grouping/DataGridGroupInterval.cs",
            "src/Vek/Components/DataGrid/Grouping/DataGridGroupIntervalEvaluator.cs",
            "src/Vek/Components/DataGrid/Columns/Column.razor.cs"
        ],
        "requirements": [
            "GroupInterval enum (DateYear, DateMonth, DateQuarter, Alphabetical, NumericRange).",
            "GroupIntervalCalculator custom delegate Func<object?, object?>.",
            "Formatted group row header display titles."
        ]
    },
    {
        "id": "012",
        "slug": "datagrid-filter-builder-dialog",
        "title": "DataGrid Visual Filter Builder Dialog with Nested Logic",
        "module": "DataGrid",
        "priority": 12,
        "status": "backlog",
        "dependencies": ["004"],
        "summary": "Visual filter builder dialog allowing users to create multi-level boolean expressions with nested AND/OR groups.",
        "target_files": [
            "src/Vek/Components/DataGrid/Filtering/FilterBuilder/VekFilterBuilder.razor",
            "src/Vek/Components/DataGrid/Filtering/FilterBuilder/FilterNode.cs",
            "src/Vek/Components/DataGrid/Filtering/FilterBuilder/FilterGroupNode.cs"
        ],
        "requirements": [
            "ShowFilterBuilderAsync() and HideFilterBuilderAsync() methods.",
            "Dynamic node tree for nested condition groups.",
            "Generates LINQ expressions compatible with VekDataGrid querying."
        ]
    },
    {
        "id": "013",
        "slug": "datagrid-context-menu-actions",
        "title": "DataGrid Context Menu Actions and Right-Click Cell Hooks",
        "module": "DataGrid",
        "priority": 13,
        "status": "ready",
        "dependencies": [],
        "summary": "Contextual right-click menus for headers, data rows, and summary footers with standard data operations.",
        "target_files": [
            "src/Vek/Components/DataGrid/Models/DataGridContextMenuArgs.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "HeaderContextMenuTemplate, RowContextMenuTemplate, CellContextMenuTemplate.",
            "Built-in actions: Copy Cell, Sort, Group By, Filter, AutoFit, Pin.",
            "OnContextMenu event callback with row/column payload and coordinates."
        ]
    },
    {
        "id": "014",
        "slug": "datagrid-column-header-filter-button",
        "title": "DataGrid Column Header Excel-Style Popup Filter Menu",
        "module": "DataGrid",
        "priority": 14,
        "status": "backlog",
        "dependencies": ["004"],
        "summary": "Excel-style header filter popup checklist showing distinct column values with search and date tree-view.",
        "target_files": [
            "src/Vek/Components/DataGrid/Filtering/DataGridHeaderFilterPopup.razor",
            "src/Vek/Components/DataGrid/Filtering/DataGridFilterItem.cs"
        ],
        "requirements": [
            "Header filter button icon in column headers.",
            "Distinct value calculation with fast caching.",
            "Date hierarchy grouping (Year -> Month -> Day) in filter tree."
        ]
    },
    {
        "id": "015",
        "slug": "datagrid-summary-formatting",
        "title": "DataGrid Summary and Aggregate Formatting with Custom Expressions",
        "module": "DataGrid",
        "priority": 15,
        "status": "ready",
        "dependencies": [],
        "summary": "Advanced formatting and custom aggregation functions for total and group summary footers.",
        "target_files": [
            "src/Vek/Components/DataGrid/Models/DataGridSummaryDefinition.cs",
            "src/Vek/Components/DataGrid/Models/DataGridAggregateCalculator.cs"
        ],
        "requirements": [
            "SummaryType enum: Sum, Min, Max, Count, Average, Custom.",
            "CustomSummaryCalculator delegate Func<IEnumerable<T>, object?>.",
            "DisplayFormat pattern string support (e.g. 'Sum: {0:C}')."
        ]
    },
    {
        "id": "016",
        "slug": "datagrid-row-drag-reorder",
        "title": "DataGrid Interactive Row Drag-and-Drop Reordering",
        "module": "DataGrid",
        "priority": 16,
        "status": "ready",
        "dependencies": [],
        "summary": "Drag-and-drop row reordering with visual drop indicators and custom reorder callbacks.",
        "target_files": [
            "src/Vek/Components/DataGrid/Rows/DataGridRowDragManager.cs",
            "src/Vek/Components/DataGrid/Models/DataGridRowReorderedEventArgs.cs"
        ],
        "requirements": [
            "AllowRowReorder boolean property.",
            "Dedicated DataGridDragHandleColumn component.",
            "OnRowReordered callback with source item, target item, and insert position."
        ]
    },
    {
        "id": "017",
        "slug": "datagrid-master-detail-auto-collapse",
        "title": "DataGrid Master-Detail Auto-Collapse and Cascade Expand Control",
        "module": "DataGrid",
        "priority": 17,
        "status": "ready",
        "dependencies": [],
        "summary": "Master-detail row management with auto-collapse mode and programmatic expand/collapse all controls.",
        "target_files": [
            "src/Vek/Components/DataGrid/Rows/DataGridDetailViewManager.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor.cs"
        ],
        "requirements": [
            "AutoCollapseDetailRows parameter.",
            "ExpandAllDetailsAsync() and CollapseAllDetailsAsync() methods.",
            "IsDetailRowExpanded(T item) and SetDetailRowExpandedAsync(T item, bool expanded)."
        ]
    },
    {
        "id": "018",
        "slug": "datagrid-column-virtualization",
        "title": "DataGrid Horizontal Column Virtualization for Ultra-Wide Grids",
        "module": "DataGrid",
        "priority": 18,
        "status": "ready",
        "dependencies": [],
        "summary": "Horizontal column DOM virtualization for wide tables containing 50+ columns.",
        "target_files": [
            "src/Vek/Components/DataGrid/Columns/DataGridColumnVirtualizer.cs",
            "src/Vek/Components/DataGrid/VekDataGrid.razor"
        ],
        "requirements": [
            "VirtualizeColumns boolean parameter.",
            "Spacer elements maintaining total horizontal scroll width.",
            "Configurable overscan column buffer to prevent scroll blanking."
        ]
    },
    {
        "id": "019",
        "slug": "datagrid-inline-create-row",
        "title": "DataGrid Inline Create Row and Top/Bottom Insertion Mode",
        "module": "DataGrid",
        "priority": 19,
        "status": "ready",
        "dependencies": [],
        "summary": "Dedicated inline new row inserted at the top or bottom of the grid with save/cancel lifecycle.",
        "target_files": [
            "src/Vek/Components/DataGrid/Rows/DataGridNewRowItem.razor",
            "src/Vek/Components/DataGrid/Models/DataGridNewRowPosition.cs"
        ],
        "requirements": [
            "NewRowPosition enum: Top, Bottom.",
            "StartNewRowAsync() and CancelNewRowAsync() API.",
            "OnRowCreating and OnRowCreated asynchronous callbacks."
        ]
    },
    {
        "id": "020",
        "slug": "datagrid-state-persistence",
        "title": "DataGrid Layout State Persistence (Save/Load JSON Settings)",
        "module": "DataGrid",
        "priority": 20,
        "status": "backlog",
        "dependencies": ["003", "005"],
        "summary": "Serialize and restore complete grid layout (column widths, order, visibility, sort, filter, page size) as JSON.",
        "target_files": [
            "src/Vek/Components/DataGrid/Models/DataGridPersistentState.cs",
            "src/Vek/Components/DataGrid/Models/DataGridStateSerializer.cs"
        ],
        "requirements": [
            "SaveLayoutStateAsync() returning JSON serialized payload.",
            "LoadLayoutStateAsync(string json) restoring layout and refreshing UI.",
            "Schema versioning handling added/removed columns gracefully."
        ]
    },
    {
        "id": "021",
        "slug": "datagrid-excel-formula-export",
        "title": "DataGrid Excel Formula and Computed Cell Styles Export",
        "module": "DataGrid",
        "priority": 21,
        "status": "backlog",
        "dependencies": ["002"],
        "summary": "Export summary and computed columns as native Excel formulas (=SUM, =AVERAGE) with cell formatting.",
        "target_files": [
            "src/Vek/Components/DataGrid/Export/DataGridXlsxFormulaWriter.cs",
            "src/Vek/Components/DataGrid/Export/DataGridExportOptions.cs"
        ],
        "requirements": [
            "Emits OpenXML formula elements in summary footer cells.",
            "Preserves accounting, currency, date, and percent formatting.",
            "Header fill styles and border styling in exported workbook."
        ]
    },
    {
        "id": "022",
        "slug": "datagrid-validation-summary-integration",
        "title": "DataGrid Built-in Edit Row Validation Summary Integration",
        "module": "DataGrid",
        "priority": 22,
        "status": "backlog",
        "dependencies": ["019"],
        "summary": "Integrated validation summary for row and inline cell editing using Blazor EditContext and DataAnnotations.",
        "target_files": [
            "src/Vek/Components/DataGrid/Validation/DataGridValidationSummary.razor",
            "src/Vek/Components/DataGrid/Validation/DataGridEditContextExtensions.cs"
        ],
        "requirements": [
            "Seamless EditContext integration with DataAnnotationsValidator.",
            "Inline error banner or cell-level validation tooltip.",
            "Prevents row save when validation constraints fail."
        ]
    },
    {
        "id": "023",
        "slug": "datagrid-cell-display-template-selector",
        "title": "DataGrid Conditional Cell Display Template Selector",
        "module": "DataGrid",
        "priority": 23,
        "status": "ready",
        "dependencies": [],
        "summary": "Dynamic cell template selection based on data rules without cluttered markup conditions.",
        "target_files": [
            "src/Vek/Components/DataGrid/Cells/DataGridCellTemplateSelector.cs",
            "src/Vek/Components/DataGrid/Columns/Column.razor.cs"
        ],
        "requirements": [
            "CellDisplayTemplateSelector delegate Func<T, RenderFragment<T>?>.",
            "Rule builder helpers for threshold KPIs (positive/negative badges, warnings).",
            "Optimized re-render minimizing component tree instantiations."
        ]
    },
    {
        "id": "024",
        "slug": "datagrid-sparkline-column",
        "title": "DataGrid Built-in Sparkline and Micro-Chart Column",
        "module": "DataGrid",
        "priority": 24,
        "status": "ready",
        "dependencies": [],
        "summary": "Inline SVG micro-charts (line, area, bar, win/loss) rendered directly inside cells with zero JS charting libraries.",
        "target_files": [
            "src/Vek/Components/DataGrid/Columns/SparklineColumn.razor",
            "src/Vek/Components/DataGrid/Columns/SparklineType.cs",
            "src/Vek/Components/DataGrid/Columns/SparklineSvgRenderer.cs"
        ],
        "requirements": [
            "SparklineColumn<T> bound to IEnumerable<double> values.",
            "Types: Line, Area, Bar, WinLoss.",
            "Marker points for min/max values and responsive SVG rendering."
        ]
    },
    {
        "id": "025",
        "slug": "datagrid-loading-overlay-animation",
        "title": "DataGrid Loading Overlay and Skeleton Rows",
        "module": "DataGrid",
        "priority": 25,
        "status": "ready",
        "dependencies": [],
        "summary": "Polished loading states with skeleton rows, progress indicators, and interaction disabling during async data calls.",
        "target_files": [
            "src/Vek/Components/DataGrid/Rows/DataGridSkeletonRow.razor",
            "src/Vek/Components/DataGrid/Models/DataGridLoadingMode.cs"
        ],
        "requirements": [
            "Loading boolean property.",
            "LoadingMode enum: SkeletonRows, SpinnerOverlay, LinearProgressBar.",
            "Shimmering placeholder rows matching active column dimensions."
        ]
    },
    {
        "id": "026",
        "slug": "datagrid-copy-to-clipboard",
        "title": "DataGrid Native Clipboard Copy (Selected Rows/Cells to TSV/CSV)",
        "module": "DataGrid",
        "priority": 26,
        "status": "backlog",
        "dependencies": ["008"],
        "summary": "Copy selected rows or rectangular cell ranges to clipboard in TSV/CSV format for direct paste into spreadsheet apps.",
        "target_files": [
            "src/Vek/Components/DataGrid/Selection/DataGridClipboardService.cs",
            "src/Vek/Components/DataGrid/Models/DataGridClipboardOptions.cs"
        ],
        "requirements": [
            "Standard keyboard shortcut (Ctrl+C / Cmd+C) integration.",
            "TSV formatting for clean paste into Excel and Google Sheets.",
            "Option to include or exclude column headers."
        ]
    },
    {
        "id": "027",
        "slug": "datagrid-custom-sorting-rules",
        "title": "DataGrid Multi-Column Custom Sorting Rules and Natural Sort",
        "module": "DataGrid",
        "priority": 27,
        "status": "ready",
        "dependencies": [],
        "summary": "Per-column custom comparers, multi-column sort priority, and built-in natural alphanumeric sorting.",
        "target_files": [
            "src/Vek/Components/DataGrid/Sorting/DataGridCustomSortComparer.cs",
            "src/Vek/Components/DataGrid/Columns/Column.razor.cs"
        ],
        "requirements": [
            "SortComparer column property (IComparer<object> or Func<object?, object?, int>).",
            "Built-in NaturalSortComparer ('Item 2' before 'Item 10').",
            "Culture-aware and case-insensitive string collation options."
        ]
    },
    {
        "id": "028",
        "slug": "datagrid-multi-level-footer-aggregates",
        "title": "DataGrid Multi-Level Group Footer and Total Footer Summaries",
        "module": "DataGrid",
        "priority": 28,
        "status": "backlog",
        "dependencies": ["015"],
        "summary": "Multi-tier summary rows showing distinct aggregate calculations at each grouping level and overall footer.",
        "target_files": [
            "src/Vek/Components/DataGrid/Rows/DataGridGroupFooterRow.razor",
            "src/Vek/Components/DataGrid/Rows/DataGridTotalFooterRow.razor"
        ],
        "requirements": [
            "GroupFooterTemplate for rendering group-specific totals.",
            "ShowGroupFooters boolean toggle parameter.",
            "Stacked multi-row total footer summaries."
        ]
    },
    {
        "id": "029",
        "slug": "datagrid-column-band-hierarchy",
        "title": "DataGrid Multi-Row Column Band Hierarchy and Header Groups",
        "module": "DataGrid",
        "priority": 29,
        "status": "ready",
        "dependencies": [],
        "summary": "Group adjacent columns under multi-tier merged column header bands with support for nesting and pinning.",
        "target_files": [
            "src/Vek/Components/DataGrid/Columns/DataGridColumnBand.razor",
            "src/Vek/Components/DataGrid/Columns/DataGridHeaderBandRow.razor"
        ],
        "requirements": [
            "DataGridColumnBand container component.",
            "Automatic colspan and rowspan calculations.",
            "Sticky positioning compatibility with pinned column bands."
        ]
    },
    {
        "id": "030",
        "slug": "datagrid-accessibility-compliance",
        "title": "DataGrid APG Grid Pattern & Screen Reader Live Announcements",
        "module": "DataGrid",
        "priority": 30,
        "status": "backlog",
        "dependencies": ["010"],
        "summary": "Complete W3C WAI-ARIA Authoring Practices Guide compliance with aria-live announcements and high-contrast focus rings.",
        "target_files": [
            "src/Vek/Components/DataGrid/Accessibility/DataGridLiveAnnouncer.razor",
            "src/Vek/Components/DataGrid/VekDataGrid.razor"
        ],
        "requirements": [
            "Live region announcements for sort, filter, page, and selection state changes.",
            "Full semantic roles (role='grid', role='row', role='gridcell', role='columnheader').",
            "WCAG 2.1 AA 3:1 non-text contrast focus indicators."
        ]
    }
]


def generate_spec_content(feat):
    status_title = feat["status"].title()
    pr_line = f"- **Pull Request**: {feat['pr_url']}\n" if feat.get("pr_url") else ""
    completed_line = f"- **Completed At**: {feat['completed_at']}\n" if feat.get("completed_at") else ""
    deps_line = f"- **Dependencies**: {json.dumps(feat.get('dependencies', []))}\n"

    target_files_str = "\n".join(f"  - `{f}`" for f in feat.get("target_files", []))
    reqs_str = "\n".join(f"  - {r}" for r in feat.get("requirements", []))

    content = f"""# Migration Specification: {feat['id']} - {feat['title']}

## 1. Metadata
- **Feature ID**: {feat['id']}
- **Feature Slug**: {feat['slug']}
- **Module**: {feat['module']}
- **Status**: {status_title}
- **Priority**: {feat['priority']}
{deps_line}{pr_line}{completed_line}
---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: {feat['summary']}
- **Functional Requirements**:
{reqs_str}
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
{target_files_str}
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Files placed strictly into designated functional subfolders (`Cells/`, `Columns/`, `Filtering/`, `Grouping/`, `Models/`, `Export/`, `Selection/`, `Rows/`, `Validation/`, `Sorting/`, `Accessibility/`).
  - Root `src/Vek/Components/DataGrid/` reserved strictly for orchestrators (`VekDataGrid.*`, `VekDataGridPager.*`).
- **Styling**:
  - Zero hardcoded colors; built with design tokens and CSS variables via `CssBuilder` and `StyleBuilder`.

---

## 4. Verification & Acceptance Criteria
- **Build**: Compiles cleanly under multi-targeting (`net8.0`, `net9.0`, `net10.0`, `net11.0`) with 0 warnings (`TreatWarningsAsErrors=true`).
- **Testing**: Dedicated bUnit & unit tests verifying happy path, boundary edge cases, and disabled states.
- **Formatting**: Passes `dotnet format --verify-no-changes` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
"""
    return content


def main():
    backlog_features = []

    print(f"Generating 30 Feature Specifications in {SPECS_DIR}...")
    for feat in FEATURES:
        filename = f"{feat['id']}-{feat['slug']}.md"
        spec_path = SPECS_DIR / filename
        spec_rel = f"specifications/{filename}"

        # Write or update specification .md
        content = generate_spec_content(feat)
        spec_path.write_text(content, encoding="utf-8")
        print(f"  [+] {spec_path.name}")

        # Build backlog entry
        b_entry = {
            "id": feat["id"],
            "slug": feat["slug"],
            "title": feat["title"],
            "module": feat["module"],
            "priority": feat["priority"],
            "status": feat["status"],
            "specification": spec_rel,
            "dependencies": feat.get("dependencies", [])
        }
        if feat.get("pr_url"):
            b_entry["pr_url"] = feat["pr_url"]
        if feat.get("completed_at"):
            b_entry["completed_at"] = feat["completed_at"]

        backlog_features.append(b_entry)

    backlog_data = {
        "$schema": "./backlog.schema.json",
        "version": "1.0",
        "pipeline": {
            "max_repair_attempts": 2,
            "default_target_branch": "master",
            "feature_branch_prefix": "feature/migration-"
        },
        "features": backlog_features
    }

    print(f"\nWriting {BACKLOG_PATH} with 30 features...")
    BACKLOG_PATH.write_text(json.dumps(backlog_data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print("Done! Backlog and 30 Feature Specifications synchronized.")


if __name__ == "__main__":
    main()
