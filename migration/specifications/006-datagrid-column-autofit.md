# Migration Specification: 006 - DataGrid Column Widths Auto-Fit to Content

## 1. Metadata
- **Feature ID**: 006
- **Feature Slug**: datagrid-column-autofit
- **Module**: DataGrid
- **Status**: Ready
- **Discovered At**: 2026-11-15 15:08:53Z
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Automated measurement of cell and header text dimensions to fit column widths dynamically to rendered content.
- **Functional Requirements**:
  - Provides AutoFitColumnWidths() and AutoFitColumnWidths(column) methods using fast DOM client bounding measurements.
  - Full keyboard accessibility and responsive layout support.
  - Observable behavior preservation (matching reference contracts).
  - Clean error handling and graceful degradation.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Location**:
  - `src/Vek/Components/DataGrid/...`
  - Subfolder discipline: Place files strictly into functional subfolders (`Filtering/`, `Models/`, `Cells/`, `Columns/`, `Grouping/`).
- **State Management**:
  - Auto-properties only for public component parameters.
  - Reactive updates managed deterministically via `ParameterState<T>` registered in constructors.
- **Styling**:
  - Zero hardcoded colors; use design tokens and CSS variables.
  - Inline and class composition via `CssBuilder` and `StyleBuilder`.

---

## 4. Verification & Acceptance Criteria
- **Build**: Compiles cleanly with 0 warnings (`TreatWarningsAsErrors=true`).
- **Unit & bUnit Tests**:
  - Behavioral coverage for main flows, edge cases, and disabled states.
  - Multi-targeting verification (`net8.0`, `net9.0`, `net10.0`).
- **Clean-Room Anonymity**:
  - Absolute zero mention of third-party vendor names or commercial brand identifiers in commits, PR, code, or documentation.
- **Pull Request**:
  - Automated PR generated upon quality gate pass with summary and verification evidence.
