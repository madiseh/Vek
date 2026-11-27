# Migration Specification: 017 - DataGrid Master-Detail Auto-Collapse and Cascade Expand Control

## 1. Metadata
- **Feature ID**: 017
- **Feature Slug**: datagrid-master-detail-auto-collapse
- **Module**: DataGrid
- **Status**: Completed
- **Priority**: 17
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/183
- **Started At**: 2026-11-27T22:24:53Z
- **Completed At**: 2026-11-27T23:19:07Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Master-detail row management with auto-collapse mode and programmatic expand/collapse all controls.
- **Functional Requirements**:
  - AutoCollapseDetailRows parameter.
  - ExpandAllDetailsAsync() and CollapseAllDetailsAsync() methods.
  - IsDetailRowExpanded(T item) and SetDetailRowExpandedAsync(T item, bool expanded).
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Rows/DataGridDetailViewManager.cs`
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
