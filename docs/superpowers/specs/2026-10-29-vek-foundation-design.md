# Vek Repository Foundation Design

## Intent

Set up the empty `Vek` workspace as a .NET 10 reusable Blazor component library repository. The library will contain multiple generic UI components, such as buttons and data grids; DataGrid is an example, not the library's sole purpose. The repository should be ready for NuGet packaging, have a runnable demo with sample JSON data, and document its architecture and contribution conventions.

## Goals

- Create `Vek.slnx` and organize projects into solution folders that mirror the source layout.
- Keep all production projects under `src/` and tests under `tests/`.
- Make the .NET 10 requirement explicit in the root `AGENTS.md`.
- Instruct agents in the root `AGENTS.md` to prefer Codebase Memory MCP for code discovery to reduce unnecessary file searching and token use.
- Use a hexagonal boundary for the demo's data flow while keeping the component library independent of the sample-data implementation.
- Provide root-level build properties, central package management, an xUnit test project, a root `.gitignore`, a root MIT `LICENSE`, and an English `README.md`.
- Prepare `Vek.Components` for NuGet packaging and add a GitHub Actions workflow that publishes it on version tags using a repository secret.
- Add governance documentation for Conventional Commits and the architecture.

## Non-goals

- Implementing the DataGrid or other production components in this setup task.
- Publishing a package or creating a GitHub release during setup.
- Adding a database, external API, authentication, or production data source to the demo.
- Adding a CI status badge before a CI workflow exists.

## Approaches Considered

### 1. Hexagonal boundary around the demo (recommended)

Treat `Vek.Components` as the reusable Blazor presentation library. Put the demo's data contracts in `Vek.Demo.Abstractions`, JSON-backed sample data in `Vek.SimulatedData`, and wiring/composition in `Vek.Demo`. The demo passes data into components; the components do not load JSON, call APIs, or depend on persistence.

This follows the user's stated data flow and keeps generic UI components reusable without adding application or persistence layers to the component package.

### 2. Full application/core layering inside the component library

Add separate core/application projects and ports/adapters around the component implementation itself. This gives more assemblies and boundaries, but the current library has no application use cases or data access to justify them.

## Agreed Requirements

- Product/repository name: `Vek`.
- Target framework: `net10.0` for all projects; Windows target property remains `net10.0-windows` as supplied.
- `AGENTS.md` must explicitly require Codebase Memory MCP for code discovery, with `search_graph`, `trace_path`, `get_code_snippet`, `query_graph`, and `get_architecture` as the preferred order; use `rg` for literals/config/non-code files or when graph results are insufficient.
- Solution format: `.slnx`.
- Architecture: hexagonal, with demo contracts/ports separated from the JSON adapter and composition host.
- Main library: `Vek.Components`, a general-purpose Razor Class Library for reusable Blazor UI components, prepared for NuGet.
- Component organization: one folder per component under `Components/`; component markup and isolated CSS live together, and a `.razor.cs` code-behind is added only when it improves clarity. Shared CSS lives under `wwwroot/css/`.
- Demo: a separate Blazor app named `Vek.Demo` that demonstrates components.
- Sample data: a separate `Vek.SimulatedData` project containing sample JSON and its loader/adapter.
- Contracts: a separate `Vek.Demo.Abstractions` project referenced by both `Vek.Demo` and `Vek.SimulatedData`.
- Data-driven component direction: data-driven components receive data and report user actions through parameters/callbacks; they do not perform direct data access. DataGrid is one example, alongside controls such as buttons.
- Tests: `tests/Components/Vek.Components.Tests`, using xUnit and bUnit for Blazor component rendering; create the project scaffold only, with no test cases in this task.
- Platform configurations: `AnyCPU`, `x86`, and `x64`. Default builds and NuGet package builds target `AnyCPU`; explicitly selected `x86` or `x64` platforms map to the matching `PlatformTarget`.
- Build settings: preserve the supplied `Directory.Build.props` values and analyzer properties, except replace the global `PlatformTarget=x64` with the conditional AnyCPU/x86/x64 mapping above, and set `Platforms` to `AnyCPU;x86;x64`.
- Central package management: enable it in `Directory.Packages.props` and declare package versions there.
- Documentation: English README with accurate static badges for .NET 10, MIT, and `Vek.Components`; do not claim a published NuGet version or CI status before those exist.
- License: MIT, copyright line `Copyright (c) 2026 AMIN SABAGHI`.
- Governance docs: `docs/1.governance/git-commits.md` and `docs/1.governance/architecture.md`.
- Commit convention: Conventional Commits 1.0.0. Clearly distinguish its normative requirements from Vek's recommended type/scope vocabulary. Cite the official specification at <https://www.conventionalcommits.org/en/v1.0.0/>.
- Release automation: Add a GitHub Actions workflow to build/pack and push `Vek.Components` to NuGet. Setup must not run a publish itself; the trigger and secret mechanism are implementation calls below.
- Add a minimal root `.editorconfig` to accompany the analyzer-related build settings.

