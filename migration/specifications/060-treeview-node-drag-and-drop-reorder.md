# Migration Specification: 060 - TreeView Interactive Node Drag-and-Drop Reordering and Reparenting

## 1. Metadata
- **Feature ID**: 060
- **Feature Slug**: treeview-node-drag-and-drop-reorder
- **Module**: TreeView
- **Status**: In Progress
- **Priority**: 60
- **Dependencies**: []
- **Started At**: 2026-11-19T19:31:22Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Drag-and-drop tree node management enabling users to reorder sibling nodes or move a node into another branch as a child.
- **Functional Requirements**:
  - AllowDragDrop boolean property.
  - Visual drop position cue: Before, After, or Into (as child).
  - OnNodeDrop EventCallback with source and target node payloads.
  - Drop validation predicate CanDropFunc.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/TreeView/VekTreeView.razor`
  - `src/Vek/Components/TreeView/TreeNodeDragManager.cs`
  - `src/Vek/Components/TreeView/TreeNodeDropEventArgs.cs`
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
- **Formatting**: Passes `dotnet format` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
