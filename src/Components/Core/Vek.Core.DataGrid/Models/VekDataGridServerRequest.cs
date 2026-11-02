namespace Vek.Core.DataGrid;

/// <summary>
/// Represents a platform-neutral server load request.
/// </summary>
/// <param name="Page">The one-based requested page index.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="Sorts">The active sorts in priority order.</param>
/// <param name="Filters">The active filters.</param>
/// <param name="GlobalSearchText">The current global search text.</param>
/// <param name="SearchColumnKeys">The explicit global search targets, or null to use columns marked searchable.</param>
public sealed record VekDataGridServerRequest(
    int Page,
    int PageSize,
    IReadOnlyList<VekDataGridSort> Sorts,
    IReadOnlyList<VekDataGridFilter> Filters,
    string GlobalSearchText = "",
    IReadOnlyList<string>? SearchColumnKeys = null);

