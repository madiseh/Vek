# Migration Specification: 002 - DataGrid Built-in Data Export (XLSX and CSV)

## 1. Metadata
- **Feature ID**: 002
- **Feature Slug**: datagrid-data-export
- **Module**: DataGrid
- **Status**: Ready
- **Dependencies**: None

---

## 2. Behavioral Scope & Requirements
- **Export Capabilities**:
  - `public Task ExportToXlsxAsync(Stream stream, DataGridExportOptions? options = null)`: Exports current grid view to Excel format.
  - `public Task ExportToCsvAsync(Stream stream, DataGridExportOptions? options = null)`: Exports current grid view to comma-separated values format.
  - Browser direct-download overloads:
    - `public Task ExportToXlsxAsync(string fileName, DataGridExportOptions? options = null)`
    - `public Task ExportToCsvAsync(string fileName, DataGridExportOptions? options = null)`
- **Export Options**:
  - `IncludeHeaders`: Boolean (default: true).
  - `OnlyFilteredItems`: Boolean (default: true - exports current filtered/sorted dataset rather than raw items).
  - `ExportHiddenColumns`: Boolean (default: false).
  - `CustomFormatters`: Optional column-level cell value format mapping.
- **Observable Behavior**:
  - Column headers respect `column.Title` or property name fallback.
  - Formatted columns format according to `column.ContentFormat` and `column.Culture`.
  - Hidden columns (`column.Hidden == true`) are excluded by default.
  - Large datasets stream efficiently without full in-memory DOM rendering.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Location**:
  - `src/Vek/Components/DataGrid/Export/DataGridExportOptions.cs`
  - `src/Vek/Components/DataGrid/Export/DataGridCsvExporter.cs`
  - `src/Vek/Components/DataGrid/Export/DataGridXlsxExporter.cs`
  - `src/Vek/Components/DataGrid/VekDataGrid.razor.cs` (Export API extension)
- **Dependencies**: Use lightweight streaming OpenXML / ZIP packaging or native CSV formatting to prevent heavy bloated packages.
- **State & Subfolders**: Place all export models, formatters, and stream writers under `src/Vek/Components/DataGrid/Export/` per subfolder discipline.

---

## 4. Verification & Acceptance Criteria
- **Build**: Compiles under `net8.0`, `net9.0`, and `net10.0` with 0 warnings.
- **Unit Tests**:
  - CSV export tests verifying quoting, commas, delimiters, headers, and formatted dates/numbers.
  - Filtered items export test: verifying export matches active search/filter results.
  - Column exclusion test: verifying hidden columns are omitted when `ExportHiddenColumns = false`.
- **Anonymity**: Zero vendor brand traces in commits, PR, code, or documentation.
- **Pull Request**: Automated PR created with test execution results and example snippet.
