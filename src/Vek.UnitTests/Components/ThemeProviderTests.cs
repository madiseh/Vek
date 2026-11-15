using System.Globalization;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Vek.Extensions;
using Vek.UnitTests.TestComponents.ThemeProvider;
using Vek.Utilities;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
#nullable enable
    [TestFixture]
    public class ThemeProviderTests : BunitTest
    {
        [Test]
        [TestCase("en-us")]
        [TestCase("de-DE")]
        [TestCase("he-IL")]
        [TestCase("ar-ER")]
        public void DifferentCultures(string cultureString)
        {
            var culture = new CultureInfo(cultureString, false);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            var comp = Context.Render<VekThemeProvider>();

            var styleNodes = comp.Nodes.OfType<IHtmlStyleElement>().ToArray();
            styleNodes.Should().HaveCount(3);

            var rootStyleNode = styleNodes[2];

            var expectedLines = new[] {
                ":root{",
                "--vek-palette-black: rgba(39,44,52,1);",
                "--vek-palette-white: rgba(255,255,255,1);",
                "--vek-palette-primary: rgba(89,74,226,1);",
                "--vek-palette-primary-rgb: 89,74,226;",
                "--vek-palette-primary-text: rgba(255,255,255,1);",
                "--vek-palette-primary-darken: rgb(62,44,221);",
                "--vek-palette-primary-lighten: rgb(118,106,231);",
                "--vek-palette-primary-hover: rgba(89,74,226,0.058823529411764705);",
                "--vek-palette-secondary: rgba(255,64,129,1);",
                "--vek-palette-secondary-rgb: 255,64,129;",
                "--vek-palette-secondary-text: rgba(255,255,255,1);",
                "--vek-palette-secondary-darken: rgb(255,31,105);",
                "--vek-palette-secondary-lighten: rgb(255,102,153);",
                "--vek-palette-secondary-hover: rgba(255,64,129,0.058823529411764705);",
                "--vek-palette-tertiary: rgba(30,200,165,1);",
                "--vek-palette-tertiary-rgb: 30,200,165;",
                "--vek-palette-tertiary-text: rgba(255,255,255,1);",
                "--vek-palette-tertiary-darken: rgb(25,169,140);",
                "--vek-palette-tertiary-lighten: rgb(42,223,187);",
                "--vek-palette-tertiary-hover: rgba(30,200,165,0.058823529411764705);",
                "--vek-palette-info: rgba(33,150,243,1);",
                "--vek-palette-info-rgb: 33,150,243;",
                "--vek-palette-info-text: rgba(255,255,255,1);",
                "--vek-palette-info-darken: rgb(12,128,223);",
                "--vek-palette-info-lighten: rgb(71,167,245);",
                "--vek-palette-info-hover: rgba(33,150,243,0.058823529411764705);",
                "--vek-palette-success: rgba(0,200,83,1);",
                "--vek-palette-success-rgb: 0,200,83;",
                "--vek-palette-success-text: rgba(255,255,255,1);",
                "--vek-palette-success-darken: rgb(0,163,68);",
                "--vek-palette-success-lighten: rgb(0,235,98);",
                "--vek-palette-success-hover: rgba(0,200,83,0.058823529411764705);",
                "--vek-palette-warning: rgba(255,152,0,1);",
                "--vek-palette-warning-rgb: 255,152,0;",
                "--vek-palette-warning-text: rgba(255,255,255,1);",
                "--vek-palette-warning-darken: rgb(214,129,0);",
                "--vek-palette-warning-lighten: rgb(255,167,36);",
                "--vek-palette-warning-hover: rgba(255,152,0,0.058823529411764705);",
                "--vek-palette-error: rgba(244,67,54,1);",
                "--vek-palette-error-rgb: 244,67,54;",
                "--vek-palette-error-text: rgba(255,255,255,1);",
                "--vek-palette-error-darken: rgb(242,28,13);",
                "--vek-palette-error-lighten: rgb(246,96,85);",
                "--vek-palette-error-hover: rgba(244,67,54,0.058823529411764705);",
                "--vek-palette-dark: rgba(66,66,66,1);",
                "--vek-palette-dark-rgb: 66,66,66;",
                "--vek-palette-dark-text: rgba(255,255,255,1);",
                "--vek-palette-dark-darken: rgb(46,46,46);",
                "--vek-palette-dark-lighten: rgb(87,87,87);",
                "--vek-palette-dark-hover: rgba(66,66,66,0.058823529411764705);",
                "--vek-palette-text-primary: rgba(66,66,66,1);",
                "--vek-palette-text-primary-rgb: 66,66,66;",
                "--vek-palette-text-secondary: rgba(0,0,0,0.5372549019607843);",
                "--vek-palette-text-secondary-rgb: 0,0,0;",
                "--vek-palette-text-disabled: rgba(0,0,0,0.3764705882352941);",
                "--vek-palette-text-disabled-rgb: 0,0,0;",
                "--vek-palette-action-default: rgba(0,0,0,0.5372549019607843);",
                "--vek-palette-action-default-hover: rgba(0,0,0,0.058823529411764705);",
                "--vek-palette-action-disabled: rgba(0,0,0,0.25882352941176473);",
                "--vek-palette-action-disabled-background: rgba(0,0,0,0.11764705882352941);",
                "--vek-palette-surface: rgba(255,255,255,1);",
                "--vek-palette-surface-rgb: 255,255,255;",
                "--vek-palette-background: rgba(255,255,255,1);",
                "--vek-palette-background-gray: rgba(245,245,245,1);",
                "--vek-palette-drawer-background: rgba(255,255,255,1);",
                "--vek-palette-drawer-text: rgba(66,66,66,1);",
                "--vek-palette-drawer-icon: rgba(97,97,97,1);",
                "--vek-palette-appbar-background: rgba(89,74,226,1);",
                "--vek-palette-appbar-text: rgba(255,255,255,1);",
                "--vek-palette-lines-default: rgba(0,0,0,0.11764705882352941);",
                "--vek-palette-lines-inputs: rgba(189,189,189,1);",
                "--vek-palette-table-lines: rgba(224,224,224,1);",
                "--vek-palette-table-striped: rgba(0,0,0,0.0196078431372549);",
                "--vek-palette-table-hover: rgba(0,0,0,0.0392156862745098);",
                "--vek-palette-divider: rgba(224,224,224,1);",
                "--vek-palette-divider-rgb: 224,224,224;",
                "--vek-palette-divider-light: rgba(0,0,0,0.8);",
                "--vek-palette-skeleton: rgba(0,0,0,0.10980392156862745);",
                "--vek-palette-gray-default: #9E9E9E;",
                "--vek-palette-gray-light: #BDBDBD;",
                "--vek-palette-gray-lighter: #E0E0E0;",
                "--vek-palette-gray-dark: #757575;",
                "--vek-palette-gray-darker: #616161;",
                "--vek-palette-overlay-dark: rgba(33,33,33,0.4980392156862745);",
                "--vek-palette-overlay-light: rgba(255,255,255,0.4980392156862745);",
                "--vek-palette-border-opacity: 1;",
                "--vek-ripple-color: var(--vek-palette-text-primary);",
                "--vek-ripple-opacity: 0.1;",
                "--vek-ripple-opacity-secondary: 0.2;",
                "--vek-elevation-0: none;",
                "--vek-elevation-1: 0px 2px 1px -1px rgba(0,0,0,0.2),0px 1px 1px 0px rgba(0,0,0,0.14),0px 1px 3px 0px rgba(0,0,0,0.12);",
                "--vek-elevation-2: 0px 3px 1px -2px rgba(0,0,0,0.2),0px 2px 2px 0px rgba(0,0,0,0.14),0px 1px 5px 0px rgba(0,0,0,0.12);",
                "--vek-elevation-3: 0px 3px 3px -2px rgba(0,0,0,0.2),0px 3px 4px 0px rgba(0,0,0,0.14),0px 1px 8px 0px rgba(0,0,0,0.12);",
                "--vek-elevation-4: 0px 2px 4px -1px rgba(0,0,0,0.2),0px 4px 5px 0px rgba(0,0,0,0.14),0px 1px 10px 0px rgba(0,0,0,0.12);",
                "--vek-elevation-5: 0px 3px 5px -1px rgba(0,0,0,0.2),0px 5px 8px 0px rgba(0,0,0,0.14),0px 1px 14px 0px rgba(0,0,0,0.12);",
                "--vek-elevation-6: 0px 3px 5px -1px rgba(0,0,0,0.2),0px 6px 10px 0px rgba(0,0,0,0.14),0px 1px 18px 0px rgba(0,0,0,0.12);",
                "--vek-elevation-7: 0px 4px 5px -2px rgba(0,0,0,0.2),0px 7px 10px 1px rgba(0,0,0,0.14),0px 2px 16px 1px rgba(0,0,0,0.12);",
                "--vek-elevation-8: 0px 5px 5px -3px rgba(0,0,0,0.2),0px 8px 10px 1px rgba(0,0,0,0.14),0px 3px 14px 2px rgba(0,0,0,0.12);",
                "--vek-elevation-9: 0px 5px 6px -3px rgba(0,0,0,0.2),0px 9px 12px 1px rgba(0,0,0,0.14),0px 3px 16px 2px rgba(0,0,0,0.12);",
                "--vek-elevation-10: 0px 6px 6px -3px rgba(0,0,0,0.2),0px 10px 14px 1px rgba(0,0,0,0.14),0px 4px 18px 3px rgba(0,0,0,0.12);",
                "--vek-elevation-11: 0px 6px 7px -4px rgba(0,0,0,0.2),0px 11px 15px 1px rgba(0,0,0,0.14),0px 4px 20px 3px rgba(0,0,0,0.12);",
                "--vek-elevation-12: 0px 7px 8px -4px rgba(0,0,0,0.2),0px 12px 17px 2px rgba(0,0,0,0.14),0px 5px 22px 4px rgba(0,0,0,0.12);",
                "--vek-elevation-13: 0px 7px 8px -4px rgba(0,0,0,0.2),0px 13px 19px 2px rgba(0,0,0,0.14),0px 5px 24px 4px rgba(0,0,0,0.12);",
                "--vek-elevation-14: 0px 7px 9px -4px rgba(0,0,0,0.2),0px 14px 21px 2px rgba(0,0,0,0.14),0px 5px 26px 4px rgba(0,0,0,0.12);",
                "--vek-elevation-15: 0px 8px 9px -5px rgba(0,0,0,0.2),0px 15px 22px 2px rgba(0,0,0,0.14),0px 6px 28px 5px rgba(0,0,0,0.12);",
                "--vek-elevation-16: 0px 8px 10px -5px rgba(0,0,0,0.2),0px 16px 24px 2px rgba(0,0,0,0.14),0px 6px 30px 5px rgba(0,0,0,0.12);",
                "--vek-elevation-17: 0px 8px 11px -5px rgba(0,0,0,0.2),0px 17px 26px 2px rgba(0,0,0,0.14),0px 6px 32px 5px rgba(0,0,0,0.12);",
                "--vek-elevation-18: 0px 9px 11px -5px rgba(0,0,0,0.2),0px 18px 28px 2px rgba(0,0,0,0.14),0px 7px 34px 6px rgba(0,0,0,0.12);",
                "--vek-elevation-19: 0px 9px 12px -6px rgba(0,0,0,0.2),0px 19px 29px 2px rgba(0,0,0,0.14),0px 7px 36px 6px rgba(0,0,0,0.12);",
                "--vek-elevation-20: 0px 10px 13px -6px rgba(0,0,0,0.2),0px 20px 31px 3px rgba(0,0,0,0.14),0px 8px 38px 7px rgba(0,0,0,0.12);",
                "--vek-elevation-21: 0px 10px 13px -6px rgba(0,0,0,0.2),0px 21px 33px 3px rgba(0,0,0,0.14),0px 8px 40px 7px rgba(0,0,0,0.12);",
                "--vek-elevation-22: 0px 10px 14px -6px rgba(0,0,0,0.2),0px 22px 35px 3px rgba(0,0,0,0.14),0px 8px 42px 7px rgba(0,0,0,0.12);",
                "--vek-elevation-23: 0px 11px 14px -7px rgba(0,0,0,0.2),0px 23px 36px 3px rgba(0,0,0,0.14),0px 9px 44px 8px rgba(0,0,0,0.12);",
                "--vek-elevation-24: 0px 11px 15px -7px rgba(0,0,0,0.2),0px 24px 38px 3px rgba(0,0,0,0.14),0px 9px 46px 8px rgba(0,0,0,0.12);",
                "--vek-elevation-25: 0 5px 5px -3px rgba(0,0,0,.06), 0 8px 10px 1px rgba(0,0,0,.042), 0 3px 14px 2px rgba(0,0,0,.036);",
                "--vek-default-borderradius: 4px;",
                "--vek-drawer-width-left: 240px;",
                "--vek-drawer-width-right: 240px;",
                "--vek-drawer-width-mini-left: 56px;",
                "--vek-drawer-width-mini-right: 56px;",
                "--vek-appbar-height: 64px;",
                "--vek-typography-default-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-default-size: .875rem;",
                "--vek-typography-default-weight: 400;",
                "--vek-typography-default-lineheight: 1.43;",
                "--vek-typography-default-letterspacing: .01071em;",
                "--vek-typography-default-text-transform: none;",
                "--vek-typography-h1-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-h1-size: 6rem;",
                "--vek-typography-h1-weight: 300;",
                "--vek-typography-h1-lineheight: 1.167;",
                "--vek-typography-h1-letterspacing: -.01562em;",
                "--vek-typography-h1-text-transform: none;",
                "--vek-typography-h2-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-h2-size: 3.75rem;",
                "--vek-typography-h2-weight: 300;",
                "--vek-typography-h2-lineheight: 1.2;",
                "--vek-typography-h2-letterspacing: -.00833em;",
                "--vek-typography-h2-text-transform: none;",
                "--vek-typography-h3-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-h3-size: 3rem;",
                "--vek-typography-h3-weight: 400;",
                "--vek-typography-h3-lineheight: 1.167;",
                "--vek-typography-h3-letterspacing: 0;",
                "--vek-typography-h3-text-transform: none;",
                "--vek-typography-h4-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-h4-size: 2.125rem;",
                "--vek-typography-h4-weight: 400;",
                "--vek-typography-h4-lineheight: 1.235;",
                "--vek-typography-h4-letterspacing: .00735em;",
                "--vek-typography-h4-text-transform: none;",
                "--vek-typography-h5-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-h5-size: 1.5rem;",
                "--vek-typography-h5-weight: 400;",
                "--vek-typography-h5-lineheight: 1.334;",
                "--vek-typography-h5-letterspacing: 0;",
                "--vek-typography-h5-text-transform: none;",
                "--vek-typography-h6-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-h6-size: 1.25rem;",
                "--vek-typography-h6-weight: 500;",
                "--vek-typography-h6-lineheight: 1.6;",
                "--vek-typography-h6-letterspacing: .0075em;",
                "--vek-typography-h6-text-transform: none;",
                "--vek-typography-subtitle1-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-subtitle1-size: 1rem;",
                "--vek-typography-subtitle1-weight: 400;",
                "--vek-typography-subtitle1-lineheight: 1.75;",
                "--vek-typography-subtitle1-letterspacing: .00938em;",
                "--vek-typography-subtitle1-text-transform: none;",
                "--vek-typography-subtitle2-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-subtitle2-size: .875rem;",
                "--vek-typography-subtitle2-weight: 500;",
                "--vek-typography-subtitle2-lineheight: 1.57;",
                "--vek-typography-subtitle2-letterspacing: .00714em;",
                "--vek-typography-subtitle2-text-transform: none;",
                "--vek-typography-body1-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-body1-size: 1rem;",
                "--vek-typography-body1-weight: 400;",
                "--vek-typography-body1-lineheight: 1.5;",
                "--vek-typography-body1-letterspacing: .00938em;",
                "--vek-typography-body1-text-transform: none;",
                "--vek-typography-body2-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-body2-size: .875rem;",
                "--vek-typography-body2-weight: 400;",
                "--vek-typography-body2-lineheight: 1.43;",
                "--vek-typography-body2-letterspacing: .01071em;",
                "--vek-typography-body2-text-transform: none;",
                "--vek-typography-button-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-button-size: .875rem;",
                "--vek-typography-button-weight: 500;",
                "--vek-typography-button-lineheight: 1.75;",
                "--vek-typography-button-letterspacing: .02857em;",
                "--vek-typography-button-text-transform: uppercase;",
                "--vek-typography-caption-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-caption-size: .75rem;",
                "--vek-typography-caption-weight: 400;",
                "--vek-typography-caption-lineheight: 1.66;",
                "--vek-typography-caption-letterspacing: .03333em;",
                "--vek-typography-caption-text-transform: none;",
                "--vek-typography-overline-family: Roboto, Helvetica, Arial, sans-serif;",
                "--vek-typography-overline-size: .75rem;",
                "--vek-typography-overline-weight: 400;",
                "--vek-typography-overline-lineheight: 2.66;",
                "--vek-typography-overline-letterspacing: .08333em;",
                "--vek-typography-overline-text-transform: none;",
                "--vek-zindex-drawer: 1100;",
                "--vek-zindex-appbar: 1300;",
                "--vek-zindex-dialog: 1400;",
                "--vek-zindex-popover: 1200;",
                "--vek-zindex-snackbar: 1500;",
                "--vek-zindex-tooltip: 1600;",
                "--vek-native-html-color-scheme: light;",
                "}"
            };

            var styleLines = rootStyleNode.InnerHtml.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            styleLines.Should().BeEquivalentTo(expectedLines);
        }

        [Test]
        public void IsDarkMode()
        {
            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.IsDarkMode, true));
            comp.Should().NotBeNull();
            comp.Instance.GetState(x => x.IsDarkMode).Should().BeTrue();
        }

        [Test]
        public void CustomThemeDarkMode()
        {
            var myCustomTheme = new VekTheme
            {
                PaletteDark = new PaletteDark
                {
                    Primary = Colors.Blue.Lighten1,
                    Secondary = "#F50057"
                }
            };
            myCustomTheme.PaletteDark.Primary.Should().Be(new VekColor(Colors.Blue.Lighten1));// Set by user
            myCustomTheme.PaletteDark.Error.Should().Be(new VekColor("#f64e62"));// Default dark overwritten from light
            myCustomTheme.PaletteDark.White.Should().Be(new VekColor(Colors.Shades.White));// Equal in dark and light.
            myCustomTheme.PaletteDark.Secondary.Should().Be(new VekColor("#F50057"));// Setting not in PaletteDark()
        }

        [Test]
        public void CustomThemeDarkModePrimaryDerivateColor()
        {
            // ensure it is backwards compatible by setting Palette() instead of PaletteDark()
            var myCustomTheme = new VekTheme()
            {
                PaletteDark = new PaletteDark()
                {
                    Primary = Colors.Green.Darken1,
                }
            };
            var expectedDarkerColor = new VekColor(Colors.Green.Darken1).ColorRgbDarken();
            myCustomTheme.PaletteDark.Primary.Should().Be(new VekColor(Colors.Green.Darken1));// Set by user
            myCustomTheme.PaletteDark.PrimaryDarken.Should().Be(expectedDarkerColor.ToString(VekColorOutputFormats.RGB));// Set by user

        }

        [Test]
        public void CustomThemeDefault()
        {
            var defaultTheme = new VekTheme();

            //Dark theme
            defaultTheme.PaletteDark.Should().BeOfType<PaletteDark>();
            defaultTheme.PaletteDark.Primary.Should().Be(new VekColor("#776be7"));
            defaultTheme.PaletteDark.Error.Should().Be(new VekColor("#f64e62"));
            defaultTheme.PaletteDark.White.Should().Be(new VekColor(Colors.Shades.White));

            //Light theme
            // Note we're testing against the base type
            defaultTheme.PaletteLight.Should().BeAssignableTo<Palette>();
            defaultTheme.PaletteLight.Primary.Should().Be(new VekColor("#594AE2"));
            defaultTheme.PaletteLight.Error.Should().Be(new VekColor(Colors.Red.Default));
            defaultTheme.PaletteLight.White.Should().Be(new VekColor(Colors.Shades.White));
        }

        [Test]
        public async Task WatchSystemDarkMode()
        {
            var systemMockValue = false;
            Task SystemChangedResult(bool newValue)
            {
                systemMockValue = newValue;
                return Task.CompletedTask;
            }
            var comp = Context.Render<VekThemeProvider>();
            await comp.Instance.WatchSystemDarkModeAsync(SystemChangedResult);
            await comp.Instance.SystemDarkModeChangedAsync(true);
            systemMockValue.Should().BeTrue();
        }

        [Test]
        [TestCase("")]
        [TestCase("root")]
        [TestCase("host")]
        [TestCase(":root")]
        [TestCase(":host")]
        public void PseudoCssScope(string scope)
        {
            var vekTheme = new VekTheme
            {
                PseudoCss = new PseudoCss
                {
                    Scope = scope
                }
            };
            var comp = Context.Render<VekThemeProvider>(parameters => parameters.Add(p => p.Theme, vekTheme));
            comp.Should().NotBeNull();

            var styleNodes = comp.Nodes.OfType<IHtmlStyleElement>().ToArray();

            var rootStyleNode = styleNodes[2];

            var styleLines = rootStyleNode.InnerHtml.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (string.IsNullOrEmpty(scope))
            {
                scope = ":root";
            }

            if (!scope.StartsWith(':'))
            {
                scope = $":{scope}";
            }

            styleLines.Should().Contain($"{scope}{{");
        }

        [Test]
        public void PseudoCssRootColor()
        {
            const string Scope = ":root";
            var vekTheme = new VekTheme
            {
                PaletteDark = new PaletteDark
                {
                    Primary = Colors.Green.Darken1,
                },
                PseudoCss = new PseudoCss
                {
                    Scope = Scope
                }
            };
            var comp = Context.Render<VekThemeProvider>(
                parameters =>
                    parameters.Add(p => p.Theme, vekTheme)
                        .Add(p => p.IsDarkMode, true)
            );
            comp.Should().NotBeNull();

            var styleNodes = comp.Nodes.OfType<IHtmlStyleElement>().ToArray();

            var rootStyleNode = styleNodes[2];

            var styleLines = rootStyleNode.InnerHtml.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            styleLines.Should().Contain($"{Scope}{{");

            var expectedPrimaryColor = Colors.Green.Darken1;
            var expectedPrimaryColorAsRgba = new VekColor(expectedPrimaryColor).ToString(VekColorOutputFormats.RGBA);
            var expectedPrimaryLine = $"--vek-palette-primary: {expectedPrimaryColorAsRgba};";
            styleLines.Should().Contain(expectedPrimaryLine);
            var expectedPrimaryDarkenColor = new VekColor(expectedPrimaryColor).ColorRgbDarken();
            var expectedPrimaryDarkenColorAsRgb = expectedPrimaryDarkenColor.ToString(VekColorOutputFormats.RGB);
            var expectedPrimaryDarkenLine = $"--vek-palette-primary-darken: {expectedPrimaryDarkenColorAsRgb};";
            styleLines.Should().Contain(expectedPrimaryDarkenLine);
        }

        [Test]
        public async Task ObserveSystemDarkModeChange()
        {
            // Arrange & Act
            Context.JSInterop.SetupVoid("vekThemeProvider.stopWatchingDarkMode");
            Context.JSInterop.SetupVoid("vekThemeProvider.watchDarkMode");
            var themeProvider = Context.Render<ThemeProviderObserveSystemDarkModeChangeTest>();

            // Assert
            Context.JSInterop.VerifyNotInvoke("vekThemeProvider.watchDarkMode");
            Context.JSInterop.VerifyNotInvoke("vekThemeProvider.stopWatchingDarkMode");

            // Act
            await themeProvider.InvokeAsync(themeProvider.Instance.EnableObserve);

            // Assert
            Context.JSInterop.VerifyInvoke("vekThemeProvider.watchDarkMode", 1);
            Context.JSInterop.VerifyNotInvoke("vekThemeProvider.stopWatchingDarkMode");

            // Act
            await themeProvider.InvokeAsync(themeProvider.Instance.DisableObserve);

            // Assert
            Context.JSInterop.VerifyInvoke("vekThemeProvider.watchDarkMode", 1);
            Context.JSInterop.VerifyInvoke("vekThemeProvider.stopWatchingDarkMode", 1);
        }

        [Test]
        public async Task Dispose_ShouldInvokeJs()
        {
            // Arrange
            Context.JSInterop.SetupVoid("vekThemeProvider.stopWatchingDarkMode");
            Context.Render<VekThemeProvider>();

            //Act
            await Context.DisposeComponentsAsync();

            // Assert
            Context.JSInterop.VerifyInvoke("vekThemeProvider.stopWatchingDarkMode");
        }

        [Test]
        public void RenderComponent_ShouldInvokeJs()
        {
            // Act & Arrange
            Context.JSInterop.SetupVoid("vekThemeProvider.watchDarkMode");
            Context.Render<VekThemeProvider>();

            // Assert
            Context.JSInterop.VerifyInvoke("vekThemeProvider.watchDarkMode");
        }

        [Test]
        public void ThemeProvider_ShouldHave_ClassName()
        {
            const string Scope = ":root";
            var vekTheme = new VekTheme
            {
                PaletteDark = new PaletteDark
                {
                    Primary = Colors.Green.Darken1,
                },
                PseudoCss = new PseudoCss
                {
                    Scope = Scope
                }
            };
            var comp = Context.Render<VekThemeProvider>(
                parameters =>
                    parameters.Add(p => p.Theme, vekTheme)
                        .Add(p => p.IsDarkMode, true)
            );
            comp.Should().NotBeNull();

            var styleNodes = comp.Nodes.OfType<IHtmlStyleElement>().ToArray();

            var rootStyleNode = styleNodes[2];
            rootStyleNode.ClassName.Should().Be("vek-theme-provider");
        }

        [Test]
        public void CurrentPalette_ShouldBeLight_WhenNotInDarkMode()
        {
            // Arrange & Act
            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.IsDarkMode, false));

            // Assert
            comp.Instance.GetState(x => x.CurrentPalette).Should().NotBeNull();
            comp.Instance.GetState(x => x.CurrentPalette).Should().BeOfType<PaletteLight>();
        }

        [Test]
        public void CurrentPalette_ShouldBeDark_WhenInDarkMode()
        {
            // Arrange & Act
            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.IsDarkMode, true));

            // Assert
            comp.Instance.GetState(x => x.CurrentPalette).Should().NotBeNull();
            comp.Instance.GetState(x => x.CurrentPalette).Should().BeOfType<PaletteDark>();
        }

        [Test]
        public async Task CurrentPalette_ShouldUpdateToLight_WhenDarkModeChangesToFalse()
        {
            // Arrange
            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.IsDarkMode, true));

            // Verify initial state
            comp.Instance.GetState(x => x.CurrentPalette).Should().BeOfType<PaletteDark>();

            // Act
            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.IsDarkMode, false));

            // Assert
            comp.Instance.GetState(x => x.CurrentPalette).Should().BeOfType<PaletteLight>();
        }

        [Test]
        public async Task CurrentPalette_ShouldUpdateToDark_WhenDarkModeChangesToTrue()
        {
            // Arrange
            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.IsDarkMode, false));

            // Verify initial state
            comp.Instance.GetState(x => x.CurrentPalette).Should().BeOfType<PaletteLight>();

            // Act
            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.IsDarkMode, true));

            // Assert
            comp.Instance.GetState(x => x.CurrentPalette).Should().BeOfType<PaletteDark>();
        }

        [Test]
        public void CurrentPalette_ShouldUseCustomTheme_WhenProvided()
        {
            // Arrange
            var customTheme = new VekTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = Colors.Green.Default
                }
            };

            // Act
            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.Theme, customTheme)
                .Add(p => p.IsDarkMode, false));

            // Assert
            var currentPalette = comp.Instance.GetState(x => x.CurrentPalette);
            currentPalette.Should().NotBeNull();
            currentPalette!.Primary.Should().Be(new VekColor(Colors.Green.Default));
        }

        [Test]
        public async Task CurrentPaletteChanged_ShouldFire_WhenDarkModeChanges()
        {
            // Arrange
            Palette? capturedPalette = null;
            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.IsDarkMode, false)
                .Add(p => p.CurrentPaletteChanged, EventCallback.Factory.Create<Palette?>(this, palette => capturedPalette = palette)));

            // Act
            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.IsDarkMode, true));

            // Assert
            capturedPalette.Should().NotBeNull();
            capturedPalette.Should().BeOfType<PaletteDark>();
        }

        [Test]
        public async Task CurrentPalette_ShouldUpdate_WhenThemeChanges()
        {
            // Arrange
            var theme1 = new VekTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = Colors.Blue.Default
                }
            };

            var theme2 = new VekTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = Colors.Red.Default
                }
            };

            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.Theme, theme1)
                .Add(p => p.IsDarkMode, false));

            // Verify initial state
            comp.Instance.GetState(x => x.CurrentPalette)!.Primary.Should().Be(new VekColor(Colors.Blue.Default));

            // Act
            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.Theme, theme2));

            // Assert
            comp.Instance.GetState(x => x.CurrentPalette)!.Primary.Should().Be(new VekColor(Colors.Red.Default));
        }

        [Test]
        public async Task CurrentPalette_ShouldReflectBothDarkModeAndThemeChanges()
        {
            // Arrange
            var customTheme = new VekTheme
            {
                PaletteLight = new PaletteLight
                {
                    Primary = Colors.Blue.Default
                },
                PaletteDark = new PaletteDark
                {
                    Primary = Colors.Green.Default
                }
            };

            var comp = Context.Render<VekThemeProvider>(parameters => parameters
                .Add(p => p.Theme, customTheme)
                .Add(p => p.IsDarkMode, false));

            // Verify light mode
            comp.Instance.GetState(x => x.CurrentPalette)!.Primary.Should().Be(new VekColor(Colors.Blue.Default));

            // Act - switch to dark mode
            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.IsDarkMode, true));

            // Assert - should use dark palette
            comp.Instance.GetState(x => x.CurrentPalette)!.Primary.Should().Be(new VekColor(Colors.Green.Default));
        }

        [Test]
        public async Task CurrentPalette_Binding_ShouldWorkInTestComponent()
        {
            // Arrange
            var comp = Context.Render<ThemeProviderCurrentPaletteTest>();

            // Initial state - light mode
            comp.Instance.GetCurrentPalette().Should().BeOfType<PaletteLight>();

            // Act - toggle to dark mode
            await comp.InvokeAsync(() => comp.Find("button").Click());

            // Assert - should be dark palette
            comp.Instance.GetCurrentPalette().Should().BeOfType<PaletteDark>();

            // Act - toggle back to light mode
            await comp.InvokeAsync(() => comp.Find("button").Click());

            // Assert - should be light palette again
            comp.Instance.GetCurrentPalette().Should().BeOfType<PaletteLight>();
        }

        [Test]
        public void FirstRender_WithoutScript_LogsGuidance()
        {
            // window.vekElementRef missing means Vek.min.js never loaded; interop degrades silently, so surface the one actionable hint.
            var loggerMock = new Mock<ILogger<VekThemeProvider>>();
            Context.Services.AddSingleton(loggerMock.Object);
            Context.JSInterop.Setup<bool>("window.hasOwnProperty", "vekElementRef").SetResult(false);

            Context.Render<VekThemeProvider>();

            loggerMock.VerifyLogging(VekThemeProvider.MissingScriptMessage, LogLevel.Error, Times.Once());
        }

        [Test]
        public void FirstRender_WithScript_DoesNotLog()
        {
            var loggerMock = new Mock<ILogger<VekThemeProvider>>();
            Context.Services.AddSingleton(loggerMock.Object);
            Context.JSInterop.Setup<bool>("window.hasOwnProperty", "vekElementRef").SetResult(true);

            Context.Render<VekThemeProvider>();

            loggerMock.VerifyLogging(VekThemeProvider.MissingScriptMessage, LogLevel.Error, Times.Never());
        }
    }
}
