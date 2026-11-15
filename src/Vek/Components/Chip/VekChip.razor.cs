using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Vek.Services;
using Vek.State;
using Vek.Utilities;

namespace Vek;


/// <summary>
/// Compact elements used to enter information, select a choice, filter content, or trigger an action.
/// </summary>
/// <typeparam name="T">The type of item managed by this component.</typeparam>
/// <seealso cref="VekChipSet{T}"/>
public partial class VekChip<T> : VekComponentBase, IAsyncDisposable
{
    [Inject]
    private IKeyInterceptorService KeyInterceptorService { get; set; } = null!;

    private readonly string _chipContainerId = $"chip-container-{Guid.NewGuid()}";
    private bool _hasKeyInterceptorSubscription;

    internal readonly ParameterState<bool> SelectedState;

    public VekChip()
    {
        using var registerScope = CreateRegisterScope();
        SelectedState = registerScope.RegisterParameter<bool>(nameof(Selected))
            .WithParameter(() => Selected)
            .WithEventCallback(() => SelectedChanged)
            .WithChangeHandler(OnSelectedChangedAsync);
    }

    private Task OnSelectedChangedAsync(ParameterChangedEventArgs<bool> args)
    {
        if (ChipSet == null)
            return Task.CompletedTask;
        return ChipSet.OnChipSelectedChangedAsync(this, args.Value);
    }

    /// <summary>
    /// Updates whether this chip is selected.
    /// </summary>
    /// <remarks>
    /// The set walks every chip whenever the selection changes, so rendering unconditionally rebuilt the whole set to move one check mark.
    /// Nothing this chip renders depends on another chip, so a chip whose own state did not change has nothing new to show.
    /// </remarks>
    /// <param name="selected">Whether this chip is now selected.</param>
    internal async Task UpdateSelectionStateAsync(bool selected)
    {
        var wasSelected = SelectedState.Value;
        await SelectedState.SetValueAsync(selected);
        if (SelectedState.Value != wasSelected)
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// The service used to navigate the browser to another URL.
    /// </summary>
    [Inject]
    public NavigationManager? UriHelper { get; set; }

    /// <summary>
    /// The service used to perform browser actions such as navigation.
    /// </summary>
    [Inject]
    public IJsApiService? JsApiService { get; set; }

    protected string Classname => new CssBuilder("vek-chip")
        .AddClass($"vek-chip-{GetVariant().ToStringFast(true)}")
        .AddClass($"vek-chip-size-{GetSize().ToStringFast(true)}")
        .AddClass($"vek-chip-color-{GetColor().ToStringFast(true)}")
        .AddClass("vek-clickable", IsButton || IsAnchor)
        .AddClass("vek-ripple", IsButton && GetRipple())
        .AddClass("vek-chip-label", GetLabel())
        .AddClass("vek-disabled", GetDisabled())
        .AddClass("vek-chip-selected", SelectedState.Value)
        .AddClass(Class)
        .Build();

    private bool IsAnchor => !string.IsNullOrWhiteSpace(Href);

    // A link is left to the browser, so Href takes precedence over OnClick and selection.
    private bool IsButton => !IsAnchor
                             && !GetDisabled()
                             && !GetReadOnly()
                             && (ChipSet is not null || OnClick.HasDelegate);

    private bool IsClosable => (OnClose.HasDelegate || ChipSet?.AllClosable == true) && !IsAnchor;

    protected string GetHtmlTag()
    {
        if (IsButton)
        {
            return "button";
        }

        if (IsAnchor)
        {
            return "a";
        }

        return "div";
    }

    protected Dictionary<string, object?> GetAttributes()
    {
        var attributes = new Dictionary<string, object?>();

        if (IsButton)
        {
            attributes.Add("tabindex", 0);
            attributes.Add("type", "button");
        }
        else if (IsAnchor)
        {
            attributes.Add("tabindex", 0);

            attributes.Add("href", Href);
            attributes.Add("target", Target);

            if (Rel is null && Target == "_blank")
            {
                attributes.Add("rel", "noopener");
            }
            else
            {
                attributes.Add("rel", Rel);
            }
        }
        else
        {
            attributes.Add("tabindex", -1);

            if (ChipSet is not null || OnClick.HasDelegate)
            {
                // A disabled or read-only chip is still a button semantically; it only renders as a plain element.
                attributes.Add("role", "button");

                if (GetDisabled())
                {
                    attributes.Add("aria-disabled", "true");
                }
            }
        }

        if (ChipSet is not null && !IsAnchor)
        {
            // Chips inside a set act as toggle buttons; expose the selected state to assistive technology.
            attributes.Add("aria-pressed", SelectedState.Value ? "true" : "false");
        }

        // User-defined attributes always take priority.
        foreach (var attribute in UserAttributes)
        {
            attributes[attribute.Key] = attribute.Value;
        }

        return attributes;
    }

    /// <summary>
    /// Opens the chip element with its attributes; the Razor file renders the content and closes it.
    /// </summary>
    /// <remarks>
    /// The tag is chosen at runtime, so the element is opened by name rather than written as markup.
    /// class and style come before the splat so a class or style supplied through <see cref="VekComponentBase.UserAttributes"/> still wins, which is the precedence the VekElement boundary gave them.
    /// </remarks>
    private void OpenElement(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, GetHtmlTag());
        builder.AddAttribute(1, "class", Classname);
        builder.AddAttribute(2, "style", Style);
        builder.AddMultipleAttributes(3, GetAttributes()!);
        builder.AddAttribute(4, "onclick", IsButton ? this.AsNonRenderingEventHandler<MouseEventArgs>(OnClickAsync) : null);
    }

