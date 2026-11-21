# Migration Specification: 097 - Stepper Non-Linear Step Completion, Editable Steps, and Error State Badges

## 1. Metadata
- **Feature ID**: 097
- **Feature Slug**: stepper-linear-vs-non-linear-step-modes
- **Module**: Stepper
- **Status**: Ready
- **Priority**: 97
- **Dependencies**: ["095"]

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Non-linear stepper mode allowing users to jump directly to any step, mark steps as completed out of order, or return to edit previous steps.
- **Functional Requirements**:
  - Linear boolean parameter (when false, any step can be clicked).
  - CompletedSteps collection tracking finished steps.
  - Editable boolean parameter on steps allowing revisit after completion.
  - Checkmark icon rendered automatically on completed steps.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Stepper/VekStepper.razor.cs`
  - `src/Vek/Components/Stepper/StepState.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Stepper/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/Stepper/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
