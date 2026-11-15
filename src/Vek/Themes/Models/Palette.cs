// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Base class for a Vek color palette, defining the primary, secondary, surface, text, and status colors shared by <see cref="PaletteLight"/> and <see cref="PaletteDark"/>.
    /// </summary>
    [JsonDerivedType(typeof(PaletteLight), typeDiscriminator: nameof(PaletteLight))]
    [JsonDerivedType(typeof(PaletteDark), typeDiscriminator: nameof(PaletteDark))]
    public abstract class Palette
    {
        private VekColor? _primaryDarken;
        private VekColor? _primaryLighten;
        private VekColor? _secondaryDarken;
        private VekColor? _secondaryLighten;
        private VekColor? _tertiaryDarken;
        private VekColor? _tertiaryLighten;
        private VekColor? _infoDarken;
        private VekColor? _infoLighten;
        private VekColor? _successDarken;
        private VekColor? _successLighten;
        private VekColor? _warningDarken;
        private VekColor? _warningLighten;
        private VekColor? _errorDarken;
        private VekColor? _errorLighten;
        private VekColor? _darkDarken;
        private VekColor? _darkLighten;

        /// <summary>
        /// The black color.
        /// </summary>
        public virtual VekColor Black { get; set; } = "#272c34";

        /// <summary>
        /// The white color.
        /// </summary>
        public virtual VekColor White { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The primary color.
        /// </summary>
        public virtual VekColor Primary { get; set; } = "#594AE2";

        /// <summary>
        /// The contrast text color for the primary color.
        /// </summary>
        public virtual VekColor PrimaryContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The secondary color.
        /// </summary>
        public virtual VekColor Secondary { get; set; } = Colors.Pink.Accent2;

        /// <summary>
        /// The contrast text color for the secondary color.
        /// </summary>
        public virtual VekColor SecondaryContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The tertiary color.
        /// </summary>
        public virtual VekColor Tertiary { get; set; } = "#1EC8A5";

        /// <summary>
        /// The contrast text color for the tertiary color.
        /// </summary>
        public virtual VekColor TertiaryContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The info color.
        /// </summary>
        public virtual VekColor Info { get; set; } = Colors.Blue.Default;

        /// <summary>
        /// The contrast text color for the info color.
        /// </summary>
        public virtual VekColor InfoContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The success color.
        /// </summary>
        public virtual VekColor Success { get; set; } = Colors.Green.Accent4;

        /// <summary>
        /// The contrast text color for the success color.
        /// </summary>
        public virtual VekColor SuccessContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The warning color.
        /// </summary>
        public virtual VekColor Warning { get; set; } = Colors.Orange.Default;

        /// <summary>
        /// The contrast text color for the warning color.
        /// </summary>
        public virtual VekColor WarningContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The error color.
        /// </summary>
        public virtual VekColor Error { get; set; } = Colors.Red.Default;

        /// <summary>
        /// The contrast text color for the error color.
        /// </summary>
        public virtual VekColor ErrorContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The dark color.
        /// </summary>
        public virtual VekColor Dark { get; set; } = Colors.Gray.Darken3;

        /// <summary>
        /// The contrast text color for the dark color.
        /// </summary>
        public virtual VekColor DarkContrastText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The primary text color.
        /// </summary>
        public virtual VekColor TextPrimary { get; set; } = Colors.Gray.Darken3;

        /// <summary>
        /// The secondary text color.
        /// </summary>
        public virtual VekColor TextSecondary { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.54).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The disabled text color.
        /// </summary>
        public virtual VekColor TextDisabled { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.38).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The default action color.
        /// </summary>
        public virtual VekColor ActionDefault { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.54).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The disabled action color.
        /// </summary>
        public virtual VekColor ActionDisabled { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.26).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The background color for disabled actions.
        /// </summary>
        public virtual VekColor ActionDisabledBackground { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.12).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The background color.
        /// </summary>
        public virtual VekColor Background { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The gray background color.
        /// </summary>
        public virtual VekColor BackgroundGray { get; set; } = Colors.Gray.Lighten4;

        /// <summary>
        /// The surface color.
        /// </summary>
        public virtual VekColor Surface { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The drawer background color.
        /// </summary>
        public virtual VekColor DrawerBackground { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The drawer text color.
        /// </summary>
        public virtual VekColor DrawerText { get; set; } = Colors.Gray.Darken3;

        /// <summary>
        /// The drawer icon color.
        /// </summary>
        public virtual VekColor DrawerIcon { get; set; } = Colors.Gray.Darken2;

        /// <summary>
        /// The appbar background color.
        /// </summary>
        public virtual VekColor AppbarBackground { get; set; } = "#594AE2";

        /// <summary>
        /// The appbar text color.
        /// </summary>
        public virtual VekColor AppbarText { get; set; } = Colors.Shades.White;

        /// <summary>
        /// The default color for lines.
        /// </summary>
        public virtual VekColor LinesDefault { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.12).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The color for input lines.
        /// </summary>
        public virtual VekColor LinesInputs { get; set; } = Colors.Gray.Lighten1;

        /// <summary>
        /// The color for table lines.
        /// </summary>
        public virtual VekColor TableLines { get; set; } = new VekColor(Colors.Gray.Lighten2).SetAlpha(1.0).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The color for striped rows in a table.
        /// </summary>
        public virtual VekColor TableStriped { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.02).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The color for table rows on hover.
        /// </summary>
        public virtual VekColor TableHover { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.04).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The color for dividers.
        /// </summary>
        public virtual VekColor Divider { get; set; } = Colors.Gray.Lighten2;

        /// <summary>
        /// The light color for dividers.
        /// </summary>
        public virtual VekColor DividerLight { get; set; } = new VekColor(Colors.Shades.Black).SetAlpha(0.8).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The color for skeletons.
        /// </summary>
        public virtual VekColor Skeleton { get; set; } = new VekColor("rgba(0, 0, 0, 0.11)").ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The darkened value of the primary color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string PrimaryDarken
        {
            get => (_primaryDarken ??= Primary.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _primaryDarken = value;
        }

        /// <summary>
        /// The lightened value of the primary color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string PrimaryLighten
        {
            get => (_primaryLighten ??= Primary.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _primaryLighten = value;
        }

        /// <summary>
        /// The darkened value of the secondary color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string SecondaryDarken
        {
            get => (_secondaryDarken ??= Secondary.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _secondaryDarken = value;
        }

        /// <summary>
        /// The lightened value of the secondary color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string SecondaryLighten
        {
            get => (_secondaryLighten ??= Secondary.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _secondaryLighten = value;
        }

        /// <summary>
        /// The darkened value of the tertiary color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string TertiaryDarken
        {
            get => (_tertiaryDarken ??= Tertiary.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _tertiaryDarken = value;
        }

        /// <summary>
        /// The lightened value of the tertiary color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string TertiaryLighten
        {
            get => (_tertiaryLighten ??= Tertiary.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _tertiaryLighten = value;
        }

        /// <summary>
        /// The darkened value of the info color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string InfoDarken
        {
            get => (_infoDarken ??= Info.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _infoDarken = value;
        }

        /// <summary>
        /// The lightened value of the info color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string InfoLighten
        {
            get => (_infoLighten ??= Info.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _infoLighten = value;
        }

        /// <summary>
        /// The darkened value of the success color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string SuccessDarken
        {
            get => (_successDarken ??= Success.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _successDarken = value;
        }

        /// <summary>
        /// The lightened value of the success color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string SuccessLighten
        {
            get => (_successLighten ??= Success.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _successLighten = value;
        }

        /// <summary>
        /// The darkened value of the warning color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string WarningDarken
        {
            get => (_warningDarken ??= Warning.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _warningDarken = value;
        }

        /// <summary>
        /// The lightened value of the warning color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string WarningLighten
        {
            get => (_warningLighten ??= Warning.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _warningLighten = value;
        }

        /// <summary>
        /// The darkened value of the error color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string ErrorDarken
        {
            get => (_errorDarken ??= Error.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _errorDarken = value;
        }

        /// <summary>
        /// The lightened value of the error color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string ErrorLighten
        {
            get => (_errorLighten ??= Error.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _errorLighten = value;
        }

        /// <summary>
        /// The darkened value of the dark color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbDarken"/> if not set.
        /// </summary>
        public virtual string DarkDarken
        {
            get => (_darkDarken ??= Dark.ColorRgbDarken()).ToString(VekColorOutputFormats.RGB);
            set => _darkDarken = value;
        }

        /// <summary>
        /// The lightened value of the dark color.<br/>
        /// This is calculated using <see cref="VekColor.ColorRgbLighten"/> if not set.
        /// </summary>
        public virtual string DarkLighten
        {
            get => (_darkLighten ??= Dark.ColorRgbLighten()).ToString(VekColorOutputFormats.RGB);
            set => _darkLighten = value;
        }

        /// <summary>
        /// The opacity value for most borders.
        /// </summary>
        public virtual double BorderOpacity { get; set; } = 1.0;

        /// <summary>
        /// The opacity value for hover effect.
        /// </summary>
        public virtual double HoverOpacity { get; set; } = 0.06;

        /// <summary>
        /// The opacity for the ripple effect.
        /// </summary>
        public virtual double RippleOpacity { get; set; } = 0.1;

        /// <summary>
        /// The opacity for the ripple effect on specific elements like filled buttons.
        /// </summary>
        public virtual double RippleOpacitySecondary { get; set; } = 0.2;

        /// <summary>
        /// The default gray color.
        /// </summary>
        public virtual string GrayDefault { get; set; } = Colors.Gray.Default;

        /// <summary>
        /// The lightened gray color.
        /// </summary>
        public virtual string GrayLight { get; set; } = Colors.Gray.Lighten1;

        /// <summary>
        /// The further lightened gray color.
        /// </summary>
        public virtual string GrayLighter { get; set; } = Colors.Gray.Lighten2;

        /// <summary>
        /// The darkened gray color.
        /// </summary>
        public virtual string GrayDark { get; set; } = Colors.Gray.Darken1;

        /// <summary>
        /// The further darkened gray color.
        /// </summary>
        public virtual string GrayDarker { get; set; } = Colors.Gray.Darken2;

        /// <summary>
        /// The dark overlay color.
        /// </summary>
        public virtual string OverlayDark { get; set; } = new VekColor("#212121").SetAlpha(0.5).ToString(VekColorOutputFormats.RGBA);

        /// <summary>
        /// The light overlay color.
        /// </summary>
        public virtual string OverlayLight { get; set; } = new VekColor(Colors.Shades.White).SetAlpha(0.5).ToString(VekColorOutputFormats.RGBA);
    }
}
