using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// A label which describes a <see cref="VekInput{T}"/> component.
    /// </summary>
    public partial class VekInputLabel : VekComponentBase
    {
        protected string Classname => new CssBuilder()
            .AddClass("vek-input-label")
            .AddClass("vek-input-label-animated")
            .AddClass($"vek-input-label-{Variant.ToStringFast(true)}")
            .AddClass($"vek-input-label-margin-{Margin.ToStringFast(true)}", Margin != Margin.None)
            .AddClass("vek-disabled", Disabled)
            .AddClass("vek-input-error", Error)
            .AddClass(Class)
            .Build();

        /// <summary>
        /// The content within this label.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Prevents the user from interacting with this label.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Disabled { get; set; }

        /// <summary>
        /// Displays this label in an error state.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Error { get; set; }

        /// <summary>
        /// The display variant of this label.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Variant.Text"/>.
        /// </remarks>
        [Parameter]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// The amount of vertical spacing to apply.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Margin.None"/>.
        /// </remarks>
        [Parameter]
        public Margin Margin { get; set; } = Margin.None;

        /// <summary>
        /// For WCAG accessibility, the ID of the input component related to this label.
        /// </summary>
        /// <remarks>
        /// Defaults to an empty string.
        /// </remarks>
        [Parameter]
        public string ForId { get; set; } = string.Empty;
    }
}
