namespace Vek.Core.DataGrid;

/// <summary>Optionally consumes a completed query transition, for example to load the requested data.</summary>
/// <remarks>Hosts implement this capability only when query transitions require additional work.</remarks>
public interface IVekDataGridQueryObserver
{
    /// <summary>Completes caller-owned work after the query has changed.</summary>
    /// <param name="change">The query dimensions changed by the operation.</param>
    /// <returns>Completion of the caller-owned work; failures propagate without rolling back the query.</returns>
    ValueTask OnQueryChangedAsync(VekDataGridQueryChange change);
}
