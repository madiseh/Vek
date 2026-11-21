# Migration Specification: 089 - Snackbar Interactive Action Buttons with Async Undo Callback

## 1. Metadata
- **Feature ID**: 089
- **Feature Slug**: snackbar-action-button-and-undo-callback
- **Module**: Snackbar
- **Status**: Completed
- **Priority**: 89
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/56
- **Started At**: 2026-11-19T19:56:58Z
- **Completed At**: 2026-11-19T21:44:36Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Toast notification with interactive action button (e.g. 'Undo', 'Retry') executing asynchronous callback before toast dismisses.
- **Functional Requirements**:
  - ActionText string and ActionCallback Func<Task> parameters.
  - Clicking action button pauses auto-dismiss and executes callback.
  - CloseButton boolean parameter for manual dismissal.
  - Severity color tokens (Success, Info, Warning, Error).
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Snackbar/VekSnackbar.razor`
  - `src/Vek/Components/Snackbar/SnackbarMessage.cs`
  - `src/Vek/Components/Snackbar/ISnackbarService.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Snackbar/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/Snackbar/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
