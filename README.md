<div align="center">

# Vek

**Modern, Fast & Extensible Component Suite for Blazor**

[![NuGet Version](https://img.shields.io/nuget/v/Vek.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/Vek)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Vek.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/Vek)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly%20%26%20Server-512BD4.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)

Vek is an enterprise-ready UI component library designed for .NET developers building modern web applications with Blazor. Written in pure C# with seamless CSS variable integration, Vek delivers unmatched developer velocity, elegant visual styling, and robust runtime performance across both Blazor WebAssembly and Blazor Server.

Maintained and developed by **Amin Sabaghi** ([@madiseh](https://github.com/madiseh)).

</div>

---

## ⚡ Highlights

- **Native C# Architecture**: Strongly typed component parameters, first-class asynchronous event handling, and declarative two-way data binding.
- **Modern Design System**: Sleek, clean visual language powered by fluid typography, elevation shadows, and a complete design token system.
- **Comprehensive Theming**: Built-in dynamic Light and Dark mode switching, custom palette generation, and instant CSS variable updates.
- **Enterprise Data Presentation**: High-throughput virtualized `VekDataGrid`, advanced filtering, multi-column sorting, and flexible pagination.
- **Accessible & Responsive**: Built with accessibility (ARIA) best practices, comprehensive keyboard navigation, and responsive layout breakpoints.
- **Multi-Target Support**: Built and tested for `.NET 8.0` and `.NET 10.0` (with full `.NET 9.0` compatibility).

---

## 🚀 Quickstart

### 1. Installation

Install the package from [NuGet](https://www.nuget.org/packages/Vek) via the .NET CLI:

```bash
dotnet add package Vek
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package Vek
```

### 2. Service Registration

Register Vek services in your `Program.cs`:

```csharp
using Vek.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Vek services
builder.Services.AddVekServices();
```

### 3. Static Assets

Include the required stylesheet and script references in your host page (`App.razor`, `_Host.cshtml`, or `index.html`):

```html
<!-- Vek Styles -->
<link href="_content/Vek/Vek.min.css" rel="stylesheet" />

<!-- Vek Scripts -->
<script src="_content/Vek/Vek.min.js"></script>
```

### 4. Global Imports

Add Vek namespaces to your `_Imports.razor`:

```razor
@using Vek
@using Vek.Components
```

### 5. Root Providers

Add the required providers in your `MainLayout.razor` or `Routes.razor`:

```razor
<VekThemeProvider />
<VekDialogProvider />
<VekSnackbarProvider />
<VekPopoverProvider />

@Body
```

---

## 💻 Code Example

```razor
@page "/dashboard"

<VekStack Spacing="4">
    <VekText Typo="Typo.h4" Color="Color.Primary">
        Welcome to Vek
    </VekText>

    <VekCard Elevation="2" Class="pa-4">
        <VekCardHeader>
            <CardHeaderContent>
                <VekText Typo="Typo.h6">Analytics Overview</VekText>
            </CardHeaderContent>
        </VekCardHeader>
        <VekCardContent>
            <VekText Typo="Typo.body2">
                Rapidly construct responsive interfaces using pure C# components.
            </VekText>
        </VekCardContent>
        <VekCardActions>
            <VekButton Variant="Variant.Filled" 
                       Color="Color.Primary" 
                       OnClick="HandleRefreshAsync">
                Refresh Metrics
            </VekButton>
            <VekButton Variant="Variant.Outlined" 
                       Color="Color.Secondary">
                Export Data
            </VekButton>
        </VekCardActions>
    </VekCard>
</VekStack>

@code {
    private async Task HandleRefreshAsync()
    {
        // Your logic here
        await Task.CompletedTask;
    }
}
```

---

## 🧩 Component Suite Overview

| Category | Components |
| :--- | :--- |
| **Layout & Structure** | `VekLayout`, `VekAppBar`, `VekDrawer`, `VekStack`, `VekGrid`, `VekContainer`, `VekPaper` |
| **Buttons & Action** | `VekButton`, `VekIconButton`, `VekFab`, `VekButtonGroup`, `VekToggleIconButton` |
| **Inputs & Forms** | `VekTextField`, `VekSelect`, `VekAutocomplete`, `VekCheckBox`, `VekRadio`, `VekSwitch`, `VekSlider` |
| **Pickers** | `VekDatePicker`, `VekDateRangePicker`, `VekTimePicker`, `VekColorPicker` |
| **Data Presentation** | `VekDataGrid`, `VekTable`, `VekTreeView`, `VekTimeline`, `VekList`, `VekBadge`, `VekChip` |
| **Feedback & Popups** | `VekDialog`, `VekSnackbar`, `VekAlert`, `VekTooltip`, `VekMenu`, `VekPopover`, `VekProgress` |
| **Navigation** | `VekNavMenu`, `VekBreadcrumbs`, `VekPagination`, `VekTabs` |

---

## 🤝 Contributing

Contributions, feature requests, and issue reports are welcome! Feel free to check out the [Issues](https://github.com/madiseh/Vek/issues) page.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'feat: Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

Distributed under the MIT License. See [LICENSE](LICENSE) for more details.
