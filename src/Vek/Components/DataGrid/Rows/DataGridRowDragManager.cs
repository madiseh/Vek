namespace Vek;

internal sealed class DataGridRowDragManager<T>
{
    private T _sourceItem = default!;
    private T _targetItem = default!;
    private int _sourceIndex = -1;
    private int _targetIndex = -1;
    private bool _insertAfterTarget;

    internal bool IsDragging { get; private set; }

    internal bool IsKeyboardDrag { get; private set; }

    internal int TargetIndex => _targetIndex;

    internal bool InsertAfterTarget => _insertAfterTarget;

    internal bool IsSource(T item, IEqualityComparer<T> comparer)
        => IsDragging && comparer.Equals(_sourceItem, item);

    internal bool IsTarget(T item, IEqualityComparer<T> comparer)
        => IsDragging && comparer.Equals(_targetItem, item);

    internal void Start(T item, IReadOnlyList<T> items, IEqualityComparer<T> comparer, bool keyboard)
    {
        var index = IndexOf(item, items, comparer);
        if (index < 0)
        {
            return;
        }

        _sourceItem = item;
        _targetItem = item;
        _sourceIndex = index;
        _targetIndex = index;
        _insertAfterTarget = false;
        IsKeyboardDrag = keyboard;
        IsDragging = true;
    }

    internal bool MoveTo(T target, bool after, IReadOnlyList<T> items, IEqualityComparer<T> comparer)
    {
        if (!IsDragging)
        {
            return false;
        }

        var index = IndexOf(target, items, comparer);
        if (index < 0)
        {
            return false;
        }

        var changed = _targetIndex != index || _insertAfterTarget != after;
        _targetItem = target;
        _targetIndex = index;
        _insertAfterTarget = after;
        return changed;
    }

    internal bool MoveKeyboard(int delta, IReadOnlyList<T> items)
    {
        if (!IsDragging || !IsKeyboardDrag || items.Count == 0)
        {
            return false;
        }

        var nextIndex = Math.Clamp(_targetIndex + delta, 0, items.Count - 1);
        var changed = nextIndex != _targetIndex;
        _targetItem = items[nextIndex];
        _targetIndex = nextIndex;
        _insertAfterTarget = _targetIndex > _sourceIndex;
        return changed;
    }

    internal DataGridRowReorderedEventArgs<T>? Complete(IReadOnlyList<T> items, IEqualityComparer<T> comparer)
    {
        if (!IsDragging || _targetIndex < 0 || _targetIndex >= items.Count)
        {
            Cancel();
            return null;
        }

        var targetItem = items[_targetIndex];
        var insertionIndex = _targetIndex + (_insertAfterTarget ? 1 : 0);
        if (_sourceIndex < insertionIndex)
        {
            insertionIndex--;
        }

        insertionIndex = Math.Clamp(insertionIndex, 0, items.Count - 1);
        var result = insertionIndex == _sourceIndex
            ? null
            : new DataGridRowReorderedEventArgs<T>(_sourceItem, targetItem, insertionIndex);
        Cancel();
        return result;
    }

    internal void Cancel()
    {
        _sourceItem = default!;
        _targetItem = default!;
        _sourceIndex = -1;
        _targetIndex = -1;
        _insertAfterTarget = false;
        IsKeyboardDrag = false;
        IsDragging = false;
    }

    private static int IndexOf(T item, IReadOnlyList<T> items, IEqualityComparer<T> comparer)
    {
        for (var index = 0; index < items.Count; index++)
        {
            if (comparer.Equals(item, items[index]))
            {
                return index;
            }
        }

        return -1;
    }
}
