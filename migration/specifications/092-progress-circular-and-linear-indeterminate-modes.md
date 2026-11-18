# Migration Specification: 092 - Progress Circular and Linear Percentage Indicators with Animated Buffer Bar

## 1. Metadata
- **Feature ID**: 092
- **Feature Slug**: progress-circular-and-linear-indeterminate-modes
- **Module**: Progress
- **Status**: Ready
- **Priority**: 92
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Circular and linear progress bars supporting determinate percentages, streaming indeterminate animations, and buffer progress bars.
- **Functional Requirements**:
  - Indeterminate boolean parameter.
  - Value double parameter (0 - 100) and BufferValue parameter.
  - Custom size, stroke width, and color tokens.
  - aria-valuenow, aria-valuemin, aria-valuemax attributes.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Progress/VekProgressLinear.razor`
  - `src/Vek/Components/Progress/VekProgressCircular.razor`
  - `src/Vek/Styles/components/_progress.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Progress/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Progress/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
