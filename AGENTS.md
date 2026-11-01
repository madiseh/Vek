# Project Instructions

## Technology and Architecture

- This project explicitly targets **.NET 10**. Project files must target `net10.0` unless a documented project-specific reason requires otherwise.
- The solution uses **hexagonal architecture**. Keep ports/contracts independent of adapters and keep reusable `Vek.Blazor.*` packages independent of Demo and sample-data projects.
- `Vek.Demo.Abstractions` contains the demo-facing ports and models. `Vek.SimulatedData` implements those ports with sample JSON data. `Vek.Demo` is the composition root.
- For a component category used by multiple UI platforms, put shared state, neutral contracts, and behavior in `Vek.Core.<Category>`. UI projects are adapters: they translate platform events and rendering into that Core API and display Core results. Keep UI rendering, native events, editors, templates, styling, and platform-only features in their UI projects.
- All Core projects must follow the canonical [Core Architecture Rule](docs/1.governance/core-architecture.md). Core must have absolutely no dependency on UI platforms, including WPF and Blazor: no project/package references, namespaces, types, APIs, platform-named members, platform detection, or implementations tailored to a UI platform, even behind neutral names. Core contains shared rules, contracts, state, and behavior only.
- The Core contract is canonical across adapters. Keep shared interface/type/property/operation names and state variable names identical in every UI adapter; do not reimplement shared algorithms in an adapter. A platform-only component does not need a Core project.
- A feature exposed by the shared Core contract must be supported by every adapter for that component. Keep native template types strongly typed through a generic Core contract (for DataGrid: `IVekDataGridColumn<TItem, TTemplate>` and `VekDataGridTemplate<TContext, TTemplate>`); adapters provide their own `TTemplate` without adding a second platform-specific column interface. Add XML documentation to public contracts and tests for observable behavior in each adapter.
- For shared remote data features, Core builds platform-neutral request/state descriptors. The UI host invokes its configured provider with the descriptor and cancellation token, then passes the returned data back to Core.
- Use the DataGrid target example in the [Core Architecture Rule](docs/1.governance/core-architecture.md#datagrid-target-example) as the reference pattern. Existing DataGrid code is subject to refactoring and must not be treated as proof of compliance.

## Repository Guidance Routing

Before carrying out work covered by a guide below, read and follow that guide. In particular, **before creating or editing a Git commit, read and follow `docs/1.governance/git-commits.md`**.

| When working on | Read and follow |
| --- | --- |
| Creating or editing Git commit messages | [`docs/1.governance/git-commits.md`](docs/1.governance/git-commits.md) |
| Project boundaries, dependency direction, or solution architecture | [`docs/1.governance/architecture.md`](docs/1.governance/architecture.md) |
| Any Core project, shared component behavior, adapter contracts, or cross-platform refactoring | [Core Architecture Rule](docs/1.governance/core-architecture.md) |

## Import Conventions

- Put namespaces shared by Razor components in the nearest `_Imports.razor` file instead of repeating `@using` directives in individual `.razor` files.
- Put namespaces shared by C# files in the project-level `GlobalUsings.cs` file with `global using` directives instead of repeating file-local `using` directives.
- When a namespace is needed in both Razor markup and C# files, add it to both central import locations; Razor `@using` directives do not apply to standalone C# files.

## C# File Organization

- Put each top-level class, interface, and record declaration in its own `.cs` file. Do not group these declarations in one source file. Name the file after the type it contains; use a distinguishing suffix when generic arities share a type name.

## Blazor Component Packages

- Put each independently packaged Blazor component category under `src/Components/Blazor/<PackageProject>/`.
- Every project under `src/Components/Blazor/` must contain a `Components/` directory. The shared `src/Components/Blazor/Directory.Build.targets` enforces this during builds.
- Keep each package focused on its component category, keep it independent of Demo and sample-data projects, and make its NuGet package ID match the project name.
- Keep a component's Razor markup and isolated CSS together under that package's `Components/` directory.

## Testing and TDD

- For every functional or code change, add or update at least one automated test for the observable behavior before changing production code.
- TDD is mandatory for functional and code changes: write the test first, run it to confirm it fails for the expected reason, implement the behavior, rerun the focused test to pass, then run the relevant test suite.
- Every C# test method marked with `[Fact]` or `[Theory]` must have an XML `/// <summary>` immediately above the test attribute. Describe the behavior the test protects.

## Code Discovery and Token Use

- Always prefer Codebase Memory MCP graph tools over grep, glob, or file search for code discovery. This reduces unnecessary source reads and token use.
- Use this order:
  1. `search_graph` to find functions, classes, routes, and variables.
  2. `trace_path` to follow callers and dependencies.
  3. `get_code_snippet` to inspect the relevant function or class.
  4. `query_graph` for multi-hop or aggregate questions.
  5. `get_architecture` for a high-level structural overview.
- Fall back to `rg` for string literals, error messages, configuration values, non-code files, or when graph results are insufficient.