    internal Variant GetVariant()
    {
        var chipSetVariant = ChipSet?.Variant ?? Vek.Variant.Filled;
        var variant = Variant ?? chipSetVariant;
        return variant switch
        {
            Vek.Variant.Text => SelectedState.Value ? Vek.Variant.Filled : Vek.Variant.Text,
            Vek.Variant.Filled => SelectedState.Value ? Vek.Variant.Text : Vek.Variant.Filled,
            Vek.Variant.Outlined => Vek.Variant.Outlined,
            _ => Vek.Variant.Outlined
        };
    }

    private Color GetColor()
    {
        var selectedColor = GetSelectedColor();
        if (SelectedState.Value && selectedColor != Vek.Color.Inherit)
        {
            return selectedColor;
        }
        return Color ?? ChipSet?.Color ?? Vek.Color.Default;
    }

    private Color GetSelectedColor() => SelectedColor ?? ChipSet?.SelectedColor ?? Vek.Color.Inherit;

    private Color GetIconColor() => IconColor ?? ChipSet?.IconColor ?? Vek.Color.Inherit;

    private Size GetSize() => Size ?? ChipSet?.Size ?? Vek.Size.Medium;

    private bool GetDisabled() => Disabled || (ChipSet?.Disabled ?? false);

    private bool GetReadOnly() => ChipSet?.ReadOnly ?? false;

    private bool GetRipple() => Ripple ?? ChipSet?.Ripple ?? true;

    private bool GetLabel() => Label ?? ChipSet?.Label ?? false;

    private string GetCheckedIcon() => CheckedIcon ?? ChipSet?.CheckedIcon ?? Icons.Material.Filled.Check;

    private string GetCloseIcon() => CloseIcon ?? ChipSet?.CloseIcon ?? Icons.Material.Filled.Cancel;

    internal bool ShowCheckMark => SelectedState.Value && ChipSet?.CheckMark == true;

    [CascadingParameter]
    private VekChipSet<T>? ChipSet { get; set; }

    /// <summary>
    /// The color of this chip.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  When <see cref="SelectedColor"/> is set, this color is used when the chip is unselected.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public Color? Color { get; set; }

    /// <summary>
    /// The color of the chip when it is selected.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  When set, this color is used for a selected chip, otherwise <see cref="Color"/> is used.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public Color? SelectedColor { get; set; }

    /// <summary>
    /// The size of the chip.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public Size? Size { get; set; }

    /// <summary>
    /// The display variation to use.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public Variant? Variant { get; set; }

    /// <summary>
    /// The avatar content to display inside the chip.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public RenderFragment? AvatarContent { get; set; }

    /// <summary>
    /// Uses the theme border radius for chip edges.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  When <c>true</c>, the <see cref="LayoutProperties.DefaultBorderRadius"/> is used for chip edges.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public bool? Label { get; set; }

    /// <summary>
    /// Prevents the user from interacting with this chip.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.  When <c>true</c>, the chip is visibly disabled and interaction is not allowed.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// The icon to display within the chip.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  Use the <see cref="IconColor"/> to control the color of this icon.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Behavior)]
    public string? Icon { get; set; }

    /// <summary>
    /// The icon to display when <see cref="Selected"/> is <c>true</c>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public string? CheckedIcon { get; set; }

    /// <summary>
    /// The color of the <see cref="Icon"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public Color? IconColor { get; set; }

    /// <summary>
    /// The close icon to display when <see cref="OnClose"/> is set.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public string? CloseIcon { get; set; }

    /// <summary>
    /// Displays a ripple effect when this chip is clicked.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Appearance)]
    public bool? Ripple { get; set; }

