# Vek Components

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![MIT License](https://img.shields.io/badge/License-MIT-yellow.svg)
![NuGet packages](https://img.shields.io/badge/Packages-Vek.Blazor-004880?logo=nuget&logoColor=white)

Vek is a collection of reusable Blazor component packages targeting .NET 10. Each feature package is a separate Razor Class Library and NuGet package.

## Repository Structure

| Path | Purpose |
| --- | --- |
| `src/Components/Blazor/Vek.Blazor.DataGrid` | Data grid components and the `Vek.Blazor.DataGrid` NuGet package. |
| `src/Components/Blazor/Vek.Blazor.CodeView` | Code viewer components and the `Vek.Blazor.CodeView` NuGet package. |
| `src/Demo/Vek.Demo` | Interactive app for demonstrating components. |
| `src/Ports/Vek.Demo.Abstractions` | Demo data contracts and sample models. |
| `src/Adapters/Vek.SimulatedData` | JSON-backed sample data provider. |
| `tests/Components/Vek.Components.Tests` | xUnit and bUnit tests for the Blazor component packages. |

Every component package under `src/Components/Blazor` must contain a `Components/` folder. Put each feature's Razor markup and isolated CSS together under that folder. This layout is checked during builds.

## Build

Install the .NET 10 SDK, then run:

```powershell
dotnet restore Vek.slnx
dotnet build Vek.slnx -c Release
```

To build an explicit platform configuration, use `-p:Platform=x86` or `-p:Platform=x64`. The default configuration is `AnyCPU`.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
