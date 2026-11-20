// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text.RegularExpressions;
using Vek.Resources;
using Vek.Utilities;
using Vek.Utilities.Exceptions;

namespace Vek;

internal sealed partial class TimeSpanConverter : IReversibleConverter<TimeSpan?, string>, ICultureAwareConverter
{
    public const string Format24Hours = "HH:mm";
    public const string Format12Hours = "hh:mm tt";
    public const string Format24HoursWithSeconds = "HH:mm:ss";
    public const string Format12HoursWithSeconds = "hh:mm:ss tt";

    public Func<string?> Format { get; set; } = () => null;

    public Func<CultureInfo> Culture { get; set; } = () => CultureInfo.CurrentUICulture;

    public string Convert(TimeSpan? input)
    {
        if (input == null)
        {
            return string.Empty;
        }

        var time = DateTime.Today.Add(input.Value);
        var format = Format();
        if (string.IsNullOrWhiteSpace(format))
        {
            format = Format24Hours;
        }

        return time.ToString(format, Culture());
    }

    public TimeSpan? ConvertBack(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        var format = Format();
        if (!string.IsNullOrWhiteSpace(format) &&
            DateTime.TryParseExact(input, format, Culture(), DateTimeStyles.None, out var time))
        {
            return time.TimeOfDay;
        }

        if (DateTime.TryParseExact(input, Format24Hours, Culture(), DateTimeStyles.None, out time))
        {
            return time.TimeOfDay;
        }

        if (DateTime.TryParseExact(input, Format24HoursWithSeconds, Culture(), DateTimeStyles.None, out time))
        {
            return time.TimeOfDay;
        }

        var m = AmPmRegularExpression().Match(input);
        if (m.Success)
        {
            if (DateTime.TryParseExact(input, Format12Hours, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                return time.TimeOfDay;
            }

            if (DateTime.TryParseExact(input, Format12HoursWithSeconds, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                return time.TimeOfDay;
            }
        }
        else
        {
            if (DateTime.TryParseExact(input, Format24Hours, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                return time.TimeOfDay;
            }

            if (DateTime.TryParseExact(input, Format24HoursWithSeconds, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                return time.TimeOfDay;
            }
        }

        throw new ConversionException(LanguageResource.Converter_InvalidTimeSpan);
    }

    [GeneratedRegex("AM|PM", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexDefaults.MatchTimeoutMilliseconds)]
    private static partial Regex AmPmRegularExpression();
}