## Proposed Layout

```text
Vek.slnx
AGENTS.md
Directory.Build.props
Directory.Packages.props
.editorconfig
.gitignore
LICENSE
README.md
src/
  Components/Vek.Components/
  Demo/Vek.Demo/
  Ports/Vek.Demo.Abstractions/
  Adapters/Vek.SimulatedData/
tests/
  Components/Vek.Components.Tests/
docs/
  1.governance/
    git-commits.md
    architecture.md
  superpowers/specs/2026-10-29-vek-foundation-design.md
.github/workflows/publish-nuget.yml
```

The corresponding `.slnx` solution folders will be `src/Components`, `src/Demo`, `src/Ports`, `src/Adapters`, and `tests/Components`.

## Dependency Direction

```text
Vek.Demo ───────> Vek.Components
     │
     ├──────────> Vek.Demo.Abstractions <──────── Vek.SimulatedData
     └──────────> Vek.SimulatedData

Vek.Components.Tests ────────────────────────────> Vek.Components
```

`Vek.SimulatedData` implements the demo-facing contract and reads JSON sample files. `Vek.Demo` is the composition root and passes the resulting data to `Vek.Components`. The component package has no dependency on demo projects.

## Calls Left to the Implementer

- Use the standard .NET Blazor Web App template for `Vek.Demo`, with Interactive Server enabled so the demo can exercise interactive components without extra hosting infrastructure.
- Use the proposed solution-folder hierarchy matching the physical layout: `src/Components`, `src/Demo`, `src/Ports`, `src/Adapters`, and `tests/Components`.
- Set only `Vek.Components` as packable; keep Demo, Abstractions, SimulatedData, and tests out of NuGet package output.
- Include bUnit alongside xUnit in the component test project so future component rendering tests have the required helper library; no tests are added now.
- Derive `PackageVersion` from the version tag by removing a leading `v`; publish only the `Vek.Components` package.
- Run the publish workflow only for version tags matching `v*.*.*`, and read the push credential from the `NUGET_API_KEY` repository secret.
- Use the more specific SemVer tag glob `v*.*.*`; derive package version from the pushed tag.
- When `SolutionDir` is not supplied (for example, a direct project pack), default it to the repository root so the requested package output folder remains inside the workspace.
- Use a small `DemoProduct` sample model (`Id`, `Name`, `Category`, `Price`) and an `IDemoProductProvider.GetProductsAsync(CancellationToken)` port, with the JSON file and implementation in `Vek.SimulatedData`.
- Use Windows GitHub runners so `$(SolutionDir)\VekNugetPackage` behaves consistently with the Windows workspace.
- Keep README English and all new governance/spec documents English for the intended NuGet audience; provide concrete Vek-specific commit examples.

## Risks and Notes

- Store generated NuGet packages under `VekNugetPackage`, matching the repository's ignored package-output directory.
- `PlatformTarget` must be conditional on the selected platform; setting it globally would defeat the requested `AnyCPU` and `x86` configurations.
- The publish workflow requires the repository owner to configure the `NUGET_API_KEY` secret and push a version tag. No secret value should be committed.
- Static README badges must describe repository facts, not claim a release or successful CI run that has not happened.
