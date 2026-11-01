namespace Vek.Core.DataGrid;

/// <summary>Provides the context required by a custom column filter template.</summary>
public sealed class VekDataGridFilterContext
{
    /// <summary>Initializes a new filter template context.</summary>
    /// <param name="value">The currently applied filter value.</param>
    /// <param name="apply">Applies a filter value.</param>
    /// <param name="clear">Removes the filter.</param>
    public VekDataGridFilterContext(string? value, Action<string?> apply, Action clear)
        => (Value, Apply, Clear) = (value, apply, clear);

    /// <summary>Gets the currently applied filter value.</summary>
    public string? Value { get; }

    /// <summary>Applies a value using the column's configured filter operator.</summary>
    public Action<string?> Apply { get; }

    /// <summary>Removes the active filter.</summary>
    public Action Clear { get; }
}

