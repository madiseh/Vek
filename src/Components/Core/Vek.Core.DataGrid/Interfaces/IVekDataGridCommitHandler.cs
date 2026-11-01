namespace Vek.Core.DataGrid;

/// <summary>
/// Callback seam for committing an item edit to persistence.
/// </summary>
/// <typeparam name="TItem">The type of item.</typeparam>
public interface IVekDataGridCommitHandler<TItem>
{
    /// <summary>
    /// Commits the edited item.
    /// </summary>
    /// <param name="originalItem">The original unmodified item.</param>
    /// <param name="draftItem">The edited draft item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if commit succeeded; otherwise false.</returns>
    ValueTask<bool> CommitAsync(TItem originalItem, TItem draftItem, CancellationToken cancellationToken = default);
}
