// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Vek.Resources;
using Vek.Utilities.Exceptions;

namespace Vek;

internal partial class DefaultConverter
{
    internal sealed class TimeSpanConverter(Func<CultureInfo> culture, Func<string?> format)
        : IReversibleConverter<TimeSpan, string?>, IReversibleConverter<TimeSpan?, string?>
    {
        private const string DefaultTimeSpanFormat = "c";

        public string Convert(TimeSpan timeSpan) => timeSpan.ToString(format.Invoke() ?? DefaultTimeSpanFormat, culture.Invoke());

        public string? Convert(TimeSpan? timeSpan) => timeSpan?.ToString(format.Invoke() ?? DefaultTimeSpanFormat, culture.Invoke());

        public TimeSpan ConvertBack(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return TimeSpan.Zero;
            }

            if (TimeSpan.TryParseExact(input, format.Invoke() ?? DefaultTimeSpanFormat, culture.Invoke(), out var result))
            {
                return result;
            }

            throw new ConversionException(LanguageResource.Converter_InvalidTimeSpan);
        }

        TimeSpan? IReversibleConverter<TimeSpan?, string?>.ConvertBack(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            return ConvertBack(input);
        }
    }
}
