# Migration Specification: 051 - ContextMenu Target Selector Binding, Long-Press Touch, and Submenus

## 1. Metadata
- **Feature ID**: 051
- **Feature Slug**: context-menu-target-selector-and-triggers
- **Module**: Menu
- **Status**: Completed
- **Priority**: 51
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/37
- **Started At**: 2026-11-19T11:27:51Z
- **Completed At**: 2026-11-19T12:16:15Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Context menu component that binds to any DOM target selector or component reference, supporting right-click, touch long-press, and nested submenus.
- **Functional Requirements**:
  - Target selector string or ElementReference binding.
  - Triggers on right-click contextmenu event and mobile long-press.
  - Multi-level hierarchical submenu flyouts.
  - Full keyboard accessibility (Arrow keys, Escape to close, Enter to invoke).
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Menu/VekContextMenu.razor`
  - `src/Vek/Components/Menu/VekContextMenu.razor.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Menu/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/Menu/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
