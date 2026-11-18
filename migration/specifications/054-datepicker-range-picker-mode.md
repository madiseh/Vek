# Migration Specification: 054 - DatePicker Dual Calendar Range Selection with Preset Ranges

## 1. Metadata
- **Feature ID**: 054
- **Feature Slug**: datepicker-range-picker-mode
- **Module**: DatePicker
- **Status**: Ready
- **Priority**: 54
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Dual-month calendar range picker with live hover highlight between start and end dates and quick-preset ranges (Today, This Week, Last 30 Days).
- **Functional Requirements**:
  - DateRange two-way bindable property (Start, End).
  - Visual continuous highlight range spanning between start and end day cells.
  - PresetRanges collection (e.g. 'Last 7 Days', 'This Month', 'Year to Date').
  - Auto-swap if end date is clicked before start date.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/DatePicker/VekDateRangePicker.razor`
  - `src/Vek/Components/DatePicker/VekDateRangePicker.razor.cs`
  - `src/Vek/Components/DatePicker/DateRangePresets.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/DatePicker/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/DatePicker/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
