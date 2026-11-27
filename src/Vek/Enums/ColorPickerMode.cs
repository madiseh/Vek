// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// Indicates the initial mode used by a <see cref="VekColorPicker"/>.
/// </summary>
[EnumExtensions]
public enum ColorPickerMode
{
    /// <summary>
    /// Red, Green, and Blue color values are used.
    /// </summary>
    RGB,

    /// <summary>
    /// Hue, Saturation, and Lightness color values are used.
    /// </summary>
    HSL,

    /// <summary>
    /// Hexadecimal values are used.
    /// </summary>
    HEX,

    /// <summary>
    /// Hue, Saturation, and Value (Brightness) color values are used.
    /// </summary>
    HSV
}
