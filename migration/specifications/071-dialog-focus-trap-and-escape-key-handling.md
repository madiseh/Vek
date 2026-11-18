# Migration Specification: 071 - Dialog W3C Modal Focus Trap and Escape Key Interception

## 1. Metadata
- **Feature ID**: 071
- **Feature Slug**: dialog-focus-trap-and-escape-key-handling
- **Module**: Dialog
- **Status**: Ready
- **Priority**: 71
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Strict W3C modal accessibility with roving focus trap keeping Tab navigation inside the dialog and customizable Escape key behavior.
- **Functional Requirements**:
  - Automatic focus trap constraining Tab / Shift+Tab within active modal.
  - CloseOnEscapeKey boolean parameter.
  - CloseOnBackdropClick boolean parameter.
  - Restores focus to trigger button upon close.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Dialog/VekDialog.razor.cs`
  - `src/Vek/Components/FocusTrap/VekFocusTrap.razor`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Dialog/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Dialog/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
