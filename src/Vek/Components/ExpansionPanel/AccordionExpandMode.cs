namespace Vek;

/// <summary>
/// Controls how many expansion panels can remain open in an accordion.
/// </summary>
public enum AccordionExpandMode
{
    /// <summary>
    /// Keeps exactly one enabled panel expanded when panels are available.
    /// </summary>
    Single,

    /// <summary>
    /// Allows any number of panels to be expanded at the same time.
    /// </summary>
    Multiple,

    /// <summary>
    /// Allows one panel or no panels to be expanded.
    /// </summary>
    SingleOrNone
}

/// <summary>
/// Positions an expansion panel's icon relative to its title.
/// </summary>
public enum AccordionExpandIconPosition
{
    /// <summary>
    /// Places the icon before the title.
    /// </summary>
    Start,

    /// <summary>
    /// Places the icon after the title.
    /// </summary>
    End
}
