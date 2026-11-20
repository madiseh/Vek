# Development & Documentation Rules

## Overview

This document specifies the core engineering guidelines, C# XML documentation standards, component authoring requirements, and Roslyn analyzer conventions enforced across the **Vek** repository.

---

## 1. Compiler & Build Standards

- **Treat Warnings as Errors**: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` is enforced repo-wide in `Directory.Build.props`. All compiler and analyzer warnings must be fixed directly; never suppress or ignore warnings.
- **Incremental Build Hygiene**: Avoid running `dotnet clean` as part of normal local iteration. Use clean builds only when project assets or incremental compilation caches are genuinely corrupted.
- **Namespace Import Verification**: Incremental local builds can occasionally resolve symbols that CI's clean build rejects with `CS0246`. When introducing or referencing types from secondary namespaces (e.g. `FormFieldChangedEventArgs` in `Vek.Utilities`, not `Vek`), explicitly verify that the required `using` directives are present.
- **No Heavy Dependencies**: Do not introduce new NuGet packages or heavy external dependencies without explicit project approval.
- **Central Package Versions**: Keep NuGet package versions in the repository-root `Directory.Packages.props`. `PackageReference` entries in project files omit `Version`; use conditional `PackageVersion` entries there when a package needs different versions by project or target framework.

---

## 2. C# XML Documentation & Metadata Standards

Every public API surface in Vek must be thoroughly, concisely, and consistently documented.

### Public Properties & Parameters
- **Concise Summaries**: Every public property, method, interface, and class must have an XML `/// <summary>...</summary>` tag. Describe concrete runtime behavior, not tautological "Gets or sets..." boilerplate.
- **Defaults in Remarks**: Include `<remarks>` specifying default values when relevant:
  ```csharp
  /// <summary>
  /// Uses compact vertical padding.
  /// </summary>
  /// <remarks>
  /// Defaults to <c>false</c>.
  /// </remarks>
  [Parameter]
  [Category(CategoryTypes.Radio.Appearance)]
  public bool Dense { get; set; }
  ```
- **Category Attributes**: Every public component `[Parameter]` **must** be decorated with the appropriate `[Category(CategoryTypes....)]` attribute.

### Constructors
- **Never Add XML Documentation to Constructors**: Do not place XML comments or `<summary>` tags above constructors. For types with primary constructors, place the summary tag above the class declaration itself.

### Private & Internal Implementation State
- **Ordinary Line Comments Only**: Never use XML documentation tags (`///`) for private or internal fields, properties, or helper methods. Use ordinary single-line `//` comments describing invariants or non-obvious design decisions.

### Documentation Pages & Example Standards
- **No Catch-All 'Other Options' Sections**: Feature-rich, complex components (such as `VekDataGrid`) must provide dedicated sections and standalone, realistic examples for each distinct functionality or scenario (e.g., column group layouts, asynchronous loading feedback, sticky headers/footers) rather than grouping unrelated properties together into generic summary blocks.
- **Guided Progression & Concrete Scenarios**: Order examples from canonical to advanced. Each scenario must use realistic domain data and models, clear visual styling, and practical explanations showing when and why the feature is applied.

---

## 3. Component Authoring Standards

### Parameter Registration & State Management
- **Auto-Properties Only**: Component parameters must be declared as pure auto-properties (`public bool Expanded { get; set; }`). Never embed business logic, side-effects, or state updates in getters or setters.
- **Parameter Registration**: Register parameters in the constructor using `CreateRegisterScope()`:
  ```csharp
  using var registerScope = CreateRegisterScope();
  _expandedState = registerScope.RegisterParameter<bool>(nameof(Expanded))
      .WithParameter(() => Expanded)
      .WithEventCallback(() => ExpandedChanged)
      .WithChangeHandler(OnExpandedChangedAsync);
  ```
- **Method-Group Handlers**: Prefer method-group handlers for shared change logic.
- **No External Mutation via @ref**: Never assign component parameters directly via component references (`@ref`). Parameter updates must flow through declarative data-binding or explicit public component methods.

### Attribute Precedence & UserAttributes
- Components must expose `[Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object>? UserAttributes { get; set; }`.
- When rendering attributes, specify framework-computed attributes first and merge user attributes afterwards, allowing consumers to override defaults (e.g. `class` or `style`) without breaking core structural behavior.
- Fallback attribute values must be used for ARIA and HTML properties unless the component's contract strictly dictates immutable values.

### Naming Conventions
- Prefer positive parameter names (e.g. `Gutters` rather than `DisableGutters`).
- Avoid negative booleans that lead to double negatives (`DisableElevation = false`).

---

## 4. Roslyn Analyzers & Code Fixes

Vek ships first-party Roslyn analyzers (`src/Analyzers/Vek.Analyzers`) and code fixes (`src/Analyzers/Vek.Analyzers.CodeFixes`) to guide consumers toward best practices and handle API migrations automatically.

