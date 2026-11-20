using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Extensions;
using Vek.Resources;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// An embedded inline calendar widget that renders directly in page layout without dropdown popovers.
/// </summary>
public partial class VekCalendar : VekComponentBase
{
    private readonly string _elementId = Identifier.Create("vekcal");
    private readonly string _componentId = Identifier.Create();
    private readonly ParameterState<DateTime?> _dateState;
    private readonly ParameterState<IReadOnlyCollection<DateTime>?> _selectedDatesState;
    private readonly ParameterState<DateTime?> _pickerMonthState;
    private readonly ParameterState<DateTime?> _minDateState;
    private readonly ParameterState<DateTime?> _maxDateState;

    private DateTime? _focusedDate;
    private bool _scrollToYearAfterRender;

    /// <summary>
    /// Initializes a new instance of the <see cref="VekCalendar"/> component.
    /// </summary>
    public VekCalendar()
    {
        using var registerScope = CreateRegisterScope();
        _dateState = registerScope.RegisterParameter<DateTime?>(nameof(Date))
            .WithParameter(() => Date)
            .WithEventCallback(() => DateChanged);

        _selectedDatesState = registerScope.RegisterParameter<IReadOnlyCollection<DateTime>?>(nameof(SelectedDates))
            .WithParameter(() => SelectedDates)
            .WithEventCallback(() => SelectedDatesChanged);

        _pickerMonthState = registerScope.RegisterParameter<DateTime?>(nameof(PickerMonth))
            .WithParameter(() => PickerMonth)
            .WithEventCallback(() => PickerMonthChanged);

        _minDateState = registerScope.RegisterParameter<DateTime?>(nameof(MinDate))
            .WithParameter(() => MinDate);

        _maxDateState = registerScope.RegisterParameter<DateTime?>(nameof(MaxDate))
            .WithParameter(() => MaxDate);
    }

    [Inject]
    protected IScrollManager ScrollManager { get; set; } = null!;

    /// <summary>
    /// The currently selected date in single selection mode.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Data)]
    public DateTime? Date { get; set; }

    /// <summary>
    /// Occurs when <see cref="Date"/> has changed.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime?> DateChanged { get; set; }

    /// <summary>
    /// The collection of currently selected dates in multiple selection mode.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Data)]
    public IReadOnlyCollection<DateTime>? SelectedDates { get; set; }

    /// <summary>
    /// Occurs when <see cref="SelectedDates"/> has changed.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<DateTime>?> SelectedDatesChanged { get; set; }

    /// <summary>
    /// Enables multiple date selection.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool MultiSelection { get; set; }

    /// <summary>
    /// Shows week numbers at the start of each week row.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public bool ShowWeekNumbers { get; set; }

    /// <summary>
    /// The current month shown in the calendar view.
    /// </summary>
    /// <remarks>
    /// Defaults to the month of <see cref="Date"/>, or the current month.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.PickerBehavior)]
    public DateTime? PickerMonth { get; set; }

    /// <summary>
    /// Occurs when <see cref="PickerMonth"/> has changed.
    /// </summary>
    [Parameter]
    public EventCallback<DateTime?> PickerMonthChanged { get; set; }

    /// <summary>
    /// The culture used for formatting dates, day names, and month titles.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="CultureInfo.CurrentCulture"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerBehavior)]
    public CultureInfo? Culture { get; set; }

    /// <summary>
    /// The day representing the first day of the week.
    /// </summary>
    /// <remarks>
    /// Defaults to the culture's <see cref="DateTimeFormatInfo.FirstDayOfWeek"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerBehavior)]
    public DayOfWeek? FirstDayOfWeek { get; set; }

    /// <summary>
    /// The earliest date selectable in the calendar.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Validation)]
    public DateTime? MinDate { get; set; }

    /// <summary>
    /// The latest date selectable in the calendar.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.FormComponent.Validation)]
    public DateTime? MaxDate { get; set; }

    /// <summary>
    /// A custom function returning whether a given date should be disabled.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Validation)]
    public Func<DateTime, bool>? IsDateDisabledFunc { get; set; }

