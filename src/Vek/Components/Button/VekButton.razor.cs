using Microsoft.AspNetCore.Components;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Buttons trigger actions, submit forms, or navigate to a link.
    /// </summary>
    /// <remarks>
    /// Creates a <see href="https://developer.mozilla.org/docs/Web/HTML/Element/Button">button</see> element,
    /// or <see href="https://developer.mozilla.org/docs/Web/HTML/Element/a">anchor</see> if <c>Href</c> is set.<br/>
    /// You can directly add attributes like <c>title</c> or <c>aria-label</c>.
    /// </remarks>
    /// <seealso cref="VekButtonGroup" />
    /// <seealso cref="VekFab" />
    /// <seealso cref="VekIconButton" />
    /// <seealso cref="VekToggleIconButton" />
    public partial class VekButton : VekBaseButton, IDisposable
    {
        private readonly ParameterState<bool> _loadingState;

        protected string Classname => new CssBuilder("vek-button-root vek-button")
            .AddClass($"vek-button-{Variant.ToStringFast(true)}")
            .AddClass($"vek-button-{Variant.ToStringFast(true)}-{Color.ToStringFast(true)}")
            .AddClass($"vek-button-{Variant.ToStringFast(true)}-size-{Size.ToStringFast(true)}")
            .AddClass($"vek-width-full", GetRealFullWith())
              .AddClass("vek-button-loading", IsLoading)
            .AddClass($"vek-ripple", Ripple)
            .AddClass($"vek-button-disable-elevation", !DropShadow)
            .AddClass(Class)
            .Build();

        protected string StartIconClass => new CssBuilder("vek-button-icon-start")
            .AddClass($"vek-button-icon-size-{(IconSize ?? Size).ToStringFast(true)}")
            .AddClass(IconClass)
            .Build();

        protected string EndIconClass => new CssBuilder("vek-button-icon-end")
            .AddClass($"vek-button-icon-size-{(IconSize ?? Size).ToStringFast(true)}")
            .AddClass(IconClass)
            .Build();

        /// <summary>
        /// The button group which owns this button.
        /// </summary>
        [CascadingParameter]
        private VekButtonGroup? ButtonGroup { get; set; }

        /// <summary>
        /// The icon displayed before the text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  Use <see cref="EndIcon"/> to display an icon after the text.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? StartIcon { get; set; }

        /// <summary>
        /// The icon displayed after the text.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  Use <see cref="StartIcon"/> to display an icon before the text.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public string? EndIcon { get; set; }

        /// <summary>
        /// The color of icons.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Inherit"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Color IconColor { get; set; } = Color.Inherit;

        /// <summary>
        /// The size of icons.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When <c>null</c>, the value of <see cref="Size"/> is used.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Size? IconSize { get; set; }

        /// <summary>
        /// The CSS classes applied to icons.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  You can use spaces to separate multiple classes.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public string? IconClass { get; set; }

        /// <summary>
        /// The color of the button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Default"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// The size of the button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Size.Medium"/>.
        /// Use the <see cref="IconSize"/> property to set the size of icons.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Size Size { get; set; } = Size.Medium;

        /// <summary>
        /// The display variation to use.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Variant.Text"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public Variant Variant { get; set; } = Variant.Text;

        /// <summary>
        /// Expands the button to 100% of the container width.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Appearance)]
        public bool FullWidth { get; set; }

        /// <summary>
        /// The content within this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Shows a loading indicator and prevents the button from being activated.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Behavior)]
        public bool Loading { get; set; }

        /// <summary>
        /// Receives changes to <see cref="Loading"/> when it is bound.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public EventCallback<bool> LoadingChanged { get; set; }

        /// <summary>
        /// Supplies custom content for the loading indicator.
        /// </summary>
        /// <remarks>
        /// Defaults to a small indeterminate progress indicator.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public RenderFragment? LoadingTemplate { get; set; }

        public VekButton()
        {
            using var registerScope = CreateRegisterScope();
            _loadingState = registerScope.RegisterParameter<bool>(nameof(Loading))
                .WithParameter(() => Loading)
                .WithEventCallback(() => LoadingChanged);
        }

        protected bool IsLoading => _loadingState.Value;

        protected override bool GetDisabledState() => IsLoading || base.GetDisabledState();

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ButtonGroup?.AddButton(this);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by this button.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ButtonGroup?.RemoveButton(this);
            }
        }

        internal bool GetRealFullWith()
        {
            if (FullWidth)
            {
                return true;
            }
            // If the button is in a group, the group is stretched and none button is explicitly stretched,
            // then the button need to be stretched
            // See https://github.com/madiseh/vek/issues/9710
            return ButtonGroup != null && ButtonGroup.FullWidth && ButtonGroup.NoneButtonIsStreched();
        }
    }
}
