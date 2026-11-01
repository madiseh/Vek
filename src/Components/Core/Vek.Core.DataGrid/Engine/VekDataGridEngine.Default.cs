namespace Vek.Core.DataGrid;

/// <summary>Provides a template-neutral compatibility engine using <see cref="object"/> templates.</summary>
/// <typeparam name="TItem">The type of row item.</typeparam>
public sealed class VekDataGridEngine<TItem> : VekDataGridEngine<TItem, object>
{
    /// <summary>Initializes a compatibility engine with object-typed columns.</summary>
    /// <param name="columns">Optional initial columns.</param>
    /// <param name="itemKeySelector">Optional selector for stable item keys.</param>
    public VekDataGridEngine(
        IEnumerable<IVekDataGridColumn<TItem, object>>? columns = null,
        Func<TItem, string>? itemKeySelector = null)
        : base(columns, itemKeySelector)
    {
    }
}
