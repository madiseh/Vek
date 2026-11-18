# Migration Specification: 033 - SplitButton with Dual Actions and DropDown Menu

## 1. Metadata
- **Feature ID**: 033
- **Feature Slug**: split-button-and-dropdown-actions
- **Module**: Button
- **Status**: Ready
- **Priority**: 33
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: SplitButton combining a primary action button and an attached dropdown arrow triggering a contextual flyout action menu.
- **Functional Requirements**:
  - Primary action button invoking OnClick.
  - Secondary dropdown trigger button opening flyout popover menu.
  - MenuItems template slot or child item collection.
  - Keyboard navigation (Alt+Down, Enter, Escape, Arrow keys).
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Button/VekSplitButton.razor`
  - `src/Vek/Components/Button/VekSplitButton.razor.cs`
  - `src/Vek/Components/Button/VekDropDownButton.razor`
  - `src/Vek/Styles/components/_button.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Button/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Button/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
