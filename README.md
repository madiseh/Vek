# Vek Components

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![MIT License](https://img.shields.io/badge/License-MIT-yellow.svg)
![NuGet package](https://img.shields.io/badge/Package-Vek.Components-004880?logo=nuget&logoColor=white)

Vek is a reusable Blazor component library targeting .NET 10. The library is being prepared for NuGet distribution. Its components will be organized by feature, with each component's markup and isolated styles kept together.

## Repository Structure

| Path | Purpose |
| --- | --- |
| `src/Components/Vek.Components` | Reusable Blazor components and the NuGet package. |
| `src/Demo/Vek.Demo` | Interactive app for demonstrating components. |
| `src/Ports/Vek.Demo.Abstractions` | Demo data contracts and sample models. |
| `src/Adapters/Vek.SimulatedData` | JSON-backed sample data provider. |
| `tests/Components/Vek.Components.Tests` | xUnit and bUnit project for future component tests. |

## Build

Install the .NET 10 SDK, then run:

```powershell
dotnet restore Vek.slnx
dotnet build Vek.slnx -c Release
```

To build an explicit platform configuration, use `-p:Platform=x86` or `-p:Platform=x64`. The default configuration is `AnyCPU`.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
