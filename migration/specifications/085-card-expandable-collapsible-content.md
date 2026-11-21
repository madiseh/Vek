# Migration Specification: 085 - Card Collapsible Body with Animated Toggle Action in Header

## 1. Metadata
- **Feature ID**: 085
- **Feature Slug**: card-expandable-collapsible-content
- **Module**: Card
- **Status**: Completed
- **Priority**: 85
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/52
- **Started At**: 2026-11-19T19:56:49Z
- **Completed At**: 2026-11-19T21:09:23Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: VekCard with built-in collapse/expand toggle in header, smooth height animation, and collapsed state binding.
- **Functional Requirements**:
  - Collapsible boolean parameter.
  - Expanded two-way boolean parameter with OnExpandedChanged event.
  - Header toggle chevron button with aria-expanded attribute.
  - Smooth CSS height transition without content overflow glitches.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Card/VekCard.razor`
  - `src/Vek/Components/Card/VekCard.razor.cs`
  - `src/Vek/Styles/components/_card.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Card/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/Card/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
