using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        private readonly ParameterState<int> _clickDebounceIntervalState;
        private readonly ParameterState<int> _doubleClickThresholdState;
        private readonly ParameterState<object?> _badgeContentState;
        private readonly ParameterState<Color> _badgeColorState;
        private readonly ParameterState<bool> _badgeDotState;
        private readonly ParameterState<BadgePosition> _badgePositionState;
        private readonly ParameterState<bool> _badgeVisibleState;
        private readonly ParameterState<bool> _badgeOverlapState;
        private readonly ParameterState<bool> _badgeBorderedState;
        private readonly ParameterState<int?> _badgeMaxState;
        private readonly ParameterState<string?> _badgeClassState;
        private readonly ParameterState<string?> _badgeAriaLabelState;
        private readonly ParameterState<bool> _rightToLeftState;
        private long? _lastAcceptedClickTimestamp;
        private long? _pendingClickTimestamp;

        protected string Classname => new CssBuilder("vek-button-root vek-button")
            .AddClass($"vek-button-{Variant.ToStringFast(true)}")
            .AddClass($"vek-button-{Variant.ToStringFast(true)}-{Color.ToStringFast(true)}")
            .AddClass($"vek-button-{Variant.ToStringFast(true)}-size-{Size.ToStringFast(true)}")
            .AddClass($"vek-width-full", GetRealFullWith())
            .AddClass("vek-button-loading", IsLoading)
            .AddClass("vek-button-has-badge", HasBadge)
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
        /// The key used by a containing button group to track this button's selection.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. Assign a non-null value when the button participates in a selectable group.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public object? Value { get; set; }

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
        /// The minimum time in milliseconds between click event invocations.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>300</c>. Set to <c>0</c> to allow every click through.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Behavior)]
        public int ClickDebounceInterval { get; set; } = 300;

        /// <summary>
        /// The time in milliseconds within which a second click raises <see cref="OnDoubleClick"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>300</c>. Set to <c>0</c> to disable double-click detection.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Behavior)]
        public int DoubleClickThreshold { get; set; } = 300;

        /// <summary>
        /// Occurs when a second click arrives within <see cref="DoubleClickThreshold"/> milliseconds.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public EventCallback<MouseEventArgs> OnDoubleClick { get; set; }

        /// <summary>
        /// Supplies custom content for the loading indicator.
        /// </summary>
        /// <remarks>
        /// Defaults to a small indeterminate progress indicator.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Button.Behavior)]
        public RenderFragment? LoadingTemplate { get; set; }

        /// <summary>
        /// Indicates whether logical start and end positions should follow right-to-left layout.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        /// <summary>
        /// The content displayed inside the badge adornment.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set to an integer, values exceeding <see cref="BadgeMax"/> are capped with a plus sign.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public object? BadgeContent { get; set; }

        /// <summary>
        /// The color of the badge adornment.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Primary"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public Color BadgeColor { get; set; } = Color.Primary;

        /// <summary>
        /// Displays the badge as a compact status dot without content.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public bool BadgeDot { get; set; }

        /// <summary>
        /// The position of the badge relative to the button.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="BadgePosition.TopEnd"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public BadgePosition BadgePosition { get; set; } = BadgePosition.TopEnd;

        /// <summary>
        /// Controls whether the badge adornment is displayed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. When <c>true</c>, the badge renders if <see cref="BadgeDot"/> is <c>true</c> or <see cref="BadgeContent"/> is non-null.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public bool BadgeVisible { get; set; } = true;

        /// <summary>
        /// Displays the badge overlapping the button border.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public bool BadgeOverlap { get; set; }

        /// <summary>
        /// Displays a border around the badge adornment.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public bool BadgeBordered { get; set; }

        /// <summary>
        /// Caps numeric badge content at this value and appends a plus sign when exceeded.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>99</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public int? BadgeMax { get; set; } = 99;

        /// <summary>
        /// Custom CSS classes applied to the badge element.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Appearance)]
        public string? BadgeClass { get; set; }

        /// <summary>
        /// The <c>aria-label</c> attribute for the badge adornment.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Button.Behavior)]
        public string? BadgeAriaLabel { get; set; }

        public VekButton()
        {
            using var registerScope = CreateRegisterScope();
            _loadingState = registerScope.RegisterParameter<bool>(nameof(Loading))
                .WithParameter(() => Loading)
                .WithEventCallback(() => LoadingChanged);
            _clickDebounceIntervalState = registerScope.RegisterParameter<int>(nameof(ClickDebounceInterval))
                .WithParameter(() => ClickDebounceInterval);
            _doubleClickThresholdState = registerScope.RegisterParameter<int>(nameof(DoubleClickThreshold))
                .WithParameter(() => DoubleClickThreshold);
            _badgeContentState = registerScope.RegisterParameter<object?>(nameof(BadgeContent))
                .WithParameter(() => BadgeContent);
            _badgeColorState = registerScope.RegisterParameter<Color>(nameof(BadgeColor))
                .WithParameter(() => BadgeColor);
            _badgeDotState = registerScope.RegisterParameter<bool>(nameof(BadgeDot))
                .WithParameter(() => BadgeDot);
            _badgePositionState = registerScope.RegisterParameter<BadgePosition>(nameof(BadgePosition))
                .WithParameter(() => BadgePosition);
            _badgeVisibleState = registerScope.RegisterParameter<bool>(nameof(BadgeVisible))
                .WithParameter(() => BadgeVisible);
            _badgeOverlapState = registerScope.RegisterParameter<bool>(nameof(BadgeOverlap))
                .WithParameter(() => BadgeOverlap);
            _badgeBorderedState = registerScope.RegisterParameter<bool>(nameof(BadgeBordered))
                .WithParameter(() => BadgeBordered);
            _badgeMaxState = registerScope.RegisterParameter<int?>(nameof(BadgeMax))
                .WithParameter(() => BadgeMax);
            _badgeClassState = registerScope.RegisterParameter<string?>(nameof(BadgeClass))
                .WithParameter(() => BadgeClass);
            _badgeAriaLabelState = registerScope.RegisterParameter<string?>(nameof(BadgeAriaLabel))
                .WithParameter(() => BadgeAriaLabel);
            _rightToLeftState = registerScope.RegisterParameter<bool>(nameof(RightToLeft))
                .WithParameter(() => RightToLeft);
        }

        protected bool IsLoading => _loadingState.Value;

        protected bool IsBadgeDot => _badgeDotState.Value;

        protected string? GetBadgeAriaLabel => _badgeAriaLabelState.Value;

        protected bool HasBadge => _badgeVisibleState.Value
            && (_badgeDotState.Value || (_badgeContentState.Value is not null && !string.IsNullOrEmpty(_badgeContentState.Value.ToString())));

        protected string BadgePositionClass
        {
            get
            {
                var position = _badgePositionState.Value;
                var vertical = position is BadgePosition.TopEnd or BadgePosition.TopStart ? "top" : "bottom";
                var isEnd = position is BadgePosition.TopEnd or BadgePosition.BottomEnd;
                var isRight = isEnd != _rightToLeftState.Value;
                var horizontal = isRight ? "right" : "left";
                return $"vek-badge-{vertical} {horizontal}";
            }
        }

        protected string BadgeWrapperClassname => new CssBuilder("vek-badge-wrapper")
            .AddClass(BadgePositionClass)
            .Build();

        protected string BadgeClassname => new CssBuilder("vek-badge vek-button-badge")
            .AddClass("vek-badge-dot", _badgeDotState.Value)
            .AddClass("vek-badge-bordered", _badgeBorderedState.Value)
            .AddClass(BadgePositionClass)
            .AddClass("vek-theme-" + _badgeColorState.Value.ToStringFast(true), _badgeColorState.Value != Color.Default)
            .AddClass("vek-badge-default", _badgeColorState.Value == Color.Default)
            .AddClass("vek-badge-overlap", _badgeOverlapState.Value)
            .AddClass(_badgeClassState.Value)
            .Build();

        protected string? BadgeFormattedContent
        {
            get
            {
                var content = _badgeContentState.Value;
                if (content is null)
                {
                    return null;
                }

                if (content is int intVal)
                {
                    var max = _badgeMaxState.Value;
                    if (max.HasValue && intVal > max.Value)
                    {
                        return $"{max.Value}+";
                    }

                    return intVal.ToString();
                }

                if (content is string strVal)
                {
                    if (int.TryParse(strVal, out var parsedInt))
                    {
                        var max = _badgeMaxState.Value;
                        if (max.HasValue && parsedInt > max.Value)
                        {
                            return $"{max.Value}+";
                        }
                    }

                    return strVal;
                }

                return content.ToString();
            }
        }

        [Inject]
        private TimeProvider TimeProvider { get; set; } = null!;

        protected override async Task OnClickHandler(MouseEventArgs ev)
        {
            if (GetDisabledState())
            {
                return;
            }

            var timestamp = TimeProvider.GetTimestamp();
            var doubleClickThreshold = _doubleClickThresholdState.Value;
            var isDoubleClick = doubleClickThreshold > 0
                                && _pendingClickTimestamp is { } previousClickTimestamp
                                && TimeProvider.GetElapsedTime(previousClickTimestamp, timestamp) <= TimeSpan.FromMilliseconds(doubleClickThreshold);

            _pendingClickTimestamp = isDoubleClick ? null : timestamp;
            if (isDoubleClick)
            {
                await OnDoubleClick.InvokeAsync(ev);
            }

            var clickDebounceInterval = _clickDebounceIntervalState.Value;
            if (clickDebounceInterval > 0
                && _lastAcceptedClickTimestamp is { } lastAcceptedClickTimestamp
                && TimeProvider.GetElapsedTime(lastAcceptedClickTimestamp, timestamp) < TimeSpan.FromMilliseconds(clickDebounceInterval))
            {
                return;
            }

            _lastAcceptedClickTimestamp = timestamp;
            if (ButtonGroup is not null)
            {
                await ButtonGroup.HandleButtonClickAsync(this);
            }
            await base.OnClickHandler(ev);
        }

        internal void RefreshSelection() => StateHasChanged();

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
