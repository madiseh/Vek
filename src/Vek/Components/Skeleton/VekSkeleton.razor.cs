using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// Displays a placeholder preview of content before the data gets loaded, reducing load-time frustration.
    /// </summary>
    public partial class VekSkeleton : VekComponentBase
    {
        protected string Classname =>
            new CssBuilder("vek-skeleton")
                .AddClass($"vek-skeleton-{SkeletonType.ToStringFast(true)}")
                .AddClass($"vek-skeleton-{Animation.ToStringFast(true)}")
                .AddClass(Class)
                .Build();

        protected string Stylename =>
            new StyleBuilder()
                .AddStyle("height", $"{Height}", !string.IsNullOrEmpty(Height))
                .AddStyle("width", $"{Width}", !string.IsNullOrEmpty(Width))
                  .AddStyle("border-radius", BorderRadius, !string.IsNullOrEmpty(BorderRadius))
                .AddStyle(Style)
                .Build();

        /// <summary>
        /// The width of this skeleton.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  Values can be in pixels (e.g. <c>"300px"</c>) or percentages (e.g. <c>"30%"</c>).
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Skeleton.Appearance)]
        public string? Width { set; get; }

        /// <summary>
        /// The height of this skeleton.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  Values can be in pixels (e.g. <c>"300px"</c>) or percentages (e.g. <c>"30%"</c>).
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Skeleton.Appearance)]
        public string? Height { set; get; }

          /// <summary>
          /// Sets the corner radius for the skeleton shape.
          /// </summary>
          /// <remarks>
          /// Defaults to the shape's built-in radius.
          /// </remarks>
          [Parameter]
          [Category(CategoryTypes.Skeleton.Appearance)]
          public string? BorderRadius { get; set; }

        /// <summary>
        /// The shape of this skeleton.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="SkeletonType.Text"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Skeleton.Appearance)]
        public SkeletonType SkeletonType { set; get; } = SkeletonType.Text;

        /// <summary>
        /// The type of animation to display.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Animation.Pulse"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Skeleton.Appearance)]
        public Animation Animation { set; get; } = Animation.Pulse;
    }
}
