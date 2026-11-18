# Migration Specification: 091 - Skeleton Pre-Configured Shape Cards, Avatars, and Synchronized Shimmer

## 1. Metadata
- **Feature ID**: 091
- **Feature Slug**: skeleton-bone-shapes-and-shimmer-animation
- **Module**: Skeleton
- **Status**: Ready
- **Priority**: 91
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Loading skeleton placeholders with pre-configured bone shapes (Circle, Text, Rectangle, Card) and synchronized wave shimmer.
- **Functional Requirements**:
  - Shape enum: Text, Circle, Rectangle, Card.
  - Animation enum: Shimmer, Pulse, None.
  - Configurable width, height, and border-radius.
  - Accessible aria-busy='true' and aria-label='Loading'.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Skeleton/VekSkeleton.razor`
  - `src/Vek/Components/Skeleton/SkeletonShape.cs`
  - `src/Vek/Styles/components/_skeleton.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Skeleton/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Skeleton/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
