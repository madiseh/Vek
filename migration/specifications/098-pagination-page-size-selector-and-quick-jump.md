# Migration Specification: 098 - Pagination Integrated Page Size Selector Dropdown and Direct Page Jump Input

## 1. Metadata
- **Feature ID**: 098
- **Feature Slug**: pagination-page-size-selector-and-quick-jump
- **Module**: Pagination
- **Status**: Ready
- **Priority**: 98
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Pagination control with integrated page size dropdown ('10, 25, 50, 100'), direct page number jump input, and total item summary text.
- **Functional Requirements**:
  - ShowPageSizeSelector boolean parameter with PageSizeOptions array.
  - ShowDirectPageJump boolean parameter adding numeric input box.
  - TotalItems and ItemRangeText display (e.g. '1-25 of 1,200 items').
  - First, Previous, Next, Last navigation buttons.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Pagination/VekPagination.razor`
  - `src/Vek/Components/Pagination/VekPagination.razor.cs`
  - `src/Vek/Styles/components/_pagination.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Pagination/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Pagination/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
