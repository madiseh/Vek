using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// Positions a tab icon relative to its text.
/// </summary>
[EnumExtensions]
public enum TabIconPosition
{
    /// <summary>
    /// Places the icon before the text.
    /// </summary>
    [Description("start")]
    Start,

    /// <summary>
    /// Places the icon after the text.
    /// </summary>
    [Description("end")]
    End,
}
