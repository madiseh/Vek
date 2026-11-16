# Migration Specification: 004 - DataGrid Advanced Filter Row with Comparison Operators

## 1. Metadata
- **Feature ID**: 004
- **Feature Slug**: datagrid-advanced-filter-row
- **Module**: DataGrid
- **Status**: Ready
- **Priority**: 4
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Dedicated filter row beneath column headers with operator selectors (Equals, Contains, StartsWith, Between, GreaterThan, LessThan) and debounced inputs.
- **Functional Requirements**:
  - ShowFilterRow boolean toggle parameter.
  - Operators: Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, Between, IsNull, IsNotNull.
  - FilterRowInputDelay debounce (default 300ms).
  - Column.AllowFiltering and Column.FilterOperator configuration.
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Filtering/DataGridFilterRow.razor`
  - `src/Vek/Components/DataGrid/Filtering/DataGridFilterOperator.cs`
  - `src/Vek/Components/DataGrid/Filtering/DataGridFilterCell.razor`
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
- **Formatting**: Passes `dotnet format whitespace` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
