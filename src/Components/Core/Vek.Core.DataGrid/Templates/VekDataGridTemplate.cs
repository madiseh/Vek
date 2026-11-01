namespace Vek.Core.DataGrid;

/// <summary>Produces a platform-native template for a platform-neutral DataGrid context.</summary>
/// <typeparam name="TContext">The shared context supplied to this template slot.</typeparam>
/// <typeparam name="TTemplate">The template type owned by the UI adapter.</typeparam>
/// <param name="context">The data or action context for the template slot.</param>
/// <returns>The native template value consumed by the UI adapter.</returns>
public delegate TTemplate VekDataGridTemplate<in TContext, out TTemplate>(TContext context);

