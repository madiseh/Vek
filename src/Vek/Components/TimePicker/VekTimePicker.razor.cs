// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Vek.Resources;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Selects a time of day from a clock shown in a drop-down, dialog, or inline.
    /// </summary>
    /// <seealso cref="VekDatePicker" />
    /// <seealso cref="VekDateRangePicker" />
    public partial class VekTimePicker : VekPicker<TimeSpan?>
    {
        private OpenTo _currentView;
        private string? _clockElementReferenceId;
        private readonly SetTime _timeSet = new();
        private readonly Lazy<DotNetObjectReference<VekTimePicker>> _dotNetReferenceLazy;

        private readonly ParameterState<TimeEditFormat?> _timeEditFormatState;
        private readonly ParameterState<bool> _showSecondsState;
        private readonly ParameterState<int> _minuteIntervalState;
        private readonly ParameterState<int> _secondIntervalState;
        private readonly ParameterState<int> _secondSelectionStepState;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        [Inject]
        private TimeProvider TimeProvider { get; set; } = null!;

        [DynamicDependency(nameof(OnStickClick))]
        [DynamicDependency(nameof(SelectTimeFromStick))]
        public VekTimePicker()
        {
            AdornmentIcon = Icons.Material.Filled.AccessTime;
            _dotNetReferenceLazy = new Lazy<DotNetObjectReference<VekTimePicker>>(CreateDotNetObjectReference);
            using var registerScope = CreateRegisterScope();
            registerScope.RegisterParameter<bool>(nameof(AmPm))
                .WithParameter(() => AmPm)
                .WithChangeHandler(FormatChangedAsync);
            registerScope.RegisterParameter<string?>(nameof(TimeFormat))
                .WithParameter(() => TimeFormat)
                .WithChangeHandler(FormatChangedAsync);
            _timeEditFormatState = registerScope.RegisterParameter<TimeEditFormat?>(nameof(TimeEditFormat))
                .WithParameter(() => TimeEditFormat)
                .WithChangeHandler(FormatChangedAsync);
            _showSecondsState = registerScope.RegisterParameter<bool>(nameof(ShowSeconds))
                .WithParameter(() => ShowSeconds)
                .WithChangeHandler(FormatChangedAsync);
            _minuteIntervalState = registerScope.RegisterParameter<int>(nameof(MinuteInterval))
                .WithParameter(() => MinuteInterval);
            _secondIntervalState = registerScope.RegisterParameter<int>(nameof(SecondInterval))
                .WithParameter(() => SecondInterval);
            _secondSelectionStepState = registerScope.RegisterParameter<int>(nameof(SecondSelectionStep))
                .WithParameter(() => SecondSelectionStep);
        }

        internal TimeSpan? TimeIntermediate { get; private set; }

        /// <summary>
        /// The initial view for this picker.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="OpenTo.Hours"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public OpenTo OpenTo { get; set; } = OpenTo.Hours;

        /// <summary>
        /// Controls which values can be edited.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TimeEditMode.Normal"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public TimeEditMode TimeEditMode { get; set; } = TimeEditMode.Normal;

        /// <summary>
        /// The amount of time, in milliseconds, to wait before closing the picker.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>200</c>. The delay gives users a moment to see the selected time before the popover disappears.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int ClosingDelay { get; set; } = 200;

        /// <summary>
        /// Closes this picker when the value is set or cleared.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When <c>true</c> and <c>PickerActions</c> are defined, 
        /// the hour and the minutes can be selected and the drop-down will close without having to 
        /// click any of the action buttons.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public bool AutoClose { get; set; }

        /// <summary>
        /// The step interval when selecting minutes.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>. For example: a value of <c>15</c> would allow minutes <c>0</c>, <c>15</c>, 
        /// <c>30</c>, and <c>45</c> be selected.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int MinuteSelectionStep { get; set; } = 1;

        /// <summary>
        /// The interval in minutes between selectable values.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>. Common intervals are <c>1</c>, <c>5</c>, <c>10</c>, <c>15</c>, and <c>30</c> minutes.
        /// When set, minutes are snapped to the nearest step during selection and keyboard navigation.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int MinuteInterval { get; set; } = 1;

        /// <summary>
        /// The interval in seconds between selectable values.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>. For example, a value of <c>5</c> allows seconds <c>0</c>, <c>5</c>, <c>10</c>, etc.
        /// Snaps seconds during selection and keyboard navigation.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int SecondInterval { get; set; } = 1;

        /// <summary>
        /// The step interval when selecting seconds.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>. Alias for <see cref="SecondInterval"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.PickerBehavior)]
        public int SecondSelectionStep { get; set; } = 1;

        /// <summary>
        /// Enables selection and display of seconds.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When <c>true</c>, a seconds column is displayed in the toolbar,
        /// and seconds can be selected via a seconds clock dial or keyboard navigation.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool ShowSeconds { get; set; }

        /// <summary>
        /// Specifies whether the picker operates in 12-hour (with AM/PM) or 24-hour mode.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, falling back to <see cref="AmPm"/>.
        /// When set to <see cref="TimeEditFormat.TwelveHour"/>, 12-hour AM/PM mode is used.
        /// When set to <see cref="TimeEditFormat.TwentyFourHour"/>, 24-hour mode is used.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public TimeEditFormat? TimeEditFormat { get; set; }

        /// <summary>
        /// The accessible name for the hours toolbar button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>"Select hours"</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string HoursButtonAriaLabel { get; set; } = "Select hours";

        /// <summary>
        /// The accessible name for the minutes toolbar button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>"Select minutes"</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string MinutesButtonAriaLabel { get; set; } = "Select minutes";

        /// <summary>
        /// The accessible name for the seconds toolbar button.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>"Select seconds"</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string SecondsButtonAriaLabel { get; set; } = "Select seconds";

        /// <summary>
        /// Shows a 12-hour selection clock.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.<br />
        /// When <c>true</c>, hours 1-12 are displayed with an AM or PM marker.<br />
        /// When <c>false</c>, hours 0-23 are displayed.<br />
        /// </remarks>
        [Parameter, ParameterState(ParameterUsage = ParameterUsageOptions.None)]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool AmPm { get; set; }

        /// <summary>
        /// The format applied to time values.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>hh:mm tt</c> when <see cref="AmPm"/> is <c>true</c>, otherwise <c>HH:mm</c>.<br />
        /// Format strings are typically a combination of these characters:<br />
        /// * <c>h</c> (lowercase) for hours in 12-hour time, <br />
        /// * <c>H</c> (uppercase) for hours in 24-hour time, <br />
        /// * <c>m</c> for minutes,<br />
        /// * <c>tt</c> for AM/PM markers.<br />
        /// For example: <c>h:mm tt</c> would display <c>6:32 PM</c>, and <c>HH:mm</c> would display <c>18:32</c>.
        /// </remarks>
        [Parameter, ParameterState(ParameterUsage = ParameterUsageOptions.None)]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public string TimeFormat { get; set; } = string.Empty;

        /// <summary>
        /// The currently selected time.
        /// </summary>
        /// <remarks>
        /// When this value changes, <see cref="TimeChanged"/> occurs.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Data)]
        public TimeSpan? Time
        {
            get => _value;
            // Programmatic parameter assignment; pass suppression explicitly so it cannot leak across the
            // awaits inside SetTimeAsync into a concurrent user clock pick on Blazor Server (PR #13328 review).
            set => SetTimeAsync(value, updateValue: true, suppressInteraction: true).CatchAndLog();
        }

        /// <summary>
        /// Occurs when <see cref="Time"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<TimeSpan?> TimeChanged { get; set; }

        /// <summary>
        /// Sets the selected time value.
        /// </summary>
        /// <param name="time">The new value to set.</param>
        /// <param name="updateValue">When <c>true</c>, the <c>Text</c> will also be updated.</param>
        /// <param name="suppressInteraction">When <c>true</c>, the change is treated as programmatic and does not mark the picker touched or fire <c>FieldChanged</c>.</param>
        protected async Task SetTimeAsync(TimeSpan? time, bool updateValue, bool suppressInteraction = false)
        {
            // Text that fails to convert leaves the value null, so a null assignment looks like a no-op and the bad text would stick.
            // Run it anyway while text remains, as VekDatePicker does.
            if (_value != time || (time is null && !string.IsNullOrEmpty(Text)))
            {
                if (!suppressInteraction)
                {
                    Touched = true;
                }
                TimeIntermediate = time;
                _value = time;
                if (updateValue)
                {
                    ResetConverterErrors();
                    await SetTextAsync(ConvertSet(_value), false);
                }

                UpdateTimeSetFromTime();
                await TimeChanged.InvokeAsync(_value);
                await BeginValidateAsync();
                if (!suppressInteraction)
                {
                    FieldChanged(_value);
                }
            }
        }

        /// <inheritdoc />
        protected override Task StringValueChangedAsync(string? value)
        {
            Touched = true;

            // Update the time property (without updating back the Value property)
            return SetTimeAsync(ConvertGet(value), false);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage] // The last line cannot be tested.
        protected override async Task OnPickerOpenedAsync()
        {
            await base.OnPickerOpenedAsync();
            _currentView = TimeEditMode switch
            {
                TimeEditMode.Normal => OpenTo,
                TimeEditMode.OnlyHours => OpenTo.Hours,
                TimeEditMode.OnlyMinutes => OpenTo.Minutes,
                TimeEditMode.OnlySeconds => OpenTo.Seconds,
                _ => _currentView
            };
        }

        /// <inheritdoc />
        protected internal override Task SubmitAsync()
        {
            if (GetReadOnlyState())
            {
                return Task.CompletedTask;
            }

            // Commit via SetTimeAsync directly, NOT the Time setter: the setter is suppressed, but a submit
            // is a user commit that must touch and fire FieldChanged (as VekDatePicker/VekDateRangePicker do).
            return SetTimeAsync(TimeIntermediate, updateValue: true);
        }

        /// <inheritdoc />
        public override async Task ClearAsync(bool close = true)
        {
            TimeIntermediate = null;
            await SetTimeAsync(null, true);

            if (AutoClose)
            {
                await CloseAsync(false);
            }
        }

        private int EffectiveMinuteInterval => _minuteIntervalState.Value > 1 ? _minuteIntervalState.Value : MinuteSelectionStep;

        private int EffectiveSecondInterval => _secondIntervalState.Value > 1 ? _secondIntervalState.Value : (_secondSelectionStepState.Value > 1 ? _secondSelectionStepState.Value : 1);

        private bool IsAmPm => _timeEditFormatState.Value switch
        {
            Vek.TimeEditFormat.TwelveHour => true,
            Vek.TimeEditFormat.TwentyFourHour => false,
            _ => AmPm
        };

        /// <summary>
        /// Gets the hour portion of the selected time.
        /// </summary>
        /// <returns>A two-character string depending on whether <see cref="AmPm"/> is set, or <c>--</c> if no value is set.</returns>
        private string GetHourString()
        {
            if (TimeIntermediate == null)
            {
                return "--";
            }

            var h = IsAmPm ? TimeIntermediate.Value.ToAmPmHour() : TimeIntermediate.Value.Hours;
            return $"{Math.Min(23, Math.Max(0, h)):D2}";
        }

        /// <summary>
        /// Gets the minute portion of the selected time.
        /// </summary>
        /// <returns>A two-digit string for minutes, or <c>--</c> if no value is set.</returns>
        private string GetMinuteString()
        {
            if (TimeIntermediate == null)
            {
                return "--";
            }

            return $"{Math.Min(59, Math.Max(0, TimeIntermediate.Value.Minutes)):D2}";
        }

        /// <summary>
        /// Gets the second portion of the selected time.
        /// </summary>
        /// <returns>A two-digit string for seconds, or <c>--</c> if no value is set.</returns>
        private string GetSecondString()
        {
            if (TimeIntermediate == null)
            {
                return "--";
            }

            return $"{Math.Min(59, Math.Max(0, TimeIntermediate.Value.Seconds)):D2}";
        }

        private Task UpdateTimeAsync()
        {
            if (GetReadOnlyState())
                return Task.CompletedTask;
            TimeIntermediate = new TimeSpan(_timeSet.Hour, _timeSet.Minute, _showSecondsState.Value ? _timeSet.Second : 0);
            if ((PickerVariant == PickerVariant.Static && PickerActions == null) || (PickerActions != null && AutoClose))
            {
                return SubmitAsync();
            }

            return Task.CompletedTask;
        }

        private async Task OnHourClickAsync()
        {
            _currentView = OpenTo.Hours;
            await FocusAsync();
        }

        private async Task OnMinutesClick()
        {
            _currentView = OpenTo.Minutes;
            await FocusAsync();
        }

        private async Task OnSecondsClickAsync()
        {
            _currentView = OpenTo.Seconds;
            await FocusAsync();
        }

        private async Task OnAmClickedAsync()
        {
            _timeSet.Hour %= 12;  // "12:-- am" is "00:--" in 24h.
            await UpdateTimeAsync();
            await FocusAsync();
        }

        private async Task OnPmClickedAsync()
        {
            if (_timeSet.Hour <= 12) // "12:-- pm" is "12:--" in 24h.
            {
                _timeSet.Hour += 12;
            }

            _timeSet.Hour %= 24;
            await UpdateTimeAsync();
            await FocusAsync();
        }

        private string HourMinuteContainerClassname =>
            new CssBuilder("vek-timepicker-hourminute vek-ltr")
                .AddClass("vek-timepicker-with-seconds", _showSecondsState.Value)
                .Build();

        protected string ToolbarClassname =>
            new CssBuilder("vek-picker-timepicker-toolbar")
                .AddClass("vek-picker-timepicker-toolbar-landscape", Orientation == Orientation.Landscape && PickerVariant == PickerVariant.Static)
                .AddClass(Class)
                .Build();

        protected string HoursButtonClassname =>
            new CssBuilder("vek-timepicker-button")
                .AddClass("vek-timepicker-toolbar-text", _currentView != OpenTo.Hours)
                .Build();

        protected string MinuteButtonClassname =>
            new CssBuilder("vek-timepicker-button")
                .AddClass("vek-timepicker-toolbar-text", _currentView != OpenTo.Minutes)
                .Build();

        protected string SecondButtonClassname =>
            new CssBuilder("vek-timepicker-button")
                .AddClass("vek-timepicker-toolbar-text", _currentView != OpenTo.Seconds)
                .Build();

        protected string AmButtonClassname =>
            new CssBuilder("vek-timepicker-button")
                .AddClass("vek-timepicker-toolbar-text", !IsAm) // gray it out.
                .Build();

        protected string PmButtonClassname =>
            new CssBuilder("vek-timepicker-button")
                .AddClass("vek-timepicker-toolbar-text", !IsPm) // gray it out.
                .Build();

        private string HourDialClassname =>
            new CssBuilder("vek-time-picker-hour")
                .AddClass("vek-time-picker-dial")
                .AddClass("vek-time-picker-dial-out", _currentView != OpenTo.Hours)
                .AddClass("vek-time-picker-dial-hidden", _currentView != OpenTo.Hours)
                .Build();

        private string MinuteDialClassname =>
            new CssBuilder("vek-time-picker-minute")
                .AddClass("vek-time-picker-dial")
                .AddClass("vek-time-picker-dial-out", _currentView != OpenTo.Minutes)
                .AddClass("vek-time-picker-dial-hidden", _currentView != OpenTo.Minutes)
                .Build();

        private string SecondDialClassname =>
            new CssBuilder("vek-time-picker-second")
                .AddClass("vek-time-picker-dial")
                .AddClass("vek-time-picker-dial-out", _currentView != OpenTo.Seconds)
                .AddClass("vek-time-picker-dial-hidden", _currentView != OpenTo.Seconds)
                .Build();

        private bool IsAm => _timeSet.Hour is >= 00 and < 12; // AM is 00:00 to 11:59.
        private bool IsPm => _timeSet.Hour is >= 12 and < 24; // PM is 12:00 to 23:59.

        private string GetClockPinColor()
        {
            return $"vek-picker-time-clock-pin vek-{Color.ToStringFast(true)}";
        }

        private string GetClockPointerColor()
        {
            return PointerMoving
                ? $"vek-picker-time-clock-pointer vek-{Color.ToStringFast(true)}"
                : $"vek-picker-time-clock-pointer vek-picker-time-clock-pointer-animation vek-{Color.ToStringFast(true)}";
        }

        private string GetClockPointerThumbColor()
        {
            var deg = GetDeg();
            return deg % 30 == 0
                ? $"vek-picker-time-clock-pointer-thumb vek-onclock-text vek-onclock-primary vek-{Color.ToStringFast(true)}"
                : $"vek-picker-time-clock-pointer-thumb vek-onclock-minute vek-{Color.ToStringFast(true)}-text";
        }

        private string GetNumberColor(int value)
        {
            if (_currentView == OpenTo.Hours)
            {
                var h = _timeSet.Hour;

                if (IsAmPm)
                {
                    h = _timeSet.Hour % 12;
                    if (_timeSet.Hour % 12 == 0)
                    {
                        h = 12;
                    }
                }

                if (h == value)
                {
                    return $"vek-clock-number vek-theme-{Color.ToStringFast(true)}";
                }
            }
            else if (_currentView == OpenTo.Minutes && _timeSet.Minute == value)
            {
                return $"vek-clock-number vek-theme-{Color.ToStringFast(true)}";
            }
            else if (_currentView == OpenTo.Seconds && _timeSet.Second == value)
            {
                return $"vek-clock-number vek-theme-{Color.ToStringFast(true)}";
            }

            return "vek-clock-number";
        }

        private double GetDeg()
        {
            double deg = 0;

            if (_currentView == OpenTo.Hours)
            {
                deg = _timeSet.Hour * 30 % 360;
            }
            else if (_currentView == OpenTo.Minutes)
            {
                deg = _timeSet.Minute * 6 % 360;
            }
            else if (_currentView == OpenTo.Seconds)
            {
                deg = _timeSet.Second * 6 % 360;
            }

            return deg;
        }

        private static string GetTransform(double angle, double radius, double offsetX, double offsetY)
        {
            angle = angle / 180 * Math.PI;
            var x = ((Math.Sin(angle) * radius) + offsetX).ToString("F3", CultureInfo.InvariantCulture);
            var y = (((Math.Cos(angle) + 1) * radius) + offsetY).ToString("F3", CultureInfo.InvariantCulture);
            return $"transform: translate({x}px, {y}px);";
        }

        private string GetPointerRotation()
        {
            var deg = GetDeg();
            return $"rotateZ({deg}deg);";
        }

        private string GetPointerHeight()
        {
            var height = 40;

            if (_currentView == OpenTo.Hours)
            {
                if (!IsAmPm && _timeSet.Hour > 0 && _timeSet.Hour < 13)
                {
                    height = 26;
                }
                else
                {
                    height = 40;
                }
            }
            else
            {
                height = 40;
            }

            return $"{height}%;";
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            AdornmentAriaLabel ??= Localizer[LanguageResource.VekTimePicker_Open];
            UpdateTimeSetFromTime();
            _currentView = OpenTo;
        }

        protected ElementReference ClockElementReference { get; private set; }

        // The clock prevents the browser context menu, and before .NET 10 Blazor only honors a handler-less preventDefault while some other contextmenu listener is registered on the page.
        // The handler does nothing, so it must not render either.
        private Action<MouseEventArgs> SuppressContextMenuHandler => _suppressContextMenuHandler ??= this.AsNonRenderingEventHandler<MouseEventArgs>(static _ => { });

        private Action<MouseEventArgs>? _suppressContextMenuHandler;

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            // Initialize the pointer events for the clock every time it's created (ex: popover opening and closing).
            if (ClockElementReference.Id != _clockElementReferenceId)
            {
                _clockElementReferenceId = ClockElementReference.Id;

                await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekTimePicker.initPointerEvents", ClockElementReference, _dotNetReferenceLazy.Value);
            }
        }

        /// <summary>
        /// Whether the pointer button is held down and moving.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, clock animations are disabled.
        /// </remarks>
        public bool PointerMoving { get; set; }

        /// <summary>
        /// Updates the position of the hands on the clock.
        /// </summary>
        /// <param name="value">The minute or hour.</param>
        /// <param name="pointerMoving">Is the pointer being moved?</param>
        /// <remarks>
        /// This method is invoked via JavaScript.
        /// </remarks>
        [JSInvokable]
        public async Task SelectTimeFromStick(int value, bool pointerMoving)
        {
            if (value == -1)
            {
                // This means a stick wasn't the target (which shouldn't happen).
                return;
            }

            PointerMoving = pointerMoving;

            // Update the .NET properties from the JavaScript events.
            if (_currentView == OpenTo.Minutes)
            {
                var minute = RoundToMinuteStepInterval(value);
                _timeSet.Minute = minute;
            }
            else if (_currentView == OpenTo.Seconds)
            {
                var second = RoundToSecondStepInterval(value);
                _timeSet.Second = second;
            }
            else if (_currentView == OpenTo.Hours)
            {
                _timeSet.Hour = HourAmPm(value);
            }

            await UpdateTimeAsync();

            // Manually update because the event won't do it from JavaScript.
            StateHasChanged();
        }

        /// <summary>
        /// Performs the click action for the sticks.
        /// </summary>
        /// <param name="value">The minute or hour.</param>
        /// <remarks>
        /// This method is invoked via JavaScript.
        /// </remarks>
        [JSInvokable]
        public async Task OnStickClick(int value)
        {
            // The pointer is up and not moving so animations can be enabled again.
            PointerMoving = false;

            // Clicking a stick will submit the time.
            if (_currentView == OpenTo.Minutes)
            {
                if (TimeEditMode == TimeEditMode.Normal && _showSecondsState.Value)
                {
                    _currentView = OpenTo.Seconds;
                }
                else
                {
                    await SubmitAndCloseAsync();
                }
            }
            else if (_currentView == OpenTo.Seconds)
            {
                await SubmitAndCloseAsync();
            }
            else if (_currentView == OpenTo.Hours)
            {
                if (TimeEditMode == TimeEditMode.Normal)
                {
                    _currentView = OpenTo.Minutes;
                }
                else if (TimeEditMode == TimeEditMode.OnlyHours)
                {
                    await SubmitAndCloseAsync();
                }
            }

            // Manually update because the event won't do it from JavaScript.
            StateHasChanged();
        }

        private void UpdateTimeSetFromTime()
        {
            if (TimeIntermediate is null)
            {
                _timeSet.Hour = 0;
                _timeSet.Minute = 0;
                _timeSet.Second = 0;
                return;
            }

            _timeSet.Hour = TimeIntermediate.Value.Hours;
            _timeSet.Minute = TimeIntermediate.Value.Minutes;
            _timeSet.Second = TimeIntermediate.Value.Seconds;
        }

        private int HourAmPm(int hour)
        {
            if (IsAmPm)
            {
                if (IsAm && hour == 12)
                {
                    return 0;
                }

                if (IsPm && hour < 12)
                {
                    return hour + 12;
                }
            }

            return hour;
        }

        private int RoundToMinuteStepInterval(int value)
        {
            var step = EffectiveMinuteInterval;
            if (step > 1)
            {
                var interval = step % 60;
                value = (value + (interval / 2)) / interval * interval;
                if (value >= 60)
                {
                    value = 0;
                }
            }

            return value;
        }

        private int RoundToSecondStepInterval(int value)
        {
            var step = EffectiveSecondInterval;
            if (step > 1)
            {
                var interval = step % 60;
                value = (value + (interval / 2)) / interval * interval;
                if (value >= 60)
                {
                    value = 0;
                }
            }

            return value;
        }

        private int RoundToStepInterval(int value) => RoundToMinuteStepInterval(value);

        private DotNetObjectReference<VekTimePicker> CreateDotNetObjectReference() => DotNetObjectReference.Create(this);

        protected async Task SubmitAndCloseAsync()
        {
            if (PickerActions == null || AutoClose)
            {
                await SubmitAsync();

                if (PickerVariant != PickerVariant.Static)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(ClosingDelay), TimeProvider);
                    await CloseAsync(false);
                }
            }
        }

        protected internal override async Task OnHandleKeyDownAsync(KeyboardEventArgs args)
        {
            if (GetDisabledState() || GetReadOnlyState())
            {
                return;
            }

            await base.OnHandleKeyDownAsync(args);

            switch (args.Key)
            {
                case "ArrowRight":
                    if (Open)
                    {
                        if (args.CtrlKey)
                        {
                            await ChangeHourAsync(1);
                        }
                        else if (args.ShiftKey)
                        {
                            if (_currentView == OpenTo.Seconds)
                            {
                                if (_timeSet.Second > 55)
                                {
                                    await ChangeMinuteAsync(1);
                                }

                                await ChangeSecondAsync(5);
                            }
                            else
                            {
                                if (_timeSet.Minute > 55)
                                {
                                    await ChangeHourAsync(1);
                                }

                                await ChangeMinuteAsync(5);
                            }
                        }
                        else
                        {
                            if (_currentView == OpenTo.Seconds)
                            {
                                var step = EffectiveSecondInterval;
                                if (_timeSet.Second + step >= 60)
                                {
                                    await ChangeMinuteAsync(1);
                                }

                                await ChangeSecondAsync(step);
                            }
                            else
                            {
                                var step = EffectiveMinuteInterval;
                                if (_timeSet.Minute + step >= 60)
                                {
                                    await ChangeHourAsync(1);
                                }

                                await ChangeMinuteAsync(step);
                            }
                        }
                    }

                    break;
                case "ArrowLeft":
                    if (Open)
                    {
                        if (args.CtrlKey)
                        {
                            await ChangeHourAsync(-1);
                        }
                        else if (args.ShiftKey)
                        {
                            if (_currentView == OpenTo.Seconds)
                            {
                                if (_timeSet.Second < 5)
                                {
                                    await ChangeMinuteAsync(-1);
                                }

                                await ChangeSecondAsync(-5);
                            }
                            else
                            {
                                if (_timeSet.Minute < 5)
                                {
                                    await ChangeHourAsync(-1);
                                }

                                await ChangeMinuteAsync(-5);
                            }
                        }
                        else
                        {
                            if (_currentView == OpenTo.Seconds)
                            {
                                var step = EffectiveSecondInterval;
                                if (_timeSet.Second == 0)
                                {
                                    await ChangeMinuteAsync(-1);
                                }

                                await ChangeSecondAsync(-step);
                            }
                            else
                            {
                                var step = EffectiveMinuteInterval;
                                if (_timeSet.Minute == 0)
                                {
                                    await ChangeHourAsync(-1);
                                }

                                await ChangeMinuteAsync(-step);
                            }
                        }
                    }

                    break;
                case "ArrowUp":
                    if (!Open && !Editable)
                    {
                        Open = true;
                    }
                    else if (args.AltKey)
                    {
                        Open = false;
                    }
                    else if (args.ShiftKey)
                    {
                        await ChangeHourAsync(5);
                    }
                    else
                    {
                        await ChangeHourAsync(1);
                    }

                    break;
                case "ArrowDown":
                    if (!Open && !Editable)
                    {
                        Open = true;
                    }
                    else if (args.ShiftKey)
                    {
                        await ChangeHourAsync(-5);
                    }
                    else
                    {
                        await ChangeHourAsync(-1);
                    }

                    break;
                case "Escape":
                    await ReturnTimeBackUpAsync();
                    break;
                case "Enter":
                case "NumpadEnter":
                    if (!Open)
                    {
                        await OpenAsync();
                    }
                    else
                    {
                        await SubmitAsync();
                        await CloseAsync();
                        _inputReference?.SetTextAsync(Text);
                    }

                    break;
                case " ":
                    if (!Editable)
                    {
                        if (!Open)
                        {
                            await OpenAsync();
                        }
                        else
                        {
                            await SubmitAsync();
                            await CloseAsync();
                            _inputReference?.SetTextAsync(Text);
                        }
                    }

                    break;
            }

            StateHasChanged();
        }

        protected override string GetFormat()
        {
            if (!string.IsNullOrEmpty(TimeFormat))
            {
                return TimeFormat;
            }

            if (_showSecondsState.Value)
            {
                return IsAmPm ? TimeSpanConverter.Format12HoursWithSeconds : TimeSpanConverter.Format24HoursWithSeconds;
            }

            return IsAmPm ? TimeSpanConverter.Format12Hours : TimeSpanConverter.Format24Hours;
        }

        /// <inheritdoc />
        protected override IConverter<TimeSpan?, string?> GetDefaultConverter()
        {
            return new TimeSpanConverter
            {
                Culture = GetCulture,
                Format = GetFormat
            };
        }

        protected Task ChangeSecondAsync(int second)
        {
            _currentView = OpenTo.Seconds;
            _timeSet.Second = (_timeSet.Second + second + 60) % 60;

            return UpdateTimeAsync();
        }

        protected Task ChangeMinuteAsync(int minute)
        {
            _currentView = OpenTo.Minutes;
            _timeSet.Minute = (_timeSet.Minute + minute + 60) % 60;

            return UpdateTimeAsync();
        }

        protected Task ChangeHourAsync(int hour)
        {
            _currentView = OpenTo.Hours;
            _timeSet.Hour = (_timeSet.Hour + hour + 24) % 24;

            return UpdateTimeAsync();
        }

        protected async Task ReturnTimeBackUpAsync()
        {
            if (Time == null)
            {
                TimeIntermediate = null;
            }
            else
            {
                _timeSet.Hour = Time.Value.Hours;
                _timeSet.Minute = Time.Value.Minutes;
                _timeSet.Second = Time.Value.Seconds;

                await UpdateTimeAsync();
            }
        }

        private Task FormatChangedAsync()
        {
            if (_timeEditFormatState.Value.HasValue)
            {
                AmPm = _timeEditFormatState.Value.Value == Vek.TimeEditFormat.TwelveHour;
            }

            Touched = true;
            return SetTextAsync(ConvertSet(_value), false);
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeAsyncCore()
        {
            await base.DisposeAsyncCore();

            if (IsJSRuntimeAvailable)
            {
                await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekTimePicker.destroyPointerEvents", ClockElementReference);
            }

            if (_dotNetReferenceLazy.IsValueCreated)
            {
                _dotNetReferenceLazy.Value.Dispose();
            }
        }

        private sealed record SetTime
        {
            public int Hour { get; set; }

            public int Minute { get; set; }

            public int Second { get; set; }
        }
    }
}
