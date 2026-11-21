// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

public partial class VekMask
{
    /// <summary>
    /// Creates a new <see cref="PatternMask"/> configured with standard tokens and the specified prompt character.
    /// </summary>
    /// <param name="pattern">The mask pattern.</param>
    /// <param name="promptChar">The prompt character displayed for unfilled positions (default: <c>'_'</c>).</param>
    /// <returns>A configured <see cref="PatternMask"/> instance.</returns>
    public static PatternMask CreatePatternMask(string pattern, char promptChar = '_')
    {
        return new PatternMask(pattern)
        {
            MaskChars = MaskTokens.Standard,
            PromptChar = promptChar
        };
    }
}
