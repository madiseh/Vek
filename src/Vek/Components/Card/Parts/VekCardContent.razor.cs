using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// The primary content area of a <see cref="VekCard"/>, holding its text and body elements.
    /// </summary>
    /// <seealso cref="VekCard" />
    /// <seealso cref="VekCardActions" />
    /// <seealso cref="VekCardHeader" />
    /// <seealso cref="VekCardMedia" />
    public partial class VekCardContent : VekComponentBase
    {
        protected string Classname => new CssBuilder("vek-card-content")
            .AddClass("vek-card-content-padding", ParentCard?.ContentPadding ?? true)
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
