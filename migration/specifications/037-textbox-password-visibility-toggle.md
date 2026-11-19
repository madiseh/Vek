# Migration Specification: 037 - TextBox Password Masking with Interactive Visibility Toggle

## 1. Metadata
- **Feature ID**: 037
- **Feature Slug**: textbox-password-visibility-toggle
- **Module**: TextField
- **Status**: Completed
- **Priority**: 37
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/51
- **Started At**: 2026-11-19T19:31:07Z
- **Completed At**: 2026-11-19T20:39:44Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Interactive password visibility toggle button switching input between masked password and plain text with accessible announcements.
- **Functional Requirements**:
  - ShowPasswordToggle boolean property on password inputs.
  - Interactive eye / eye-slash icon button toggling input type.
  - Preserves cursor position and focus during toggle.
  - Dynamic aria-label announcing 'Show password' / 'Hide password'.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/TextField/VekTextField.razor`
  - `src/Vek/Components/TextField/VekTextField.razor.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/TextField/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/TextField/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
