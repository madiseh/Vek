namespace Vek.Core.DataGrid;

/// <summary>
/// Abstraction for validating item edits.
/// </summary>
/// <typeparam name="TItem">The type of item being edited.</typeparam>
public interface IVekDataGridValidator<TItem>
{
    /// <summary>
    /// Validates an edited item draft.
    /// </summary>
    /// <param name="item">The draft item.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    ValueTask<VekDataGridValidationResult> ValidateAsync(TItem item, CancellationToken cancellationToken = default);
}
