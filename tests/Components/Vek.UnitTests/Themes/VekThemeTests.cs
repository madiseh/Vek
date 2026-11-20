// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Vek.UnitTests.Dummy;
using Vek.Utilities;
using NUnit.Framework;

namespace Vek.UnitTests.Themes;

#nullable enable
[TestFixture]
public class VekThemeTests
{
    [Test]
    public void VekTheme_STJ_SourceGen_Serialization()
    {
        var originalVekTheme = new VekTheme
        {
            ZIndex = new ZIndex
            {
                Drawer = 5000
            }
        };

        var vekThemeType = typeof(VekTheme);
        var context = VekThemeSerializerContext.Default;

        var jsonString = System.Text.Json.JsonSerializer.Serialize(originalVekTheme, vekThemeType, context);
        var deserializeVekTheme = (VekTheme)System.Text.Json.JsonSerializer.Deserialize(jsonString, vekThemeType, context)!;

        deserializeVekTheme.ZIndex.Drawer.Should().Be(originalVekTheme.ZIndex.Drawer);
        deserializeVekTheme.Should().NotBeSameAs(originalVekTheme, "Objects have same values, but instances are different and has on custom Equals");
    }

    [Test]
    public void VekTheme_STJ_SourceGen_PreservesPolymorphicPaletteTypeAndColors()
    {
        // Palette is abstract with [JsonDerivedType] discriminators; round-trip must keep
        // the concrete subtype and the custom VekColor value.
        var originalVekTheme = new VekTheme
        {
            PaletteLight = new PaletteLight { Primary = "#112233" },
            PaletteDark = new PaletteDark { Primary = "#abcdef" }
        };

        var vekThemeType = typeof(VekTheme);
        var context = VekThemeSerializerContext.Default;

        var jsonString = System.Text.Json.JsonSerializer.Serialize(originalVekTheme, vekThemeType, context);
        var deserializeVekTheme = (VekTheme)System.Text.Json.JsonSerializer.Deserialize(jsonString, vekThemeType, context)!;

        deserializeVekTheme.PaletteLight.Should().BeOfType<PaletteLight>();
        deserializeVekTheme.PaletteDark.Should().BeOfType<PaletteDark>();
        deserializeVekTheme.PaletteLight.Primary.Should().Be(new VekColor("#112233"));
        deserializeVekTheme.PaletteDark.Primary.Should().Be(new VekColor("#abcdef"));
    }

    [Test]
    public void VekTheme_STJ_SourceGen_PreservesExplicitlySetDarken()
    {
        // PrimaryDarken is normally computed from Primary, but an explicitly set value
        // must survive the round-trip rather than reverting to the computed default.
        var explicitDarken = new VekColor("#010203").ToString(VekColorOutputFormats.RGB);
        var originalVekTheme = new VekTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#594AE2",
                PrimaryDarken = explicitDarken
            }
        };

        var vekThemeType = typeof(VekTheme);
        var context = VekThemeSerializerContext.Default;

        var jsonString = System.Text.Json.JsonSerializer.Serialize(originalVekTheme, vekThemeType, context);
        var deserializeVekTheme = (VekTheme)System.Text.Json.JsonSerializer.Deserialize(jsonString, vekThemeType, context)!;

        deserializeVekTheme.PaletteLight.PrimaryDarken.Should().Be(explicitDarken);
        // Sanity: explicit value differs from what Primary would compute.
        deserializeVekTheme.PaletteLight.PrimaryDarken.Should().NotBe(new VekColor("#594AE2").ColorRgbDarken().ToString(VekColorOutputFormats.RGB));
    }
}
