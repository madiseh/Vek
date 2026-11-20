// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Docs
{
    public class Theme
    {
        public static VekTheme LandingPageTheme()
        {
            return new VekTheme
            {
                PaletteLight = LandingPageLightPalette,
                PaletteDark = LandingPageDarkPalette,
                Shadows = ModernShadows,
                LayoutProperties = CommonLayoutProperties,
                ZIndex = new ZIndex(),
                Typography = ModernTypography
            };
        }

        public static VekTheme DocsTheme()
        {
            return new VekTheme
            {
                PaletteLight = DocsLightPalette,
                PaletteDark = DocsDarkPalette,
                Shadows = ModernShadows,
                LayoutProperties = CommonLayoutProperties,
                ZIndex = new ZIndex(),
                Typography = ModernTypography
            };
        }

        #region Layout & Typography

        private static readonly LayoutProperties CommonLayoutProperties = new()
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "260px",
            AppbarHeight = "64px"
        };

        private static readonly string[] ModernFontFamily =
        [
            "Plus Jakarta Sans",
            "-apple-system",
            "BlinkMacSystemFont",
            "Segoe UI",
            "Roboto",
            "Helvetica Neue",
            "Arial",
            "sans-serif"
        ];

        private static readonly Typography ModernTypography = new()
        {
            Default = new DefaultTypography
            {
                FontFamily = ModernFontFamily,
                LetterSpacing = "-0.011em",
                LineHeight = "1.6"
            },
            H1 = new H1Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "3.25rem",
                FontWeight = "700",
                LineHeight = "1.15",
                LetterSpacing = "-0.025em"
            },
            H2 = new H2Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "2.25rem",
                FontWeight = "700",
                LineHeight = "1.25",
                LetterSpacing = "-0.02em"
            },
            H3 = new H3Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "1.75rem",
                FontWeight = "600",
                LineHeight = "1.3",
                LetterSpacing = "-0.015em"
            },
            H4 = new H4Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "1.35rem",
                FontWeight = "600",
                LineHeight = "1.35"
            },
            H5 = new H5Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "1.15rem",
                FontWeight = "600",
                LineHeight = "1.4"
            },
            H6 = new H6Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "1rem",
                FontWeight = "600",
                LineHeight = "1.5"
            },
            Subtitle1 = new Subtitle1Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "1.05rem",
                FontWeight = "400",
                LineHeight = "1.6"
            },
            Subtitle2 = new Subtitle2Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "0.95rem",
                FontWeight = "600",
                LineHeight = "1.5"
            },
            Body1 = new Body1Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "0.95rem",
                FontWeight = "400",
                LineHeight = "1.65"
            },
            Body2 = new Body2Typography
            {
                FontFamily = ModernFontFamily,
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.6"
            },
            Button = new ButtonTypography
            {
                FontFamily = ModernFontFamily,
                TextTransform = "none",
                FontWeight = "600",
                LetterSpacing = "normal"
            }
        };

        #endregion

        #region Docs Palettes

        private static readonly PaletteLight DocsLightPalette = new()
        {
            Primary = "#2563EB",
            Secondary = "#0EA5E9",
            Tertiary = "#10B981",
            Info = "#0284C7",
            Success = "#16A34A",
            Warning = "#D97706",
            Error = "#DC2626",
            Black = "#0F172A",
            Dark = "#0F172A",
            DarkLighten = "#1E293B",
            AppbarText = "#0F172A",
            AppbarBackground = "rgba(255, 255, 255, 0.85)",
            Background = "#F8FAFC",
            BackgroundGray = "#F1F5F9",
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#334155",
            DrawerIcon = "#64748B",
            TextPrimary = "#0F172A",
            TextSecondary = "#475569",
            TextDisabled = "#94A3B8",
            ActionDefault = "#475569",
            LinesDefault = "#E2E8F0",
            TableLines = "#E2E8F0",
            Divider = "#E2E8F0",
            DividerLight = "#F1F5F9",
            GrayDefault = "#64748B",
            GrayLight = "#E2E8F0",
            GrayLighter = "#F8FAFC"
        };

        private static readonly PaletteDark DocsDarkPalette = new()
        {
            Primary = "#3B82F6",
            Secondary = "#38BDF8",
            Tertiary = "#34D399",
            Info = "#60A5FA",
            Success = "#22C55E",
            Warning = "#FBBF24",
            Error = "#F87171",
            Dark = "#030712",
            DarkLighten = "#0F172A",
            Background = "#0B0F17",
            BackgroundGray = "#070A0F",
            Surface = "#111827",
            AppbarText = "#F8FAFC",
            AppbarBackground = "rgba(11, 15, 23, 0.85)",
            DrawerBackground = "#0B0F17",
            DrawerIcon = "#94A3B8",
            DrawerText = "#94A3B8",
            ActionDefault = "#94A3B8",
            ActionDisabled = "rgba(255, 255, 255, 0.2)",
            ActionDisabledBackground = "rgba(255, 255, 255, 0.06)",
            TextPrimary = "#F8FAFC",
            TextSecondary = "#94A3B8",
            TextDisabled = "rgba(255, 255, 255, 0.25)",
            GrayDefault = "#94A3B8",
            GrayLight = "#1E293B",
            GrayLighter = "#111827",
            LinesDefault = "rgba(255, 255, 255, 0.08)",
            TableLines = "rgba(255, 255, 255, 0.08)",
            Divider = "rgba(255, 255, 255, 0.08)",
            DividerLight = "rgba(255, 255, 255, 0.04)",
            OverlayLight = "rgba(15, 23, 42, 0.75)"
        };

        #endregion

        #region LandingPage Palettes

        private static readonly PaletteLight LandingPageLightPalette = new()
        {
            Primary = "#2563EB",
            Secondary = "#0EA5E9",
            Tertiary = "#10B981",
            AppbarText = "#0F172A",
            AppbarBackground = "rgba(0, 0, 0, 0)",
            Background = "#FFFFFF",
            BackgroundGray = "#F8FAFC",
            Surface = "#FFFFFF",
            TextPrimary = "#0F172A",
            TextSecondary = "#475569",
            Dark = "#0F172A",
            DarkLighten = "#1E293B",
            GrayDefault = "#64748B",
            GrayLight = "#E2E8F0",
            GrayLighter = "#F8FAFC",
            LinesDefault = "#E2E8F0",
            Divider = "#E2E8F0"
        };

        private static readonly PaletteDark LandingPageDarkPalette = new()
        {
            Primary = "#3B82F6",
            Secondary = "#38BDF8",
            Tertiary = "#34D399",
            AppbarText = "#F8FAFC",
            AppbarBackground = "rgba(0, 0, 0, 0)",
            Background = "#0B0F17",
            BackgroundGray = "#070A0F",
            Surface = "#111827",
            Dark = "#030712",
            DarkLighten = "#0F172A",
            TextPrimary = "#F8FAFC",
            TextSecondary = "#94A3B8",
            TextDisabled = "rgba(255, 255, 255, 0.25)",
            ActionDefault = "#94A3B8",
            DrawerIcon = "#94A3B8",
            DrawerText = "#94A3B8",
            DrawerBackground = "#0B0F17",
            LinesDefault = "rgba(255, 255, 255, 0.08)",
            Divider = "rgba(255, 255, 255, 0.08)",
            OverlayLight = "rgba(15, 23, 42, 0.75)"
        };

        #endregion

        #region Modern Shadows

        private static readonly Shadow ModernShadows = new()
        {
            Elevation =
            [
                "none",
                "0 1px 2px 0 rgba(0, 0, 0, 0.05)",
                "0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px -1px rgba(0, 0, 0, 0.1)",
                "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1)",
                "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1)",
                "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)",
                "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
                "0 1px 3px 0 rgba(15, 23, 42, 0.08)",
                "0 2px 4px 0 rgba(15, 23, 42, 0.08)",
                "0 4px 8px 0 rgba(15, 23, 42, 0.08)",
                "0 6px 12px 0 rgba(15, 23, 42, 0.08)",
                "0 8px 16px 0 rgba(15, 23, 42, 0.08)",
                "0 12px 24px 0 rgba(15, 23, 42, 0.08)",
                "0 16px 32px 0 rgba(15, 23, 42, 0.08)",
                "0 20px 40px 0 rgba(15, 23, 42, 0.08)",
                "0 24px 48px 0 rgba(15, 23, 42, 0.08)",
                "0 28px 56px 0 rgba(15, 23, 42, 0.08)",
                "0 32px 64px 0 rgba(15, 23, 42, 0.08)",
                "0 36px 72px 0 rgba(15, 23, 42, 0.08)",
                "0 40px 80px 0 rgba(15, 23, 42, 0.08)",
                "0 44px 88px 0 rgba(15, 23, 42, 0.08)",
                "0 48px 96px 0 rgba(15, 23, 42, 0.08)",
                "0 52px 104px 0 rgba(15, 23, 42, 0.08)",
                "0 56px 112px 0 rgba(15, 23, 42, 0.08)",
                "0 60px 120px 0 rgba(15, 23, 42, 0.08)",
                "0 64px 128px 0 rgba(15, 23, 42, 0.08)"
            ]
        };

        #endregion
    }
}
