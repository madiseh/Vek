# Migration Specification: 074 - FileUpload Instant Image Thumbnail Preview and File Type/Size Client Validation

## 1. Metadata
- **Feature ID**: 074
- **Feature Slug**: fileupload-image-preview-and-client-validation
- **Module**: FileUpload
- **Status**: Ready
- **Priority**: 74
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Instant client-side image thumbnail preview generated via object URL before upload, with client-side extension, MIME, and size validation.
- **Functional Requirements**:
  - Instant image thumbnail preview using URL.createObjectURL.
  - AllowedExtensions and AllowedMimeTypes validation arrays.
  - MaxFileSize parameter with localized error messages.
  - Validation errors displayed per file item.
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
  - `src/Vek/Components/FileUpload/VekFileUpload.razor`
  - `src/Vek/Components/FileUpload/FileUploadValidator.cs`
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/FileUpload/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/FileUpload/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
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
