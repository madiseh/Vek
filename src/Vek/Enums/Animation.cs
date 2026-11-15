using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// Indicates the type of animation used for a <see cref="VekSkeleton"/> component.
/// </summary>
[EnumExtensions]
public enum Animation
{
    /// <summary>
    /// No animation occurs.
    /// </summary>
    [Description("false")]
    False,

    /// <summary>
    /// The animation fades in and out in a pulsing loop.
    /// </summary>
    [Description("pulse")]
    Pulse,

    /// <summary>
    /// A left-to-right wave effect occurs.
    /// </summary>
    [Description("wave")]
    Wave
}
