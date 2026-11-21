# Migration Specification: 086 - Card Aspect-Ratio Media Box, Header Context Menu, and Action Button Toolbar

## 1. Metadata
- **Feature ID**: 086
- **Feature Slug**: card-header-actions-and-media-ratio
- **Module**: Card
- **Status**: Ready
- **Priority**: 86
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Rich card header with trailing action buttons, contextual dropdown trigger, and 16:9 / 4:3 aspect-ratio media box.
- **Functional Requirements**:
  - CardHeader Actions RenderFragment slot.
  - AspectRatio parameter on VekCardMedia (16:9, 4:3, 1:1, Auto).
  - Title, Subtitle, and Avatar slots in header.
  - Zero extra component allocations.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Card/VekCardHeader.razor`
  - `src/Vek/Components/Card/VekCardMedia.razor`
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
