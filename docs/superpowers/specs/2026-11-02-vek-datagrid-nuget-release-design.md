<!--
Copyright (c) 2026 AMIN SABAGHI
Licensed under the MIT License. See the LICENSE file in the project root for more information.
Created: 2026-11-02
-->

# Vek DataGrid NuGet Release Design

## Intent

Publish the first stable `1.0.0` release of the Blazor and WPF DataGrid adapters through the repository's GitHub Actions workflow and the NuGet Trusted Publishing policy.

## Goals

- Publish `Vek.Blazor.DataGrid` and `Vek.WPF.DataGrid`, both at version `1.0.0`.
- Keep `Vek.Core.DataGrid` as an implementation dependency bundled into both adapter packages; do not publish a separate Core package.
- Use the existing `v*.*.*` tag workflow with NuGet OIDC authentication and publish only the two DataGrid packages.
- Provide complete NuGet metadata and package readmes for both packages.

## Non-goals

- Do not publish `Vek.Blazor.CodeView` or a standalone `Vek.Core.DataGrid` package.
- Do not redesign the DataGrid APIs or change runtime behavior.

## Decisions and constraints

- NuGet Trusted Publishing policy is active for owner `AminSabaghi`, repository `madiseh/Vek`, workflow `publish-nuget.yml`, with the two DataGrid package IDs and push-new-packages-and-versions scope.
- The policy has no GitHub Environment restriction; the workflow should not declare an environment.
- Blazor targets `net10.0`; WPF targets `net10.0-windows`.
- A pushed `v1.0.0` tag is the release trigger. NuGet publishing uses `NuGet/login@v1` and GitHub OIDC rather than a long-lived API key.

## Approach

Add NuGet metadata and a package readme to the WPF project, repair the Blazor package's missing readme, and package the Core assembly within each adapter nupkg while suppressing a standalone Core package dependency. In-repository consumers that use Core contracts directly will declare Core project references explicitly. Update `.github/workflows/publish-nuget.yml` to restore, build, pack, and push only the two adapter packages using the active Trusted Publishing policy. Before release, build and pack locally, then inspect each nupkg to confirm its ID, version, framework assets, bundled Core assembly, readme, and dependency list. Push the approved `v1.0.0` tag to start publication.

## Remaining implementation choices

Use the repository's existing MIT license expression and author metadata. Write concise English readmes with installation and basic usage guidance based on the shipped public components. Keep package contents framework-specific and avoid adding Demo or sample-data projects.
