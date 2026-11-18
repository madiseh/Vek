# Migration Specification: 038 - TextBox Configurable Value Binding Modes and Debounce Delay

## 1. Metadata
- **Feature ID**: 038
- **Feature Slug**: textbox-debounced-bind-value-modes
- **Module**: TextField
- **Status**: Ready
- **Priority**: 38
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Fine-grained value binding modes (OnInput, OnChange, OnDelayedInput) with configurable debounce delay for high-performance forms.
- **Functional Requirements**:
  - BindValueMode enum: OnInput, OnChange, OnDelayedInput.
  - InputDelay debounce interval parameter (default: 300ms).
  - Prevents expensive re-renders and model updates on every single keystroke.
  - Immediate commit on Enter key or blur.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/TextField/VekTextField.razor.cs`
  - `src/Vek/Components/Input/VekInputControl.cs`
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
