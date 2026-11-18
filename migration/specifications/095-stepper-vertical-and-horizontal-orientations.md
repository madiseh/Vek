# Migration Specification: 095 - Stepper Horizontal and Vertical Orientation with Responsive Step Collapse

## 1. Metadata
- **Feature ID**: 095
- **Feature Slug**: stepper-vertical-and-horizontal-orientations
- **Module**: Stepper
- **Status**: Ready
- **Priority**: 95
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Multi-step wizard stepper supporting horizontal and vertical orientations with responsive collapse of step titles on narrow screens.
- **Functional Requirements**:
  - Orientation enum: Horizontal, Vertical.
  - ActiveIndex two-way binding with NextStepAsync() and PreviousStepAsync() API.
  - Step icons, titles, and optional subtitles.
  - Responsive icon-only mode when viewport width is restricted.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Stepper/VekStepper.razor`
  - `src/Vek/Components/Stepper/VekStep.razor`
  - `src/Vek/Styles/components/_stepper.scss`
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
