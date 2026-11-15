using Microsoft.AspNetCore.Components;
using Vek.Extensions;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// A container for a <see cref="VekDrawer"/> component.
    /// </summary>
    /// <seealso cref="VekDrawer"/>
    /// <seealso cref="VekDrawerHeader"/>
    public partial class VekDrawerContainer : VekComponentBase
    {
        protected bool Fixed { get; set; } = false;
        private readonly List<VekDrawer> _drawers = new();

        protected virtual string Classname =>
            new CssBuilder()
                .AddClass(GetDrawerClass(FindLeftDrawer()))
                .AddClass(GetDrawerClass(FindRightDrawer()))
                .AddClass(Class)
                .Build();

        protected string Stylename =>
            new StyleBuilder()
                .AddStyle("--vek-drawer-width-left", GetDrawerWidth(FindLeftDrawer()), !string.IsNullOrEmpty(GetDrawerWidth(FindLeftDrawer())))
                .AddStyle("--vek-drawer-width-right", GetDrawerWidth(FindRightDrawer()), !string.IsNullOrEmpty(GetDrawerWidth(FindRightDrawer())))
                .AddStyle("--vek-drawer-height-top", GetDrawerHeight(FindTopDrawer()), !string.IsNullOrEmpty(GetDrawerHeight(FindTopDrawer())))
                .AddStyle("--vek-drawer-height-bottom", GetDrawerHeight(FindBottomDrawer()), !string.IsNullOrEmpty(GetDrawerHeight(FindBottomDrawer())))
                .AddStyle("--vek-drawer-width-mini-left", GetMiniDrawerWidth(FindLeftMiniDrawer()), !string.IsNullOrEmpty(GetMiniDrawerWidth(FindLeftMiniDrawer())))
                .AddStyle("--vek-drawer-width-mini-right", GetMiniDrawerWidth(FindRightMiniDrawer()), !string.IsNullOrEmpty(GetMiniDrawerWidth(FindRightMiniDrawer())))
                .AddStyle(Style)
                .Build();

        /// <summary>
        /// Displays drawers right-to-left.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        /// <summary>
        /// The custom content inside this drawer.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Drawer.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        internal void Add(VekDrawer drawer)
        {
            if (Fixed && !drawer.IsFixed)
                return;

            _drawers.Add(drawer);
            StateHasChanged();
        }

        internal void Remove(VekDrawer drawer)
        {
            _drawers.Remove(drawer);
            StateHasChanged();
        }

        private static string GetDrawerClass(VekDrawer? drawer)
        {
            if (drawer is null)
            {
                return string.Empty;
            }

            var variant = drawer.EffectiveVariant;
            var className = $"vek-drawer-{(drawer.GetState<bool>(nameof(VekDrawer.Open)) ? "open" : "close")}-{variant.ToStringFast(true)}";
            if (variant is DrawerVariant.Responsive or DrawerVariant.Mini)
            {
                className += $"-{drawer.NormalizedBreakpoint.ToStringFast(true)}";
            }
            className += $"-{drawer.GetPosition()}";

            className += $" vek-drawer-{drawer.GetPosition()}-clipped-{drawer.ClipMode.ToStringFast(true)}";

            return className;
        }

        private static string? GetDrawerWidth(VekDrawer? drawer)
        {
            if (drawer is null)
            {
                return string.Empty;
            }

            return drawer.Width;
        }

        private static string? GetDrawerHeight(VekDrawer? drawer)
        {
            if (drawer is null)
            {
                return string.Empty;
            }

            return drawer.Height;
        }

        private static string? GetMiniDrawerWidth(VekDrawer? drawer)
        {
            if (drawer is null)
            {
                return string.Empty;
            }

            return drawer.MiniWidth;
        }

        private VekDrawer? FindLeftDrawer()
        {
            var anchor = RightToLeft ? Anchor.End : Anchor.Start;

            return _drawers.FirstOrDefault(d => d.Anchor == anchor || d.Anchor == Anchor.Left);
        }

        private VekDrawer? FindRightDrawer()
        {
            var anchor = RightToLeft ? Anchor.Start : Anchor.End;

            return _drawers.FirstOrDefault(d => d.Anchor == anchor || d.Anchor == Anchor.Right);
        }

        private VekDrawer? FindTopDrawer()
        {
            return _drawers.FirstOrDefault(d => d.Anchor == Anchor.Top);
        }

        private VekDrawer? FindBottomDrawer()
        {
            return _drawers.FirstOrDefault(d => d.Anchor == Anchor.Bottom);
        }

        private VekDrawer? FindLeftMiniDrawer()
        {
            var anchor = RightToLeft ? Anchor.End : Anchor.Start;

            return _drawers.FirstOrDefault(d => d.Variant == DrawerVariant.Mini && (d.Anchor == anchor || d.Anchor == Anchor.Left));
        }

        private VekDrawer? FindRightMiniDrawer()
        {
            var anchor = RightToLeft ? Anchor.Start : Anchor.End;

            return _drawers.FirstOrDefault(d => d.Variant == DrawerVariant.Mini && (d.Anchor == anchor || d.Anchor == Anchor.Right));
        }
    }
}