    /// <summary>
    /// A custom template for rendering individual day cells.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public RenderFragment<DatePickerDayContext>? DayCellTemplate { get; set; }

    /// <summary>
    /// The initial view displayed by the calendar.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="OpenTo.Date"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerBehavior)]
    public OpenTo OpenTo { get; set; } = OpenTo.Date;

    /// <summary>
    /// Shows the header toolbar displaying the selected year and date.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerAppearance)]
    public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// The theme color for selected dates, current day indicator, and toolbar.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Color.Primary"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public Color Color { get; set; } = Color.Primary;

    /// <summary>
    /// The size of the drop shadow elevation.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>1</c>. Set to <c>0</c> for no shadow.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public int Elevation { get; set; } = 1;

    /// <summary>
    /// Displays an outline border around the calendar paper card.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public bool Outlined { get; set; }

    /// <summary>
    /// Uses rounded corners.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public bool Rounded { get; set; } = true;

    /// <summary>
    /// Disables rounded corners.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public bool Square { get; set; }

    /// <summary>
    /// Shows a button to jump to and select today's date.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerBehavior)]
    public bool ShowTodayButton { get; set; }

    /// <summary>
    /// The label text for the today button.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>"Today"</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerAppearance)]
    public string TodayButtonText { get; set; } = "Today";

    /// <summary>
    /// The display variant of the picker.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="PickerVariant.Static"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerBehavior)]
    public PickerVariant PickerVariant { get; set; } = PickerVariant.Static;

    /// <summary>
    /// Prevents user selection while preserving visual appearance.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Disables user interaction and dims the calendar.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// The layout orientation of the calendar toolbar and content.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Orientation.Portrait"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerAppearance)]
    public Orientation Orientation { get; set; } = Orientation.Portrait;

    /// <summary>
    /// The custom format string used for the date in the toolbar title.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>"ddd, dd MMM"</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerAppearance)]
    public string? TitleDateFormat { get; set; }

    /// <summary>
    /// A custom function returning additional CSS classes for a given day button.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public Func<DateTime, string?>? AdditionalDateClassesFunc { get; set; }

    /// <summary>
    /// Accessible label for the calendar landmark region.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>"Calendar"</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string? AriaLabel { get; set; } = "Calendar";

    /// <summary>
    /// The icon used for previous navigation buttons.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Icons.Material.Filled.ChevronLeft"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerAppearance)]
    public string PreviousIcon { get; set; } = Icons.Material.Filled.ChevronLeft;

    /// <summary>
    /// The icon used for next navigation buttons.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Icons.Material.Filled.ChevronRight"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.FormComponent.PickerAppearance)]
    public string NextIcon { get; set; } = Icons.Material.Filled.ChevronRight;

