# Vek Repository Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a .NET 10 repository skeleton for the reusable Vek Blazor component library, demo, simulated-data adapter, tests, governance documentation, and NuGet publishing workflow.

**Architecture:** `Vek.Components` is an independent Razor Class Library. The demo composes it with a ports project (`Vek.Demo.Abstractions`) and a JSON-backed adapter (`Vek.SimulatedData`). The solution folders mirror `src/` and `tests/`.

**Tech Stack:** .NET 10, C#, Blazor Web App with Interactive Server for Demo, Razor Class Library, xUnit, bUnit, MSBuild central package management, GitHub Actions, NuGet.

**Spec:** `docs/superpowers/specs/2026-10-29-vek-foundation-design.md`

## Global Constraints

- Target framework: `net10.0`.
- Solution format: `Vek.slnx`.
- Platform list: `AnyCPU;x86;x64`; default platform target is `AnyCPU`, with `x86` and `x64` selected conditionally.
- Root build properties retain the requested values, including `PackageOutputPath=$(SolutionDir)\VekNugetPackage`, except the global x64 target is replaced by the conditional platform mapping.
- When `SolutionDir` is unset for a direct project build, default it to the repository root so package output stays inside the workspace.
- Central package management is enabled in `Directory.Packages.props`; versions: `Microsoft.AspNetCore.Components.Web` 10.0.14, `Microsoft.NET.Test.Sdk` 18.11.0, `bunit` 2.13.3, `xunit.v3.mtp-off` 4.0.2, and `xunit.runner.visualstudio` 4.0.1.
- `Vek.Components` is the only NuGet-packable project.
- Tests use xUnit and bUnit; this task creates no test cases and runs no tests.
- GitHub publishing uses tag versions and the `NUGET_API_KEY` secret; no publish is triggered during setup.
- `AGENTS.md` explicitly says the repository targets .NET 10, uses hexagonal boundaries, and prefers Codebase Memory MCP for code discovery to reduce unnecessary search/token use.

## Review Focus

- **Solution membership and folder mapping:** every project path appears under its intended `.slnx` solution folder; verify by listing the solution and building it.
- **MSBuild property import and platform selection:** root properties apply to all nested projects, with `AnyCPU` default and `x86`/`x64` selected explicitly; verify Release builds for all three platforms.
- **NuGet package contents:** only `Vek.Components` is packable and its package contains the README; pack it and inspect the output directory/package contents.
- **Hexagonal sample-data flow:** Demo depends on the provider port and JSON adapter, while Components has no dependency on demo projects; verify project references and build.
- **Publishing workflow safety:** only version tags trigger publishing and the key comes from GitHub Secrets; inspect workflow trigger and secret references without running a publish.

---

## File Structure

```text
Vek.slnx
AGENTS.md
Directory.Build.props
Directory.Packages.props
.editorconfig
.gitignore
LICENSE
README.md
src/Components/Vek.Components/
src/Demo/Vek.Demo/
src/Ports/Vek.Demo.Abstractions/
src/Adapters/Vek.SimulatedData/
tests/Components/Vek.Components.Tests/
docs/1.governance/git-commits.md
docs/1.governance/architecture.md
.github/workflows/publish-nuget.yml
```

`Vek.Demo.Abstractions` owns `DemoProduct` and `IDemoProductProvider`. `Vek.SimulatedData` owns `products.json` and its JSON provider implementation. `Vek.Demo` is the composition root. `Vek.Components` remains independent.

### Task 1: Repository-wide settings and root files

**Files:**
- Create: `Directory.Build.props`
- Create: `Directory.Packages.props`
- Create: `.editorconfig`
- Create: `.gitignore`
- Create: `AGENTS.md`
- Create: `LICENSE`
- Create: `README.md`

**Interfaces:**
- `Directory.Build.props` supplies shared target, platform, nullable, implicit-using, build, package-output, and analyzer properties to nested projects.
- `Directory.Packages.props` enables central package versions for every package reference.

- [ ] **Step 1: Add root build settings**

Preserve the user-provided properties. Set `Platforms` to `AnyCPU;x86;x64`; set `PlatformTarget` to `AnyCPU` by default, and map explicit `x86` and `x64` platform selections to matching targets. If `SolutionDir` is empty (direct project invocation), default it to the repository root. Keep analyzer enablement false by default and controlled through `EnableAnalyzers`.

- [ ] **Step 2: Add central package management and editor configuration**

Set `ManagePackageVersionsCentrally` to `true`; add the versions in Global Constraints for the exact packages used in Task 2. Add a small `.editorconfig` for consistent C# and Razor formatting without turning analyzers on by default.

- [ ] **Step 3: Add ignore rules and agent instructions**

Ignore Visual Studio, .NET build, user-specific, and generated NuGet package artifacts. In `AGENTS.md`, explicitly state .NET 10 and hexagonal architecture. Add the Codebase Memory MCP priority order (`search_graph`, `trace_path`, `get_code_snippet`, `query_graph`, `get_architecture`) and the allowed `rg` fallback cases.

- [ ] **Step 4: Add license and README**

Use the MIT license with `Copyright (c) 2026 AMIN SABAGHI`. Write README in English with accurate .NET 10, MIT, and package-name badges; do not add unpublished version or non-existent CI status badges.

### Task 2: Projects, references, and `.slnx`