### Migration Hints & Parameter Obsoletion
- When renaming or removing a component parameter:
  1. Mark the old parameter `[Obsolete("...")]` with a clear migration message.
  2. Add a `VEK0002` migration hint to `src/Analyzers/Vek.Analyzers/Internal/ParameterMigration.cs`.
  3. Add a test case in `MigrationHintCases` within `tests/Analyzers/Vek.UnitTests.Analyzers`.
  4. Document the migration row in the analyzer documentation page (`src/Docs/Vek.Docs/Pages/Features/AnalyzersPage.razor`).
- **Binary Compatibility**: Note that changing an `EventCallback` to `EventCallback<T>` is a binary breaking change even though it remains source-compatible in Razor. Prefer additive overloads or deliberate major-version transitions.

---

## 5. Formatting & File Encodings

- **UTF-8 with BOM for C# Files**: In accordance with `.editorconfig` (`charset = utf-8-bom`), all `.cs` files must be UTF-8 encoded with a Byte Order Mark. Agent file generation tools often omit BOMs; the final whitespace pass corrects this.
- **Line Endings**: Standardize on LF line endings across all text files.
-- **Pre-PR Whitespace Formatting**:
  - Run `dotnet format whitespace --folder --include <path/to/files>` once at the very end of a task from the repository root.
  - Paths passed to `--include` must be relative to the repository root:
    ```bash
    dotnet format whitespace --folder --include src/Vek/Components/Menu/VekMenu.razor.cs
    ```

---

## 6. Git Commit and Pull Request Titles

Use the Conventional Commits format for commit subjects and pull request titles:

```text
<type>(<scope>): <imperative summary>
```

- Use a lowercase type and scope. Choose a type that describes the change: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, or `revert`.
- Use a concise scope for the affected component or area, such as `button`, `datepicker`, `unit-tests`, or `docs`. Omit the scope only when the change is repository-wide.
- Start the summary with an imperative verb, keep it concise, and omit the final period.
- Use `!` after the type or scope to mark a breaking change, and include a `BREAKING CHANGE:` footer in the commit body when additional migration detail is needed.
- Make the PR title describe the full PR and use the same format as its intended squash commit subject. Do not prefix titles with `[Migration NNN]`; put migration identifiers and context in the PR description instead.
- Reference related GitHub issues in the PR description with GitHub closing keywords such as `Fixes #123` or `Closes #123`, rather than treating migration IDs as issue numbers.

Examples:

```text
feat(button): add loading state with auto-disable
fix(menu): preserve focus when closing with Escape
docs(governance): define commit and pull request title rules
```

---

## 7. Clean-Room Implementation & Brand Anonymity Policy

- **Zero Third-Party Brand Traces**: Never mention, reference, or leave traces of third-party commercial UI vendors, product lines, or proprietary component suites (e.g. commercial grid/component packages) anywhere in commit messages, branch names, PR titles, PR descriptions, source code, comments, or documentation.
- **Clean-Room Reverse Engineering**: When features are inspired by, reverse-engineered from, or benchmarked against external UI libraries:
  1. Inspect behaviors, contracts, and interaction models locally (e.g. using decompiler inspection or evidence-gathering toolkits).
  2. Implement all functionality 100% cleanly from the ground up using native VekBlazor architecture (`ParameterState<T>`, `CssBuilder`, `RenderTreeBuilder`, bUnit).
  3. Never copy-paste decompiled third-party proprietary source code or naming artifacts.
- **Functional Naming Discipline**: Always use vendor-neutral, functional terminology that describes the feature's role in the library (e.g., "DataGrid Search Panel with Live Match Highlighting", "word grouping parser", "multi-column search evaluator").
- **Subfolder Discipline for Compound Components**: Complex compound components (particularly `DataGrid`) must strictly enforce subfolder separation. Never place new models, helpers, or sub-components directly into the component's root folder when dedicated functional subdirectories (`Filtering/`, `Models/`, `Cells/`, `Columns/`, `Grouping/`) exist.

---

## 8. Frontend Assets & Minified Bundles Policy

- **No Minified Assets in PRs**: Never commit generated minified files (`*.min.css`, `*.min.js`, `*.min.*`) or their source maps (`*.map`) from any project (`src/Vek/wwwroot/`, `src/Docs/Vek.Docs/wwwroot/`, etc.) in feature, bug-fix, or documentation PRs.
- **Auto-Regeneration Guard**: Building the project or running frontend asset pipelines regenerates minified files locally. Before staging and committing changes, always inspect `git status` and restore any generated minified files:
  ```bash
  git checkout -- "**/wwwroot/*.min.*" "**/wwwroot/*.map"
  ```
- **Release-Only Scope**: Generated minified bundles and maps are committed strictly during official release publication routines (`publish-release`), never in regular feature or maintenance pull requests.
