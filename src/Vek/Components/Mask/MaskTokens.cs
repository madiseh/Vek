// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// Predefined standard mask tokens for numeric, alphabetic, alphanumeric, and optional characters.
/// </summary>
public static class MaskTokens
{
    /// <summary>
    /// Mandatory digit (0-9).
    /// </summary>
    /// <param name="c">The mask character representing this token (default: <c>'0'</c>).</param>
    public static MaskChar Digit(char c = '0') => MaskChar.Digit(c, optional: false);

    /// <summary>
    /// Optional digit (0-9).
    /// </summary>
    /// <param name="c">The mask character representing this token (default: <c>'9'</c>).</param>
    public static MaskChar OptionalDigit(char c = '9') => new(c, @"\d", optional: true);

    /// <summary>
    /// Optional digit, plus/minus sign, or space.
    /// </summary>
    /// <param name="c">The mask character representing this token (default: <c>'#'</c>).</param>
    public static MaskChar DigitOrSign(char c = '#') => new(c, @"[\d\+\- ]", optional: true);

    /// <summary>
    /// Mandatory letter (a-z, A-Z).
    /// </summary>
    /// <param name="c">The mask character representing this token (default: <c>'L'</c>).</param>
    public static MaskChar Letter(char c = 'L') => MaskChar.Letter(c, optional: false);

    /// <summary>
    /// Optional letter.
    /// </summary>
    /// <param name="c">The mask character representing this token (default: <c>'?'</c>).</param>
    public static MaskChar OptionalLetter(char c = '?') => new(c, @"\p{L}", optional: true);

    /// <summary>
    /// Mandatory alphanumeric (letter or digit).
    /// </summary>
    /// <param name="c">The mask character representing this token (default: <c>'A'</c>).</param>
    public static MaskChar Alphanumeric(char c = 'A') => MaskChar.LetterOrDigit(c, optional: false);

    /// <summary>
    /// Optional alphanumeric (letter or digit).
    /// </summary>
    /// <param name="c">The mask character representing this token (default: <c>'a'</c>).</param>
    public static MaskChar OptionalAlphanumeric(char c = 'a') => new(c, @"\p{L}|\d", optional: true);

    /// <summary>
    /// Standard set of tokens for pattern masks, providing numeric, alphabetic, alphanumeric, and optional rules.
    /// </summary>
    public static MaskChar[] Standard =>
    [
        Digit('0'),
        OptionalDigit('9'),
        DigitOrSign('#'),
        Letter('L'),
        OptionalLetter('?'),
        Alphanumeric('A'),
        MaskChar.Letter('a'),
        MaskChar.LetterOrDigit('*')
    ];
}