    /// <summary>
    /// The currently active view mode.
    /// </summary>
    protected OpenTo CurrentView { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        CurrentView = OpenTo;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_scrollToYearAfterRender && CurrentView == OpenTo.Year)
        {
            _scrollToYearAfterRender = false;
            var selectedYear = _dateState.Value ?? GetMonthStart();
            var id = $"{_componentId}{GetCalendarYear(selectedYear)}";
            await ScrollManager.ScrollToYearAsync(id);
        }
    }

    /// <summary>
    /// Gets the currently selected date from the internal state.
    /// </summary>
    public DateTime? SelectedDate => _dateState.Value;

    /// <summary>
    /// Gets the CSS classes applied to the root calendar container.
    /// </summary>
    protected string Classname => new CssBuilder("vek-paper vek-calendar vek-picker-static")
        .AddClass($"vek-elevation-{Elevation}", Elevation > 0 && !Outlined)
        .AddClass("vek-paper-outlined", Outlined)
        .AddClass("vek-rounded", Rounded && !Square)
        .AddClass("vek-calendar-landscape", Orientation == Orientation.Landscape)
        .AddClass("vek-calendar-disabled", Disabled)
        .AddClass(Class)
        .Build();

    /// <summary>
    /// Gets the culture used for calendar calculations.
    /// </summary>
    public CultureInfo GetCulture()
    {
        return Culture ?? CultureInfo.CurrentCulture;
    }

    /// <summary>
    /// Gets the first day of the currently displayed month.
    /// </summary>
    public DateTime GetMonthStart()
    {
        var current = _pickerMonthState.Value ?? _dateState.Value ?? TimeProvider.GetLocalNow().Date;
        var culture = GetCulture();
        return current.StartOfMonth(culture);
    }

    /// <summary>
    /// Gets the last day of the currently displayed month.
    /// </summary>
    public DateTime GetMonthEnd()
    {
        var culture = GetCulture();
        return GetMonthStart().EndOfMonth(culture);
    }

    private DayOfWeek GetFirstDayOfWeek()
    {
        return FirstDayOfWeek ?? GetCulture().DateTimeFormat.FirstDayOfWeek;
    }

    private int GetCalendarYear(DateTime date)
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        return calendar.GetYear(date);
    }

    private string GetFormattedYearString()
    {
        var selected = _dateState.Value ?? GetMonthStart();
        return GetCalendarYear(selected).ToString(CultureInfo.InvariantCulture);
    }

    private string GetTitleDateString()
    {
        if (MultiSelection)
        {
            var count = _selectedDatesState.Value?.Count ?? 0;
            if (count == 0)
                return "No date selected";
            if (count == 1)
                return FormatTitleDate(_selectedDatesState.Value!.First());
            return $"{count} dates selected";
        }

        return FormatTitleDate(_dateState.Value ?? GetMonthStart());
    }

    private string FormatTitleDate(DateTime? date)
    {
        return date?.ToString(TitleDateFormat ?? "ddd, dd MMM", GetCulture()) ?? string.Empty;
    }

    private bool IsDateSelected(DateTime day)
    {
        if (MultiSelection)
        {
            var selected = _selectedDatesState.Value;
            if (selected == null || selected.Count == 0)
                return false;
            return selected.Any(d => d.Date == day.Date);
        }

        return _dateState.Value.HasValue && _dateState.Value.Value.Date == day.Date;
    }

    private bool IsAdjacentMonthDay(DateTime day)
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        var currentMonth = GetMonthStart();
        return calendar.GetMonth(day) != calendar.GetMonth(currentMonth) || calendar.GetYear(day) != calendar.GetYear(currentMonth);
    }

    private bool IsDayDisabled(DateTime day)
    {
        if (Disabled)
            return true;
        if (_minDateState.Value.HasValue && day.Date < _minDateState.Value.Value.Date)
            return true;
        if (_maxDateState.Value.HasValue && day.Date > _maxDateState.Value.Value.Date)
            return true;
        if (IsDateDisabledFunc != null && IsDateDisabledFunc(day.Date))
            return true;
        return false;
    }

    private string GetDayClasses(DateTime day, bool isSelected, bool isDisabled)
    {
        var b = new CssBuilder("vek-button-root vek-icon-button vek-ripple vek-ripple-icon vek-picker-calendar-day vek-day");
        if (AdditionalDateClassesFunc != null)
        {
            var extra = AdditionalDateClassesFunc(day);
            if (!string.IsNullOrEmpty(extra))
                b.AddClass(extra);
        }
        b.AddClass("vek-adjacent-month", IsAdjacentMonthDay(day));
        if (isSelected)
        {
            b.AddClass("vek-selected").AddClass($"vek-theme-{Color.ToStringFast(true)}");
        }
        else if (day.Date == TimeProvider.GetLocalNow().Date)
        {
            b.AddClass("vek-current vek-button-outlined")
             .AddClass($"vek-button-outlined-{Color.ToStringFast(true)} vek-{Color.ToStringFast(true)}-text");
        }
        else if (_focusedDate.HasValue && _focusedDate.Value.Date == day.Date)
        {
            b.AddClass("vek-calendar-day-focused");
        }
        if (isDisabled)
        {
            b.AddClass("vek-disabled");
        }
        return b.Build();
    }

    private async Task HandleDayButtonClickAsync(DateTime day)
    {
        if (Disabled || ReadOnly || IsDayDisabled(day))
            return;

        _focusedDate = day;

        if (MultiSelection)
        {
            var current = _selectedDatesState.Value != null
                ? new List<DateTime>(_selectedDatesState.Value)
                : new List<DateTime>();

            var existing = current.FindIndex(d => d.Date == day.Date);
            if (existing >= 0)
            {
                current.RemoveAt(existing);
            }
            else
            {
                current.Add(day.Date);
                current.Sort();
            }

            await _selectedDatesState.SetValueAsync(current.AsReadOnly());
            await _dateState.SetValueAsync(day);
        }
        else
        {
            await _dateState.SetValueAsync(day);
            await _selectedDatesState.SetValueAsync(new List<DateTime> { day.Date }.AsReadOnly());
        }

        var culture = GetCulture();
        var calendar = culture.Calendar;
        var currentMonth = GetMonthStart();
        if (calendar.GetMonth(day) != calendar.GetMonth(currentMonth) || calendar.GetYear(day) != calendar.GetYear(currentMonth))
        {
            await SetPickerMonthAsync(day.StartOfMonth(culture));
        }
    }

    private async Task SetPickerMonthAsync(DateTime month)
    {
        var firstOfMonth = month.StartOfMonth(GetCulture());
        await _pickerMonthState.SetValueAsync(firstOfMonth);
    }

    private void GoToSelectedYear()
    {
        CurrentView = OpenTo.Year;
        _scrollToYearAfterRender = true;
        StateHasChanged();
    }

    private void OnFormattedDateClick()
    {
        CurrentView = OpenTo.Date;
        StateHasChanged();
    }

    private void OnYearClick()
    {
        CurrentView = OpenTo.Year;
        _scrollToYearAfterRender = true;
        StateHasChanged();
    }

    private void OnMonthClick()
    {
        CurrentView = OpenTo.Month;
        StateHasChanged();
    }

    private async Task OnYearClickedAsync(int year)
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        var current = GetMonthStart();
        var newMonth = new DateTime(year, calendar.GetMonth(current), 1, calendar);
        await SetPickerMonthAsync(newMonth);
        CurrentView = OpenTo.Date;
        StateHasChanged();
    }

    private async Task OnMonthSelectedAsync(DateTime month)
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        var current = GetMonthStart();
        var newMonth = new DateTime(calendar.GetYear(current), calendar.GetMonth(month), 1, calendar);
        await SetPickerMonthAsync(newMonth);
        CurrentView = OpenTo.Date;
        StateHasChanged();
    }

    private async Task OnPreviousMonthClickAsync()
    {
        var current = GetMonthStart();
        var culture = GetCulture();
        var calendar = culture.Calendar;
        if (calendar.GetYear(current) == DateTime.MinValue.Year && calendar.GetMonth(current) == DateTime.MinValue.Month)
            return;
        var newMonth = current.AddDays(-1).StartOfMonth(culture);
        await SetPickerMonthAsync(newMonth);
    }

    private async Task OnNextMonthClickAsync()
    {
        var current = GetMonthEnd();
        var culture = GetCulture();
        var calendar = culture.Calendar;
        if (calendar.GetYear(current) == DateTime.MaxValue.Year && calendar.GetMonth(current) == DateTime.MaxValue.Month)
            return;
        var newMonth = current.AddDays(1).StartOfMonth(culture);
        await SetPickerMonthAsync(newMonth);
    }

    private async Task OnPreviousYearClickAsync()
    {
        var current = GetMonthStart();
        var culture = GetCulture();
        var calendar = culture.Calendar;
        if (calendar.GetYear(current) <= DateTime.MinValue.Year)
            return;
        var newMonth = current.AddYears(-1);
        await SetPickerMonthAsync(newMonth);
    }

    private async Task OnNextYearClickAsync()
    {
        var current = GetMonthStart();
        var culture = GetCulture();
        var calendar = culture.Calendar;
        if (calendar.GetYear(current) >= DateTime.MaxValue.Year)
            return;
        var newMonth = current.AddYears(1);
        await SetPickerMonthAsync(newMonth);
    }

    /// <summary>
    /// Jumps to and selects the current date.
    /// </summary>
    public async Task GoToTodayAsync()
    {
        var today = TimeProvider.GetLocalNow().Date;
        if (IsDayDisabled(today))
            return;
        await SetPickerMonthAsync(today.StartOfMonth(GetCulture()));
        await HandleDayButtonClickAsync(today);
    }

    private IEnumerable<DateTime> GetWeek(int index)
    {
        if (index is < 0 or > 5)
        {
            throw new ArgumentException("Index must be between 0 and 5", nameof(index));
        }
        var culture = GetCulture();
        var calendar = culture.Calendar;
        var monthFirst = GetMonthStart();
        if ((calendar.MaxSupportedDateTime - monthFirst).Days >= index * 7)
        {
            var weekFirst = monthFirst.AddDays(index * 7).StartOfWeek(GetFirstDayOfWeek(), culture);
            for (var i = 0; i < 7; i++)
            {
                if ((calendar.MaxSupportedDateTime - weekFirst).Days >= i)
                    yield return weekFirst.AddDays(i);
                else
                    yield return calendar.MaxSupportedDateTime;
            }
        }
    }

    private string GetWeekNumber(int index)
    {
        if (index is < 0 or > 5)
        {
            throw new ArgumentException("Index must be between 0 and 5", nameof(index));
        }

        var culture = GetCulture();
        var calendar = culture.Calendar;
        var monthFirst = GetMonthStart();
        var weekFirst = monthFirst.AddDays(index * 7).StartOfWeek(GetFirstDayOfWeek(), culture);
        if (monthFirst.Month == 1 && index == 0)
        {
            weekFirst = monthFirst;
        }

        if (weekFirst.Month != monthFirst.Month && weekFirst.AddDays(6).Month != monthFirst.Month)
        {
            return string.Empty;
        }

        return calendar.GetWeekOfYear(weekFirst, culture.DateTimeFormat.CalendarWeekRule, FirstDayOfWeek ?? culture.DateTimeFormat.FirstDayOfWeek).ToString(CultureInfo.InvariantCulture);
    }

    private IEnumerable<string> GetAbbreviatedDayNames()
    {
        var dayNamesNormal = GetCulture().DateTimeFormat.AbbreviatedDayNames;
        return Shift(dayNamesNormal, (int)GetFirstDayOfWeek());
    }

    private static T[] Shift<T>(T[] array, int positions)
    {
        var copy = new T[array.Length];
        Array.Copy(array, 0, copy, array.Length - positions, positions);
        Array.Copy(array, positions, copy, 0, array.Length - positions);
        return copy;
    }

    private int GetMinYear()
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        if (_minDateState.Value is { } minDate)
            return calendar.GetYear(minDate);
        return calendar.GetYear(TimeProvider.GetLocalNow().Date) - 100;
    }

    private int GetMaxYear()
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        if (_maxDateState.Value is { } maxDate)
            return calendar.GetYear(maxDate);
        return calendar.GetYear(TimeProvider.GetLocalNow().Date) + 100;
    }

    private Typo GetYearTypo(int year)
    {
        var selectedYear = _dateState.Value ?? GetMonthStart();
        var calendar = GetCulture().Calendar;
        if (year == calendar.GetYear(selectedYear))
            return Typo.h5;

        return Typo.subtitle1;
    }

    private string? GetYearClasses(int year)
    {
        var selectedYear = _dateState.Value ?? GetMonthStart();
        var calendar = GetCulture().Calendar;
        if (year == calendar.GetYear(selectedYear))
            return $"vek-picker-year-selected vek-{Color.ToStringFast(true)}-text";

        return null;
    }

    private IEnumerable<DateTime> GetAllMonths()
    {
        var current = GetMonthStart();
        var culture = GetCulture();
        var calendar = culture.Calendar;
        var calendarYear = calendar.GetYear(current);
        var firstOfCalendarYear = calendar.ToDateTime(calendarYear, 1, 1, 0, 0, 0, 0);
        for (var i = 0; i < calendar.GetMonthsInYear(calendarYear); i++)
            yield return calendar.AddMonths(firstOfCalendarYear, i);
    }

    private string GetAbbreviatedMonthName(DateTime month)
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        var calendarMonth = calendar.GetMonth(month);
        return culture.DateTimeFormat.AbbreviatedMonthNames[calendarMonth - 1];
    }

    private string GetMonthName(DateTime month)
    {
        var culture = GetCulture();
        var calendar = culture.Calendar;
        var calendarMonth = calendar.GetMonth(month);
        return culture.DateTimeFormat.MonthNames[calendarMonth - 1];
    }

    private string GetMonthYearTitle(DateTime month)
    {
        var culture = GetCulture();
        return month.ToString(culture.DateTimeFormat.YearMonthPattern, culture);
    }

    private Typo GetMonthTypo(DateTime month)
    {
        var selected = _dateState.Value ?? GetMonthStart();
        var calendar = GetCulture().Calendar;
        if (calendar.GetYear(month) == calendar.GetYear(selected) && calendar.GetMonth(month) == calendar.GetMonth(selected))
            return Typo.h5;

        return Typo.subtitle1;
    }

    private string? GetMonthClasses(DateTime month)
    {
        var selected = _dateState.Value ?? GetMonthStart();
        var calendar = GetCulture().Calendar;
        if (calendar.GetYear(month) != calendar.GetYear(selected))
            return null;

        if (calendar.GetMonth(month) == calendar.GetMonth(selected) && !IsMonthDisabled(selected))
            return $"vek-picker-month-selected vek-{Color.ToStringFast(true)}-text";

        return null;
    }

    private bool IsMonthDisabled(DateTime month)
    {
        var culture = GetCulture();
        if (_minDateState.Value.HasValue && month.EndOfMonth(culture) < _minDateState.Value.Value)
            return true;
        if (_maxDateState.Value.HasValue && month.StartOfMonth(culture) > _maxDateState.Value.Value)
            return true;
        return false;
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled)
            return;

        var current = _focusedDate ?? _dateState.Value ?? GetMonthStart();
        var culture = GetCulture();

        switch (args.Key)
        {
            case "ArrowLeft":
                _focusedDate = current.AddDays(-1);
                if (IsAdjacentMonthDay(_focusedDate.Value))
                    await SetPickerMonthAsync(_focusedDate.Value.StartOfMonth(culture));
                StateHasChanged();
                break;
            case "ArrowRight":
                _focusedDate = current.AddDays(1);
                if (IsAdjacentMonthDay(_focusedDate.Value))
                    await SetPickerMonthAsync(_focusedDate.Value.StartOfMonth(culture));
                StateHasChanged();
                break;
            case "ArrowUp":
                _focusedDate = current.AddDays(-7);
                if (IsAdjacentMonthDay(_focusedDate.Value))
                    await SetPickerMonthAsync(_focusedDate.Value.StartOfMonth(culture));
                StateHasChanged();
                break;
            case "ArrowDown":
                _focusedDate = current.AddDays(7);
                if (IsAdjacentMonthDay(_focusedDate.Value))
                    await SetPickerMonthAsync(_focusedDate.Value.StartOfMonth(culture));
                StateHasChanged();
                break;
            case "PageUp":
                _focusedDate = current.AddMonths(-1);
                await SetPickerMonthAsync(_focusedDate.Value.StartOfMonth(culture));
                StateHasChanged();
                break;
            case "PageDown":
                _focusedDate = current.AddMonths(1);
                await SetPickerMonthAsync(_focusedDate.Value.StartOfMonth(culture));
                StateHasChanged();
                break;
            case "Home":
                _focusedDate = current.StartOfMonth(culture);
                await SetPickerMonthAsync(_focusedDate.Value);
                StateHasChanged();
                break;
            case "End":
                _focusedDate = current.EndOfMonth(culture);
                await SetPickerMonthAsync(_focusedDate.Value.StartOfMonth(culture));
                StateHasChanged();
                break;
            case "Enter" or " ":
                if (_focusedDate.HasValue && !IsDayDisabled(_focusedDate.Value))
                {
                    await HandleDayButtonClickAsync(_focusedDate.Value);
                }
                break;
        }
    }
}
