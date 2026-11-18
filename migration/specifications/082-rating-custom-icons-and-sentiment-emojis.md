# Migration Specification: 082 - Rating Custom SVG Icon Sets, Hearts, and Multi-Sentiment Emojis

## 1. Metadata
- **Feature ID**: 082
- **Feature Slug**: rating-custom-icons-and-sentiment-emojis
- **Module**: Rating
- **Status**: Ready
- **Priority**: 82
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Configurable icon sets for rating control, including hearts, thumbs up/down, and sentiment emojis (angry to happy).
- **Functional Requirements**:
  - FullIcon and EmptyIcon SVG string parameters.
  - ItemHoverColor and ItemActiveColor parameters.
  - ItemTemplate RenderFragment for custom emoji or animated icons.
  - Clearable boolean parameter allowing resetting rating to 0.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Rating/VekRating.razor`
  - `src/Vek/Components/Rating/RatingIconType.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Rating/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Rating/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
