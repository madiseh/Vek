# Development & Documentation Rules

## Overview

This document specifies the core engineering guidelines, C# XML documentation standards, component authoring requirements, and Roslyn analyzer conventions enforced across the **Vek** repository.

---

## 1. Compiler & Build Standards

- **Treat Warnings as Errors**: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` is enforced repo-wide in `src/Directory.Build.props`. All compiler and analyzer warnings must be fixed directly; never suppress or ignore warnings.
- **Incremental Build Hygiene**: Avoid running `dotnet clean` as part of normal local iteration. Use clean builds only when project assets or incremental compilation caches are genuinely corrupted.
- **Namespace Import Verification**: Incremental local builds can occasionally resolve symbols that CI's clean build rejects with `CS0246`. When introducing or referencing types from secondary namespaces (e.g. `FormFieldChangedEventArgs` in `Vek.Utilities`, not `Vek`), explicitly verify that the required `using` directives are present.
- **No Heavy Dependencies**: Do not introduce new NuGet packages or heavy external dependencies without explicit project approval.

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

Vek ships first-party Roslyn analyzers (`src/Vek.Analyzers`) and code fixes (`src/Vek.Analyzers.CodeFixes`) to guide consumers toward best practices and handle API migrations automatically.

### Migration Hints & Parameter Obsoletion
- When renaming or removing a component parameter:
  1. Mark the old parameter `[Obsolete("...")]` with a clear migration message.
  2. Add a `VEK0002` migration hint to `src/Vek.Analyzers/Internal/ParameterMigration.cs`.
  3. Add a test case in `MigrationHintCases` within `src/Vek.UnitTests.Analyzers`.
  4. Document the migration row in the analyzer documentation page (`src/Vek.Docs/Pages/Features/AnalyzersPage.razor`).
- **Binary Compatibility**: Note that changing an `EventCallback` to `EventCallback<T>` is a binary breaking change even though it remains source-compatible in Razor. Prefer additive overloads or deliberate major-version transitions.

---

## 5. Formatting & File Encodings

- **UTF-8 with BOM for C# Files**: In accordance with `src/.editorconfig` (`charset = utf-8-bom`), all `.cs` files must be UTF-8 encoded with a Byte Order Mark. Agent file generation tools often omit BOMs; the final whitespace pass corrects this.
- **Line Endings**: Standardize on LF line endings across all text files.
- **Pre-PR Whitespace Formatting**:
  - Run `dotnet format whitespace --folder --include <path/to/files>` once at the very end of a task from the `src/` directory.
  - Paths passed to `--include` must be relative to `src/`:
    ```bash
    dotnet format whitespace --folder --include Vek/Components/Menu/VekMenu.razor.cs
    ```

---

## 6. Clean-Room Implementation & Brand Anonymity Policy

- **Zero Third-Party Brand Traces**: Never mention, reference, or leave traces of third-party commercial UI vendors, product lines, or proprietary component suites (e.g. commercial grid/component packages) anywhere in commit messages, branch names, PR titles, PR descriptions, source code, comments, or documentation.
- **Clean-Room Reverse Engineering**: When features are inspired by, reverse-engineered from, or benchmarked against external UI libraries:
  1. Inspect behaviors, contracts, and interaction models locally (e.g. using decompiler inspection or evidence-gathering toolkits).
  2. Implement all functionality 100% cleanly from the ground up using native VekBlazor architecture (`ParameterState<T>`, `CssBuilder`, `RenderTreeBuilder`, bUnit).
  3. Never copy-paste decompiled third-party proprietary source code or naming artifacts.
- **Functional Naming Discipline**: Always use vendor-neutral, functional terminology that describes the feature's role in the library (e.g., "DataGrid Search Panel with Live Match Highlighting", "word grouping parser", "multi-column search evaluator").
- **Subfolder Discipline for Compound Components**: Complex compound components (particularly `DataGrid`) must strictly enforce subfolder separation. Never place new models, helpers, or sub-components directly into the component's root folder when dedicated functional subdirectories (`Filtering/`, `Models/`, `Cells/`, `Columns/`, `Grouping/`) exist.
