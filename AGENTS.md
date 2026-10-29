# Project Instructions

## Technology and Architecture

- This project explicitly targets **.NET 10**. Project files must target `net10.0` unless a documented project-specific reason requires otherwise.
- The solution uses **hexagonal architecture**. Keep ports/contracts independent of adapters and keep the reusable `Vek.Components` library independent of Demo and sample-data projects.
- `Vek.Demo.Abstractions` contains the demo-facing ports and models. `Vek.SimulatedData` implements those ports with sample JSON data. `Vek.Demo` is the composition root.

## Repository Guidance Routing

Before carrying out work covered by a guide below, read and follow that guide. In particular, **before creating or editing a Git commit, read and follow `docs/1.governance/git-commits.md`**.

| When working on | Read and follow |
| --- | --- |
| Creating or editing Git commit messages | [`docs/1.governance/git-commits.md`](docs/1.governance/git-commits.md) |
| Project boundaries, dependency direction, or solution architecture | [`docs/1.governance/architecture.md`](docs/1.governance/architecture.md) |

## Import Conventions

- Put namespaces shared by Razor components in the nearest `_Imports.razor` file instead of repeating `@using` directives in individual `.razor` files.
- Put namespaces shared by C# files in the project-level `GlobalUsings.cs` file with `global using` directives instead of repeating file-local `using` directives.
- When a namespace is needed in both Razor markup and C# files, add it to both central import locations; Razor `@using` directives do not apply to standalone C# files.

## Code Discovery and Token Use

- Always prefer Codebase Memory MCP graph tools over grep, glob, or file search for code discovery. This reduces unnecessary source reads and token use.
- Use this order:
  1. `search_graph` to find functions, classes, routes, and variables.
  2. `trace_path` to follow callers and dependencies.
  3. `get_code_snippet` to inspect the relevant function or class.
  4. `query_graph` for multi-hop or aggregate questions.
  5. `get_architecture` for a high-level structural overview.
- Fall back to `rg` for string literals, error messages, configuration values, non-code files, or when graph results are insufficient.
