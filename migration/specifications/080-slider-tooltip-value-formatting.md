# Migration Specification: 080 - Slider Floating Value Tooltip with Custom String and Currency Formatter

## 1. Metadata
- **Feature ID**: 080
- **Feature Slug**: slider-tooltip-value-formatting
- **Module**: Slider
- **Status**: Ready
- **Priority**: 80
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Floating tooltip above slider thumb displaying live formatted value during dragging, with custom string formatting and currency options.
- **Functional Requirements**:
  - ValueLabelDisplay enum: Never, OnHover, Always.
  - ValueLabelFormat string parameter (e.g. '{0}%', '{0:C}').
  - Tooltip moves synchronously with thumb position.
  - High-contrast tooltip styling with theme tokens.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Slider/VekSlider.razor`
  - `src/Vek/Components/Slider/SliderTooltipMode.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Slider/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/Slider/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
