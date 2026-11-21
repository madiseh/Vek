# Migration Specification: 052 - Toolbar Adaptive Layout with Overflow Dropdown Menu

## 1. Metadata
- **Feature ID**: 052
- **Feature Slug**: toolbar-adaptive-collapse-and-overflow-menu
- **Module**: ToolBar
- **Status**: Completed
- **Priority**: 52
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/38
- **Started At**: 2026-11-19T11:38:15Z
- **Completed At**: 2026-11-19T12:16:51Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Adaptive responsive toolbar that automatically moves items overflowing the available container width into an overflow dropdown menu.
- **Functional Requirements**:
  - AdaptiveOverflow boolean parameter.
  - Automatic calculation of container width vs total items width.
  - Overflow button (...) opening popover containing hidden actions.
  - Item CollapsePriority parameter controlling which items collapse first.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/ToolBar/VekToolBar.razor`
  - `src/Vek/Components/ToolBar/VekToolBar.razor.cs`
  - `src/Vek/Styles/components/_toolbar.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/ToolBar/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/ToolBar/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
