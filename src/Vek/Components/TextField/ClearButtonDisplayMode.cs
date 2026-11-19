namespace Vek;

/// <summary>
/// Controls when a text field's clear button is displayed.
/// </summary>
public enum ClearButtonDisplayMode
{
    /// <summary>
    /// Hides the clear button.
    /// </summary>
    Never,

    /// <summary>
    /// Displays the clear button while the field has a value.
    /// </summary>
    WhileEditing,

    /// <summary>
    /// Displays the clear button even when the field is empty.
    /// </summary>
    Always
}
