namespace Vek;

using System.Diagnostics.CodeAnalysis;

internal sealed class DataGridColumnVirtualizer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    private readonly List<DataGridColumnVirtualizationEntry<T>> _entries = [];
    private double _scrollLeft;
    private double _viewportWidth;
    private int _overscanCount;
    private bool _enabled;
    private bool _rightToLeft;

    internal IReadOnlyList<DataGridColumnVirtualizationEntry<T>> Entries => _entries;

    internal bool SetViewport(double scrollLeft, double viewportWidth, bool rightToLeft)
    {
        if (double.IsNaN(scrollLeft) || double.IsInfinity(scrollLeft))
        {
            scrollLeft = 0;
        }

        if (double.IsNaN(viewportWidth) || double.IsInfinity(viewportWidth) || viewportWidth < 0)
        {
            viewportWidth = 0;
        }

        if (_scrollLeft.Equals(scrollLeft) && _viewportWidth.Equals(viewportWidth) && _rightToLeft == rightToLeft)
        {
            return false;
        }

        _scrollLeft = scrollLeft;
        _viewportWidth = viewportWidth;
        _rightToLeft = rightToLeft;
        return true;
    }

    internal bool SetOptions(bool enabled, int overscanCount)
    {
        overscanCount = Math.Max(0, overscanCount);
        if (_enabled == enabled && _overscanCount == overscanCount)
        {
            return false;
        }

        _enabled = enabled;
        _overscanCount = overscanCount;
        return true;
    }

    internal bool Update(IReadOnlyList<Column<T>> columns)
    {
        var previousCount = _entries.Count;
        var previous = previousCount == 0 ? null : _entries.ToArray();
        _entries.Clear();

        var visibleCount = 0;
        for (var i = 0; i < columns.Count; i++)
        {
            if (!columns[i].HiddenState.Value)
            {
                visibleCount++;
            }
        }

        if (!_enabled)
        {
            AddAllColumns(columns);
            return !EntriesEqual(previous, previousCount);
        }

        if (_viewportWidth <= 0 || visibleCount <= 1)
        {
            AddAllVisibleColumns(columns);
            return !EntriesEqual(previous, previousCount);
        }

        var totalWidth = GetTotalWidth(columns);
        var normalizedScrollLeft = NormalizeScrollLeft(totalWidth);
        var viewportEnd = normalizedScrollLeft + _viewportWidth;
        var firstVisibleIndex = 0;
        var lastVisibleIndex = visibleCount - 1;
        var currentOffset = 0d;
        var visibleIndex = 0;

        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            if (column.HiddenState.Value)
            {
                continue;
            }

            var width = GetColumnWidth(column);
            if (currentOffset + width > normalizedScrollLeft)
            {
                firstVisibleIndex = visibleIndex;
                break;
            }

            currentOffset += width;
            visibleIndex++;
        }

        currentOffset = 0;
        visibleIndex = 0;
        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            if (column.HiddenState.Value)
            {
                continue;
            }

            currentOffset += GetColumnWidth(column);
            if (currentOffset >= viewportEnd)
            {
                lastVisibleIndex = visibleIndex;
                break;
            }

            visibleIndex++;
        }

        var firstIndex = Math.Max(0, firstVisibleIndex - _overscanCount);
        var lastIndex = Math.Min(visibleCount - 1, lastVisibleIndex + _overscanCount);
        visibleIndex = 0;
        var spacerWidth = 0d;

        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            if (column.HiddenState.Value)
            {
                continue;
            }

            var pinned = column.IsPinnedLeft || column.IsPinnedRight;
            if (pinned || (visibleIndex >= firstIndex && visibleIndex <= lastIndex))
            {
                if (spacerWidth > 0)
                {
                    _entries.Add(DataGridColumnVirtualizationEntry<T>.Spacer(spacerWidth));
                    spacerWidth = 0;
                }

                _entries.Add(DataGridColumnVirtualizationEntry<T>.ForColumn(column, visibleIndex));
            }
            else
            {
                spacerWidth += GetColumnWidth(column);
            }

            visibleIndex++;
        }

        if (spacerWidth > 0)
        {
            _entries.Add(DataGridColumnVirtualizationEntry<T>.Spacer(spacerWidth));
        }

        return !EntriesEqual(previous, previousCount);
    }

    private double NormalizeScrollLeft(double totalWidth)
    {
        if (!_rightToLeft)
        {
            return Math.Max(0, _scrollLeft);
        }

        return _scrollLeft < 0
            ? Math.Abs(_scrollLeft)
            : Math.Max(0, totalWidth - _viewportWidth - _scrollLeft);
    }

    private static double GetTotalWidth(IReadOnlyList<Column<T>> columns)
    {
        var totalWidth = 0d;
        for (var i = 0; i < columns.Count; i++)
        {
            if (!columns[i].HiddenState.Value)
            {
                totalWidth += GetColumnWidth(columns[i]);
            }
        }

        return totalWidth;
    }

    private static double GetColumnWidth(Column<T> column)
    {
        var width = column.GetEstimatedWidthPx();
        return double.IsFinite(width) && width > 0 ? width : 120;
    }

    private void AddAllVisibleColumns(IReadOnlyList<Column<T>> columns)
    {
        var visibleIndex = 0;
        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            if (!column.HiddenState.Value)
            {
                _entries.Add(DataGridColumnVirtualizationEntry<T>.ForColumn(column, visibleIndex++));
            }
        }
    }

    private void AddAllColumns(IReadOnlyList<Column<T>> columns)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            _entries.Add(DataGridColumnVirtualizationEntry<T>.ForColumn(columns[i], i));
        }
    }

    private bool EntriesEqual(DataGridColumnVirtualizationEntry<T>[]? previous, int previousCount)
    {
        if (previousCount != _entries.Count)
        {
            return false;
        }

        if (previous is null)
        {
            return _entries.Count == 0;
        }

        for (var i = 0; i < previousCount; i++)
        {
            if (!previous[i].Equals(_entries[i]))
            {
                return false;
            }
        }

        return true;
    }
}

internal readonly struct DataGridColumnVirtualizationEntry<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    private DataGridColumnVirtualizationEntry(Column<T>? column, double spacerWidth, int visibleIndex)
    {
        Column = column;
        SpacerWidth = spacerWidth;
        VisibleIndex = visibleIndex;
    }

    internal Column<T>? Column { get; }

    internal double SpacerWidth { get; }

    internal int VisibleIndex { get; }

    internal bool IsSpacer => Column is null;

    internal static DataGridColumnVirtualizationEntry<T> ForColumn(Column<T> column, int visibleIndex) => new(column, 0, visibleIndex);

    internal static DataGridColumnVirtualizationEntry<T> Spacer(double width) => new(null, width, -1);
}
