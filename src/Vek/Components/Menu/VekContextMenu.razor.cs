using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// Opens a cursor-positioned menu from a DOM target, right-click, or touch long-press.
/// </summary>
public partial class VekContextMenu : VekComponentBase, IAsyncDisposable
{
    private readonly ParameterState<string?> _targetSelectorState;
    private readonly ParameterState<ElementReference?> _targetElementState;
    private readonly ParameterState<int> _longPressDelayState;
    private readonly ParameterState<bool> _disabledState;
    private readonly ParameterState<RenderFragment?> _childContentState;
    private VekMenu? _menu;
    private DotNetObjectReference<VekContextMenu>? _dotNetReference;
    private ElementReference? _attachedElement;
    private string? _attachedSelector;
    private int? _listenerId;
    private bool _open;
    private bool _disposed;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [DynamicDependency(nameof(OpenAtAsync))]
    public VekContextMenu()
    {
        using var registerScope = CreateRegisterScope();
        _targetSelectorState = registerScope.RegisterParameter<string?>(nameof(TargetSelector))
            .WithParameter(() => TargetSelector);
        _targetElementState = registerScope.RegisterParameter<ElementReference?>(nameof(TargetElement))
            .WithParameter(() => TargetElement);
        _longPressDelayState = registerScope.RegisterParameter<int>(nameof(LongPressDelay))
            .WithParameter(() => LongPressDelay);
        _disabledState = registerScope.RegisterParameter<bool>(nameof(Disabled))
            .WithParameter(() => Disabled);
        _childContentState = registerScope.RegisterParameter<RenderFragment?>(nameof(ChildContent))
            .WithParameter(() => ChildContent);
    }

    /// <summary>
    /// Selects every matching DOM element as a context-menu target.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. A matching element must exist when the component renders.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.Menu.Behavior)]
    public string? TargetSelector { get; set; }

    /// <summary>
    /// Uses a referenced DOM element as the context-menu target.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>. When both target parameters are set, this element takes precedence.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.Menu.Behavior)]
    public ElementReference? TargetElement { get; set; }

    /// <summary>
    /// Sets how long a touch must remain on the target before opening the menu.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>500</c> milliseconds.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.Menu.Behavior)]
    public int LongPressDelay { get; set; } = 500;

    /// <summary>
    /// Prevents the context menu from opening.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.Menu.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// The menu items and nested menus to display.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.Menu.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        await UpdateTargetListenerAsync();
    }

    /// <summary>
    /// Opens the menu at coordinates supplied by a DOM context-menu, touch, or keyboard event.
    /// </summary>
    [JSInvokable]
    public Task OpenAtAsync(double pageX, double pageY, double clientX, double clientY)
    {
        if (_disposed || _disabledState.Value || _menu is null)
        {
            return Task.CompletedTask;
        }

        return _menu.OpenMenuAsync(new MouseEventArgs
        {
            PageX = pageX,
            PageY = pageY,
            ClientX = clientX,
            ClientY = clientY,
            Button = 2
        });
    }

    private async Task UpdateTargetListenerAsync()
    {
        var targetElement = _targetElementState.Value;
        var targetSelector = _targetSelectorState.Value;
        var selector = targetElement is null ? targetSelector : null;
        var targetChanged = !EqualityComparer<ElementReference?>.Default.Equals(_attachedElement, targetElement)
            || !string.Equals(_attachedSelector, selector, StringComparison.Ordinal);

        if (_listenerId is not null && (targetChanged || _disabledState.Value))
        {
            await RemoveTargetListenerAsync();
        }

        if (_disposed || _disabledState.Value || _listenerId is not null)
        {
            return;
        }

        if (targetElement is null && string.IsNullOrWhiteSpace(targetSelector))
        {
            return;
        }

        _dotNetReference = DotNetObjectReference.Create(this);
        var listenerId = targetElement is ElementReference target
            ? await JsRuntime.InvokeAsync<int>("vekElementRef.addContextMenuListenerForElement", target, _dotNetReference, _longPressDelayState.Value)
            : await JsRuntime.InvokeAsync<int>("vekElementRef.addContextMenuListenerForSelector", targetSelector, _dotNetReference, _longPressDelayState.Value);

        if (listenerId == 0)
        {
            _dotNetReference.Dispose();
            _dotNetReference = null;
            return;
        }

        _listenerId = listenerId;
        _attachedElement = targetElement;
        _attachedSelector = selector;
    }

    private async Task OnMenuOpenChangedAsync(bool open)
    {
        _open = open;
        if (_listenerId is int listenerId)
        {
            await JsRuntime.InvokeVoidAsync("vekElementRef.updateContextMenuExpanded", listenerId, open);
        }
    }

    private async ValueTask RemoveTargetListenerAsync()
    {
        if (_listenerId is int listenerId)
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("vekElementRef.removeContextMenuListener", listenerId);
            }
            catch (JSException)
            {
                // The page can disconnect before a circuit disposes its component.
            }
        }

        _listenerId = null;
        _attachedElement = null;
        _attachedSelector = null;
        _dotNetReference?.Dispose();
        _dotNetReference = null;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await RemoveTargetListenerAsync();
        GC.SuppressFinalize(this);
    }
}
