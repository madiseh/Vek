namespace Vek.Core.DataGrid;

/// <summary>
/// Represents a platform-neutral server load request.
/// </summary>
/// <param name="Page">The one-based requested page index.</param>
/// <param name="PageSize">The page size.</param>
/// <param name="Sorts">The active sorts in priority order.</param>
/// <param name="Filters">The active filters.</param>
public sealed record VekDataGridServerRequest(
    int Page,
    int PageSize,
    IReadOnlyList<VekDataGridSort> Sorts,
    IReadOnlyList<VekDataGridFilter> Filters);

