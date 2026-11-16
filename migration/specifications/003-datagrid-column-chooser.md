# Migration Specification: 003 - DataGrid Column Chooser and Visibility Customization Dialog

## 1. Metadata
- **Feature ID**: 003
- **Feature Slug**: datagrid-column-chooser
- **Module**: DataGrid
- **Status**: Ready
- **Priority**: 3
- **Dependencies**: []
- **Started At**: 2026-11-16T09:30:54Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Modal dialog or popup panel allowing users to toggle column visibility and reorder columns dynamically.
- **Functional Requirements**:
  - ShowColumnChooserAsync() and HideColumnChooserAsync() methods.
  - ColumnChooserTemplate for custom customization UI.
  - Column-level AllowHiding toggle to protect fixed columns.
  - Search filter input inside chooser for grids with large column sets.
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Columns/DataGridColumnChooser.razor`
  - `src/Vek/Components/DataGrid/Models/DataGridColumnChooserContext.cs`
  - `src/Vek/Components/DataGrid/VekDataGrid.razor.cs`
  - `src/Vek/Styles/components/_datagrid.scss`
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
- **Formatting**: Passes `dotnet format whitespace` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
