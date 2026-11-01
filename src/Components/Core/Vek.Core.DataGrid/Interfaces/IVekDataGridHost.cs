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

    /// <summary>Optionally consumes a completed paging transition; page-size changes are identified separately.</summary>
    ValueTask AfterPageChangeAsync(bool pageSizeChanged) => ValueTask.CompletedTask;
    /// <summary>Optionally consumes a completed column transition.</summary>
    ValueTask AfterColumnChangeAsync(bool orderChanged) => ValueTask.CompletedTask;
    /// <summary>Optionally consumes a completed selection transition.</summary>
    ValueTask AfterSelectionChangeAsync() => ValueTask.CompletedTask;
    /// <summary>Optionally consumes a completed row-detail transition.</summary>
    ValueTask AfterRowDetailsChangeAsync() => ValueTask.CompletedTask;
    /// <summary>Optionally consumes a completed group-expansion transition.</summary>
    ValueTask AfterGroupChangeAsync() => ValueTask.CompletedTask;
    /// <summary>Optionally prepares caller-owned validation state for a newly started edit.</summary>
    ValueTask AfterEditStartedAsync(TItem item) => ValueTask.CompletedTask;
    /// <summary>Optionally validates an editable draft using caller-owned validation.</summary>
    ValueTask<bool> ValidateEditDraftAsync(TItem item) => ValueTask.FromResult(true);
    /// <summary>Optionally persists a draft; returning false retains the active edit session.</summary>
    ValueTask<bool> CommitEditDraftAsync(TItem original, TItem draft) => ValueTask.FromResult(true);
    /// <summary>Optionally consumes a successfully committed draft.</summary>
    ValueTask AfterEditCommittedAsync(TItem item) => ValueTask.CompletedTask;
    /// <summary>Optionally consumes edit cancellation with the original row.</summary>
    ValueTask AfterEditCancelledAsync(TItem item) => ValueTask.CompletedTask;
    /// <summary>Optionally updates caller-owned validation state after replacing a draft.</summary>
    void AfterEditDraftChanged() { }
}
