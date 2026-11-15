using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// The action bar of a <see cref="VekCard"/>, typically holding buttons that trigger the card's related actions.
    /// </summary>
    /// <seealso cref="VekCard" />
    /// <seealso cref="VekCardContent" />
    /// <seealso cref="VekCardHeader" />
    /// <seealso cref="VekCardMedia" />
    public partial class VekCardActions : VekComponentBase
    {
        protected string Classname => new CssBuilder("vek-card-actions")
            .AddClass("vek-card-actions-padding", ParentCard?.ContentPadding ?? true)
            .AddClass(Class)
            .Build();

        [CascadingParameter]
        private VekCard? ParentCard { get; set; }

        /// <summary>
        /// The content within this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public RenderFragment? ChildContent { get; set; }
    }
}
