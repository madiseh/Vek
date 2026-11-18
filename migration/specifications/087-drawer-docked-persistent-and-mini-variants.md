# Migration Specification: 087 - Drawer Mini / Rail Variant with Icon-Only Mode and Smooth Width Expansion

## 1. Metadata
- **Feature ID**: 087
- **Feature Slug**: drawer-docked-persistent-and-mini-variants
- **Module**: Drawer
- **Status**: Ready
- **Priority**: 87
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Navigation drawer with Mini Rail variant that collapses to icon-only strip and smoothly expands on hover or toggle.
- **Functional Requirements**:
  - Variant enum: Temporary, Persistent, Mini.
  - MiniWidth parameter (e.g. 56px) showing only icons.
  - ExpandOnHover boolean parameter for mini drawer.
  - Content area automatically offsets width when drawer is docked.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Drawer/VekDrawer.razor`
  - `src/Vek/Components/Drawer/VekDrawer.razor.cs`
  - `src/Vek/Components/Drawer/DrawerVariant.cs`
  - `src/Vek/Styles/components/_drawer.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Drawer/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Drawer/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
