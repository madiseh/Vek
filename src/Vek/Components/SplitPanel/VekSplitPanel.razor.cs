using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// Allows users to dynamically resize content with a draggable splitter.
/// </summary>
public partial class VekSplitPanel : VekComponentBase, IAsyncDisposable
{
    private readonly ParameterState<double?> _paneSizeState;

    public VekSplitPanel()
    {
        using var registerScope = CreateRegisterScope();
        _paneSizeState = registerScope.RegisterParameter<double?>(nameof(PaneSize))
            .WithParameter(() => PaneSize)
            .WithEventCallback(() => PaneSizeChanged);
    }

    private string Classname => new CssBuilder("vek-split-panel")
        .AddClass("flex-column", Horizontal)
        .AddClass("absolute", UseAsOverlay)
        .AddClass(Class)
        .Build();

    private string ClassnameFirstPanel => new CssBuilder("child-panel")
        .AddClass($"vek-elevation-{Elevation}", Elevation != 0 && FirstPanel != null)
        .AddClass("transparent", FirstPanel == null || Transparent)
        .AddClass($"pa-{Padding}", Padding != 0)
        .AddClass("rounded", Rounded)
        .AddClass(ClassFirstPanel)
        .Build();

    private string ClassnameSecondPanel => new CssBuilder("child-panel")
        .AddClass($"vek-elevation-{Elevation}", Elevation != 0 && SecondPanel != null)
        .AddClass("transparent", SecondPanel == null || Transparent)
        .AddClass($"pa-{Padding}", Padding != 0)
        .AddClass("rounded", Rounded)
        .AddClass(ClassSecondPanel)
        .Build();

    private string ClassnameDivider => new CssBuilder("divider")
        .AddClass("horizontal", Horizontal)
        .AddClass(ClassDivider)
        .Build();

    /// <summary>
    /// The CSS classes applied to the first panel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public string? ClassFirstPanel { get; set; }

    /// <summary>
    /// The CSS classes applied to the second panel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public string? ClassSecondPanel { get; set; }

    /// <summary>
    /// The CSS classes applied to the divider.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public string? ClassDivider { get; set; }

    /// <summary>
    /// Whether the panels should be divided horizontally instead of vertically. 
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public bool Horizontal { get; set; }

    /// <summary>
    /// Positions this panel absolute and above other content.
    /// Note that you have to set <c>position: relative</c> on the parent element.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public bool UseAsOverlay { get; set; }

    /// <summary>
    /// Whether to use rounded corners for the panels.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public bool Rounded { get; set; }

    /// <summary>
    /// The size of the drop shadow.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>0</c>.
    /// A higher number creates a heavier drop shadow.
    /// Set to <c>0</c> to disable the drop shadow.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public int Elevation { get; set; }

    /// <summary>
    /// Sets the initial height or width of the first panel in pixels.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public int? FirstPanelInitialSize { get; set; }

    /// <summary>
    /// The size of the first pane, in the unit selected by <see cref="PaneSizeUnit"/>.
    /// </summary>
    /// <remarks>Defaults to an even split. Bind this parameter to receive changes made by dragging or keyboard input.</remarks>
    [Parameter, ParameterState, Category(CategoryTypes.SplitPanel.Behavior)]
    public double? PaneSize { get; set; }

    /// <summary>
    /// Raised when the first pane is resized.
    /// </summary>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public EventCallback<double?> PaneSizeChanged { get; set; }

    /// <summary>
    /// Selects whether pane sizes and constraints use percentages or pixels.
    /// </summary>
    /// <remarks>Defaults to <see cref="SplitPanelSizeUnit.Percentage"/>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public SplitPanelSizeUnit PaneSizeUnit { get; set; } = SplitPanelSizeUnit.Percentage;

    /// <summary>
    /// The minimum size of the first pane, in <see cref="PaneSizeUnit"/> units.
    /// </summary>
    /// <remarks>Defaults to <c>null</c>; <see cref="MinPanelSize"/> still applies.</remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public double? MinSize { get; set; }

