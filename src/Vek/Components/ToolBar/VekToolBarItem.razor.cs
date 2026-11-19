// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// A toolbar action that can move into its adaptive overflow menu.
/// </summary>
public partial class VekToolBarItem : VekComponentBase, IDisposable
{
    private double _measuredWidth;

    [CascadingParameter]
    internal VekToolBar? ParentToolBar { get; set; }

    protected string Classname =>
        new CssBuilder("vek-toolbar-item")
            .AddClass("vek-toolbar-item-overflow-measure", IsOverflowed)
            .AddClass(Class)
            .Build();

    internal ElementReference ElementReference { get; set; }

    internal bool IsOverflowed { get; private set; }

    internal double MeasuredWidth => _measuredWidth;

    /// <summary>
    /// The text announced and shown for this action in the overflow menu.
    /// </summary>
    [Parameter, EditorRequired]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The icon shown for this action in the overflow menu.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public string? Icon { get; set; }

    /// <summary>
    /// Determines when this action moves into the overflow menu.
    /// </summary>
    /// <remarks>
    /// Lower values collapse first. Actions with equal priorities collapse from the end of the toolbar. Defaults to <c>0</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public int CollapsePriority { get; set; }

    /// <summary>
    /// Prevents the overflow menu action from being activated.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// Performs the action when it is selected from the overflow menu.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public EventCallback<MouseEventArgs> OnOverflowClick { get; set; }

    /// <summary>
    /// The toolbar content displayed while this action fits in the available width.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ParentToolBar?.RegisterItem(this);
    }

    internal bool UpdateMeasuredWidth(double width)
    {
        if (width <= 0 || Math.Abs(_measuredWidth - width) < 0.5)
        {
            return false;
        }

        _measuredWidth = width;
        return true;
    }

    internal async Task<bool> SetOverflowedAsync(bool isOverflowed)
    {
        if (IsOverflowed == isOverflowed)
        {
            return false;
        }

        IsOverflowed = isOverflowed;
        await InvokeAsync(StateHasChanged);
        return true;
    }

    public void Dispose()
    {
        ParentToolBar?.RemoveItem(this);
    }
}
