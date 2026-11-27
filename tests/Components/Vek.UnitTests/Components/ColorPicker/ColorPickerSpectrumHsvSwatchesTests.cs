#nullable enable
using System.Globalization;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek.Utilities;

namespace Vek.UnitTests.Components.ColorPicker;

[TestFixture]
public class ColorPickerSpectrumHsvSwatchesTests : BunitTest
{
    [Test]
    public async Task HsvMode_RendersAndUpdatesChannelInputsAndValue()
    {
        VekColor? boundValue = new("#3b82f6ff");
        var mode = ColorPickerMode.HSV;

        var comp = Context.Render<VekColorPicker>(p => p
            .Add(x => x.PickerVariant, PickerVariant.Static)
            .Add(x => x.ColorPickerMode, mode)
            .Add(x => x.ColorPickerModeChanged, m => mode = m)
            .Add(x => x.Value, boundValue)
            .Add(x => x.ValueChanged, v => boundValue = v));

        var inputs = comp.FindAll(".vek-picker-color-inputs input").Cast<IHtmlInputElement>().ToArray();
        inputs.Should().HaveCount(4);

        var (expectedH, expectedS, expectedV) = boundValue.ToHsv();
        inputs[0].Value.Should().Be(expectedH.ToString(CultureInfo.CurrentUICulture));
        inputs[1].Value.Should().Be(expectedS.ToString(CultureInfo.CurrentUICulture));
        inputs[2].Value.Should().Be(expectedV.ToString(CultureInfo.CurrentUICulture));
        inputs[3].Value.Should().Be("1");

        // Update HSV Value (brightness) to 0.50 and verify bound color updates cleanly.
        await inputs[2].ChangeAsync("0.5");
        boundValue.Should().NotBeNull();
        var updatedHsv = boundValue!.ToHsv();
        updatedHsv.V.Should().Be(0.5);
        updatedHsv.H.Should().Be(expectedH);

        // Cycle mode switch from HSV back to RGB and confirm ColorPickerModeChanged fires.
        await comp.Find(".vek-picker-control-switch button").ClickAsync();
        mode.Should().Be(ColorPickerMode.RGB);
    }

    [Test]
    public void VekColor_HsvConversionsAndParsing_HandleEdgeCases()
    {
        var pureRed = VekColor.FromHsv(0, 1.0, 1.0);
        pureRed.Should().Be(new VekColor(255, 0, 0, 255));
        pureRed.ToHsv().Should().Be((0, 1.0, 1.0));

        var blackWithHue = VekColor.FromHsv(210, 0.8, 0.0);
        blackWithHue.R.Should().Be(0);
        blackWithHue.G.Should().Be(0);
        blackWithHue.B.Should().Be(0);
        blackWithHue.H.Should().Be(210);

        VekColor.TryParse("hsv(120, 100%, 50%)", out var parsedHsv).Should().BeTrue();
        parsedHsv!.R.Should().Be(0);
        parsedHsv.G.Should().Be(128);
        parsedHsv.B.Should().Be(0);

        VekColor.TryParse("rgba(37, 99, 235, 0.5)", out var parsedRgba).Should().BeTrue();
        parsedRgba!.R.Should().Be(37);
        parsedRgba.G.Should().Be(99);
        parsedRgba.B.Should().Be(235);
        parsedRgba.A.Should().Be(127);
    }

