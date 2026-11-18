namespace Vek;

internal sealed class DataGridCellSelectionModel
{
    private (int Row, int Column)? _anchor;
    private (int Row, int Column)? _active;

    public DataGridCellRange? Range => _anchor is { } anchor && _active is { } active
        ? new DataGridCellRange(anchor.Row, anchor.Column, active.Row, active.Column)
        : null;

    public bool IsActive(int row, int column) => _active == (row, column);

    public DataGridCellRange? SetCell(int row, int column, bool extend)
    {
        if (!extend || _anchor is null)
        {
            _anchor = (row, column);
        }

        _active = (row, column);
        return Range;
    }

    public DataGridCellRange? SetRange(DataGridCellRange? range)
    {
        if (range is null)
        {
            _anchor = null;
            _active = null;
            return null;
        }

        var value = range.Value;
        _anchor = (value.StartRow, value.StartColumn);
        _active = (value.EndRow, value.EndColumn);
        return value;
    }

    public void BeginAt(int row, int column)
    {
        _anchor = (row, column);
        _active = (row, column);
    }

    public DataGridCellRange? MoveActiveCell(int rowDelta, int columnDelta, int rowCount, int columnCount)
    {
        if (rowCount <= 0 || columnCount <= 0)
        {
            return Range;
        }

        var active = _active ?? (0, 0);
        var nextRow = Math.Clamp(active.Row + rowDelta, 0, rowCount - 1);
        var nextColumn = Math.Clamp(active.Column + columnDelta, 0, columnCount - 1);
        return SetCell(nextRow, nextColumn, extend: true);
    }
}
