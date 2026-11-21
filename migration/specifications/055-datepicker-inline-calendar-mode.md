# Migration Specification: 055 - DatePicker Embedded Inline Calendar Display (Non-Popup Mode)

## 1. Metadata
- **Feature ID**: 055
- **Feature Slug**: datepicker-inline-calendar-mode
- **Module**: DatePicker
- **Status**: Completed
- **Priority**: 55
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/80
- **Started At**: 2026-11-20T22:08:58Z
- **Completed At**: 2026-11-20T23:39:01Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Inline permanent calendar widget that renders directly in page layout without dropdown popovers.
- **Functional Requirements**:
  - PickerVariant enum: Popup, Inline.
  - Direct calendar view rendering inside layout without popover overlay.
  - Full month/year header navigation and keyboard accessibility.
  - Multiple selection and week number column display options.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DatePicker/VekCalendar.razor`
  - `src/Vek/Components/DatePicker/VekCalendar.razor.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/DatePicker/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/DatePicker/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
