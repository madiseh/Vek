# Migration Specification: 068 - Dialog Header Maximize / Restore Toggle and Fullscreen Mode

## 1. Metadata
- **Feature ID**: 068
- **Feature Slug**: dialog-fullscreen-toggle-and-maximize
- **Module**: Dialog
- **Status**: Ready
- **Priority**: 68
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Window management buttons in dialog header for Maximize / Restore to full viewport dimensions, and Fullscreen toggle.
- **Functional Requirements**:
  - ShowMaximizeButton boolean parameter in header.
  - Maximize / Restore button toggling 100vw/100vh fullscreen layout.
  - Double-clicking header bar toggles maximize.
  - Restores previous width, height, and coordinates on restore.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Dialog/VekDialog.razor`
  - `src/Vek/Components/Dialog/VekDialog.razor.cs`
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
