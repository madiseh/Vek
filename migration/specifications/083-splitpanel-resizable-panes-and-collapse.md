# Migration Specification: 083 - SplitPanel Resizable Splitter Bar with Double-Click Pane Collapse

## 1. Metadata
- **Feature ID**: 083
- **Feature Slug**: splitpanel-resizable-panes-and-collapse
- **Module**: SplitPanel
- **Status**: In Progress
- **Priority**: 83
- **Dependencies**: []
- **Started At**: 2026-11-19T19:56:48Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: SplitPanel with draggable divider bar, minimum/maximum pane constraints, and double-click to collapse one pane.
- **Functional Requirements**:
  - PaneSize two-way double parameter (percentage or pixels).
  - MinSize and MaxSize constraints for Left/Top pane.
  - Collapsible boolean parameter: double-clicking splitter collapses pane.
  - Keyboard arrow navigation on focused splitter bar.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/SplitPanel/VekSplitPanel.razor`
  - `src/Vek/Components/SplitPanel/VekSplitPanel.razor.cs`
  - `src/Vek/Styles/components/_splitpanel.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/SplitPanel/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/SplitPanel/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
