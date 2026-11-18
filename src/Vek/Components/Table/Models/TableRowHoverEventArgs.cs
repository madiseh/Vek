using System;
using Microsoft.AspNetCore.Components.Web;

namespace Vek;


/// <summary>
/// The information passed when entering a row for a <see cref="VekTable{T}"/>.
/// </summary>
/// <typeparam name="T">The type of data displayed in the table.</typeparam>
public class TableRowHoverEventArgs<T> : EventArgs
{
    /// <summary>
    /// The coordinates of the hover.
    /// </summary>
    public PointerEventArgs PointerEventArgs { get; }

    /// <summary>
    /// The row being hovered over.
    /// </summary>
    public VekTr Row { get; }

    /// <summary>
    /// The data related to the row being hovered over.
    /// </summary>
    public T? Item { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="pointerEventArgs">The coordinates of the hover.</param>
    /// <param name="row">The row being hovered over.</param>
    /// <param name="item">The data related to the row being hovered over.</param>
    public TableRowHoverEventArgs(PointerEventArgs pointerEventArgs, VekTr row, T? item)
    {
        PointerEventArgs = pointerEventArgs;
        Row = row;
        Item = item;
    }
}
