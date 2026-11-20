using Microsoft.AspNetCore.Components;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Groups related <see cref="VekButton"/> components together visually.
    /// </summary>
    /// <seealso cref="VekButton" />
    /// <seealso cref="VekToggleGroup{T}"/>
    public partial class VekButtonGroup : VekComponentBase
    {
        private readonly ParameterState<ButtonGroupSelectionMode> _selectionModeState;
        private readonly ParameterState<object?> _selectedValueState;
        private readonly ParameterState<IReadOnlyCollection<object?>?> _selectedValuesState;

        public VekButtonGroup()
        {
            using var registerScope = CreateRegisterScope();
            _selectionModeState = registerScope.RegisterParameter<ButtonGroupSelectionMode>(nameof(SelectionMode))
                .WithParameter(() => SelectionMode)
                .WithChangeHandler(RefreshButtons);
            _selectedValueState = registerScope.RegisterParameter<object?>(nameof(SelectedValue))
                .WithParameter(() => SelectedValue)
                .WithEventCallback(() => SelectedValueChanged)
                .WithChangeHandler(RefreshButtons);
            _selectedValuesState = registerScope.RegisterParameter<IReadOnlyCollection<object?>?>(nameof(SelectedValues))
                .WithParameter(() => SelectedValues)
                .WithEventCallback(() => SelectedValuesChanged)
                .WithChangeHandler(RefreshButtons);
        }

        protected string Classname => new CssBuilder("vek-button-group-root")
            .AddClass($"vek-button-group-override-styles", OverrideStyles)
            .AddClass($"vek-button-group-{Variant.ToStringFast(true)}")
            .AddClass($"vek-button-group-{Variant.ToStringFast(true)}-{Color.ToStringFast(true)}")
            .AddClass($"vek-button-group-{Variant.ToStringFast(true)}-size-{Size.ToStringFast(true)}")
            .AddClass("vek-button-group-vertical", Vertical)
            .AddClass("vek-button-group-horizontal", !Vertical)
            .AddClass("vek-button-group-disable-elevation", !DropShadow)
            .AddClass("vek-button-group-rtl", RightToLeft)
            .AddClass("vek-width-full", FullWidth)
            .AddClass(Class)
            .Build();

        /// <summary>
        /// Displays buttons right-to-left.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        /// <summary>
        /// Overrides individual button styles with this group's style.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.  When <c>true</c>, the button styles are defined by this group.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Appearance)]
        public bool OverrideStyles { get; set; } = true;

        /// <summary>
        /// The custom content within this group.
        /// </summary>
        /// <remarks>
        /// This property allows for custom content to displayed inside of the group, but is not required.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Displays buttons vertically.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  When <c>true</c>, buttons will be displayed vertically, otherwise horizontally.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Appearance)]
        public bool Vertical { get; set; }

        /// <summary>
        /// Displays a shadow.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Appearance)]
        public bool DropShadow { get; set; } = true;

        /// <summary>
        /// The color of all buttons in this group.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default" />.  Theme colors are supported.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// The size of all buttons in the group.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Size.Medium"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Appearance)]
        public Size Size { get; set; } = Size.Medium;

        /// <summary>
        /// The display variant of all buttons in the group.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Variant.Text"/>.  Other supported values are <see cref="Variant.Outlined"/> and <see cref="Variant.Filled"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Appearance)]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// If true, the button group will take up 100% of available width.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Appearance)]
        public bool FullWidth { get; set; }

        /// <summary>
        /// Controls whether buttons act as ordinary actions or retain a selection.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="ButtonGroupSelectionMode.None"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.ButtonGroup.Behavior)]
        public ButtonGroupSelectionMode SelectionMode { get; set; }

        /// <summary>
        /// The selected button key when <see cref="SelectionMode"/> is <see cref="ButtonGroupSelectionMode.Single"/>.
        /// </summary>
        [Parameter, ParameterState]
        [Category(CategoryTypes.ButtonGroup.Behavior)]
        public object? SelectedValue { get; set; }

        /// <summary>
        /// Receives changes to <see cref="SelectedValue"/>.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Behavior)]
        public EventCallback<object?> SelectedValueChanged { get; set; }

        /// <summary>
        /// The selected button keys when <see cref="SelectionMode"/> is <see cref="ButtonGroupSelectionMode.Multiple"/>.
        /// </summary>
        [Parameter, ParameterState]
        [Category(CategoryTypes.ButtonGroup.Behavior)]
        public IReadOnlyCollection<object?>? SelectedValues { get; set; }

        /// <summary>
        /// Receives changes to <see cref="SelectedValues"/>.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Behavior)]
        public EventCallback<IReadOnlyCollection<object?>?> SelectedValuesChanged { get; set; }

        /// <summary>
        /// Occurs when the selected button keys change.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.ButtonGroup.Behavior)]
        public EventCallback<IReadOnlyCollection<object?>> OnSelectionChanged { get; set; }

        private readonly List<VekButton> _renderedButtons = [];

        internal void AddButton(VekButton button)
        {
            _renderedButtons.Add(button);
        }

        internal bool? GetAriaPressed(VekButton button)
        {
            if (button.Value is null)
            {
                return _selectionModeState.Value == ButtonGroupSelectionMode.None ? null : false;
            }

            return _selectionModeState.Value switch
            {
                ButtonGroupSelectionMode.Single => Equals(_selectedValueState.Value, button.Value),
                ButtonGroupSelectionMode.Multiple => _selectedValuesState.Value?.Contains(button.Value) ?? false,
                _ => null
            };
        }

        internal async Task HandleButtonClickAsync(VekButton button)
        {
            if (button.Value is not { } value)
            {
                return;
            }

            switch (_selectionModeState.Value)
            {
                case ButtonGroupSelectionMode.Single:
                    if (Equals(_selectedValueState.Value, value))
                    {
                        return;
                    }

                    await _selectedValueState.SetValueAsync(value);
                    RefreshButtons();
                    await OnSelectionChanged.InvokeAsync(new object?[] { value });
                    break;

                case ButtonGroupSelectionMode.Multiple:
                    var selectedValues = new HashSet<object?>(_selectedValuesState.Value ?? []);
                    if (!selectedValues.Remove(value))
                    {
                        selectedValues.Add(value);
                    }

                    await _selectedValuesState.SetValueAsync(selectedValues);
                    RefreshButtons();
                    await OnSelectionChanged.InvokeAsync(selectedValues);
                    break;
            }
        }

        internal void RemoveButton(VekButton button)
        {
            _renderedButtons.Remove(button);
        }

        internal bool NoneButtonIsStreched()
        {
            return !_renderedButtons.Any(b => b.FullWidth);
        }

        private void RefreshButtons()
        {
            foreach (var button in _renderedButtons)
            {
                button.RefreshSelection();
            }
        }
    }
}
