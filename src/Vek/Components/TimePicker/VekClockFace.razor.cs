// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// An interactive analog radial clock face and touch hand selector component for choosing hours, minutes, and seconds.
    /// </summary>
    public partial class VekClockFace : VekComponentBase
    {
        private const double CenterX = 130.0;
        private const double CenterY = 130.0;
        private const double OuterRadius = 102.0;
        private const double InnerRadius = 68.0;
        private const double TickRadius = 114.0;
        private const double RingThreshold = 85.0;

        private readonly ParameterState<TimeSpan?> _timeState;
        private readonly ParameterState<OpenTo> _viewState;
        private readonly ParameterState<TimeEditFormat> _timeEditFormatState;
        private readonly ParameterState<bool?> _amPmState;
        private readonly ParameterState<bool> _autoTransitionState;
        private readonly ParameterState<int> _minuteSelectionStepState;
        private readonly ParameterState<int> _secondSelectionStepState;
        private readonly ParameterState<bool> _showSecondsState;
        private readonly ParameterState<bool> _showHeaderState;
        private readonly ParameterState<Color> _colorState;
        private readonly ParameterState<bool> _disabledState;
        private readonly ParameterState<bool> _readOnlyState;

        /// <summary>
        /// Initializes a new instance of the <see cref="VekClockFace"/> component.
        /// </summary>
        public VekClockFace()
        {
            using var registerScope = CreateRegisterScope();
            _timeState = registerScope.RegisterParameter<TimeSpan?>(nameof(Time))
                .WithParameter(() => Time)
                .WithEventCallback(() => TimeChanged);
            _viewState = registerScope.RegisterParameter<OpenTo>(nameof(View))
                .WithParameter(() => View)
                .WithEventCallback(() => ViewChanged);
            _timeEditFormatState = registerScope.RegisterParameter<TimeEditFormat>(nameof(TimeEditFormat))
                .WithParameter(() => TimeEditFormat)
                .WithEventCallback(() => TimeEditFormatChanged);
            _amPmState = registerScope.RegisterParameter<bool?>(nameof(AmPm))
                .WithParameter(() => AmPm)
                .WithEventCallback(() => AmPmChanged);
            _autoTransitionState = registerScope.RegisterParameter<bool>(nameof(AutoTransition))
                .WithParameter(() => AutoTransition);
            _minuteSelectionStepState = registerScope.RegisterParameter<int>(nameof(MinuteSelectionStep))
                .WithParameter(() => MinuteSelectionStep);
            _secondSelectionStepState = registerScope.RegisterParameter<int>(nameof(SecondSelectionStep))
                .WithParameter(() => SecondSelectionStep);
            _showSecondsState = registerScope.RegisterParameter<bool>(nameof(ShowSeconds))
                .WithParameter(() => ShowSeconds);
            _showHeaderState = registerScope.RegisterParameter<bool>(nameof(ShowHeader))
                .WithParameter(() => ShowHeader);
            _colorState = registerScope.RegisterParameter<Color>(nameof(Color))
                .WithParameter(() => Color);
            _disabledState = registerScope.RegisterParameter<bool>(nameof(Disabled))
                .WithParameter(() => Disabled);
            _readOnlyState = registerScope.RegisterParameter<bool>(nameof(ReadOnly))
                .WithParameter(() => ReadOnly);
        }

        /// <summary>
        /// The currently selected time value.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public TimeSpan? Time { get; set; }

        /// <summary>
        /// Occurs when the selected time changes.
        /// </summary>
        [Parameter]
        public EventCallback<TimeSpan?> TimeChanged { get; set; }

        /// <summary>
        /// The current selection view mode (hours, minutes, or seconds).
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="OpenTo.Hours"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public OpenTo View { get; set; } = OpenTo.Hours;

        /// <summary>
        /// Occurs when the clock view mode changes.
        /// </summary>
        [Parameter]
        public EventCallback<OpenTo> ViewChanged { get; set; }

        /// <summary>
        /// Specifies the display and selection format for hours (12-hour or 24-hour).
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="TimeEditFormat.TwelveHour"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public TimeEditFormat TimeEditFormat { get; set; } = TimeEditFormat.TwelveHour;

        /// <summary>
        /// Occurs when the time edit format changes.
        /// </summary>
        [Parameter]
        public EventCallback<TimeEditFormat> TimeEditFormatChanged { get; set; }

        /// <summary>
        /// Indicates whether the clock is in AM (<c>true</c>) or PM (<c>false</c>) mode when using 12-hour format.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, which infers AM/PM from <see cref="Time"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public bool? AmPm { get; set; }

        /// <summary>
        /// Occurs when the AM/PM state changes.
        /// </summary>
        [Parameter]
        public EventCallback<bool?> AmPmChanged { get; set; }

        /// <summary>
        /// Automatically transitions from hours view to minutes view once an hour is selected.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public bool AutoTransition { get; set; } = true;

        /// <summary>
        /// The step interval for minute selection (e.g. 1, 5, 10, 15).
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public int MinuteSelectionStep { get; set; } = 1;

        /// <summary>
        /// The step interval for second selection.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public int SecondSelectionStep { get; set; } = 1;

        /// <summary>
        /// Enables selection of seconds.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public bool ShowSeconds { get; set; } = false;

        /// <summary>
        /// Shows an interactive time header above the clock dial.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public bool ShowHeader { get; set; } = false;

        /// <summary>
        /// The theme color for the clock dial hand and selection highlights.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Primary"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Appearance)]
        public Color Color { get; set; } = Color.Primary;

        /// <summary>
        /// Disables user interaction with the clock face.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public bool Disabled { get; set; } = false;

        /// <summary>
        /// Displays the clock in read-only mode without allowing value changes.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Picker.Behavior)]
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// Fired when a full selection is picked or confirmed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Picker.Behavior)]
        public EventCallback<TimeSpan?> OnPicked { get; set; }

        /// <summary>
        /// The accessible ARIA label for the clock dial element.
        /// </summary>
        /// <remarks>
        /// Defaults to a localized description of the current view mode.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Picker.Behavior)]
        public string? AriaLabel { get; set; }

        /// <summary>
        /// Gets whether the user is actively dragging the clock hand.
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// Gets the CSS classes applied to this component.
        /// </summary>
        protected string ClassName =>
            new CssBuilder("vek-clock-face")
                .AddClass($"vek-color-{_colorState.Value.ToStringFast(true)}")
                .AddClass("vek-disabled", _disabledState.Value)
                .AddClass("vek-readonly", _readOnlyState.Value)
                .AddClass(Class)
                .Build();

        private TimeSpan CurrentTime => _timeState.Value ?? TimeSpan.Zero;

        private bool Is24Hour => _timeEditFormatState.Value == TimeEditFormat.TwentyFourHour;

        private bool IsAm
        {
            get
            {
                if (_amPmState.Value.HasValue)
                {
                    return _amPmState.Value.Value;
                }
                return CurrentTime.Hours < 12;
            }
        }

        private int CurrentHour => CurrentTime.Hours;

        private int CurrentMinute => CurrentTime.Minutes;

        private int CurrentSecond => CurrentTime.Seconds;

        private double HandAngle
        {
            get
            {
                return _viewState.Value switch
                {
                    OpenTo.Hours => CurrentHour % 12 * 30.0,
                    OpenTo.Minutes => CurrentMinute * 6.0,
                    OpenTo.Seconds => CurrentSecond * 6.0,
                    _ => 0.0
                };
            }
        }

        private double HandRadius
        {
            get
            {
                if (_viewState.Value == OpenTo.Hours && Is24Hour)
                {
                    if (CurrentHour >= 13 || CurrentHour == 0)
                    {
                        return InnerRadius;
                    }
                    return OuterRadius;
                }
                return OuterRadius;
            }
        }

        private double HandEndX => CenterX + (HandRadius * Math.Sin(HandAngle * (Math.PI / 180.0)));

        private double HandEndY => CenterY - (HandRadius * Math.Cos(HandAngle * (Math.PI / 180.0)));

        private double ThumbRadius
        {
            get
            {
                if (_viewState.Value == OpenTo.Minutes && (CurrentMinute % 5 != 0))
                {
                    return 5.0;
                }
                if (_viewState.Value == OpenTo.Seconds && (CurrentSecond % 5 != 0))
                {
                    return 5.0;
                }
                return 16.0;
            }
        }

        private string AriaLabelText => AriaLabel ?? _viewState.Value switch
        {
            OpenTo.Hours => "Select hours",
            OpenTo.Minutes => "Select minutes",
            OpenTo.Seconds => "Select seconds",
            _ => "Clock face"
        };

        private int CurrentAriaValue => _viewState.Value switch
        {
            OpenTo.Hours => Is24Hour ? CurrentHour : (CurrentHour % 12 == 0 ? 12 : CurrentHour % 12),
            OpenTo.Minutes => CurrentMinute,
            OpenTo.Seconds => CurrentSecond,
            _ => 0
        };

        private int AriaValueMin => _viewState.Value switch
        {
            OpenTo.Hours => Is24Hour ? 0 : 1,
            _ => 0
        };

        private int AriaValueMax => _viewState.Value switch
        {
            OpenTo.Hours => Is24Hour ? 23 : 12,
            _ => 59
        };

        private string AriaValueText => _viewState.Value switch
        {
            OpenTo.Hours => $"{CurrentAriaValue} hours",
            OpenTo.Minutes => $"{CurrentMinute} minutes",
            OpenTo.Seconds => $"{CurrentSecond} seconds",
            _ => CurrentTime.ToString()
        };

        private static (double x, double y) GetCoordinates(double angleDegrees, double radius)
        {
            var radians = angleDegrees * (Math.PI / 180.0);
            var x = CenterX + (radius * Math.Sin(radians));
            var y = CenterY - (radius * Math.Cos(radians));
            return (x, y);
        }

        /// <summary>
        /// Sets the current view mode of the clock face.
        /// </summary>
        /// <param name="view">The view to switch to.</param>
        public async Task SetViewAsync(OpenTo view)
        {
            if (_viewState.Value != view)
            {
                await _viewState.SetValueAsync(view);
            }
        }

        /// <summary>
        /// Sets the 12-hour AM/PM state.
        /// </summary>
        /// <param name="isAm"><c>true</c> for AM; <c>false</c> for PM.</param>
        public async Task SetAmPmAsync(bool isAm)
        {
            if (Is24Hour)
            {
                return;
            }

            await _amPmState.SetValueAsync(isAm);

            var oldTime = CurrentTime;
            int h12 = oldTime.Hours % 12;
            int newHour = isAm ? h12 : (h12 + 12);
            var newTime = new TimeSpan(newHour, oldTime.Minutes, oldTime.Seconds);
            await _timeState.SetValueAsync(newTime);
        }

        private async Task HandlePickAsync()
        {
            if (_autoTransitionState.Value && _viewState.Value == OpenTo.Hours)
            {
                await SetViewAsync(OpenTo.Minutes);
            }
            else if (_autoTransitionState.Value && _viewState.Value == OpenTo.Minutes && _showSecondsState.Value)
            {
                await SetViewAsync(OpenTo.Seconds);
            }
            else
            {
                await OnPicked.InvokeAsync(_timeState.Value);
            }
        }

        private async Task OnNumberClickAsync(int hourVal, bool isInner)
        {
            if (_disabledState.Value || _readOnlyState.Value)
            {
                return;
            }

            TimeSpan oldTime = CurrentTime;
            int hour;
            if (Is24Hour)
            {
                hour = hourVal;
            }
            else
            {
                if (IsAm)
                {
                    hour = (hourVal == 12) ? 0 : hourVal;
                }
                else
                {
                    hour = (hourVal == 12) ? 12 : (hourVal + 12);
                }
            }

            var newTime = new TimeSpan(hour, oldTime.Minutes, oldTime.Seconds);
            await _timeState.SetValueAsync(newTime);
            await HandlePickAsync();
        }

        private async Task OnMinuteClickAsync(int minVal)
        {
            if (_disabledState.Value || _readOnlyState.Value)
            {
                return;
            }

            TimeSpan oldTime = CurrentTime;
            var newTime = new TimeSpan(oldTime.Hours, minVal, oldTime.Seconds);
            await _timeState.SetValueAsync(newTime);
            await HandlePickAsync();
        }

        private async Task OnSecondClickAsync(int secVal)
        {
            if (_disabledState.Value || _readOnlyState.Value)
            {
                return;
            }

            TimeSpan oldTime = CurrentTime;
            var newTime = new TimeSpan(oldTime.Hours, oldTime.Minutes, secVal);
            await _timeState.SetValueAsync(newTime);
            await HandlePickAsync();
        }

        private void HandlePointerDown(PointerEventArgs e)
        {
            if (_disabledState.Value || _readOnlyState.Value)
            {
                return;
            }

            IsDragging = true;
            SetTimeFromPointer(e, autoTransition: false);
        }

        private void HandlePointerMove(PointerEventArgs e)
        {
            if (!IsDragging || _disabledState.Value || _readOnlyState.Value)
            {
                return;
            }

            SetTimeFromPointer(e, autoTransition: false);
        }

        private void HandlePointerUp(PointerEventArgs e)
        {
            if (!IsDragging)
            {
                return;
            }

            IsDragging = false;
            SetTimeFromPointer(e, autoTransition: true);
        }

        private void HandlePointerCancel(PointerEventArgs e)
        {
            IsDragging = false;
        }

        private void SetTimeFromPointer(PointerEventArgs e, bool autoTransition)
        {
            double x = e.OffsetX != 0 || e.OffsetY != 0 ? e.OffsetX : e.ClientX;
            double y = e.OffsetX != 0 || e.OffsetY != 0 ? e.OffsetY : e.ClientY;

            double dx = x - CenterX;
            double dy = y - CenterY;
            double radius = Math.Sqrt((dx * dx) + (dy * dy));

            double angleRad = Math.Atan2(dy, dx);
            double angleDeg = (angleRad * (180.0 / Math.PI)) + 90.0;
            if (angleDeg < 0) angleDeg += 360.0;
            if (angleDeg >= 360.0) angleDeg -= 360.0;

            TimeSpan oldTime = CurrentTime;
            TimeSpan newTime;

            if (_viewState.Value == OpenTo.Hours)
            {
                int hourStep = (int)Math.Round(angleDeg / 30.0) % 12;
                int hour12 = hourStep == 0 ? 12 : hourStep;

                if (Is24Hour)
                {
                    bool isInner = radius < RingThreshold;
                    int hour24 = isInner ? (hourStep == 0 ? 0 : hourStep + 12) : hour12;
                    newTime = new TimeSpan(hour24, oldTime.Minutes, oldTime.Seconds);
                }
                else
                {
                    int hour = IsAm
                        ? (hour12 == 12 ? 0 : hour12)
                        : (hour12 == 12 ? 12 : hour12 + 12);
                    newTime = new TimeSpan(hour, oldTime.Minutes, oldTime.Seconds);
                }
            }
            else if (_viewState.Value == OpenTo.Minutes)
            {
                int minute = (int)Math.Round(angleDeg / 6.0) % 60;
                int step = Math.Max(1, _minuteSelectionStepState.Value);
                if (step > 1)
                {
                    minute = (int)Math.Round((double)minute / step) * step % 60;
                }
                newTime = new TimeSpan(oldTime.Hours, minute, oldTime.Seconds);
            }
            else
            {
                int second = (int)Math.Round(angleDeg / 6.0) % 60;
                int step = Math.Max(1, _secondSelectionStepState.Value);
                if (step > 1)
                {
                    second = (int)Math.Round((double)second / step) * step % 60;
                }
                newTime = new TimeSpan(oldTime.Hours, oldTime.Minutes, second);
            }

            _ = ApplyTimeChangeAsync(newTime, autoTransition);
        }

        private async Task ApplyTimeChangeAsync(TimeSpan newTime, bool autoTransition)
        {
            await _timeState.SetValueAsync(newTime);
            if (autoTransition)
            {
                await HandlePickAsync();
            }
        }

        /// <summary>
        /// Handles keyboard arrow and navigation keys for rotating the clock hand.
        /// </summary>
        /// <param name="e">The keyboard event arguments.</param>
        protected async Task HandleKeyDownAsync(KeyboardEventArgs e)
        {
            if (_disabledState.Value || _readOnlyState.Value)
            {
                return;
            }

            TimeSpan oldTime = CurrentTime;
            TimeSpan newTime = oldTime;
            bool handled = false;

            switch (e.Key)
            {
                case "ArrowRight":
                case "ArrowUp":
                    if (_viewState.Value == OpenTo.Hours)
                    {
                        int maxHour = Is24Hour ? 24 : 12;
                        int current = Is24Hour ? oldTime.Hours : (oldTime.Hours % 12 == 0 ? 12 : oldTime.Hours % 12);
                        int next = (current + 1) % maxHour;
                        if (!Is24Hour && next == 0) next = 12;
                        newTime = SetHourRespectingFormat(oldTime, next);
                    }
                    else if (_viewState.Value == OpenTo.Minutes)
                    {
                        int step = Math.Max(1, _minuteSelectionStepState.Value);
                        int next = (oldTime.Minutes + step) % 60;
                        newTime = new TimeSpan(oldTime.Hours, next, oldTime.Seconds);
                    }
                    else
                    {
                        int step = Math.Max(1, _secondSelectionStepState.Value);
                        int next = (oldTime.Seconds + step) % 60;
                        newTime = new TimeSpan(oldTime.Hours, oldTime.Minutes, next);
                    }
                    handled = true;
                    break;

                case "ArrowLeft":
                case "ArrowDown":
                    if (_viewState.Value == OpenTo.Hours)
                    {
                        int maxHour = Is24Hour ? 24 : 12;
                        int current = Is24Hour ? oldTime.Hours : (oldTime.Hours % 12 == 0 ? 12 : oldTime.Hours % 12);
                        int prev = (current - 1 + maxHour) % maxHour;
                        if (!Is24Hour && prev == 0) prev = 12;
                        newTime = SetHourRespectingFormat(oldTime, prev);
                    }
                    else if (_viewState.Value == OpenTo.Minutes)
                    {
                        int step = Math.Max(1, _minuteSelectionStepState.Value);
                        int prev = (oldTime.Minutes - step + 60) % 60;
                        newTime = new TimeSpan(oldTime.Hours, prev, oldTime.Seconds);
                    }
                    else
                    {
                        int step = Math.Max(1, _secondSelectionStepState.Value);
                        int prev = (oldTime.Seconds - step + 60) % 60;
                        newTime = new TimeSpan(oldTime.Hours, oldTime.Minutes, prev);
                    }
                    handled = true;
                    break;

                case "PageUp":
                    if (_viewState.Value == OpenTo.Hours)
                    {
                        int next = (oldTime.Hours + 12) % 24;
                        newTime = new TimeSpan(next, oldTime.Minutes, oldTime.Seconds);
                    }
                    else
                    {
                        int next = (oldTime.Minutes + 5) % 60;
                        newTime = new TimeSpan(oldTime.Hours, next, oldTime.Seconds);
                    }
                    handled = true;
                    break;

                case "PageDown":
                    if (_viewState.Value == OpenTo.Hours)
                    {
                        int prev = (oldTime.Hours - 12 + 24) % 24;
                        newTime = new TimeSpan(prev, oldTime.Minutes, oldTime.Seconds);
                    }
                    else
                    {
                        int prev = (oldTime.Minutes - 5 + 60) % 60;
                        newTime = new TimeSpan(oldTime.Hours, prev, oldTime.Seconds);
                    }
                    handled = true;
                    break;

                case "Home":
                    if (_viewState.Value == OpenTo.Hours)
                    {
                        newTime = SetHourRespectingFormat(oldTime, Is24Hour ? 0 : 12);
                    }
                    else
                    {
                        newTime = new TimeSpan(oldTime.Hours, 0, oldTime.Seconds);
                    }
                    handled = true;
                    break;

                case "End":
                    if (_viewState.Value == OpenTo.Hours)
                    {
                        newTime = SetHourRespectingFormat(oldTime, Is24Hour ? 23 : 11);
                    }
                    else
                    {
                        newTime = new TimeSpan(oldTime.Hours, 59, oldTime.Seconds);
                    }
                    handled = true;
                    break;

                case "Enter":
                case " ":
                    await HandlePickAsync();
                    handled = true;
                    break;
            }

            if (handled && newTime != oldTime)
            {
                await _timeState.SetValueAsync(newTime);
            }
        }

        private TimeSpan SetHourRespectingFormat(TimeSpan oldTime, int hour)
        {
            if (Is24Hour)
            {
                return new TimeSpan(hour, oldTime.Minutes, oldTime.Seconds);
            }

            int h = IsAm
                ? (hour == 12 ? 0 : hour)
                : (hour == 12 ? 12 : hour + 12);
            return new TimeSpan(h, oldTime.Minutes, oldTime.Seconds);
        }

        private string GetHeaderHourButtonClass() =>
            _viewState.Value == OpenTo.Hours
                ? "vek-clock-face-time-button vek-active"
                : "vek-clock-face-time-button";

        private string GetHeaderMinuteButtonClass() =>
            _viewState.Value == OpenTo.Minutes
                ? "vek-clock-face-time-button vek-active"
                : "vek-clock-face-time-button";

        private string GetHeaderSecondButtonClass() =>
            _viewState.Value == OpenTo.Seconds
                ? "vek-clock-face-time-button vek-active"
                : "vek-clock-face-time-button";

        private string GetHourString()
        {
            if (Is24Hour)
            {
                return CurrentHour.ToString("D2");
            }
            int h12 = CurrentHour % 12 == 0 ? 12 : CurrentHour % 12;
            return h12.ToString("D2");
        }

        private string GetMinuteString() => CurrentMinute.ToString("D2");

        private string GetSecondString() => CurrentSecond.ToString("D2");
    }
}
