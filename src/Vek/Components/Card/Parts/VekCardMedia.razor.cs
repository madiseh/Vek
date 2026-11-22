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
        protected string StyleString => StyleBuilder.Default($"background-image:url(\"{Image}\");")
            .AddStyle(AspectRatio is null ? $"height: {Height}px;" : $"aspect-ratio: {AspectRatio};")
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
        /// The aspect ratio of the media box, such as <c>16 / 9</c> or <c>1 / 1</c>.
        /// </summary>
        /// <remarks>
        /// When set, the aspect ratio determines the box height and takes precedence over <see cref="Height"/>. Defaults to <c>null</c>, which uses <see cref="Height"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Card.Behavior)]
        public string? AspectRatio { get; set; }

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
