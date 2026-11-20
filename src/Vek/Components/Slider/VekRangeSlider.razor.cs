using System.Globalization;
using System.Numerics;
using Microsoft.AspNetCore.Components;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Allows users to select a numeric range defined by lower and upper bounds using two draggable thumbs.
    /// </summary>
    /// <typeparam name="T">The type of the values the range slider represents.</typeparam>
    public partial class VekRangeSlider<T> : VekComponentBase where T : struct, INumber<T>
    {
        private enum ActiveThumb
        {
            Start,
            End
        }

        private int _tickMarkCount = 0;
        private ActiveThumb _activeThumb = ActiveThumb.Start;
        private bool _isSynchronizing = false;

        private readonly ParameterState<T> _valueStartState;
        private readonly ParameterState<T> _valueEndState;
        private readonly ParameterState<SliderRange<T>?> _valueState;

        /// <summary>
        /// Initializes a new instance of the <see cref="VekRangeSlider{T}"/> class.
        /// </summary>
        public VekRangeSlider()
        {
            using var registerScope = CreateRegisterScope();
            _valueStartState = registerScope.RegisterParameter<T>(nameof(ValueStart))
                .WithParameter(() => ValueStart)
                .WithEventCallback(() => ValueStartChanged)
                .WithChangeHandler(OnValueStartParameterChangedAsync);

            _valueEndState = registerScope.RegisterParameter<T>(nameof(ValueEnd))
                .WithParameter(() => ValueEnd)
                .WithEventCallback(() => ValueEndChanged)
                .WithChangeHandler(OnValueEndParameterChangedAsync);

            _valueState = registerScope.RegisterParameter<SliderRange<T>?>(nameof(Value))
                .WithParameter(() => Value)
                .WithEventCallback(() => ValueChanged)
                .WithChangeHandler(OnValueParameterChangedAsync);
        }

        protected string Classname =>
            new CssBuilder("vek-slider")
                .AddClass("vek-slider-range")
                .AddClass($"vek-slider-{Size.ToStringFast(true)}")
                .AddClass($"vek-slider-{Color.ToStringFast(true)}")
                .AddClass("vek-slider-disabled", Disabled)
                .AddClass("vek-slider-vertical", Vertical)
                .AddClass(Class)
                .Build();

        /// <summary>
        /// The minimum allowed value.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>0</c>. Must be less than <see cref="Max"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Validation)]
        public T Min { get; set; } = T.Zero;

        /// <summary>
        /// The maximum allowed value.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>100</c>. Must be greater than <see cref="Min"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Validation)]
        public T Max { get; set; } = T.CreateTruncating(100);

        /// <summary>
        /// How much the values change on each step.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Validation)]
        public T Step { get; set; } = T.One;

        /// <summary>
        /// The minimum distance required between the lower and upper thumbs.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>0</c>. Prevents thumbs from crossing or coming closer than this distance.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Validation)]
        public T MinDistance { get; set; } = T.Zero;

        /// <summary>
        /// The lower or starting value of the selected range.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>0</c>. When this value changes, <see cref="ValueStartChanged"/> occurs.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Slider.Data)]
        public T ValueStart { get; set; } = T.Zero;

        /// <summary>
        /// Occurs when <see cref="ValueStart"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<T> ValueStartChanged { get; set; }

        /// <summary>
        /// The upper or ending value of the selected range.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>100</c>. When this value changes, <see cref="ValueEndChanged"/> occurs.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Slider.Data)]
        public T ValueEnd { get; set; } = T.CreateTruncating(100);

        /// <summary>
        /// Occurs when <see cref="ValueEnd"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<T> ValueEndChanged { get; set; }

        /// <summary>
        /// The range representing both start and end values as a single model.
        /// </summary>
        /// <remarks>
        /// When this value changes, <see cref="ValueChanged"/> occurs.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.Slider.Data)]
        public SliderRange<T>? Value { get; set; }

        /// <summary>
        /// Occurs when <see cref="Value"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<SliderRange<T>?> ValueChanged { get; set; }

        /// <summary>
        /// Prevents the user from interacting with this slider.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Behavior)]
        public bool Disabled { get; set; } = false;

        /// <summary>
        /// Controls when the value is updated.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.<br />
        /// When <c>true</c>, dragging the slider changes values immediately.<br />
        /// When <c>false</c>, values change when releasing the handle.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Behavior)]
        public bool Immediate { get; set; } = true;

        /// <summary>
        /// The optional content rendered above the slider.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Slider.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The color of this slider.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Primary"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public Color Color { get; set; } = Color.Primary;

        /// <summary>
        /// The size of this slider.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Size.Small"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public Size Size { get; set; } = Size.Small;

        /// <summary>
        /// Displays this slider vertically.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public bool Vertical { get; set; } = false;

        /// <summary>
        /// Displays tick marks along the track.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public bool TickMarks { get; set; } = false;

        /// <summary>
        /// Displays tick marks along the track.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. This is an alias for <see cref="TickMarks"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public bool ShowTicks { get; set; }

        /// <summary>
        /// Sets the spacing between displayed tick marks.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Step"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public T? TickInterval { get; set; }

        /// <summary>
        /// The tick mark labels for each step.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public string[]? TickMarkLabels { get; set; }

        /// <summary>
        /// Formats numeric tick values when explicit <see cref="TickMarkLabels"/> are not provided.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public Func<double, string>? TickMarkLabelFormatter { get; set; }

        /// <summary>
        /// Displays the value label over each slider thumb.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public bool ValueLabel { get; set; }

        /// <summary>
        /// The culture used to format value labels.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// The format string of the value labels.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public string? ValueLabelFormat { get; set; }

        /// <summary>
        /// The custom content template for value labels.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Slider.Appearance)]
        public RenderFragment<T>? ValueLabelContent { get; set; }

        /// <summary>
        /// The accessible label for the start/lower thumb.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>"Start value"</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Behavior)]
        public string? AriaLabelStart { get; set; } = "Start value";

        /// <summary>
        /// The accessible label for the end/upper thumb.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>"End value"</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Slider.Behavior)]
        public string? AriaLabelEnd { get; set; } = "End value";

        /// <summary>
        /// Displays content in right-to-left order.
        /// </summary>
        [CascadingParameter(Name = "RightToLeft")]
        public bool RightToLeft { get; set; }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            UpdateTickMarkCount();

            if (_valueState.Value is not null && (!_valueStartState.Value.Equals(_valueState.Value.Start) || !_valueEndState.Value.Equals(_valueState.Value.End)))
            {
                var (start, end) = ClampRange(_valueState.Value.Start, _valueState.Value.End);
                _ = _valueStartState.SetValueAsync(start);
                _ = _valueEndState.SetValueAsync(end);
            }
            else
            {
                var clampedStart = ClampStart(_valueStartState.Value, _valueEndState.Value);
                var clampedEnd = ClampEnd(clampedStart, _valueEndState.Value);
                if (!clampedStart.Equals(_valueStartState.Value))
                {
                    _ = _valueStartState.SetValueAsync(clampedStart);
                }
                if (!clampedEnd.Equals(_valueEndState.Value))
                {
                    _ = _valueEndState.SetValueAsync(clampedEnd);
                }
            }

            base.OnParametersSet();
        }

        private string GetStartInputZIndexStyle() => _activeThumb == ActiveThumb.Start ? "z-index: 4;" : "z-index: 3;";

        private string GetEndInputZIndexStyle() => _activeThumb == ActiveThumb.End ? "z-index: 4;" : "z-index: 3;";

        private string GetValueStartText => _valueStartState.Value.ToString(null, CultureInfo.InvariantCulture);

        private string GetValueEndText => _valueEndState.Value.ToString(null, CultureInfo.InvariantCulture);

        private async Task SetValueStartTextAsync(string? text)
        {
            if (Disabled)
            {
                return;
            }

            if (T.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                var snapped = SnapToStep(result);
                var clamped = ClampStart(snapped, _valueEndState.Value);
                _activeThumb = ActiveThumb.Start;
                await UpdateStartAndRangeAsync(clamped);
            }
        }

        private async Task SetValueEndTextAsync(string? text)
        {
            if (Disabled)
            {
                return;
            }

            if (T.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                var snapped = SnapToStep(result);
                var clamped = ClampEnd(_valueStartState.Value, snapped);
                _activeThumb = ActiveThumb.End;
                await UpdateEndAndRangeAsync(clamped);
            }
        }

        private async Task UpdateStartAndRangeAsync(T newStart)
        {
            if (_isSynchronizing)
            {
                return;
            }

            _isSynchronizing = true;
            try
            {
                await _valueStartState.SetValueAsync(newStart);
                if (ValueChanged.HasDelegate || _valueState.Value is not null)
                {
                    await _valueState.SetValueAsync(new SliderRange<T>(newStart, _valueEndState.Value));
                }
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private async Task UpdateEndAndRangeAsync(T newEnd)
        {
            if (_isSynchronizing)
            {
                return;
            }

            _isSynchronizing = true;
            try
            {
                await _valueEndState.SetValueAsync(newEnd);
                if (ValueChanged.HasDelegate || _valueState.Value is not null)
                {
                    await _valueState.SetValueAsync(new SliderRange<T>(_valueStartState.Value, newEnd));
                }
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private async Task OnValueStartParameterChangedAsync(ParameterChangedEventArgs<T> args)
        {
            if (_isSynchronizing)
            {
                return;
            }

            _isSynchronizing = true;
            try
            {
                var clampedStart = ClampStart(args.Value, _valueEndState.Value);
                if (!clampedStart.Equals(args.Value))
                {
                    await _valueStartState.SetValueAsync(clampedStart);
                }

                if (ValueChanged.HasDelegate || _valueState.Value is not null)
                {
                    await _valueState.SetValueAsync(new SliderRange<T>(clampedStart, _valueEndState.Value));
                }
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private async Task OnValueEndParameterChangedAsync(ParameterChangedEventArgs<T> args)
        {
            if (_isSynchronizing)
            {
                return;
            }

            _isSynchronizing = true;
            try
            {
                var clampedEnd = ClampEnd(_valueStartState.Value, args.Value);
                if (!clampedEnd.Equals(args.Value))
                {
                    await _valueEndState.SetValueAsync(clampedEnd);
                }

                if (ValueChanged.HasDelegate || _valueState.Value is not null)
                {
                    await _valueState.SetValueAsync(new SliderRange<T>(_valueStartState.Value, clampedEnd));
                }
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private async Task OnValueParameterChangedAsync(ParameterChangedEventArgs<SliderRange<T>?> args)
        {
            if (_isSynchronizing || args.Value is null)
            {
                return;
            }

            _isSynchronizing = true;
            try
            {
                var (start, end) = ClampRange(args.Value.Start, args.Value.End);
                await _valueStartState.SetValueAsync(start);
                await _valueEndState.SetValueAsync(end);
                if (!start.Equals(args.Value.Start) || !end.Equals(args.Value.End))
                {
                    await _valueState.SetValueAsync(new SliderRange<T>(start, end));
                }
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private T SnapToStep(T value)
        {
            var min = Convert.ToDecimal(Min);
            var max = Convert.ToDecimal(Max);
            var step = Convert.ToDecimal(Step);
            if (max < min || step <= 0)
            {
                return value;
            }

            var clampedValue = Math.Clamp(Convert.ToDecimal(value), min, max);
            var stepCount = decimal.Round((clampedValue - min) / step, MidpointRounding.AwayFromZero);
            var snappedValue = Math.Clamp(min + (stepCount * step), min, max);
            return T.CreateChecked(snappedValue);
        }

        private T ClampStart(T candidateStart, T currentEnd)
        {
            var min = Min;
            var max = Max;
            var minDistance = MinDistance;

            if (candidateStart < min)
            {
                candidateStart = min;
            }

            var maxAllowed = currentEnd - minDistance;
            if (maxAllowed < min)
            {
                maxAllowed = min;
            }

            if (candidateStart > maxAllowed)
            {
                candidateStart = maxAllowed;
            }

            return candidateStart;
        }

        private T ClampEnd(T currentStart, T candidateEnd)
        {
            var min = Min;
            var max = Max;
            var minDistance = MinDistance;

            if (candidateEnd > max)
            {
                candidateEnd = max;
            }

            var minAllowed = currentStart + minDistance;
            if (minAllowed > max)
            {
                minAllowed = max;
            }

            if (candidateEnd < minAllowed)
            {
                candidateEnd = minAllowed;
            }

            return candidateEnd;
        }

        private (T Start, T End) ClampRange(T candidateStart, T candidateEnd)
        {
            var min = Min;
            var max = Max;
            var minDistance = MinDistance;

            var start = candidateStart < min ? min : candidateStart;
            var end = candidateEnd > max ? max : candidateEnd;

            if (start + minDistance > end)
            {
                if (start + minDistance <= max)
                {
                    end = start + minDistance;
                }
                else
                {
                    end = max;
                    start = end - minDistance >= min ? end - minDistance : min;
                }
            }

            return (start, end);
        }

        private double CalculatePercent(T value)
        {
            var min = Convert.ToDouble(Min);
            var max = Convert.ToDouble(Max);
            var val = Convert.ToDouble(value);
            if (max <= min)
            {
                return 0.0;
            }

            var result = 100.0 * (val - min) / (max - min);
            return Math.Clamp(Math.Round(result, 2), 0.0, 100.0);
        }

        private string RangeTrackStyle
        {
            get
            {
                var startPercent = CalculatePercent(_valueStartState.Value);
                var endPercent = CalculatePercent(_valueEndState.Value);
                var width = Math.Max(0.0, endPercent - startPercent);

                var startStr = startPercent.ToString(CultureInfo.InvariantCulture);
                var widthStr = width.ToString(CultureInfo.InvariantCulture);

                return RightToLeft
                    ? $"right:{startStr}%;width:{widthStr}%;"
                    : $"left:{startStr}%;width:{widthStr}%;";
            }
        }

        private string ValueLabelStartPositionStyle
        {
            get
            {
                var startStr = CalculatePercent(_valueStartState.Value).ToString(CultureInfo.InvariantCulture);
                return RightToLeft ? $"right:{startStr}%;" : $"left:{startStr}%;";
            }
        }

        private string ValueLabelEndPositionStyle
        {
            get
            {
                var endStr = CalculatePercent(_valueEndState.Value).ToString(CultureInfo.InvariantCulture);
                return RightToLeft ? $"right:{endStr}%;" : $"left:{endStr}%;";
            }
        }

        private string FormatValueLabel(T value)
        {
            if (ValueLabelFormat is not null)
            {
                return value.ToString(ValueLabelFormat, Culture);
            }

            return value.ToString(null, Culture);
        }

        private bool ShouldShowTicks => TickMarks || ShowTicks;

        private void UpdateTickMarkCount()
        {
            if (!ShouldShowTicks)
            {
                _tickMarkCount = 0;
                return;
            }

            var min = Convert.ToDecimal(Min);
            var max = Convert.ToDecimal(Max);
            var interval = Convert.ToDecimal(TickInterval ?? Step);
            if (max < min || interval <= 0)
            {
                _tickMarkCount = 0;
                return;
            }

            var range = max - min;
            var fullIntervals = (int)decimal.Floor(range / interval);
            var hasMaximumTick = range % interval != 0;
            _tickMarkCount = fullIntervals + 1 + (hasMaximumTick ? 1 : 0);
        }

        private T GetTickMarkValue(int index)
        {
            var min = Convert.ToDecimal(Min);
            var max = Convert.ToDecimal(Max);
            if (index == _tickMarkCount - 1)
            {
                return Max;
            }

            var interval = Convert.ToDecimal(TickInterval ?? Step);
            return T.CreateChecked(min + (interval * index));
        }

        private string GetTickMarkPositionStyle(int index)
        {
            var min = Convert.ToDecimal(Min);
            var max = Convert.ToDecimal(Max);
            if (max <= min)
            {
                return "left:0%;";
            }

            var value = Convert.ToDecimal(GetTickMarkValue(index));
            var position = (double)((value - min) / (max - min) * 100);
            return $"left:{position.ToString("0.##", CultureInfo.InvariantCulture)}%;";
        }

        private string? GetTickMarkLabel(int index)
        {
            if (TickMarkLabels is not null && index < TickMarkLabels.Length)
            {
                return TickMarkLabels[index];
            }

            return TickMarkLabelFormatter?.Invoke(Convert.ToDouble(GetTickMarkValue(index)));
        }
    }
}
