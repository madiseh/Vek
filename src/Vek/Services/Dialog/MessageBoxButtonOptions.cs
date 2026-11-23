namespace Vek;

/// <summary>
/// Defines a caller-provided action for a message box.
/// </summary>
public class MessageBoxButtonOptions
{
    /// <summary>
    /// The text displayed on the action button.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The value returned when the action is selected.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Uses destructive styling for the action button.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    public bool IsDestructive { get; set; }

    /// <summary>
    /// The optional icon rendered before the action text.
    /// </summary>
    public string? StartIcon { get; set; }
}
