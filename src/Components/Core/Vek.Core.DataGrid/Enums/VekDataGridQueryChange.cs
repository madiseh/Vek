namespace Vek.Core.DataGrid;

/// <summary>Identifies query dimensions affected by a completed shared operation.</summary>
[Flags]
public enum VekDataGridQueryChange
{
    /// <summary>No query dimension changed.</summary>
    None = 0,
    /// <summary>Filter descriptors changed.</summary>
    Filters = 1,
    /// <summary>Sort descriptors changed.</summary>
    Sorts = 2,
    /// <summary>The requested page changed.</summary>
    Page = 4,
    /// <summary>The requested page size changed.</summary>
    PageSize = 8,
    /// <summary>Grouping changed.</summary>
    Groups = 16,
    /// <summary>A complete query state was restored.</summary>
    State = Filters | Sorts | Page | PageSize | Groups
}