    /// <summary>
    /// The content within this chip.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.Chip.Behavior)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The URL to navigate to when the chip is clicked.
    /// </summary>
    /// <remarks>
    /// <para>Defaults to <c>null</c>.  Use <see cref="Target"/> to control where the URL is opened.</para>
    /// <para>When set, the chip renders as a link and the browser handles the click, so <see cref="OnClick"/> is not raised and the chip cannot be selected in a <see cref="VekChipSet{T}"/>.</para>
    /// <para>Note: The close button cannot be enabled if this is set because <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/a#technical_summary">interactive content violates the HTML spec</see>.</para>
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.ClickAction)]
    public string? Href { get; set; }

    /// <summary>
    /// The target to open URLs if <see cref="Href"/> is set.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  This value is typically <c>_blank</c>, <c>_self</c>, <c>_parent</c>, <c>_top</c>, or the name of an <c>iframe</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.ClickAction)]
    public string? Target { get; set; }

    /// <summary>
    /// The relationship between the current document and the linked document when <see cref="Href"/> is set.
    /// </summary>
    /// <remarks>
    /// This property is typically used by web crawlers to get more information about a link.  Common values can be found here: <see href="https://www.w3schools.com/tags/att_a_rel.asp" />
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.ClickAction)]
    public string? Rel { get; set; }

    /// <summary>
    /// The text label for the chip.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.  This will be shown so long as <see cref="ChildContent"/> is not set.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Behavior)]
    public string? Text { get; set; }

    /// <summary>
    /// The value applied when the chip is selected.
    /// </summary>
    /// <remarks>
    /// When part of a <see cref="VekChipSet{T}"/>, the <see cref="VekChipSet{T}.SelectedValue"/> is set to this value when the chip is selected.  Once set, the value should not change.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Behavior)]
    public T? Value { get; set; }

    /// <summary>
    /// Selects this chip by default when part of a <see cref="VekChipSet{T}"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.Chip.Behavior)]
    public bool? Default { get; set; }

    /// <summary>
    /// Occurs when this chip is clicked.
    /// </summary>
    /// <remarks>
    /// If an <see cref="Href"/> is set, this callback will not be triggered and the browser will handle the click.
    /// </remarks>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Occurs when this chip has been closed.
    /// </summary>
    /// <remarks>
    /// Subscribing to this event enables the close button, unless <see cref="Href"/> is also set.
    /// </remarks>
    [Parameter]
    public EventCallback<VekChip<T>> OnClose { get; set; }

    /// <summary>
    /// Selects this chip.
    /// </summary>
    /// <remarks>
    /// When <c>true</c>, the chip is displayed in a selected state.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.Chip.Behavior)]
    public bool Selected { get; set; }

    /// <summary>
    /// Occurs when the <see cref="Selected"/> property has changed.
    /// </summary>
    [Parameter]
    public EventCallback<bool> SelectedChanged { get; set; }

    internal T? GetValue()
    {
        if (typeof(T) == typeof(string) && Value is null && Text is not null)
            return (T)(object)Text;

        return Value;
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (ChipSet is not null)
        {
            await ChipSet.AddAsync(this);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        var needsKeyInterceptor = CanHandleKeys() && (IsButton || IsClosable);
        if (needsKeyInterceptor && !_hasKeyInterceptorSubscription)
        {
            await SubscribeToKeyInterceptorAsync();
            _hasKeyInterceptorSubscription = true;
        }
        else if (!needsKeyInterceptor && _hasKeyInterceptorSubscription)
        {
            await KeyInterceptorService.UnsubscribeAsync(_chipContainerId);
            _hasKeyInterceptorSubscription = false;
        }
    }

    private Task SubscribeToKeyInterceptorAsync()
    {
        var options = new KeyInterceptorOptions(
            "vek-chip",
            [
                new(" ", preventDown: "key+none", preventUp: "key+none"),
                new("Backspace", preventDown: "key+none"),
                new("Delete", preventDown: "key+none")
            ]);

        return KeyInterceptorService.SubscribeAsync(_chipContainerId, options, keys => keys
            .When(CanHandleKeys, builder => builder
                .OnKeyDown(" ", () => OnClickAsync(new MouseEventArgs()))
                .OnKeyDownAny(["Backspace", "Delete"], () => OnCloseAsync(new MouseEventArgs()))));
    }

    private bool CanHandleKeys() => !GetDisabled() && !GetReadOnly();

    protected Task HandleKeyDownAsync(KeyboardEventArgs obj) => KeyInterceptorService.DispatchAsync(_chipContainerId, KeyEventKind.Down, obj);

    protected internal async Task OnClickAsync(MouseEventArgs ev)
    {
        if (ChipSet?.ReadOnly == true || IsAnchor)
        {
            return;
        }
        if (ChipSet != null)
        {
            // Go through the same helper as the set does, so this chip renders because its own state changed rather than relying on the set to sweep every chip.
            await UpdateSelectionStateAsync(!SelectedState.Value);
            await ChipSet.OnChipSelectedChangedAsync(this, SelectedState.Value);
        }

        await OnClick.InvokeAsync(ev);
    }

    protected async Task OnCloseAsync(MouseEventArgs ev)
    {
        if (GetReadOnly() || !IsClosable)
        {
            return;
        }
        await OnClose.InvokeAsync(this);
        if (ChipSet is not null)
        {
            await ChipSet.OnChipDeletedAsync(this);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Releases unused resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (ChipSet is not null)
            {
                await ChipSet.RemoveAsync(this);
            }

            if (IsJSRuntimeAvailable && _hasKeyInterceptorSubscription)
            {
                await KeyInterceptorService.UnsubscribeAsync(_chipContainerId);
                _hasKeyInterceptorSubscription = false;
            }
        }
        catch (Exception)
        {
            /* ignore! */
        }
    }
}
