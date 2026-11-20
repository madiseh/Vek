// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

[TestFixture]
public class PickerToolbarTests : BunitTest
{
    [Test]
    public void PickerToolbar_ShouldBeLandscape_WhenStaticAndOrientationLandscape()
    {
        var component = Context.Render<VekPickerToolbar>(parameters => parameters
            .Add(p => p.PickerVariant, PickerVariant.Static)
            .Add(p => p.Orientation, Orientation.Landscape));

        component.FindAll(".vek-picker-toolbar-landscape").Count.Should().Be(1);
    }

    [Test]
    [TestCase(PickerVariant.Inline)]
    [TestCase(PickerVariant.Dialog)]
    public void PickerToolbar_ShouldNotBeLandscape_WhenNonStaticAndOrientationLandscape(PickerVariant pickerVariant)
    {
        var component = Context.Render<VekPickerToolbar>(parameters => parameters
            .Add(p => p.PickerVariant, pickerVariant)
            .Add(p => p.Orientation, Orientation.Landscape));

        component.FindAll(".vek-picker-toolbar-landscape").Count.Should().Be(0);
    }

    [Test]
    [TestCase(Color.Primary, "vek-theme-primary")]
    [TestCase(Color.Secondary, "vek-theme-secondary")]
    public void PickerToolbar_AppliesThemeColorClass(Color color, string expectedClass)
    {
        var component = Context.Render<VekPickerToolbar>(parameters => parameters
            .Add(p => p.Color, color));

        component.Find(".vek-picker-toolbar").ClassList.Should().Contain(expectedClass);
    }

    [Test]
    public void PickerToolbar_RendersChildContent_WhenShowToolbar()
    {
        var component = Context.Render<VekPickerToolbar>(parameters => parameters
            .Add(p => p.ShowToolbar, true)
            .AddChildContent("<span class=\"probe\"></span>"));

        component.FindAll(".probe").Count.Should().Be(1);
    }

    [Test]
    public void PickerToolbar_HidesContent_WhenShowToolbarFalse()
    {
        var component = Context.Render<VekPickerToolbar>(parameters => parameters
            .Add(p => p.ShowToolbar, false)
            .AddChildContent("<span class=\"probe\"></span>"));

        component.FindAll(".vek-picker-toolbar").Count.Should().Be(0);
        component.FindAll(".probe").Count.Should().Be(0);
    }

    [Test]
    public void PickerContent_RendersClassAndChildContent()
    {
        var component = Context.Render<VekPickerContent>(parameters => parameters
            .Add(p => p.Class, "my-content")
            .AddChildContent("<span class=\"probe\"></span>"));

        var content = component.Find(".vek-picker-content");
        content.ClassList.Should().Contain("my-content");
        component.FindAll(".probe").Count.Should().Be(1);
    }
}
