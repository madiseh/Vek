# Migration Specification: 035 - Button Floating Badge, Status Dot, and Counter Adornment

## 1. Metadata
- **Feature ID**: 035
- **Feature Slug**: button-badge-and-counter-adornment
- **Module**: Button
- **Status**: Completed
- **Priority**: 35
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/73
- **Started At**: 2026-11-20T11:25:25Z
- **Completed At**: 2026-11-20T12:28:32Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Direct badge integration on buttons displaying notification numbers, status indicator dots, or badges without outer wrapper components.
- **Functional Requirements**:
  - BadgeContent string/object parameter.
  - BadgeColor and BadgeDot boolean parameters.
  - BadgePosition enum (TopEnd, TopStart, BottomEnd, BottomStart).
  - Zero extra component allocation per button.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Button/VekButton.razor`
  - `src/Vek/Components/Button/VekButton.razor.cs`
  - `src/Vek/Styles/components/_button.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Button/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/Button/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
