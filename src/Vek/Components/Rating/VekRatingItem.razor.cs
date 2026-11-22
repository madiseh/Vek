// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Extensions;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// A clickable item as part of a <see cref="VekRating"/>.
    /// </summary>
    /// <seealso cref="VekRating"/>
    public partial class VekRatingItem : VekComponentBase
    {
        /// <summary>
        /// The CSS classes applied to this component.
        /// </summary>
        protected string ClassName =>
            new CssBuilder("vek-rating-item")
                .AddClass($"vek-rating-item-half-{HalfPosition}", HalfPosition is not null)
                .AddClass("vek-ripple", Ripple && !ReadOnly)
                .AddClass($"vek-{Color.ToStringFast(true)}-text", Color != Color.Default)
                .AddClass("vek-rating-item-active", Active)
                .AddClass("vek-disabled", Disabled)
                .AddClass("vek-readonly", ReadOnly)
                .AddClass(Class)
                .Build();

        /// <summary>
        /// The parent <see cref="VekRating"/> containing this item.
        /// </summary>
        [CascadingParameter]
        private VekRating? Rating { get; set; }

        /// <summary>
        /// The value for this item.
        /// </summary>
        /// <remarks>
        /// Defaults to the item's whole or fractional position in the parent <see cref="VekRating"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Rating.Data)]
        public int ItemValue { get; set; }

        /// <summary>
        /// Supplies a fractional position when this item represents part of a rating increment.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Rating.Data)]
        public double? PreciseItemValue { get; set; }

        /// <summary>Identifies the visible half when this item is part of a half-step rating.</summary>
        [Parameter]
        [Category(CategoryTypes.Rating.Appearance)]
        public string? HalfPosition { get; set; }

        /// <summary>
        /// The size of this item.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Size.Medium"/>.  When used within a <see cref="VekRating"/>, the value of <see cref="VekRating.Size"/> is applied instead.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Rating.Appearance)]
        public Size Size { get; set; } = Size.Medium;

        /// <summary>
        /// The color of this item.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.  When used within a <see cref="VekRating"/>, the value of <see cref="VekRating.Color"/> is applied instead.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Rating.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// Show a ripple effect when the user clicks the button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  When used within a <see cref="VekRating"/>, the value of <see cref="VekRating.Ripple"/> is applied instead.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Rating.Appearance)]
        public bool Ripple { get; set; } = false;

        /// <summary>
        /// Prevents the user from interacting with this item, and uses a disabled style.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Rating.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Prevents this item from being changed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Rating.Behavior)]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Occurs when this item is clicked.
        /// </summary>
        /// <remarks>
        /// When clicked, the <see cref="VekRating.SelectedValue"/> is changed.
        /// </remarks>
        [Parameter]
        public EventCallback<int> ItemClicked { get; set; }

        /// <summary>Occurs when a fractional rating position is clicked.</summary>
        [Parameter]
        [Category(CategoryTypes.Rating.Data)]
        public EventCallback<double> PreciseItemClicked { get; set; }

        /// <summary>
        /// Occurs when the user hovers over this item.
        /// </summary>
        /// <remarks>
        /// When hovered, the <see cref="VekRating.HoveredValue"/> is changed.
        /// </remarks>
        [Parameter]
        public EventCallback<int?> ItemHovered { get; set; }

        /// <summary>Occurs when the pointer enters or leaves a fractional rating position.</summary>
        [Parameter]
        [Category(CategoryTypes.Rating.Data)]
        public EventCallback<double?> PreciseItemHovered { get; set; }

        [Parameter]
        public RenderFragment<int>? ItemTemplate { get; set; }

        [Parameter]
        public Color? ItemHoverColor { get; set; }

        [Parameter]
        public Color? ItemActiveColor { get; set; }

        [Parameter]
        public bool Clearable { get; set; } = true;

        internal Color ItemColor { get; set; }

        internal string? ItemIcon { get; set; }

        internal bool Active { get; set; }

        private double CurrentItemValue => PreciseItemValue ?? ItemValue;

        private bool Checked => CurrentItemValue == Rating?.CurrentValue;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            ItemIcon = SelectIcon();
            ItemColor = SelectIconColor();
        }

        internal string? SelectIcon()
        {
            if (Rating is null)
            {
                return null;
            }

            if (Rating.HoveredValue.HasValue && Rating.HoveredValue.Value >= CurrentItemValue)
            {
                // full icon when RatingItem hovered
                return Rating.FullIcon;
            }

            var ratingSelectedValue = Rating.CurrentValue;
            if (ratingSelectedValue >= CurrentItemValue)
            {
                if (Rating.HoveredValue.HasValue && Rating.HoveredValue.Value < CurrentItemValue)
                {
                    // empty icon when equal or higher RatingItem value clicked, but less value hovered
                    return Rating.EmptyIcon;
                }

                // full icon when equal or higher RatingItem value clicked
                return Rating.FullIcon;
            }

            // empty icon when this or higher RatingItem is not clicked and not hovered
            return Rating.EmptyIcon;
        }

        internal Color SelectIconColor()
        {
            if (Rating is null)
            {
                return Color.Inherit;
            }

            if (Rating.HoveredValue.HasValue && Rating.HoveredValue.Value >= CurrentItemValue && Rating.ItemHoverColor.HasValue)
            {
                return Rating.ItemHoverColor.Value;
            }

            if (Rating.CurrentValue >= CurrentItemValue && Rating.ItemActiveColor.HasValue && (!Rating.HoveredValue.HasValue || Rating.HoveredValue.Value >= CurrentItemValue))
            {
                return Rating.ItemActiveColor.Value;
            }

            if (Rating.FullIconColor is null || Rating.EmptyIconColor is null)
            {
                return Color.Inherit;
            }

            if (Rating.HoveredValue.HasValue && Rating.HoveredValue.Value >= CurrentItemValue)
            {
                return Rating.FullIconColor.Value;
            }

            var ratingSelectedValue = Rating.CurrentValue;
            if (ratingSelectedValue >= CurrentItemValue)
            {
                if (Rating.HoveredValue.HasValue && Rating.HoveredValue.Value < CurrentItemValue)
                {
                    // empty icon color when equal or higher RatingItem value clicked, but less value hovered
                    return Rating.EmptyIconColor.Value;
                }

                return Rating.ItemActiveColor ?? Rating.FullIconColor.Value;
            }

            // empty icon color when this or higher RatingItem is not clicked and not hovered
            return Rating.EmptyIconColor.Value;
        }

        // rating item lose hover
        internal Task HandlePointerOutAsync(PointerEventArgs e)
        {
            if (Disabled || ReadOnly || Rating is null)
            {
                return Task.CompletedTask;
            }

            Active = false;

            return PreciseItemHovered.HasDelegate
                ? PreciseItemHovered.InvokeAsync(null)
                : ItemHovered.InvokeAsync(null);
        }

        internal Task HandlePointerOverAsync(PointerEventArgs e)
        {
            if (Disabled || ReadOnly)
            {
                return Task.CompletedTask;
            }

            Active = true;

            return PreciseItemHovered.HasDelegate
                ? PreciseItemHovered.InvokeAsync(CurrentItemValue)
                : ItemHovered.InvokeAsync(ItemValue);
        }

        private Task HandleClickAsync()
        {
            if (Disabled || ReadOnly)
            {
                return Task.CompletedTask;
            }

            Active = false;
            var ratingSelectedValue = Rating?.CurrentValue;

            if (PreciseItemClicked.HasDelegate)
            {
                return PreciseItemClicked.InvokeAsync(Clearable && ratingSelectedValue == CurrentItemValue ? 0 : CurrentItemValue);
            }

            return ItemClicked.InvokeAsync(Clearable && ratingSelectedValue == ItemValue ? 0 : ItemValue);
        }
    }
}
