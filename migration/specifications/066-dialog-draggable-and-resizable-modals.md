# Migration Specification: 066 - Dialog Draggable Header, Viewport Clamping, and Resizable Edges

## 1. Metadata
- **Feature ID**: 066
- **Feature Slug**: dialog-draggable-and-resizable-modals
- **Module**: Dialog
- **Status**: Ready
- **Priority**: 66
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Draggable modal dialog header enabling free repositioning, boundary clamping within viewport, and 8-direction edge/corner resizing.
- **Functional Requirements**:
  - Draggable boolean parameter allowing dragging by header bar.
  - Resizable boolean parameter adding resizable grab handles.
  - Viewport boundary clamp preventing dialog from getting dragged offscreen.
  - Preserves position on state re-render.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/Dialog/VekDialog.razor`
  - `src/Vek/Components/Dialog/VekDialog.razor.cs`
  - `src/Vek/Styles/components/_dialog.scss`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/Dialog/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/Dialog/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
