using Vek.Utilities;

namespace Vek;

internal sealed class DataGridDetailViewManager<T>
{
    internal DataGridDetailViewManager(HashSet<T> expandedItems)
    {
        ExpandedItems = expandedItems;
    }

    internal HashSet<T> ExpandedItems { get; private set; }

    internal bool IsExpanded(T item) => ExpandedItems.Contains(item);

    internal IReadOnlyList<DataGridHierarchyVisibilityToggledEventArgs<T>> SetExpanded(
        T item,
        bool expanded,
        bool autoCollapse,
        Func<T, bool>? isDisabled)
    {
        if (isDisabled?.Invoke(item) is true)
        {
            return [];
        }

        var changes = new List<DataGridHierarchyVisibilityToggledEventArgs<T>>();
        if (expanded && autoCollapse)
        {
            foreach (var expandedItem in ExpandedItems.Where(candidate => !ExpandedItems.Comparer.Equals(candidate, item)).ToList())
            {
                ExpandedItems.Remove(expandedItem);
                changes.Add(new(expandedItem, false));
            }
        }

        var changed = expanded ? ExpandedItems.Add(item) : ExpandedItems.Remove(item);
        if (changed)
        {
            changes.Add(new(item, expanded));
        }

        return changes;
    }

    internal IReadOnlyList<DataGridHierarchyVisibilityToggledEventArgs<T>> Toggle(
        T item,
        bool autoCollapse,
        Func<T, bool>? isDisabled)
    {
        if (autoCollapse)
        {
            return SetExpanded(item, true, true, isDisabled);
        }

        return SetExpanded(item, !IsExpanded(item), false, isDisabled);
    }

    internal IReadOnlyList<DataGridHierarchyVisibilityToggledEventArgs<T>> ExpandAll(
        IEnumerable<T> items,
        Func<T, bool>? isDisabled)
    {
        var changes = new List<DataGridHierarchyVisibilityToggledEventArgs<T>>();
        foreach (var item in items)
        {
            if (isDisabled?.Invoke(item) is not true && ExpandedItems.Add(item))
            {
                changes.Add(new(item, true));
            }
        }

        return changes;
    }

    internal IReadOnlyList<DataGridHierarchyVisibilityToggledEventArgs<T>> CollapseAll(Func<T, bool>? isDisabled)
    {
        var changes = new List<DataGridHierarchyVisibilityToggledEventArgs<T>>();
        foreach (var item in ExpandedItems.Where(item => isDisabled?.Invoke(item) is not true).ToList())
        {
            ExpandedItems.Remove(item);
            changes.Add(new(item, false));
        }

        return changes;
    }

    internal void UpdateComparer(IEqualityComparer<T>? comparer)
    {
        ExpandedItems = new HashSet<T>(ExpandedItems, comparer ?? EqualityComparer<T>.Default);
    }
}
