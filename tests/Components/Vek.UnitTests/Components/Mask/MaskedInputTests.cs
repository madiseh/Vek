// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.Mask;

[TestFixture]
public class MaskedInputTests : BunitTest
{
    [Test]
    public void MaskTokens_StandardDefinitions_MatchExpectedRules()
    {
        var digit = MaskTokens.Digit('0');
        digit.Char.Should().Be('0');
        digit.Optional.Should().BeFalse();
        digit.Regex.Should().Be(@"\d");

        var optDigit = MaskTokens.OptionalDigit('9');
        optDigit.Char.Should().Be('9');
        optDigit.Optional.Should().BeTrue();
        optDigit.Regex.Should().Be(@"\d");

        var digitOrSign = MaskTokens.DigitOrSign('#');
        digitOrSign.Char.Should().Be('#');
        digitOrSign.Optional.Should().BeTrue();

        var letter = MaskTokens.Letter('L');
        letter.Char.Should().Be('L');
        letter.Optional.Should().BeFalse();
        letter.Regex.Should().Be(@"\p{L}");

        var optLetter = MaskTokens.OptionalLetter('?');
        optLetter.Char.Should().Be('?');
        optLetter.Optional.Should().BeTrue();

        var alphaNum = MaskTokens.Alphanumeric('A');
        alphaNum.Char.Should().Be('A');
        alphaNum.Optional.Should().BeFalse();

        var optAlphaNum = MaskTokens.OptionalAlphanumeric('a');
        optAlphaNum.Char.Should().Be('a');
        optAlphaNum.Optional.Should().BeTrue();

        MaskTokens.Standard.Should().NotBeNull();
        MaskTokens.Standard.Length.Should().BeGreaterThanOrEqualTo(7);
    }

    [Test]
    public void PatternMask_PromptChar_FillsUnenteredPositions()
    {
        var mask = new PatternMask("(000) 000-0000", MaskTokens.Standard, promptChar: '_');
        mask.Insert("123");

        mask.Text.Should().Be("(123) ___-____");
        mask.GetValueText(saveLiterals: true).Should().Be("(123) -");
        mask.GetValueText(saveLiterals: false).Should().Be("123");
    }

    [Test]
    public void PatternMask_CustomPromptChar_UsesCustomSymbol()
    {
        var mask = new PatternMask("000-000", MaskTokens.Standard, promptChar: '#');
        mask.Insert("45");

        mask.Text.Should().Be("45#-###");
        mask.GetValueText(saveLiterals: true).Should().Be("45-");
        mask.GetValueText(saveLiterals: false).Should().Be("45");
    }

    [Test]
    public void PatternMask_OptionalTokens_AllowSkipping()
    {
        // 9 is optional digit, 0 is required digit
        var mask = new PatternMask("999-0000", MaskTokens.Standard, promptChar: '_');
        mask.Insert("12-3456");

        mask.Text.Should().StartWith("12");
        mask.GetCleanText().Should().Contain("12");
    }

    [Test]
    public void PatternMask_AlphanumericTokens_EnforceCharacterClasses()
    {
        var mask = new PatternMask("LLL-000-AAA", MaskTokens.Standard, promptChar: '_') { CleanDelimiters = true };
        mask.Insert("ABC123Z9Y");

        mask.Text.Should().Be("ABC-123-Z9Y");
        mask.GetCleanText().Should().Be("ABC123Z9Y");
        mask.GetValueText(saveLiterals: true).Should().Be("ABC-123-Z9Y");
        mask.GetValueText(saveLiterals: false).Should().Be("ABC123Z9Y");
    }

    [Test]
    public async Task VekMaskedInput_DefaultRendering_RendersExpectedStructure()
    {
        var comp = Context.Render<VekMaskedInput>(parameters =>
        {
            parameters.Add(p => p.Mask, "(000) 000-0000");
            parameters.Add(p => p.Label, "Phone Number");
            parameters.Add(p => p.PromptChar, '_');
            parameters.Add(p => p.ShowLiterals, ShowLiterals.Always);
        });

        comp.Find("div.vek-input-control").Should().NotBeNull();
        comp.Find("input.vek-input-slot").Should().NotBeNull();
        comp.Find("label.vek-input-label").TextContent.Should().Contain("Phone Number");

        var mask = comp.FindComponent<VekMask>();
        mask.Should().NotBeNull();
        mask.Instance.PromptChar.Should().Be('_');
        mask.Instance.ShowLiterals.Should().Be(ShowLiterals.Always);
        mask.Instance.SaveLiteralRepresentation.Should().BeTrue();
    }