    /// <summary>
    /// The maximum size of the first pane, in <see cref="PaneSizeUnit"/> units.
    /// </summary>
    /// <remarks>Defaults to <c>null</c>.</remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public double? MaxSize { get; set; }

    /// <summary>
    /// Allows double-clicking the divider to collapse and restore the first pane.
    /// </summary>
    /// <remarks>Defaults to <c>false</c>.</remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public bool Collapsible { get; set; }

    /// <summary>
    /// The height and width in pixel each panel can't be made smaller than.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>50</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public int MinPanelSize { get; set; } = 50;

    /// <summary>
    /// The height or width of the divider between the panels in pixel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>4</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public int PanelGap { get; set; } = 4;

    /// <summary>
    /// The padding of the panels.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>0</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public int Padding { get; set; }

    /// <summary>
    /// If enabled resets the panel sizes to their initial values on double-clicking the divider.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public bool ResetOnDoubleClick { get; set; } = true;

    /// <summary>
    /// Makes the panels backgrounds transparent.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Appearance)]
    public bool Transparent { get; set; }

    /// <summary>
    /// The contents of the first i.e. left/upper panel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public RenderFragment? FirstPanel { get; set; }

    /// <summary>
    /// The contents of the second i.e. right/lower panel.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, Category(CategoryTypes.SplitPanel.Behavior)]
    public RenderFragment? SecondPanel { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    private readonly string _containerId = Guid.NewGuid().ToString();

    // The id actually rendered on the container div: a consumer-supplied id (captured into UserAttributes and splatted
    // after the explicit id, so it wins) overrides _containerId. JS interop must target this same id or document
    // .getElementById fails and the divider's drag/keyboard handlers are never attached. Captured at build time so
    // teardown unsubscribes the exact id that was built.
    private string? _builtContainerId;

    private DotNetObjectReference<VekSplitPanel>? _dotNetReference;

    private string ResolvedContainerId => _builtContainerId ?? GetEffectiveElementId(_containerId);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            _builtContainerId = GetEffectiveElementId(_containerId);
            _dotNetReference = DotNetObjectReference.Create(this);
            await JsRuntime.InvokeVoidAsync("vekSplitPanel.build", _builtContainerId, Horizontal, ResetOnDoubleClick, MinPanelSize, FirstPanelInitialSize, PanelGap, _paneSizeState.Value, PaneSizeUnit, MinSize, MaxSize, Collapsible, _dotNetReference);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (IsJSRuntimeAvailable)
        {
            await JsRuntime.InvokeVoidAsync("vekSplitPanel_update", ResolvedContainerId, Horizontal, ResetOnDoubleClick, MinPanelSize, PanelGap, _paneSizeState.Value, PaneSizeUnit, MinSize, MaxSize, Collapsible);
        }
    }

    /// <summary>
    /// Updates the bound pane size after an interactive resize.
    /// </summary>
    [JSInvokable]
    public Task OnPaneSizeChangedAsync(double? size) => _paneSizeState.SetValueAsync(size);

    /// <summary>
    /// Resets the divider position to its initial value.
    /// </summary>
    public async Task ResetDividerPositionAsync()
    {
        await JsRuntime.InvokeVoidAsync("vekSplitPanel_resetDividerPosition", ResolvedContainerId);
    }

    /// <summary>
    /// Sets the divider to the given offset.
    /// </summary>
    /// <remarks>
    /// Note that this function ignores <see cref="MinPanelSize"/>.
    /// </remarks>
    /// <param name="offset">The offset in pixels from the left or top border.</param>
    public async Task SetDividerPositionAsync(int offset)
    {
        await JsRuntime.InvokeVoidAsync("vekSplitPanel_setDividerPosition", ResolvedContainerId, offset);
    }

    /// <summary>
    /// Returns the current offset of the divider from the left or top border in pixels.
    /// </summary>
    public async Task<int> GetDividerPositionAsync()
    {
        return await JsRuntime.InvokeAsync<int>("vekSplitPanel_getDividerPosition", ResolvedContainerId);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekSplitPanel_destroy", ResolvedContainerId);
        _dotNetReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
