# Migration Specification: 045 - Tabs Dynamic Close Button and Tab Removal Lifecycle

## 1. Metadata
- **Feature ID**: 045
- **Feature Slug**: tabs-closable-tabs-and-lifecycle-events
- **Module**: Tabs
- **Status**: Completed
- **Priority**: 45
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/60
- **Completed At**: 2026-11-20T08:12:36Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Dynamic tab close buttons on tab headers with cancellable closing events and automatic adjacent tab activation.
- **Functional Requirements**:
  - CloseButtonDisplayMode enum: Never, Always, OnHover, ActiveTabOnly.
  - OnTabClosing cancellable EventCallback (allows prompting to save unsaved changes).
  - OnTabClosed EventCallback emitted when tab is removed.
  - Auto-selection of next logical tab when current active tab is closed.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Tabs/VekTabPanel.razor`
  - `src/Vek/Components/Tabs/VekTabPanel.razor.cs`
  - `src/Vek/Components/Tabs/VekTabs.razor.cs`
  - `src/Vek/Components/Tabs/TabClosingEventArgs.cs`
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
