# Migration Specification: 011 - DataGrid Custom Group Intervals (Date, Numeric, Alphabetic)

## 1. Metadata
- **Feature ID**: 011
- **Feature Slug**: datagrid-custom-group-interval
- **Module**: DataGrid
- **Status**: Completed
- **Priority**: 11
- **Dependencies**: []
- **Started At**: 2026-11-18T20:39:30Z
- **Completed At**: 2026-11-18T20:59:20Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Group rows by custom interval buckets (Year, Month, Alphabetical, NumericRange) or custom delegate calculators.
- **Functional Requirements**:
  - GroupInterval enum (DateYear, DateMonth, DateQuarter, Alphabetical, NumericRange).
  - GroupIntervalCalculator custom delegate Func<object?, object?>.
  - Formatted group row header display titles.
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Grouping/DataGridGroupInterval.cs`
  - `src/Vek/Components/DataGrid/Grouping/DataGridGroupIntervalEvaluator.cs`
  - `src/Vek/Components/DataGrid/Columns/Column.razor.cs`
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
