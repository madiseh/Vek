// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Numerics;
using Vek.Resources;
using Vek.Utilities.Exceptions;

namespace Vek;

internal partial class DefaultConverter
{
    internal sealed class BigIntegerConverter(Func<CultureInfo> culture, Func<string?> format)
        : IReversibleConverter<BigInteger, string?>, IReversibleConverter<BigInteger?, string?>
    {
        public string Convert(BigInteger input) => input.ToString(format.Invoke(), culture.Invoke());

        public string? Convert(BigInteger? input) => input?.ToString(format.Invoke(), culture.Invoke());

        public BigInteger ConvertBack(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return BigInteger.Zero;
            }

            var currentCulture = culture.Invoke();

            if (BigInteger.TryParse(input, NumberStyles.Any, currentCulture, out var result))
            {
                return result;
            }

            throw new ConversionException(LanguageResource.Converter_InvalidNumber);
        }

        BigInteger? IReversibleConverter<BigInteger?, string?>.ConvertBack(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            return ConvertBack(input);
        }
    }
}
