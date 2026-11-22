# Migration Specification: 063 - TreeView In-Place Node Text Editing and Keyboard Renaming (F2/Enter)

## 1. Metadata
- **Feature ID**: 063
- **Feature Slug**: treeview-node-inline-editing-and-renaming
- **Module**: TreeView
- **Status**: In Progress
- **Priority**: 63
- **Dependencies**: []
- **Started At**: 2026-11-22T07:28:49Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: In-place inline renaming for tree nodes triggered by F2 shortcut or double-click, with validation and save/cancel callbacks.
- **Functional Requirements**:
  - AllowNodeRenaming boolean parameter.
  - F2 key enters inline text editing on focused node.
  - Enter commits rename; Escape cancels editing.
  - OnNodeRenamed EventCallback with validation support.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/TreeView/VekTreeItem.razor`
  - `src/Vek/Components/TreeView/TreeNodeRenameEventArgs.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/TreeView/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/TreeView/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
- **Styling**:
  - Zero hardcoded colors; use Vek design tokens and CSS variables composed via `CssBuilder` and `StyleBuilder`.

---

## 4. Verification & Acceptance Criteria
- **Build**: Compiles cleanly under multi-targeting (`net8.0`, `net9.0`, `net10.0`, `net11.0`) with 0 warnings (`TreatWarningsAsErrors=true`).
- **Testing**: Dedicated bUnit & unit tests verifying happy path, boundary edge cases, and disabled states.
- **Documentation Build**: `dotnet build src/Docs/Vek.Docs/Vek.Docs.csproj` succeeds with 0 errors.
- **Formatting**: Passes `dotnet format` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
