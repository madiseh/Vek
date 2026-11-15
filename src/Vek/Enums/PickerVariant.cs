using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// Indicates the display behavior of a <see cref="VekPicker{T}"/> component.
/// </summary>
[EnumExtensions]
public enum PickerVariant
{
    /// <summary>
    /// The picker displays when the input is clicked.
    /// </summary>
    [Description("inline")]
    Inline,

    /// <summary>
    /// A dialog is displayed to pick a value.
    /// </summary>
    [Description("dialog")]
    Dialog,

    /// <summary>
    /// The picker is always visible.
    /// </summary>
    [Description("static")]
    Static,
}
