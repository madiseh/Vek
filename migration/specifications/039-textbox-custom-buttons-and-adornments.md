# Migration Specification: 039 - TextBox Embedded Custom Action Buttons and Adornment Slots

## 1. Metadata
- **Feature ID**: 039
- **Feature Slug**: textbox-custom-buttons-and-adornments
- **Module**: TextField
- **Status**: Ready
- **Priority**: 39
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Embedded custom action buttons and adornment templates rendered directly inside the input border frame.
- **Functional Requirements**:
  - Buttons RenderFragment slot accepting multiple VekEditorButton instances.
  - Leading and trailing position configuration per button.
  - Seamless click handling without losing focus on the parent text box.
  - Consistent height, padding, and theme token styling.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/TextField/VekEditorButton.cs`
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
- **Formatting**: Passes `dotnet format whitespace` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
