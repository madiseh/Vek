# Migration Specification: 001 - DataGrid Search Panel with Live Match Highlighting

## 1. Metadata
- **Feature ID**: 001
- **Feature Slug**: datagrid-search-panel
- **Module**: DataGrid
- **Status**: Completed
- **Priority**: 1
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/5
- **Completed At**: 2026-11-15T16:44:46Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Full-text search toolbar with configurable word parsing (GroupWordsByAnd, GroupWordsByOr, ExactMatch) and high-performance DOM mark highlighting.
- **Functional Requirements**:
  - ShowSearchBox toggle property.
  - SearchText two-way binding.
  - SearchTextParseMode (GroupWordsByAnd, GroupWordsByOr, ExactMatch).
  - SearchBoxInputDelay debounce parameter (default: 300ms).
  - Column.AllowSearch participation toggle.
  - HighlightSearchResults live token highlighting.
- **Observable Behavior**:
  - Seamlessly integrates with native VekDataGrid lifecycle and parameter state framework.
  - Preserves clean DOM rendering without unnecessary component allocations.
  - Adheres strictly to W3C ARIA accessibility best practices.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DataGrid/Filtering/DataGridSearchEngine.cs`
  - `src/Vek/Components/DataGrid/Filtering/DataGridSearchTextParseMode.cs`
  - `src/Vek/Components/DataGrid/Models/DataGridSearchBoxContext.cs`
  - `src/Vek/Components/DataGrid/Columns/Column.razor.cs`
  - `src/Vek/Components/DataGrid/VekDataGrid.razor`
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
