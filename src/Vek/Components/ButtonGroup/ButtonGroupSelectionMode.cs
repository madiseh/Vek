namespace Vek;

/// <summary>
/// Controls how buttons in a button group retain selection.
/// </summary>
public enum ButtonGroupSelectionMode
{
    /// <summary>
    /// Leaves buttons as ordinary actions without selected state.
    /// </summary>
    None,

    /// <summary>
    /// Keeps at most one button selected at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Allows any number of buttons to be selected.
    /// </summary>
    Multiple
}
