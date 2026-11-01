namespace Vek.Core.DataGrid;

/// <summary>Provides the per-instance engine used by shared grid operations.</summary>
/// <typeparam name="TItem">The row item type.</typeparam>
/// <typeparam name="TTemplate">An opaque template value supplied by the caller.</typeparam>
public interface IVekDataGridHost<TItem, TTemplate>
{
    /// <summary>Gets the authoritative shared state and behavior for this grid.</summary>
    VekDataGridEngine<TItem, TTemplate> Engine { get; }

    /// <summary>Completes host-owned work after a successful shared filtering operation.</summary>
    /// <returns>Completion of the host work; failures propagate without rolling back the completed mutation.</returns>
    ValueTask AfterCoreChangeAsync();

    /// <summary>Optionally completes host-owned work after a successful shared sorting change.</summary>
    /// <returns>Completion of the host work; the default requires no additional work.</returns>
    /// <remarks>This continuation is independent of filtering notifications. Failures do not roll back the completed change.</remarks>
    ValueTask AfterSortChangeAsync() => ValueTask.CompletedTask;
}
