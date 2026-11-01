<!--
Copyright (c) 2026 AMIN SABAGHI
Licensed under the MIT License. See the LICENSE file in the project root for more information.
Created: 2026-11-02
-->

# Vek DataGrid NuGet Release Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Prepare and publish `Vek.Blazor.DataGrid` and `Vek.WPF.DataGrid` version `1.0.0` through the active NuGet Trusted Publishing policy.

**Architecture:** Keep Core as the shared source project and include its built assembly in each adapter package. Configure both adapter packages as the only release outputs and authenticate GitHub Actions to NuGet through OIDC.

**Tech Stack:** .NET 10, MSBuild/NuGet pack, GitHub Actions, NuGet Trusted Publishing.

**Spec:** `docs/superpowers/specs/2026-11-02-vek-datagrid-nuget-release-design.md`

## Global Constraints

- Publish only `Vek.Blazor.DataGrid` and `Vek.WPF.DataGrid`, both at `1.0.0`.
- Do not publish a standalone `Vek.Core.DataGrid` package or `Vek.Blazor.CodeView`.
- Bundle `Vek.Core.DataGrid.dll` into each adapter package and omit a Core package dependency from each nuspec.
- Keep target frameworks `net10.0` for Blazor and `net10.0-windows` for WPF.
- Use the active NuGet policy for `madiseh/Vek` and `publish-nuget.yml`; the workflow uses OIDC, declares no GitHub environment, and publishes on version tags.
- Package documentation is English, copyright header uses the repository's required form, and licenses use MIT.

## Review Focus

- Missing package readme or malformed PackageReadmeFile entry causes packing failure or absent NuGet package docs.
- ProjectReference metadata leaks a `Vek.Core.DataGrid` NuGet dependency into the adapter nuspec.
- Core assembly is absent from a framework-specific package folder or the adapter fails to compile in a consumer project.
- Workflow requests OIDC without `id-token: write`, uses the wrong NuGet login user, or pushes packages outside the policy patterns.
- Tag version parsing, package output filenames, and WPF framework-specific assets disagree.

## File Structure

- Modify: `src/Components/Blazor/Vek.Blazor.DataGrid/Vek.Blazor.DataGrid.csproj` — package metadata, Core bundling, readme inclusion.
- Create: `src/Components/Blazor/Vek.Blazor.DataGrid/README.md` — install and basic usage instructions.
- Modify: `src/Components/WPF/Vek.WPF.DataGrid/Vek.WPF.DataGrid.csproj` — complete NuGet metadata and Core bundling.
- Modify: `src/Demo/Vek.Demo/Vek.Demo.csproj` and `tests/Components/Vek.Components.Tests/Vek.Components.Tests.csproj` — declare direct Core source-project usage after adapter Core references become private to package consumers.
- Create: `src/Components/WPF/Vek.WPF.DataGrid/README.md` — Windows install and basic usage instructions.
- Modify: `.github/workflows/publish-nuget.yml` — build/pack/push the two packages with OIDC.

## Task 1: Make both adapters self-contained NuGet packages

**Interfaces:** Each package exposes its adapter assembly and bundled Core assembly under its target framework and has no dependency on a separately published Vek Core package.

- [x] Create the Blazor package readme with install and usage guidance; include the required copyright/license/date header.
- [x] Update the Blazor project package metadata and readme packaging.
- [x] Create the WPF package readme with Windows-specific installation and usage guidance; include the required header.
- [x] Add complete WPF package metadata matching project/package ID and MIT author conventions.
- [x] Make the Core project reference private to package consumers while adding `Vek.Core.DataGrid.dll` to each adapter package's `lib/<target-framework>` output.
- [x] Add explicit Core project references to the Blazor demo and component tests, which directly use Core contracts exposed by the adapter.
- [x] Build and pack both projects at version `1.0.0`.
- [x] Inspect both `.nupkg` archives and nuspec files; confirm IDs, versions, target-framework assets, README, Core assembly, and no Core package dependency.

## Task 2: Publish only the two packages with Trusted Publishing

**Interfaces:** A pushed version tag triggers one Windows GitHub Actions job that authenticates to NuGet using the active OIDC policy and pushes exactly the two version-matched nupkg files.

- [x] Grant `id-token: write` on the publish job and use `NuGet/login@v1` with the NuGet owner username `AminSabaghi`.
- [x] Remove the long-lived `NUGET_API_KEY` workflow dependency.
- [x] Pack only `Vek.Blazor.DataGrid` and `Vek.WPF.DataGrid`; remove CodeView from workflow outputs and push paths.
- [x] Preserve validated version-tag parsing and verify both package paths exist before push.
- [x] Let pushes fail when a package version already exists; do not mask an immutable version collision with `--skip-duplicate`.
- [x] Run the package workflow steps locally where possible and inspect the final diff.
- [ ] Read `docs/1.governance/git-commits.md`, commit the release preparation, tag `v1.0.0`, and push the branch/tag so GitHub Actions performs the NuGet publication.

## Review

- Review the workflow against NuGet's OIDC policy: repository owner `madiseh`, repository `Vek`, workflow file `publish-nuget.yml`, no environment, and the two exact package patterns.
- Review both nupkg contents and generated nuspec dependency groups before creating the release tag.
