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
