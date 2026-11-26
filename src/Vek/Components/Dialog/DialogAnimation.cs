using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// The type of entrance animation applied to a <see cref="VekDialog"/>.
/// </summary>
[EnumExtensions]
public enum DialogAnimation
{
    /// <summary>
    /// Smooth opacity fade-in transition.
    /// </summary>
    [Description("fade")]
    Fade,

    /// <summary>
    /// Scaling entrance transition zooming from smaller scale to full size.
    /// </summary>
    [Description("zoom")]
    Zoom,

    /// <summary>
    /// Upward sliding entrance transition.
    /// </summary>
    [Description("slideup")]
    SlideUp,

    /// <summary>
    /// Downward sliding entrance transition.
    /// </summary>
    [Description("slidedown")]
    SlideDown,

    /// <summary>
    /// No entrance animation.
    /// </summary>
    [Description("none")]
    None
}
