# Migration Specification: 002 - DataGrid Built-in Data Export (XLSX and CSV)

## 1. Metadata
- **Feature ID**: 002
- **Feature Slug**: datagrid-data-export
- **Module**: DataGrid
- **Status**: Completed
- **Priority**: 2
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/7
- **Completed At**: 2026-11-15T15:32:07Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Built-in streaming export engine for CSV and OpenXML Excel (.xlsx) formats respecting active sorting, filtering, and column formats.
- **Functional Requirements**:
  - ExportToXlsxAsync(Stream, DataGridExportOptions) and file download overload.
  - ExportToCsvAsync(Stream, DataGridExportOptions) and file download overload.
  - Options: IncludeHeaders, OnlyFilteredItems, ExportHiddenColumns.
  - Column formatting (dates, currencies, numbers) preserved in export.
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Export/DataGridExportOptions.cs`
  - `src/Vek/Components/DataGrid/Export/DataGridCsvExporter.cs`
  - `src/Vek/Components/DataGrid/Export/DataGridXlsxExporter.cs`
  - `src/Vek/Components/DataGrid/VekDataGrid.razor.cs`
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
- **Formatting**: Passes `dotnet format` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
