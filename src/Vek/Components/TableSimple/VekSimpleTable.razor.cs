using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// A table similar to <see cref="VekTable{T}"/> but with basic styling features.
    /// </summary>
    /// <seealso cref="VekTable{T}"/>
    public partial class VekSimpleTable : VekComponentBase
    {
        protected string Classname =>
            new CssBuilder("vek-table vek-simple-table")
                .AddClass($"vek-table-dense", Dense)
                .AddClass($"vek-table-hover", Hover)
                .AddClass($"vek-table-bordered", Bordered)
                .AddClass($"vek-table-outlined", Outlined)
                .AddClass($"vek-table-striped", Striped)
                .AddClass($"vek-table-square", Square)
                .AddClass($"vek-table-sticky-header", FixedHeader)
                .AddClass($"vek-elevation-{Elevation}", !Outlined)
                .AddClass(Class)
                .Build();

        /// <summary>
        /// The size of the drop shadow.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.  A higher number creates a heavier drop shadow.  Use a value of <c>0</c> for no shadow.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public int Elevation { set; get; } = 1;

        /// <summary>
        /// Highlights rows when hovering over them.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public bool Hover { get; set; }

        /// <summary>
        /// Uses square corners for the table.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public bool Square { get; set; }

        /// <summary>
        /// Uses compact padding for all rows.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public bool Dense { get; set; }

        /// <summary>
        /// Shows borders around the table.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public bool Outlined { get; set; }

        /// <summary>
        /// Shows left and right borders for each table cell.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public bool Bordered { get; set; }

        /// <summary>
        /// Uses alternating colors for table rows.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public bool Striped { get; set; }

        /// <summary>
        /// Fixes the table header in place while the table is scrolled.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When <c>true</c>, the CSS <c>Height</c> must also be set.  Example: <c>Style="height:300px;"</c>
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Appearance)]
        public bool FixedHeader { get; set; }

        /// <summary>
        /// The content within this table.
        /// </summary>
        /// <remarks>
        /// Use table HTML tags such as <c>&lt;thead&gt;</c>, <c>&lt;tbody&gt;</c>, <c>&lt;tr&gt;</c>, <c>&lt;th&gt;</c> or <c>&lt;td&gt;</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.SimpleTable.Behavior)]
        public RenderFragment? ChildContent { get; set; }
    }
}
