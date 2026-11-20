using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Badges show notifications, counts, or status information on navigation items and icons.
    /// </summary>
    public partial class VekBadge : VekComponentBase
    {
        protected string Classname => new CssBuilder("vek-badge-root")
            .AddClass(Class)
            .Build();

        protected string WrapperClass => new CssBuilder("vek-badge-wrapper")
              .AddClass(BadgePositionClass)
            .Build();

        protected string BadgeClassname => new CssBuilder("vek-badge")
            .AddClass("vek-badge-dot", Dot)
            .AddClass("vek-badge-bordered", Bordered)
            .AddClass("vek-badge-icon", !string.IsNullOrEmpty(Icon) && !Dot)
              .AddClass(BadgePositionClass)
            .AddClass($"vek-elevation-{Elevation.ToString()}")
            .AddClass("vek-theme-" + Color.ToStringFast(true), Color != Color.Default)
            .AddClass("vek-badge-default", Color == Color.Default)
            .AddClass("vek-badge-overlap", Overlap)
            .AddClass(BadgeClass)
            .Build();

        /// <summary>
        /// The location of the badge.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Origin.TopRight" />.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Appearance)]
        public Origin Origin { get; set; } = Origin.TopRight;

        /// <summary>
        /// Positions the badge at a logical corner of its content.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, which preserves the position set by <see cref="Origin"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Appearance)]
        public BadgePosition? Position { get; set; }

        /// <summary>
        /// The size of the drop shadow.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>0</c>.  A higher number creates a heavier drop shadow.  Use a value of <c>0</c> for no shadow.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Appearance)]
        public int Elevation { set; get; } = 0;

        /// <summary>
        /// Displays this badge.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public bool Visible { get; set; } = true;

        /// <summary>
        /// The color of the badge.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.  Theme colors are supported.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// Displays a dot instead of any content.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public bool Dot { get; set; }

        /// <summary>
        /// Displays <see cref="ChildContent"/> over the main badge content.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Appearance)]
        public bool Overlap { get; set; }

        /// <summary>
        /// Displays a border around the badge.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Appearance)]
        public bool Bordered { get; set; }

        /// <summary>
        /// The icon to display in the badge.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public string? Icon { get; set; }

        /// <summary>
        /// The maximum number allowed in the <see cref="Content"/> property.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>99</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public int Max { get; set; } = 99;

        /// <summary>
        /// Caps numeric badge content at this value and appends a plus sign when exceeded.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, which uses <see cref="Max"/> (default <c>99</c>).
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public int? MaxNumber { get; set; }

        /// <summary>
        /// The <c>string</c> or <c>int</c> value to display inside the badge.
        /// </summary>
        /// <remarks>
        /// Supported types are <c>string</c> and <c>int</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public object? Content { get; set; }

        /// <summary>
        /// The CSS classes applied to the badge.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  You can use spaces to separate multiple classes.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Appearance)]
        public string? BadgeClass { get; set; }

        /// <summary>
        /// The <c>aria-label</c> for the badge.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public string? BadgeAriaLabel { get; set; }

        /// <summary>
        /// The content within this badge.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Badge.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Occurs when the badge has been clicked.
        /// </summary>
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        private string? _content;

        /// <summary>
        /// Indicates whether logical start and end positions should follow right-to-left layout.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        private string BadgePositionClass
        {
            get
            {
                if (Position is not { } position)
                    return $"vek-badge-{Origin.ToStringFast(true).Replace("-", " ")}";

                var vertical = position is BadgePosition.TopEnd or BadgePosition.TopStart ? "top" : "bottom";
                var isEnd = position is BadgePosition.TopEnd or BadgePosition.BottomEnd;
                var isRight = isEnd != RightToLeft;
                var horizontal = isRight ? "right" : "left";
                return $"vek-badge-{vertical} {horizontal}";
            }
        }

        // Without this gate the badge always registers a DOM click listener, even when nobody subscribed to OnClick.
        // A null delegate is not enough, because Razor still hands Blazor an EventCallback carrying this component as the receiver, which the render tree treats as a live handler.
        // Only a default EventCallback keeps the listener off the element.
        private EventCallback<MouseEventArgs> BadgeClickCallback =>
            OnClick.HasDelegate
                ? EventCallback.Factory.Create<MouseEventArgs>(this, this.AsNonRenderingEventHandler<MouseEventArgs>(HandleBadgeClick))
                : default;

        internal Task HandleBadgeClick(MouseEventArgs e)
        {
            if (OnClick.HasDelegate)
                return OnClick.InvokeAsync(e);

            return Task.CompletedTask;
        }

        protected override void OnParametersSet()
        {
            if (Content is string stringContent)
            {
                _content = stringContent;
            }
            else if (Content is int numberContent)
            {
                var maxNumber = MaxNumber ?? Max;
                if (numberContent > maxNumber)
                {
                    _content = maxNumber + "+";
                }
                else
                {
                    _content = numberContent.ToString();
                }
            }
            else
            {
                _content = null;
            }
        }
    }
}
