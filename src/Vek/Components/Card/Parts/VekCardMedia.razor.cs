using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// The media area of a <see cref="VekCard"/>, displaying an image or graphic sized to a set height.
    /// </summary>
    /// <seealso cref="VekCard" />
    /// <seealso cref="VekCardActions" />
    /// <seealso cref="VekCardContent" />
    /// <seealso cref="VekCardHeader" />
    public partial class VekCardMedia : VekComponentBase
    {
        protected string StyleString => StyleBuilder.Default($"background-image:url(\"{Image}\");height: {Height}px;")
            .AddStyle(Style)
            .Build();

        protected string Classname => new CssBuilder("vek-card-media")
            .AddClass(Class)
            .Build();

        /// <summary>
        /// Text for the <c>title</c> attribute which provides a basic tooltip.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public string? Title { get; set; }

        /// <summary>
        /// The URL of the image to display.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public string? Image { get; set; }

        /// <summary>
        /// The height, in pixels, of the <see cref="Image"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>300</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public int Height { get; set; } = 300;
    }
}
