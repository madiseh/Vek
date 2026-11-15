using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// The header area of a <see cref="VekCard"/>, typically holding an avatar, title, and action buttons.
    /// </summary>
    /// <seealso cref="VekCard" />
    /// <seealso cref="VekCardActions" />
    /// <seealso cref="VekCardContent" />
    /// <seealso cref="VekCardMedia" />
    public partial class VekCardHeader : VekComponentBase
    {
        protected string Classname => new CssBuilder("vek-card-header")
            .AddClass("vek-card-header-padding", ParentCard?.ContentPadding ?? true)
            .AddClass(Class)
            .Build();

        [CascadingParameter]
        private VekCard? ParentCard { get; set; }

        /// <summary>
        /// The avatar to display within this header.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public RenderFragment? CardHeaderAvatar { get; set; }

        /// <summary>
        /// The main content of this header.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public RenderFragment? CardHeaderContent { get; set; }

        /// <summary>
        /// The actions displayed within this header.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public RenderFragment? CardHeaderActions { get; set; }

        /// <summary>
        /// The custom content within this header.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public RenderFragment? ChildContent { get; set; }
    }
}
