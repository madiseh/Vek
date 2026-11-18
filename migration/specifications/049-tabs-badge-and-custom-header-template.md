# Migration Specification: 049 - Tabs Custom Header Template, Status Badges, and Icon Slots

## 1. Metadata
- **Feature ID**: 049
- **Feature Slug**: tabs-badge-and-custom-header-template
- **Module**: Tabs
- **Status**: Ready
- **Priority**: 49
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Rich tab headers with custom template slots, status notification badges, and leading/trailing icon properties.
- **Functional Requirements**:
  - HeaderTemplate RenderFragment slot per VekTabPanel.
  - BadgeContent and BadgeColor parameters directly on VekTabPanel.
  - Icon and IconPosition (Start, End) parameters on tab panels.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Tabs/VekTabPanel.razor`
  - `src/Vek/Components/Tabs/VekTabPanel.razor.cs`
  - `src/Vek/Styles/components/_tabs.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Tabs/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Tabs/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
