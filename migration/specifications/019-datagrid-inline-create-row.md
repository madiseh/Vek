# Migration Specification: 019 - DataGrid Inline Create Row and Top/Bottom Insertion Mode

## 1. Metadata
- **Feature ID**: 019
- **Feature Slug**: datagrid-inline-create-row
- **Module**: DataGrid
- **Status**: In Progress
- **Priority**: 19
- **Dependencies**: []
- **Started At**: 2026-11-27T23:06:19Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Dedicated inline new row inserted at the top or bottom of the grid with save/cancel lifecycle.
- **Functional Requirements**:
  - NewRowPosition enum: Top, Bottom.
  - StartNewRowAsync() and CancelNewRowAsync() API.
  - OnRowCreating and OnRowCreated asynchronous callbacks.
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Rows/DataGridNewRowItem.razor`
  - `src/Vek/Components/DataGrid/Models/DataGridNewRowPosition.cs`
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
