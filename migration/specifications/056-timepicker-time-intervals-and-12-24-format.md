# Migration Specification: 056 - TimePicker Configurable Minute Intervals, Seconds, and 12/24-Hour Formats

## 1. Metadata
- **Feature ID**: 056
- **Feature Slug**: timepicker-time-intervals-and-12-24-format
- **Module**: TimePicker
- **Status**: Completed
- **Priority**: 56
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/61
- **Started At**: 2026-11-19T19:31:19Z
- **Completed At**: 2026-11-20T08:15:12Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: TimePicker with configurable step intervals (15, 30 min), optional seconds column, and AM/PM 12-hour or 24-hour display format.
- **Functional Requirements**:
  - MinuteInterval parameter (1, 5, 10, 15, 30).
  - ShowSeconds boolean parameter adding a third seconds column.
  - TimeFormat enum: TwelveHour (AM/PM) or TwentyFourHour.
  - Scroll list column snapping with smooth touch scrolling.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/TimePicker/VekTimePicker.razor`
  - `src/Vek/Components/TimePicker/VekTimePicker.razor.cs`
  - `src/Vek/Components/TimePicker/TimeEditFormat.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/TimePicker/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/TimePicker/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
