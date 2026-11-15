// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// A list of navigation links with support for groups.
    /// </summary>
    /// <seealso cref="VekNavGroup"/>
    /// <seealso cref="VekNavLink"/>
    public partial class VekNavMenu : VekComponentBase
    {
        private readonly List<VekNavGroup> _groups = [];

        /// <summary>
        /// When <c>true</c>, multiple top-level <see cref="VekNavGroup"/> can be expanded at a time.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. When set to <c>false</c>, expanding one group collapses the other top-level groups; nested groups are unaffected.
        /// Only applies when a group is expanded by the user; setting <see cref="VekNavGroup.Expanded"/> programmatically bypasses this.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public bool MultiExpansion { get; set; } = true;

        internal void RegisterGroup(VekNavGroup group)
        {
            if (!_groups.Contains(group))
            {
                _groups.Add(group);
            }
        }

        internal void UnregisterGroup(VekNavGroup group)
        {
            _groups.Remove(group);
        }

        internal async Task NotifyGroupExpandedAsync(VekNavGroup expandedGroup)
        {
            if (MultiExpansion)
            {
                return;
            }

            foreach (VekNavGroup group in _groups)
            {
                if (!ReferenceEquals(group, expandedGroup))
                {
                    await group.CollapseAsync();
                }
            }
        }
        protected string Classname =>
            new CssBuilder("vek-navmenu")
                .AddClass($"vek-navmenu-{Color.ToStringFast(true)}")
                .AddClass($"vek-navmenu-margin-{Margin.ToStringFast(true)}")
                .AddClass("vek-navmenu-dense", Dense)
                .AddClass("vek-navmenu-rounded", Rounded)
                .AddClass($"vek-navmenu-bordered vek-border-{Color.ToStringFast(true)}", Bordered)
                .AddClass(Class)
                .Build();

        [CascadingParameter]
        private NavigationContext? NavigationContext { get; set; }

        /// <summary>
        /// The color of the active <see cref="VekNavLink" />.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// Shows a border on the active <see cref="VekNavLink"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public bool Bordered { get; set; }

        /// <summary>
        /// Shows a rounded border for all <see cref="VekNavLink" /> items.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// When <c>true</c>, the theme <c>border-radius</c> value will be used. 
        /// Only takes affect if <see cref="Bordered"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public bool Rounded { get; set; }

        /// <summary>
        /// The vertical spacing between <see cref="VekNavLink" /> items.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Margin.None"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public Margin Margin { get; set; } = Margin.None;

        /// <summary>
        /// Uses compact vertical padding to all <see cref="VekNavLink"/> items.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  
        /// Will be overridden if <see cref="Margin"/> is not <see cref="Margin.None"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Appearance)]
        public bool Dense { get; set; }

        /// <summary>
        /// The content within this menu.
        /// </summary>
        /// <remarks>
        /// Typically contains <see cref="VekNavLink" />, <see cref="VekNavGroup"/>, <see cref="VekText"/>, and <see cref="VekDivider"/> components.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.NavMenu.Behavior)]
        public RenderFragment? ChildContent { get; set; }
    }
}
