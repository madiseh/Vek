# Vek Code View Design

## Intent

Add a reusable, read-only code viewer to the `Vek.Components` Blazor library so documentation and consuming applications can display readable syntax-highlighted code.

## Goals

- Provide a component whose public name starts with `Vek` (`VekCodeView`).
- Support HTML, Java, C#, and SQL syntax highlighting.
- Show one explicitly selected language per component instance.
- Include an accessible copy action and clear copied feedback.
- Add documentation samples demonstrating each supported language.
- Keep `Vek.Components` independent of Demo and sample-data projects.

## Non-goals

- Editing code inside the component.
- Supporting languages beyond HTML, Java, C#, and SQL in this change.
- Adding MudBlazor or a third-party syntax-highlighting dependency.

## Constraints and decisions

- The repository is a .NET 10 Blazor component library with a separate demo host.
- MudBlazor is not currently referenced by the solution.
- The component will use a small built-in highlighter scoped to the four requested languages.
- Component styling follows the existing Vek component and documentation conventions; syntax colors must retain readable contrast.
- The implementation will preserve the pre-existing unrelated DataGrid documentation changes in the working tree.

## Approaches

1. **Built-in focused highlighter — selected.** Keeps the library self-contained and targets the four requested languages. Vek owns the highlighting rules.
2. **Third-party JavaScript highlighter.** Offers broader language grammars but adds a dependency and JavaScript asset integration to the component package.

The built-in approach was selected to keep the component dependency-free and aligned with the requested language set.

## Builder decisions

- Implement `VekCodeView` in `Vek.Components` with a code value and an explicit language parameter (exact parameter types are an implementation detail).
- Render code read-only, with syntax highlighting and a copy button.
- Add four examples to the Demo documentation, one each for HTML, Java, C#, and SQL.
- Keep all sample content in the Demo layer; do not add Demo references to `Vek.Components`.
