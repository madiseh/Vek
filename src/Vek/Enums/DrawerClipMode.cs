using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek;

/// <summary>
/// Indicates the clipping behavior of a <see cref="VekDrawer"/> when inside of a <see cref="VekLayout"/>.
/// </summary>
[EnumExtensions]
public enum DrawerClipMode
{
    /// <summary>
    /// The drawer will display over the <see cref="VekAppBar"/> and other content.
    /// </summary>
    [Description("never")]
    Never,

    /// <summary>
    /// The drawer will display underneath the <see cref="VekAppBar"/> and push content to the side when opening.
    /// </summary>
    [Description("docked")]
    Docked,

    /// <summary>
    /// The drawer will display underneath the <see cref="VekAppBar"/> and display over content when opened.
    /// </summary>
    [Description("always")]
    Always
}
