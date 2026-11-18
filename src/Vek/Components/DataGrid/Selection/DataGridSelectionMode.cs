namespace Vek;

/// <summary>
/// Determines which data grid selection behavior is available.
/// </summary>
public enum DataGridSelectionMode
{
    /// <summary>
    /// Selects data grid rows using the existing row selection behavior.
    /// </summary>
    Row,

    /// <summary>
    /// Selects rectangular ranges of cells.
    /// </summary>
    CellRange
}
