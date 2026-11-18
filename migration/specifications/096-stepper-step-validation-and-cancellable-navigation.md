# Migration Specification: 096 - Stepper Per-Step Form Validation and Async Cancellable Navigation Guards

## 1. Metadata
- **Feature ID**: 096
- **Feature Slug**: stepper-step-validation-and-cancellable-navigation
- **Module**: Stepper
- **Status**: Ready
- **Priority**: 96
- **Dependencies**: ["095"]

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Asynchronous validation guards on wizard steps preventing progression until the current step's form fields are valid.
- **Functional Requirements**:
  - BeforeStepChange cancellable EventCallback with validation logic.
  - HasError boolean parameter displaying red error icon on step.
  - Prevents jumping forward to incomplete steps in linear mode.
  - Automatic validation trigger on Next button click.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Stepper/VekStep.razor.cs`
  - `src/Vek/Components/Stepper/StepNavigationContext.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Stepper/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Stepper/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
