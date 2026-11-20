// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Docs.Enums;
using Vek.Docs.Services;

namespace Vek.Docs.Shared;

public partial class AppbarButtons
{
    [Inject]
    private LayoutService LayoutService { get; set; } = null!;

    /// <summary>
    /// Gets the text for the RTL toggle button, indicating the next state.
    /// </summary>
    public string RtlButtonText => LayoutService.IsRTL ? "Left-to-right" : "Right-to-left";

    /// <summary>
    /// Gets the icon for the RTL toggle button.
    /// </summary>
    public string RtlButtonIcon => LayoutService.IsRTL ? @Icons.Material.Filled.FormatTextdirectionLToR : @Icons.Material.Filled.FormatTextdirectionRToL;

    /// <summary>
    /// Gets the text for the dark/light mode toggle button, indicating the next mode.
    /// </summary>
    public string DarkLightModeButtonText => LayoutService.CurrentDarkLightMode switch
    {
        DarkLightMode.Dark => "Auto mode",
        DarkLightMode.Light => "Dark mode",
        _ => "Light mode"
    };

    /// <summary>
    /// Gets the icon for the dark/light mode toggle button.
    /// </summary>
    public string DarkLightModeButtonIcon => LayoutService.CurrentDarkLightMode switch
    {
        DarkLightMode.Dark => Icons.Material.Rounded.AutoMode,
        DarkLightMode.Light => Icons.Material.Outlined.DarkMode,
        _ => Icons.Material.Filled.LightMode
    };

}
