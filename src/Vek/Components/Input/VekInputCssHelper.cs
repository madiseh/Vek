using Vek.Utilities;

namespace Vek;

/// <summary>
/// A set of methods which generate CSS classes for <see cref="VekBaseInput{T}" /> components.
/// </summary>
internal static class VekInputCssHelper
{
    /// <summary>
    /// Gets the CSS classes for the specified input component.
    /// </summary>
    /// <typeparam name="T">The type of data collect by the input.</typeparam>
    /// <param name="baseInput">The input control to use.</param>
    /// <param name="shrinkWhen">The function which determines when to shrink the input.</param>
    /// <returns>A set of CSS classes.</returns>
    public static string GetClassname<T>(VekBaseInput<T> baseInput, Func<bool> shrinkWhen) =>
        new CssBuilder("vek-input")
            .AddClass($"vek-input-{baseInput.Variant.ToStringFast(true)}")
            .AddClass($"vek-input-{baseInput.Variant.ToStringFast(true)}-with-label", !string.IsNullOrEmpty(baseInput.Label))
            .AddClass($"vek-input-adorned-{baseInput.Adornment.ToStringFast(true)}", baseInput.Adornment != Adornment.None)
            .AddClass($"vek-input-margin-{baseInput.Margin.ToStringFast(true)}", baseInput.Margin != Margin.None)
            .AddClass("vek-input-underline", baseInput.Underline && baseInput.Variant != Variant.Outlined)
            .AddClass("vek-shrink", shrinkWhen)
            .AddClass("vek-disabled", baseInput.Disabled)
            .AddClass("vek-input-error", baseInput.HasErrors)
            .AddClass("vek-ltr", baseInput.GetInputType() == InputType.Email || baseInput.GetInputType() == InputType.Telephone)
            .AddClass($"vek-typography-{baseInput.Typo.ToStringFast(true)}")
            .AddClass(baseInput.Class)
            .Build();

    /// <summary>
    /// Gets the CSS classes for the specified input component slot.
    /// </summary>
    /// <typeparam name="T">The type of data collect by the input.</typeparam>
    /// <param name="baseInput">The input control to use.</param>
    /// <returns>A set of CSS classes.</returns>
    public static string GetInputClassname<T>(VekBaseInput<T> baseInput) =>
        new CssBuilder("vek-input-slot")
            .AddClass("vek-input-root")
            .AddClass($"vek-input-root-{baseInput.Variant.ToStringFast(true)}")
            .AddClass($"vek-input-root-adorned-{baseInput.Adornment.ToStringFast(true)}", baseInput.Adornment != Adornment.None)
            .AddClass($"vek-input-root-margin-{baseInput.Margin.ToStringFast(true)}", baseInput.Margin != Margin.None)
            .AddClass(baseInput.Class)
            .Build();

    /// <summary>
    /// Gets the CSS classes for the specified input adornment.
    /// </summary>
    /// <typeparam name="T">The type of data collect by the input.</typeparam>
    /// <param name="baseInput">The input control to use.</param>
    /// <returns>A set of CSS classes.</returns>
    public static string GetAdornmentClassname<T>(VekBaseInput<T> baseInput) =>
        new CssBuilder()
            .AddClass($"vek-input-adornment-{baseInput.Adornment.ToStringFast(true)}", baseInput.Adornment != Adornment.None)
            .AddClass($"vek-text", !string.IsNullOrEmpty(baseInput.AdornmentText))
            .AddClass($"vek-input-root-filled-shrink", baseInput.Variant == Variant.Filled)
            .Build();
}
