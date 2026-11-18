# Migration Specification: 065 - TreeView Multi-Column TreeList Layout with Resizable Columns

## 1. Metadata
- **Feature ID**: 065
- **Feature Slug**: treeview-multi-column-tree-list-layout
- **Module**: TreeView
- **Status**: Ready
- **Priority**: 65
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Multi-column TreeList combining hierarchical tree expand/collapse in the first column with tabular data columns and resizable headers.
- **Functional Requirements**:
  - VekTreeList component with Column definitions.
  - Hierarchical indent and chevron in primary tree column.
  - Tabular cell values aligned across columns.
  - Header sorting and column resizing.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/TreeView/VekTreeList.razor`
  - `src/Vek/Components/TreeView/VekTreeListColumn.razor`
  - `src/Vek/Styles/components/_treeview.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/TreeView/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/TreeView/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
- **Styling**:
  - Zero hardcoded colors; use Vek design tokens and CSS variables composed via `CssBuilder` and `StyleBuilder`.

---

## 4. Verification & Acceptance Criteria
- **Build**: Compiles cleanly under multi-targeting (`net8.0`, `net9.0`, `net10.0`, `net11.0`) with 0 warnings (`TreatWarningsAsErrors=true`).
- **Testing**: Dedicated bUnit & unit tests verifying happy path, boundary edge cases, and disabled states.
- **Documentation Build**: `dotnet build src/Vek.Docs/Vek.Docs.csproj` succeeds with 0 errors.
- **Formatting**: Passes `dotnet format whitespace` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
