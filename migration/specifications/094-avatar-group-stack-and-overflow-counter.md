# Migration Specification: 094 - AvatarGroup Overlapping Facepile Stack with +N Excess Member Counter

## 1. Metadata
- **Feature ID**: 094
- **Feature Slug**: avatar-group-stack-and-overflow-counter
- **Module**: Avatar
- **Status**: Ready
- **Priority**: 94
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: AvatarGroup stacking multiple user avatars into an overlapping facepile, showing a '+N' counter badge for remaining users.
- **Functional Requirements**:
  - Max parameter specifying maximum visible avatars.
  - Automatic '+N' overflow avatar generated for extra users.
  - Spacing parameter controlling overlap offset.
  - Hover elevation raising active avatar above peers.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Avatar/VekAvatarGroup.razor`
  - `src/Vek/Components/Avatar/VekAvatarGroup.razor.cs`
  - `src/Vek/Styles/components/_avatar.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Avatar/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Avatar/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
