# Migration Specification: 069 - MessageBox Fluent Async ShowConfirmAsync, ShowPromptAsync, and Custom Buttons

## 1. Metadata
- **Feature ID**: 069
- **Feature Slug**: messagebox-fluent-confirm-prompt-api
- **Module**: MessageBox
- **Status**: Completed
- **Priority**: 69
- **Dependencies**: []
- **Pull Request**: https://github.com/madiseh/Vek/pull/114
- **Started At**: 2026-11-23T11:58:05Z
- **Completed At**: 2026-11-23T12:51:16Z

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Fluent C# API for displaying alert, confirm, and text prompt message boxes asynchronously awaiting user response without markup boilerplate.
- **Functional Requirements**:
  - ShowConfirmAsync(title, message) returning Task<bool>.
  - ShowPromptAsync(title, message, defaultValue) returning Task<string?>.
  - Custom button definitions with destructive styling and icon support.
  - Return focus to trigger element upon closing.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/MessageBox/VekMessageBox.razor` and `VekMessageBox.razor.cs`
  - `src/Vek/Services/Dialog/IDialogService.cs` and `DialogService.cs`
  - `src/Vek/Services/Dialog/MessageBoxOptions.cs` and `MessageBoxButtonOptions.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/MessageBox/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Docs/Vek.Docs/Pages/Components/MessageBox/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
