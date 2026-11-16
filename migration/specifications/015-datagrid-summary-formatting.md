# Migration Specification: 015 - DataGrid Summary and Aggregate Formatting with Custom Expressions

## 1. Metadata
- **Feature ID**: 015
- **Feature Slug**: datagrid-summary-formatting
- **Module**: DataGrid
- **Status**: Ready
- **Priority**: 15
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Advanced formatting and custom aggregation functions for total and group summary footers.
- **Functional Requirements**:
  - SummaryType enum: Sum, Min, Max, Count, Average, Custom.
  - CustomSummaryCalculator delegate Func<IEnumerable<T>, object?>.
  - DisplayFormat pattern string support (e.g. 'Sum: {0:C}').
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Models/DataGridSummaryDefinition.cs`
  - `src/Vek/Components/DataGrid/Models/DataGridAggregateCalculator.cs`
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
