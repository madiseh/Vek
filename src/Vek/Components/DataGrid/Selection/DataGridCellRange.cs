namespace Vek;

/// <summary>
/// Describes a rectangular range of zero-based data grid row and column coordinates.
/// </summary>
/// <param name="StartRow">The row where the range was started.</param>
/// <param name="StartColumn">The column where the range was started.</param>
/// <param name="EndRow">The row at the active end of the range.</param>
/// <param name="EndColumn">The column at the active end of the range.</param>
public readonly record struct DataGridCellRange(int StartRow, int StartColumn, int EndRow, int EndColumn)
{
    /// <summary>
    /// Gets the first row included in the range, regardless of selection direction.
    /// </summary>
    public int MinRow => Math.Min(StartRow, EndRow);

    /// <summary>
    /// Gets the last row included in the range, regardless of selection direction.
    /// </summary>
    public int MaxRow => Math.Max(StartRow, EndRow);

    /// <summary>
    /// Gets the first column included in the range, regardless of selection direction.
    /// </summary>
    public int MinColumn => Math.Min(StartColumn, EndColumn);

    /// <summary>
    /// Gets the last column included in the range, regardless of selection direction.
    /// </summary>
    public int MaxColumn => Math.Max(StartColumn, EndColumn);

    /// <summary>
    /// Gets the number of rows included in the range.
    /// </summary>
    public int RowCount => MaxRow - MinRow + 1;

    /// <summary>
    /// Gets the number of columns included in the range.
    /// </summary>
    public int ColumnCount => MaxColumn - MinColumn + 1;

    /// <summary>
    /// Gets the total number of cells included in the range.
    /// </summary>
    public int CellCount => RowCount * ColumnCount;

    /// <summary>
    /// Determines whether the specified cell coordinates are within the range.
    /// </summary>
    /// <param name="row">The zero-based row coordinate.</param>
    /// <param name="column">The zero-based column coordinate.</param>
    /// <returns><see langword="true"/> when the cell is contained in the range.</returns>
    public bool Contains(int row, int column)
    {
        return row >= MinRow && row <= MaxRow && column >= MinColumn && column <= MaxColumn;
    }
}
