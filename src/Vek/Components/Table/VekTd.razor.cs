using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// A cell within a <see cref="VekTr" />, <see cref="VekTHeadRow"/>, or <see cref="VekTFootRow"/> row component.
    /// </summary>
    public partial class VekTd : VekComponentBase
    {
        protected string Classname =>
            new CssBuilder("vek-table-cell")
                .AddClass(Context?.Table?.CellClass)
                .AddClass("vek-table-cell-hide", HideSmall)
                .AddClass(Class)
                .Build();

        /// <summary>
        /// The current state of the <see cref="VekTable{T}"/> containing this group.
        /// </summary>
        [CascadingParameter]
        public TableContext? Context { get; set; }

        /// <summary>
        /// The content within this cell.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The label for this cell when the table is in small-device mode.
        /// </summary>
        [Parameter]
        public string? DataLabel { get; set; }

        /// <summary>
        /// Hides this cell if the breakpoint is smaller than <see cref="VekTableBase.Breakpoint"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool HideSmall { get; set; }

        /// <summary>
        /// Suppresses the browser's default context menu when a custom <c>@oncontextmenu</c> handler is supplied via <see cref="VekComponentBase.UserAttributes"/>.
        /// </summary>
        protected bool PreventContextMenuDefault => UserAttributes is not null && UserAttributes.ContainsKey("oncontextmenu");
    }
}