**Files:**
- Create: `Vek.slnx`
- Create: `src/Components/Vek.Components/Vek.Components.csproj`
- Create: `src/Demo/Vek.Demo/Vek.Demo.csproj` and the Blazor app entry files
- Create: `src/Ports/Vek.Demo.Abstractions/Vek.Demo.Abstractions.csproj`
- Create: `src/Ports/Vek.Demo.Abstractions/DemoProduct.cs`
- Create: `src/Ports/Vek.Demo.Abstractions/IDemoProductProvider.cs`
- Create: `src/Adapters/Vek.SimulatedData/Vek.SimulatedData.csproj`
- Create: `src/Adapters/Vek.SimulatedData/products.json`
- Create: `src/Adapters/Vek.SimulatedData/JsonDemoProductProvider.cs`
- Create: `tests/Components/Vek.Components.Tests/Vek.Components.Tests.csproj`

**Interfaces:**
- `DemoProduct` is a record with `Id: int`, `Name: string`, `Category: string`, and `Price: decimal`.
- `IDemoProductProvider.GetProductsAsync(CancellationToken cancellationToken = default)` returns `Task<IReadOnlyList<DemoProduct>>`.
- `JsonDemoProductProvider` implements `IDemoProductProvider`, loading `products.json` with `System.Text.Json`.

- [ ] **Step 1: Create the four .NET 10 production projects and the test project**

Use the Razor Class Library template for Components, the Blazor Web App template with Interactive Server for Demo, and class-library templates for Abstractions and SimulatedData. Create the xUnit test project and add bUnit. Keep the component test project empty of test cases.

- [ ] **Step 2: Add the demo port and sample model**

Create `DemoProduct` and `IDemoProductProvider` in Abstractions with the exact signatures above.

- [ ] **Step 3: Add the JSON adapter**

Add a small representative `products.json`, copy it to the output directory, and implement `JsonDemoProductProvider` using `System.Text.Json`. Add a project reference from SimulatedData to Abstractions.

- [ ] **Step 4: Wire the Demo composition root**

Reference Components, Abstractions, and SimulatedData from Demo. Register `IDemoProductProvider` to `JsonDemoProductProvider` and show the sample products on the starter page so the JSON-to-demo path is visible. Do not implement a Vek Button or DataGrid in this scaffold.

- [ ] **Step 5: Configure package metadata and references**

Mark only Components packable, assign package ID `Vek.Components`, include README metadata, and add the project reference from `Vek.Components.Tests` to Components. Explicitly set all other projects as non-packable.

- [ ] **Step 6: Create the `.slnx` and its folders**

Add all projects. Organize solution folders to mirror physical paths: `src/Components`, `src/Demo`, `src/Ports`, `src/Adapters`, and `tests/Components`.

- [ ] **Step 7: Build-check the solution**

Run `dotnet build Vek.slnx -c Release` and then build with `-p:Platform=x86` and `-p:Platform=x64`. Expected: all three complete successfully without test execution.

### Task 3: Governance documentation

**Files:**
- Create: `docs/1.governance/git-commits.md`
- Create: `docs/1.governance/architecture.md`
- Modify: `docs/superpowers/specs/2026-10-29-vek-foundation-design.md` to reflect the final created layout, if needed

- [ ] **Step 1: Write the Conventional Commits guide**

Document the official message grammar, required type/description, optional scope/body/footers, breaking-change syntax, `feat`/`fix` SemVer mapping, and the limits of what the spec assigns to other types. Clearly mark any Vek-recommended type and scope lists as project policy. Link the official source: `https://www.conventionalcommits.org/en/v1.0.0/`.

- [ ] **Step 2: Write the architecture guide**

Explain the project roles and allowed dependency direction, include a Mermaid diagram, and document that Components are general-purpose UI while the demo's JSON provider implements a port consumed by the Demo composition root.

- [ ] **Step 3: Check documentation references**

Check that README links, solution/project names, folder names, project references, and workflow/secret names match the created files.

### Task 4: NuGet publishing workflow

**Files:**
- Create: `.github/workflows/publish-nuget.yml`

**Interfaces:**
- Trigger: pushed version tags matching `v*.*.*`.
- Credential: GitHub repository secret `NUGET_API_KEY`.
- Version: tag name without leading `v`.

- [ ] **Step 1: Add a .NET 10 GitHub Actions workflow**

Use a Windows runner to preserve the backslash-based `$(SolutionDir)\VekNugetPackage` output path. Restore and build the solution in Release, pack only Components with the tag-derived package version, then push the package using `NUGET_API_KEY`. Do not echo or persist the secret.

- [ ] **Step 2: Inspect publishing guards**

Confirm the workflow triggers only on version tags, references the secret expression for NuGet push, and targets only `Vek.Components`. Do not execute a publish.

- [ ] **Step 3: Pack-check the library**

Run `dotnet pack src/Components/Vek.Components/Vek.Components.csproj -c Release -p:PackageVersion=0.1.0` and inspect `VekNugetPackage` to confirm only the intended `Vek.Components` package is produced and includes its README.

## Verification Summary

- `dotnet build Vek.slnx -c Release`
- `dotnet build Vek.slnx -c Release -p:Platform=x86`
- `dotnet pack` the Components project and inspect the resulting `.nupkg` contents.
- Manually inspect workflow trigger, package path, and secret handling; never publish during scaffold setup.
- Do not run `dotnet test` or add test cases in this task.
