# Migration Specification: 090 - Snackbar Multi-Toast Queue Stacking, Visual Progress Countdown, and Pause on Hover

## 1. Metadata
- **Feature ID**: 090
- **Feature Slug**: snackbar-queue-stacking-and-auto-dismiss-timer
- **Module**: Snackbar
- **Status**: Ready
- **Priority**: 90
- **Dependencies**: ["089"]

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Multi-notification queue stacking (bottom-right, top-center), animated progress bar countdown, and timer pause on mouse hover.
- **Functional Requirements**:
  - MaxDisplayedSnackbars parameter managing toast queue.
  - Animated progress bar displaying remaining duration.
  - Pauses dismiss timer while user hovers mouse over toast.
  - Screen reader live region (aria-live='polite') announcements.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Snackbar/VekSnackbarProvider.razor`
  - `src/Vek/Components/Snackbar/SnackbarOptions.cs`
  - `src/Vek/Styles/components/_snackbar.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Snackbar/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Snackbar/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
