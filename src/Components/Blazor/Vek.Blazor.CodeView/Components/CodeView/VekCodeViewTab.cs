namespace Vek.Components.Components.CodeView;

/// <summary>Provides one titled code sample for <see cref="VekCodeViewMulti"/>.</summary>
/// <param name="Title">The label shown on the sample's tab.</param>
/// <param name="Language">The language used to highlight the sample.</param>
/// <param name="Code">The source text shown when the tab is selected.</param>
public sealed record VekCodeViewTab(string Title, VekCodeLanguage Language, string Code);
