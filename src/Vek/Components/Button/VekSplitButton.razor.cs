using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Extensions;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// A split button combines a primary action button and an attached dropdown trigger opening a contextual flyout action menu.
    /// </summary>
    /// <seealso cref="VekButton" />
    /// <seealso cref="VekButtonGroup" />
    /// <seealso cref="VekDropDownButton" />
    /// <seealso cref="VekMenu" />
    public partial class VekSplitButton : VekComponentBase
    {
        private readonly ParameterState<bool> _openState;
        private readonly ParameterState<bool> _disabledState;
        private readonly ParameterState<Variant> _variantState;
        private readonly ParameterState<Color> _colorState;
        private readonly ParameterState<Size> _sizeState;
        private VekButton? _primaryButtonRef;
        private VekMenu? _menuRef;

        public VekSplitButton()
        {
            using var registerScope = CreateRegisterScope();
            _openState = registerScope.RegisterParameter<bool>(nameof(Open))
                .WithParameter(() => Open)
                .WithEventCallback(() => OpenChanged)
                .WithChangeHandler(OnOpenChangedAsync);
            _disabledState = registerScope.RegisterParameter<bool>(nameof(Disabled))
                .WithParameter(() => Disabled);
            _variantState = registerScope.RegisterParameter<Variant>(nameof(Variant))
                .WithParameter(() => Variant);
            _colorState = registerScope.RegisterParameter<Color>(nameof(Color))
                .WithParameter(() => Color);
            _sizeState = registerScope.RegisterParameter<Size>(nameof(Size))
                .WithParameter(() => Size);
        }

        protected string Classname => new CssBuilder("vek-split-button-root vek-button-group-root")
            .AddClass("vek-button-group-override-styles")
            .AddClass($"vek-button-group-{_variantState.Value.ToStringFast(true)}")
            .AddClass($"vek-button-group-{_variantState.Value.ToStringFast(true)}-{_colorState.Value.ToStringFast(true)}")
            .AddClass($"vek-button-group-{_variantState.Value.ToStringFast(true)}-size-{_sizeState.Value.ToStringFast(true)}")
            .AddClass("vek-button-group-vertical", Vertical)
            .AddClass("vek-button-group-horizontal", !Vertical)
            .AddClass("vek-button-group-disable-elevation", !DropShadow)
            .AddClass("vek-button-group-rtl", RightToLeft)
            .AddClass("vek-width-full", FullWidth)
            .AddClass(Class)
            .Build();

        protected string PrimaryButtonClassname => new CssBuilder("vek-split-button-primary")
            .AddClass("vek-width-full", FullWidth)
            .Build();

        protected string MenuClassname => new CssBuilder("vek-split-button-menu")
            .Build();

        private bool IsPrimaryDisabled => _disabledState.Value || PrimaryDisabled;

        private bool IsDropDownDisabled => _disabledState.Value || DropDownDisabled;

        /// <summary>
        /// Displays buttons right-to-left.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        /// <summary>
        /// Displays the text on the primary action button.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? Text { get; set; }

        /// <summary>
        /// Custom content rendered inside the primary action button, or child menu items when MenuItems is specified.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The menu items displayed in the dropdown contextual flyout menu.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupBehavior)]
        public RenderFragment? MenuItems { get; set; }

        /// <summary>
        /// Callback invoked when the primary action button is clicked.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.ClickAction)]
        public EventCallback<MouseEventArgs> OnClick { get; set; }

        /// <summary>
        /// Whether the contextual dropdown menu is currently open.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Menu.PopupBehavior)]
        public bool Open { get; set; }

        /// <summary>
        /// Callback invoked when the open state of the dropdown menu changes.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupBehavior)]
        public EventCallback<bool> OpenChanged { get; set; }

        /// <summary>
        /// Disables both the primary action button and the attached dropdown arrow trigger.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Disables only the primary action button while keeping the dropdown menu trigger active.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public bool PrimaryDisabled { get; set; }

        /// <summary>
        /// Disables only the dropdown menu trigger while keeping the primary action button active.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public bool DropDownDisabled { get; set; }

        /// <summary>
        /// The color of the split button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// The display variant of the split button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Variant.Text"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// The size of the buttons.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Size.Medium"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public Size Size { get; set; } = Size.Medium;

        /// <summary>
        /// Displays a drop shadow elevation.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public bool DropShadow { get; set; } = true;

        /// <summary>
        /// Shows a ripple effect on click.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public bool Ripple { get; set; } = true;

        /// <summary>
        /// Stretches the split button to 100% of the available container width.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public bool FullWidth { get; set; }

        /// <summary>
        /// Displays the primary button and dropdown arrow vertically.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public bool Vertical { get; set; }

        /// <summary>
        /// Uses compact vertical padding in the dropdown menu list items.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public bool Dense { get; set; }

        /// <summary>
        /// The icon displayed before the primary button text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? StartIcon { get; set; }

        /// <summary>
        /// The icon displayed after the primary button text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? EndIcon { get; set; }

        /// <summary>
        /// The icon displayed on the dropdown trigger button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.ArrowDropDown"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string DropDownIcon { get; set; } = Icons.Material.Filled.ArrowDropDown;

        /// <summary>
        /// The color of the primary button icons.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Inherit"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Color IconColor { get; set; } = Color.Inherit;

        /// <summary>
        /// The size of the primary button icons.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, using the button's <see cref="Size"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Size? IconSize { get; set; }

        /// <summary>
        /// Custom CSS classes applied to primary button icons.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public string? IconClass { get; set; }

        /// <summary>
        /// Accessible label applied to the primary action button.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? AriaLabel { get; set; }

        /// <summary>
        /// Accessible label applied to the dropdown menu trigger button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>"Open menu"</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string DropDownAriaLabel { get; set; } = "Open menu";

        /// <summary>
        /// Custom CSS class applied to the dropdown popover.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public string? PopoverClass { get; set; }

        /// <summary>
        /// The anchor origin point from which the dropdown popover opens.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Origin.BottomRight"/> for horizontal and <see cref="Origin.BottomLeft"/> for vertical layouts.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public Origin? AnchorOrigin { get; set; }

        /// <summary>
        /// The transform origin point of the dropdown popover.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Origin.TopLeft"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public Origin TransformOrigin { get; set; } = Origin.TopLeft;

        /// <summary>
        /// Maximum height in pixels for the dropdown menu popover.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>300</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Menu.PopupAppearance)]
        public int? MaxHeight { get; set; } = 300;

        /// <summary>
        /// Opens the contextual dropdown menu.
        /// </summary>
        public Task OpenMenuAsync()
        {
            return _openState.SetValueAsync(true);
        }

        /// <summary>
        /// Closes the contextual dropdown menu.
        /// </summary>
        public Task CloseMenuAsync()
        {
            return _openState.SetValueAsync(false);
        }

        /// <summary>
        /// Toggles the open state of the contextual dropdown menu.
        /// </summary>
        public Task ToggleMenuAsync()
        {
            return _openState.SetValueAsync(!_openState.Value);
        }

        private Task HandlePrimaryClickAsync(MouseEventArgs args)
        {
            if (IsPrimaryDisabled)
            {
                return Task.CompletedTask;
            }

            return OnClick.InvokeAsync(args);
        }

        private async Task HandleRootKeyDownAsync(KeyboardEventArgs e)
        {
            if (_disabledState.Value)
            {
                return;
            }

            if (e.AltKey && e.Key == "ArrowDown")
            {
                if (!_openState.Value)
                {
                    await OpenMenuAsync();
                }
            }
            else if ((e.AltKey && e.Key == "ArrowUp") || (e.Key == "Escape" && _openState.Value))
            {
                await CloseMenuAsync();
            }
        }

        private async Task OnOpenChangedAsync(ParameterChangedEventArgs<bool> args)
        {
            if (_menuRef is null)
            {
                return;
            }

            if (args.Value)
            {
                await _menuRef.OpenMenuAsync(EventArgs.Empty);
            }
            else
            {
                await _menuRef.CloseMenuAsync();
            }
        }

        private Task HandleOpenChangedAsync(bool isOpen)
        {
            return _openState.SetValueAsync(isOpen);
        }
    }
}
