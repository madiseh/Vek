namespace Vek;

/// <summary>
/// Controls how tab headers scroll when they exceed the available width.
/// </summary>
public enum TabsScrollMode
{
    /// <summary>
    /// Shows navigation buttons when the tab headers overflow.
    /// </summary>
    Auto,

    /// <summary>
    /// Keeps navigation buttons visible, even when all tabs fit.
    /// </summary>
    NavButtons,

    /// <summary>
    /// Disables header scrolling and hides navigation buttons.
    /// </summary>
    None
}
