// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Interop;
using Vek.Services;
using Vek.Utilities;

namespace Vek;


/// <summary>
/// A set of action buttons.  
/// </summary>
/// <seealso cref="VekIconButton" />
public partial class VekToolBar : VekComponentBase, IAsyncDisposable
{
    private readonly List<VekToolBarItem> _items = [];
    private readonly HashSet<ElementReference> _observedElements = new(ResizeObserver.ElementReferenceComparer.Default);
    private readonly Dictionary<ElementReference, VekToolBarItem> _itemsByElement = new(ResizeObserver.ElementReferenceComparer.Default);
    private ElementReference _toolbarContentElement;
    private ElementReference _overflowActivatorElement;
    private IResizeObserver? _resizeObserver;
    private double _toolbarWidth;
    private double _overflowActivatorWidth;

    [Inject]
    private IResizeObserverFactory ResizeObserverFactory { get; set; } = null!;

    protected string Classname =>
        new CssBuilder("vek-toolbar")
            .AddClass("vek-toolbar-dense", Dense)
            .AddClass("vek-toolbar-gutters", Gutters)
            .AddClass("vek-toolbar-wrap-content", WrapContent)
            .AddClass(Class)
            .Build();

    protected string OverflowActivatorClassname =>
        new CssBuilder("vek-toolbar-overflow-activator")
            .AddClass("vek-toolbar-overflow-activator-hidden", !HasOverflow)
            .Build();

    internal bool HasOverflow => _items.Any(item => item.IsOverflowed);

    internal IEnumerable<VekToolBarItem> OverflowItems => _items.Where(item => item.IsOverflowed);

    /// <summary>
    /// Uses compact vertical padding.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Appearance)]
    public bool Dense { get; set; }

    /// <summary>
    /// Adds left and right padding.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Appearance)]
    public bool Gutters { get; set; } = true;

    /// <summary>
    /// The content of the toolbar.
    /// </summary>
    /// <remarks>
    /// Typically a set of <see cref="VekIconButton"/> components.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Allows the toolbar's content to wrap.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public bool WrapContent { get; set; }

    /// <summary>
    /// Moves toolbar items that do not fit into an accessible overflow menu.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>. Wrap actions that may move into the menu in <see cref="VekToolBarItem"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.ToolBar.Behavior)]
    public bool AdaptiveOverflow { get; set; }

    internal void RegisterItem(VekToolBarItem item)
    {
        if (!_items.Contains(item))
        {
            _items.Add(item);
        }
    }

    internal void RemoveItem(VekToolBarItem item)
    {
        _items.Remove(item);

        var reference = item.ElementReference;
        if (_itemsByElement.Remove(reference))
        {
            _observedElements.Remove(reference);
            if (_resizeObserver?.IsElementObserved(reference) == true)
            {
                _ = _resizeObserver.Unobserve(reference);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!AdaptiveOverflow)
        {
            var resetOverflow = false;
            foreach (var item in _items)
            {
                resetOverflow |= await item.SetOverflowedAsync(false);
            }

            if (_resizeObserver is not null)
            {
                _resizeObserver.OnResized -= OnResized;
                await _resizeObserver.DisposeAsync();
                _resizeObserver = null;
                _observedElements.Clear();
                _itemsByElement.Clear();
                _toolbarWidth = 0;
                _overflowActivatorWidth = 0;
            }

            if (resetOverflow)
            {
                StateHasChanged();
            }

            return;
        }

        if (_resizeObserver is null)
        {
            _resizeObserver = ResizeObserverFactory.Create();
            _resizeObserver.OnResized += OnResized;
        }

        var changed = await ObserveElementAsync(_toolbarContentElement);
        changed |= await ObserveElementAsync(_overflowActivatorElement);

        foreach (var item in _items)
        {
            changed |= await ObserveElementAsync(item.ElementReference, item);
        }

        changed |= await UpdateOverflowLayoutAsync();

        if (changed)
        {
            StateHasChanged();
        }
    }

    private async Task<bool> ObserveElementAsync(ElementReference element, VekToolBarItem? item = null)
    {
        if (_resizeObserver is null || !_observedElements.Add(element))
        {
            return false;
        }

        if (item is not null)
        {
            _itemsByElement[element] = item;
        }

        var size = await _resizeObserver.Observe(element);
        return size is not null && UpdateMeasuredWidth(element, size.Width);
    }

    private async void OnResized(IDictionary<ElementReference, BoundingClientRect> changes)
    {
        await InvokeAsync(async () =>
        {
            var changed = false;
            foreach (var (element, size) in changes)
            {
                changed |= UpdateMeasuredWidth(element, size.Width);
            }

            changed |= await UpdateOverflowLayoutAsync();

            if (changed)
            {
                StateHasChanged();
            }
        });
    }

    private bool UpdateMeasuredWidth(ElementReference element, double width)
    {
        if (ResizeObserver.ElementReferenceComparer.Default.Equals(element, _toolbarContentElement))
        {
            if (Math.Abs(_toolbarWidth - width) < 0.5)
            {
                return false;
            }

            _toolbarWidth = width;
            return true;
        }

        if (ResizeObserver.ElementReferenceComparer.Default.Equals(element, _overflowActivatorElement))
        {
            if (Math.Abs(_overflowActivatorWidth - width) < 0.5)
            {
                return false;
            }

            _overflowActivatorWidth = width;
            return true;
        }

        if (_itemsByElement.TryGetValue(element, out var item))
        {
            return item.UpdateMeasuredWidth(width);
        }

        return false;
    }

    private async Task<bool> UpdateOverflowLayoutAsync()
    {
        if (!AdaptiveOverflow || _toolbarWidth <= 0 || _overflowActivatorWidth <= 0 || _items.Count == 0 || _items.Any(item => item.MeasuredWidth <= 0))
        {
            return false;
        }

        var availableWidth = _toolbarWidth;
        var itemWidths = _items.Select(item => (item.MeasuredWidth, item.CollapsePriority)).ToArray();
        var overflowIndices = GetOverflowIndices(availableWidth, _overflowActivatorWidth, itemWidths);
        var changed = false;

        for (var index = 0; index < _items.Count; index++)
        {
            changed |= await _items[index].SetOverflowedAsync(overflowIndices.Contains(index));
        }

        return changed;
    }

    internal static HashSet<int> GetOverflowIndices(
        double availableWidth,
        double overflowActivatorWidth,
        IReadOnlyList<(double Width, int CollapsePriority)> items)
    {
        var totalWidth = items.Sum(item => item.Width);
        var overflowedIndices = new HashSet<int>();

        if (totalWidth <= availableWidth)
        {
            return overflowedIndices;
        }

        foreach (var item in items
                     .Select((value, index) => (value, index))
                     .OrderBy(entry => entry.value.CollapsePriority)
                     .ThenByDescending(entry => entry.index))
        {
            overflowedIndices.Add(item.index);
            totalWidth -= item.value.Width;

            if (totalWidth + overflowActivatorWidth <= availableWidth)
            {
                break;
            }
        }

        return overflowedIndices;
    }

    public async ValueTask DisposeAsync()
    {
        if (_resizeObserver is not null)
        {
            _resizeObserver.OnResized -= OnResized;
            await _resizeObserver.DisposeAsync();
        }
    }
}
