# Vek

# Material Design components for Blazor

Vek is an ambitious Material Design component framework for Blazor with an emphasis on ease of use and clear structure. It is perfect for .NET developers who want to rapidly build web applications without having to struggle with CSS and Javascript. Vek, being written entirely in C#, empowers you to adapt, fix or extend the framework.

Maintained and developed by **Amin Sabaghi** ([@madiseh](https://github.com/madiseh)).

## 💎 Why Choose Vek?

- Clean and aesthetic graphic design based on Material Design.
- Clear and easy to understand structure.
- Good documentation with many examples and source snippets.
- All components are written entirely in C#, no JavaScript allowed (except where absolutely necessary).
- Users can make beautiful apps without needing CSS (but they can of course use CSS too).
- Stability and high test coverage.

## 🚀 Getting Started

### Example Usage

```razor
<VekText Typo="Typo.h6">
    Vek is @Text
</VekText>

<VekButton Variant="Variant.Filled" 
           Color="Color.Primary" 
           OnClick="ButtonOnClick">
    @ButtonText
</VekButton>
```

### Installation

Add the Vek service in `Program.cs`:

```csharp
builder.Services.AddVekServices();
```

Add imports to `_Imports.razor`:

```razor
@using Vek
@using Vek.Components
```

Add providers to your root layout:

```razor
<VekThemeProvider />
<VekDialogProvider />
<VekSnackbarProvider />
<VekPopoverProvider />
```

## 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