    [Test]
    public async Task VekMaskedInput_TwoWayBinding_UpdatesValue()
    {
        var boundValue = string.Empty;
        var comp = Context.Render<VekMaskedInput>(parameters =>
        {
            parameters.Add(p => p.Mask, "(000) 000-0000");
            parameters.Add(p => p.PromptChar, '_');
            parameters.Add(p => p.SaveLiteralRepresentation, true);
            parameters.Add(p => p.Value, boundValue);
            parameters.Add(p => p.ValueChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<string>(this, s => boundValue = s));
        });

        var mask = comp.FindComponent<VekMask>().Instance;
        await comp.InvokeAsync(() => mask.OnInputAsync("5551234567"));

        boundValue.Should().Be("(555) 123-4567");
        comp.Instance.ReadText.Should().Be("(555) 123-4567");
    }

    [Test]
    public async Task VekMaskedInput_SaveLiteralRepresentation_False_EmitsCleanValue()
    {
        var boundValue = string.Empty;
        var comp = Context.Render<VekMaskedInput>(parameters =>
        {
            parameters.Add(p => p.Mask, "(000) 000-0000");
            parameters.Add(p => p.PromptChar, '_');
            parameters.Add(p => p.SaveLiteralRepresentation, false);
            parameters.Add(p => p.Value, boundValue);
            parameters.Add(p => p.ValueChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<string>(this, s => boundValue = s));
        });

        var mask = comp.FindComponent<VekMask>().Instance;
        await comp.InvokeAsync(() => mask.OnInputAsync("5551234567"));

        boundValue.Should().Be("5551234567");
    }

    [Test]
    public async Task VekMask_ShowLiterals_Never_DisplaysRawInputOnly()
    {
        var comp = Context.Render<VekMask>(parameters =>
        {
            parameters.Add(p => p.MaskPattern, "(000) 000-0000");
            parameters.Add(p => p.PromptChar, '_');
            parameters.Add(p => p.ShowLiterals, ShowLiterals.Never);
        });

        var mask = comp.Instance;
        await comp.InvokeAsync(() => mask.OnInputAsync("1234567890"));

        mask.GetDisplayText().Should().Be("1234567890");
        mask.ReadText.Should().Be("1234567890");
    }

    [Test]
    public async Task VekMask_ShowLiterals_WhileEditing_AdaptsOnFocusAndBlur()
    {
        var comp = Context.Render<VekMask>(parameters =>
        {
            parameters.Add(p => p.MaskPattern, "(000) 000-0000");
            parameters.Add(p => p.PromptChar, '_');
            parameters.Add(p => p.ShowLiterals, ShowLiterals.WhileEditing);
        });

        var mask = comp.Instance;

        // When not focused and empty, should display empty
        mask.GetDisplayText().Should().BeEmpty();

        // Simulate focus
        await comp.InvokeAsync(() => mask.OnFocused(new FocusEventArgs()));
        mask.GetDisplayText().Should().Be("(___) ___-____");

        // Enter partial text
        await comp.InvokeAsync(() => mask.OnInputAsync("123"));
        mask.GetDisplayText().Should().Be("(123) ___-____");

        // Simulate blur
        await comp.InvokeAsync(() => mask.OnBlurredAsync(new FocusEventArgs()));
        // Unfilled prompt characters hidden on blur
        mask.GetDisplayText().Should().Be("(123) -");
    }

    [Test]
    public async Task VekMaskedInput_DisabledAndReadOnly_ForwardedCorrectly()
    {
        var comp = Context.Render<VekMaskedInput>(parameters =>
        {
            parameters.Add(p => p.Mask, "00/00/0000");
            parameters.Add(p => p.Disabled, true);
            parameters.Add(p => p.ReadOnly, true);
        });

        var input = comp.Find("input.vek-input-slot");
        input.HasAttribute("disabled").Should().BeTrue();
        input.HasAttribute("readonly").Should().BeTrue();
    }

    [Test]
    public async Task VekMaskedInput_ClearButton_ClearsValue()
    {
        var boundValue = "12345";
        var comp = Context.Render<VekMaskedInput>(parameters =>
        {
            parameters.Add(p => p.Mask, "00000");
            parameters.Add(p => p.Clearable, true);
            parameters.Add(p => p.Value, boundValue);
            parameters.Add(p => p.ValueChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<string>(this, s => boundValue = s));
        });

        await comp.InvokeAsync(() => comp.Instance.Clear());
        comp.Instance.ReadText.Should().BeEmpty();
    }

    [Test]
    public void VekMaskedInput_CustomMaskInstance_TakesPrecedence()
    {
        var customMask = new PatternMask("AAAA-AAAA", MaskTokens.Standard);
        var comp = Context.Render<VekMaskedInput>(parameters =>
        {
            parameters.Add(p => p.Mask, "0000-0000");
            parameters.Add(p => p.MaskInstance, customMask);
        });

        comp.Instance.EffectiveMask.Should().BeSameAs(customMask);
    }
}
