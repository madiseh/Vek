// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Vek
{
    /// <summary>
    /// A tab component where the tabs are controlled dynamically, similar to browser tabs.
    /// </summary>
    public partial class VekDynamicTabs : VekTabs
    {
        /// <summary>
        /// The icon for the add button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.Add"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tabs.Appearance)]
        public string AddTabIcon { get; set; } = Icons.Material.Filled.Add;

        /// <summary>
        /// The icon for the close button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.Close"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tabs.Appearance)]
        public string CloseTabIcon { get; set; } = Icons.Material.Filled.Close;

        /// <summary>
        /// Occurs when the "Add" button has been clicked.
        /// </summary>
        [Parameter] public EventCallback AddTab { get; set; }

        /// <summary>
        /// Occurs when a tab has been closed.
        /// </summary>
        [Parameter] public EventCallback<VekTabPanel> CloseTab { get; set; }

        /// <summary>
        /// The CSS classes applied to the "Add" button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>""</c>. Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tabs.Appearance)]
        public string AddIconClass { get; set; } = string.Empty;

        /// <summary>
        /// The CSS styles applied to the "Add" button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>""</c>.
        /// </remarks>
        [Obsolete("Prefer the AddIconClass property with CSS https://github.com/madiseh/vek/issues/12047")]
        [Parameter]
        [Category(CategoryTypes.Tabs.Appearance)]
        public string AddIconStyle { get; set; } = string.Empty;

        /// <summary>
        /// The CSS styles applied to the "Close" button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>""</c>.
        /// </remarks>
        [Obsolete("Prefer the CloseIconClass property with CSS https://github.com/madiseh/vek/issues/12047")]
        [Parameter]
        [Category(CategoryTypes.Tabs.Appearance)]
        public string CloseIconStyle { get; set; } = string.Empty;

        /// <summary>
        /// The text shown when the user hovers over the "Add" button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>""</c>. When <c>null</c> or empty, no tooltip will be shown.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Tabs.Behavior)]
        public string AddIconToolTip { get; set; } = string.Empty;

        protected override string InternalClassName { get; } = "vek-dynamic-tabs";

        protected override async Task HandleTabKeyDownAsync(KeyboardEventArgs e, VekTabPanel panel)
        {
            await base.HandleTabKeyDownAsync(e, panel);

            if ((e.Key == "Delete" || e.Key == "Backspace") && panel.ShowCloseIcon)
            {
                if (CloseTab.HasDelegate)
                {
                    await CloseTab.InvokeAsync(panel);
                }
            }
        }
    }
}
