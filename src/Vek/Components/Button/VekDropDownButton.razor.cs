using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Extensions;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// A dropdown button combines an action button label and an arrow icon to open a contextual flyout action menu.
    /// </summary>
    /// <seealso cref="VekButton" />
    /// <seealso cref="VekSplitButton" />
    /// <seealso cref="VekMenu" />
    public partial class VekDropDownButton : VekComponentBase
    {
        private readonly ParameterState<bool> _openState;
        private readonly ParameterState<bool> _disabledState;
        private readonly ParameterState<Variant> _variantState;
        private readonly ParameterState<Color> _colorState;
        private readonly ParameterState<Size> _sizeState;
        private VekMenu? _menuRef;

        public VekDropDownButton()
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

        protected string Classname => new CssBuilder("vek-dropdown-button-root")
            .AddClass("vek-dropdown-button-rtl", RightToLeft)
            .AddClass("vek-width-full", FullWidth)
            .AddClass(Class)
            .Build();

        protected string MenuClassname => new CssBuilder("vek-dropdown-button-menu")
            .AddClass("vek-width-full", FullWidth)
            .Build();

        protected string ButtonClassname => new CssBuilder("vek-dropdown-button")
            .AddClass("vek-width-full", FullWidth)
            .Build();

        /// <summary>
        /// Displays buttons right-to-left.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        /// <summary>
        /// Text displayed on the dropdown button.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? Text { get; set; }

        /// <summary>
        /// Custom content rendered inside the dropdown button, or child menu items when MenuItems is specified.
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
        /// Disables the dropdown button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Behavior)]
        public bool Disabled { get; set; }

        /// <summary>
        /// The color of the button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// The display variant of the button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Variant.Text"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// The size of the button.
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
        /// Stretches the button to 100% of the available container width.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public bool FullWidth { get; set; }

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
        /// The icon displayed before the button text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? StartIcon { get; set; }

        /// <summary>
        /// The icon displayed after the button text. When null, <see cref="DropDownIcon"/> is used.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? EndIcon { get; set; }

        /// <summary>
        /// The icon displayed to indicate dropdown functionality.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.ArrowDropDown"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string DropDownIcon { get; set; } = Icons.Material.Filled.ArrowDropDown;

        /// <summary>
        /// The color of the button icons.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Inherit"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Color IconColor { get; set; } = Color.Inherit;

        /// <summary>
        /// The size of the button icons.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, using the button's <see cref="Size"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Size? IconSize { get; set; }

        /// <summary>
        /// Custom CSS classes applied to button icons.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public string? IconClass { get; set; }

        /// <summary>
        /// Accessible label applied to the dropdown button.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? AriaLabel { get; set; }

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
        /// Defaults to <see cref="Origin.BottomLeft"/>.
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
