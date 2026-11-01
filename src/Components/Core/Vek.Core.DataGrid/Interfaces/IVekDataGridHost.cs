namespace Vek.Core.DataGrid;

/// <summary>Provides the per-instance engine used by shared grid operations.</summary>
/// <typeparam name="TItem">The row item type.</typeparam>
/// <typeparam name="TTemplate">An opaque template value supplied by the caller.</typeparam>
public interface IVekDataGridHost<TItem, TTemplate>
{
    /// <summary>Gets the authoritative shared state and behavior for this grid.</summary>
    VekDataGridEngine<TItem, TTemplate> Engine { get; }
}
