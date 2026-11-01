namespace Vek.Core.DataGrid;

/// <summary>
/// Result of an edit validation check.
/// </summary>
/// <param name="IsValid">True if validation succeeded.</param>
/// <param name="Errors">List of validation error messages if any.</param>
public sealed record VekDataGridValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    /// <summary>Represents a successful validation result.</summary>
    public static readonly VekDataGridValidationResult Success = new(true, Array.Empty<string>());

    /// <summary>Creates a failed validation result.</summary>
    public static VekDataGridValidationResult Failed(params string[] errors) => new(false, errors);
}

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
