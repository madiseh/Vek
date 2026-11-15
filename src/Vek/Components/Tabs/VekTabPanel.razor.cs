// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Utilities;

namespace Vek;


/// <summary>
/// A tab as part of a <see cref="VekTabs"/> or <see cref="VekDynamicTabs"/> component.
/// </summary>
public partial class VekTabPanel : VekComponentBase
{
    private bool _disposed;
    private bool _alreadyRendered;

    internal string Classname =>
        new CssBuilder("vek-tab-panel")
            .AddClass("vek-tab-panel-hidden", !Visible)
            .AddClass(Class)
            .Build();

    internal string PanelClassname =>
        new CssBuilder("vek-tab-panel")
            .AddClass("vek-tab-panel-active", Parent?.ActivePanel == this)
            .AddClass("vek-tab-panel-hidden", !Visible)
            .AddClass(PanelClass)
            .Build();

    [CascadingParameter]
    private VekTabs? Parent { get; set; }

    /// <summary>
    /// The reference to the underlying panel element.
    /// </summary>
    public ElementReference PanelRef;

    /// <summary>
    /// The title of this tab panel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. Only applies when <see cref="TabContent"/> is not set.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public string? Text { get; set; }

    /// <summary>
    /// The icon for this tab.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public string? Icon { get; set; }

    /// <summary>
    /// The color of this tab's icon.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Color.Default" />. When set, overrides the <see cref="VekTabs.IconColor" /> property.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public Color IconColor { get; set; }

    /// <summary>
    /// Prevents the user from interacting with this panel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// Shows the tab.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// For dynamic tabs, shows a "Close" icon for this tab.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>. Only applies within a <see cref="VekDynamicTabs"/> component.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public bool ShowCloseIcon { get; set; } = true;

    /// <summary>
    /// The badge shown for this tab.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. Typically a string such as <c>"3"</c> or <c>"live"</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public object? BadgeData { get; set; }

    /// <summary>
    /// The maximum number allowed in the badge.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>99</c>. Applies when <see cref="BadgeData"/> is an <c>int</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public int BadgeMax { get; set; } = 99;

    /// <summary>
    /// Optional icon to be shown in the badge instead of text.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public string? BadgeIcon { get; set; }

    /// <summary>
    /// Shows a dot instead of text or icon for the badge.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>. When <c>true</c>, a dot instead of <see cref="BadgeData"/> is displayed.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public bool BadgeDot { get; set; }

    /// <summary>
    /// The color of the badge.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Color.Primary"/>. Applies when <see cref="BadgeData"/> is set.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Appearance)]
    public Color BadgeColor { get; set; } = Color.Primary;

    /// <summary>
    /// The unique ID for this panel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. Should be unique across tabs.<br />
    /// Useful for activating tabs manually via the <c>ActivatePanel</c> method of the <see cref="VekTabs"/> and 
    /// <see cref="VekDynamicTabs"/> components.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public object? ID { get; set; }

    /// <summary>
    /// Occurs when this tab is clicked.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when a mouse down event occurs.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public EventCallback<MouseEventArgs> OnMouseDown { get; set; }

    /// <summary>
    /// Gets or sets the callback that is invoked when a context menu event occurs.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public EventCallback<MouseEventArgs> OnContextMenu { get; set; }

    /// <summary>
    /// The content within this tab.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The content within this tab's title.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. Typically used to display more than just text in the title.<br />
    /// For basic text, use the <see cref="Text"/> parameter.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public RenderFragment? TabContent { get; set; }

    /// <summary>
    /// The content enclosing this tab.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. Typically used to wrap the entire tab content in other content, such 
    /// as a <see cref="VekTooltip"/>, which wraps the content they refer to.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public RenderFragment<RenderFragment>? TabWrapperContent { get; set; }

    /// <summary>
    /// The tooltip displayed for this tab.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  Only applies when <see cref="TabContent"/> is not set.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Behavior)]
    public string? ToolTip { get; set; }

    /// <summary>
    /// Value to use when ordering tabs lexicographically, in place of <see cref="VekTabPanel.Text" />.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Appearance)]
    public string? SortKey { get; set; }

    /// <summary>
    /// The CSS classes applied to the panel. The <c>Class</c> property applies to the tab button only.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. Multiple classes must be separated by spaces.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Tabs.Appearance)]
    public string? PanelClass { get; set; }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender && Parent != null)
        {
            await Parent.SetPanelRefAsync(PanelRef);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        // NOTE: we must not throw here because we need the component to be able to live for the API docs to be able to infer default values
        //if (Parent == null)
        //    throw new ArgumentNullException(nameof(Parent), "TabPanel must exist within a Tabs component");
        await base.OnInitializedAsync();

        if (Parent != null)
            await Parent.AddPanelAsync(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) { return; }

        _disposed = true;
        if (Parent is not null)
            await Parent.RemovePanel(this);
    }
}