    [Test]
    public async Task TextBinding_SupportsHexAndRgbaOutputFormats()
    {
        string? text = "rgba(37, 99, 235, 0.5)";
        VekColor? value = new("rgba(37, 99, 235, 0.5)");

        var comp = Context.Render<VekColorPicker>(p => p
            .Add(x => x.PickerVariant, PickerVariant.Static)
            .Add(x => x.OutputFormat, VekColorOutputFormats.RGBA)
            .Add(x => x.Value, value)
            .Add(x => x.ValueChanged, v => value = v)
            .Add(x => x.Text, text)
            .Add(x => x.TextChanged, t => text = t));

        // Clicking alpha slider updates both Value and rgba(...) Text string.
        await comp.Find(".vek-picker-color-slider.alpha input").InputAsync("255");
        value.Should().Be(new VekColor(37, 99, 235, 255));
        text.Should().Be("rgba(37,99,235,1)");

        // Switching OutputFormat to Hex outputs #RRGGBB on subsequent updates.
        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.OutputFormat, VekColorOutputFormats.Hex));
        await comp.Find(".vek-picker-color-slider.alpha input").InputAsync("128");
        text.Should().Be("#2563eb");
    }

    [Test]
    public async Task SpectrumCanvas_SupportsAriaAndKeyboardNavigation()
    {
        VekColor? boundValue = new("#804040ff");

        var comp = Context.Render<VekColorPicker>(p => p
            .Add(x => x.PickerVariant, PickerVariant.Static)
            .Add(x => x.Value, boundValue)
            .Add(x => x.ValueChanged, v => boundValue = v));

        var canvas = comp.Find(".vek-picker-color-overlay-black .vek-picker-color-overlay");
        canvas.GetAttribute("role").Should().Be("slider");
        canvas.GetAttribute("tabindex").Should().Be("0");
        canvas.GetAttribute("aria-label").Should().NotBeNullOrWhiteSpace();
        canvas.GetAttribute("aria-valuetext").Should().Contain("Saturation");

        var initialColor = boundValue;
        await canvas.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight", ShiftKey = true });
        boundValue.Should().NotBe(initialColor);

        var afterRight = boundValue;
        await comp.Find(".vek-picker-color-overlay-black .vek-picker-color-overlay")
            .KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown", ShiftKey = true });
        boundValue.Should().NotBe(afterRight);

        // Home moves saturation to 0% (achromatic left edge).
        await comp.Find(".vek-picker-color-overlay-black .vek-picker-color-overlay")
            .KeyDownAsync(new KeyboardEventArgs { Key = "Home" });
        boundValue!.R.Should().Be(boundValue.G);
        boundValue.G.Should().Be(boundValue.B);
    }

    [Test]
    public async Task ShowSwatches_RendersCustomPaletteAndSupportsClickAndKeyboardSelection()
    {
        VekColor? boundValue = new("#2563ebff");
        VekColor[] presets = ["#2563eb", "#10b981", "#ef4444"];

        var comp = Context.Render<VekColorPicker>(p => p
            .Add(x => x.PickerVariant, PickerVariant.Static)
            .Add(x => x.ShowSwatches, true)
            .Add(x => x.Palette, presets)
            .Add(x => x.Value, boundValue)
            .Add(x => x.ValueChanged, v => boundValue = v));

        var swatchGroup = comp.Find(".vek-picker-color-swatches");
        swatchGroup.GetAttribute("role").Should().Be("group");
        swatchGroup.Children.Should().HaveCount(3);
        swatchGroup.Children[0].GetAttribute("aria-pressed").Should().Be("true");
        swatchGroup.Children[1].GetAttribute("aria-pressed").Should().Be("false");

        // Select second swatch via Enter key.
        await swatchGroup.Children[1].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        boundValue.Should().Be(new VekColor("#10b981"));
        comp.Find(".vek-picker-color-swatches").Children[1].GetAttribute("aria-pressed").Should().Be("true");

        // Select third swatch via click.
        await comp.Find(".vek-picker-color-swatches").Children[2].ClickAsync();
        boundValue.Should().Be(new VekColor("#ef4444"));

        // Preview dot toggles quick swatches with aria-expanded and Space key.
        var previewDot = comp.Find(".vek-picker-color-dot-current");
        previewDot.GetAttribute("role").Should().Be("button");
        previewDot.GetAttribute("aria-expanded").Should().Be("false");
        await previewDot.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        comp.Find(".vek-picker-color-dot-current").GetAttribute("aria-expanded").Should().Be("true");
    }

    [Test]
    public void DisabledState_AppliesDisabledStylingAndClass()
    {
        var comp = Context.Render<VekColorPicker>(p => p
            .Add(x => x.PickerVariant, PickerVariant.Static)
            .Add(x => x.Disabled, true));

        comp.Find(".vek-picker").ClassList.Should().Contain("vek-disabled");
        comp.Find(".vek-picker-paper").GetAttribute("style").Should().Contain("pointer-events:none");
    }
}
