// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using AwesomeAssertions;
using Vek.Utilities;
using NUnit.Framework;

namespace Vek.UnitTests.Utilities;

#nullable enable
[TestFixture]
public class VekColorComparerTests
{
    [Test]
    public void Singleton_Instances_ShouldBeCreated()
    {
        // Arrange & Act
        var rgba = VekColor.VekColorComparer.Rgba;
        var hsl = VekColor.VekColorComparer.Hsl;
        var both = VekColor.VekColorComparer.RgbaAndHsl;

        // Assert
        rgba.Should().NotBeNull();
        hsl.Should().NotBeNull();
        both.Should().NotBeNull();
    }

    [Test]
    public void Singleton_Instances_ShouldHaveCorrectComparisonModes()
    {
        // Arrange & Act
        var rgba = VekColor.VekColorComparer.Rgba;
        var hsl = VekColor.VekColorComparer.Hsl;
        var both = VekColor.VekColorComparer.RgbaAndHsl;

        // Assert
        rgba.Comparison.Should().Be(VekColorComparison.Rgba);
        hsl.Comparison.Should().Be(VekColorComparison.Hsl);
        both.Comparison.Should().Be(VekColorComparison.RgbaAndHsl);
    }

    [Test]
    public void Equals_ShouldUseFallback_WhenComparisonIsInvalid()
    {
        // Arrange
        var comparer = ConstructInvalidComparer();

        var color1 = new VekColor(10, 20, 30, 40);
        var color2 = new VekColor(10, 20, 30, 40);

        // Act
        var result = comparer.Equals(color1, color2);

        // Assert
        result.Should().BeTrue("fallback: x.Equals(y)");
    }

    [Test]
    [TestCaseSource(nameof(AllComparers))]
    public void Equals_ShouldReturnTrue_WhenBothNull(VekColor.VekColorComparer comparer)
    {
        // Arrange & Act
        var result = comparer.Equals(null, null);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    [TestCaseSource(nameof(AllComparers))]
    public void Equals_ShouldReturnFalse_WhenOneNull(VekColor.VekColorComparer comparer)
    {
        // Arrange
        var color = new VekColor("#ff0000");

        // Act
        var result1 = comparer.Equals(color, null);
        var result2 = comparer.Equals(null, color);

        // Assert
        result1.Should().BeFalse();
        result2.Should().BeFalse();
    }

    [Test]
    [TestCaseSource(nameof(AllComparers))]
    public void Equals_ShouldReturnTrue_WhenSameReference(VekColor.VekColorComparer comparer)
    {
        // Arrange
        var color = new VekColor("#ff0000");

        // Act
        var result = comparer.Equals(color, color);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_RGBA_ShouldReturnTrue_WhenRgbaMatches()
    {
        // Arrange
        var color1 = new VekColor("#ff0000");
        var color2 = new VekColor("#ff0000");

        // Act
        var result = VekColor.VekColorComparer.Rgba.Equals(color1, color2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_RGBA_ShouldReturnFalse_WhenRgbaDiffers()
    {
        // Arrange
        var red = new VekColor("#ff0000");
        var blue = new VekColor("#0000ff");

        // Act
        var result = VekColor.VekColorComparer.Rgba.Equals(red, blue);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetHashCode_RGBA_ShouldMatchForEqualColors()
    {
        // Arrange
        var color1 = new VekColor("#ff0000");
        var color2 = new VekColor("#ff0000");

        // Act
        var h1 = VekColor.VekColorComparer.Rgba.GetHashCode(color1);
        var h2 = VekColor.VekColorComparer.Rgba.GetHashCode(color2);

        // Assert
        h1.Should().Be(h2);
    }

    [Test]
    public void GetHashCode_RGBA_ShouldDifferForDifferentColors()
    {
        // Arrange
        var color1 = new VekColor("#ff0000");
        var color2 = new VekColor("#0000ff");

        // Act
        var h1 = VekColor.VekColorComparer.Rgba.GetHashCode(color1);
        var h2 = VekColor.VekColorComparer.Rgba.GetHashCode(color2);

        // Assert
        h1.Should().NotBe(h2);
    }

    [Test]
    public void Equals_HSL_ShouldReturnTrue_WhenHslMatches()
    {
        // Arrange
        var color1 = new VekColor(245, 0.34, 0.95, 1);
        var color2 = new VekColor(245, 0.34, 0.95, 1);

        // Act
        var result = VekColor.VekColorComparer.Hsl.Equals(color1, color2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_HSL_ShouldReturnFalse_WhenHslDiffers()
    {
        // Arrange
        var color1 = new VekColor(245, 0.34, 0.95, 1);
        var color2 = new VekColor(245, 0.35, 0.95, 1);

        // Act
        var result = VekColor.VekColorComparer.Hsl.Equals(color1, color2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetHashCode_HSL_ShouldMatchForEqualColors()
    {
        // Arrange
        var color1 = new VekColor(245, 0.34, 0.95, 1);
        var color2 = new VekColor(245, 0.34, 0.95, 1);

        // Act
        var h1 = VekColor.VekColorComparer.Hsl.GetHashCode(color1);
        var h2 = VekColor.VekColorComparer.Hsl.GetHashCode(color2);

        // Assert
        h1.Should().Be(h2);
    }

    [Test]
    public void GetHashCode_HSL_ShouldDifferForDifferentColors()
    {
        // Arrange
        var color1 = new VekColor(245, 0.34, 0.95, 1);
        var color2 = new VekColor(245, 0.35, 0.95, 1);

        // Act
        var h1 = VekColor.VekColorComparer.Hsl.GetHashCode(color1);
        var h2 = VekColor.VekColorComparer.Hsl.GetHashCode(color2);

        // Assert
        h1.Should().NotBe(h2);
    }

    [Test]
    public void Equals_Both_ShouldReturnTrue_WhenBothRgbaAndHslMatch()
    {
        // Arrange
        var color1 = new VekColor(239, 238, 247, 1);
        var color2 = new VekColor(color1.H, color1.S, color1.L, 1);

        // Act
        var result = VekColor.VekColorComparer.RgbaAndHsl.Equals(color1, color2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_Both_ShouldReturnFalse_WhenRgbaMatchesButHslDiffers()
    {
        // Arrange
        var color1 = new VekColor(239, 238, 247, 1);
        var color2 = new VekColor(color1.H, color1.S + 0.01, color1.L, 1);

        // Act
        var result = VekColor.VekColorComparer.RgbaAndHsl.Equals(color1, color2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetHashCode_Both_ShouldMatchForEqualColors()
    {
        // Arrange
        var color1 = new VekColor(239, 238, 247, 1);
        var color2 = new VekColor(color1.H, color1.S, color1.L, 1);

        // Act
        var h1 = VekColor.VekColorComparer.RgbaAndHsl.GetHashCode(color1);
        var h2 = VekColor.VekColorComparer.RgbaAndHsl.GetHashCode(color2);

        // Assert
        h1.Should().Be(h2);
    }

    [Test]
    public void GetHashCode_Both_ShouldDiffer_WhenOnlyOneAspectMatches()
    {
        // Arrange
        var color1 = new VekColor(239, 238, 247, 1);
        var color2 = new VekColor(color1.H, color1.S + 0.01, color1.L, 1);

        // Act
        var h1 = VekColor.VekColorComparer.RgbaAndHsl.GetHashCode(color1);
        var h2 = VekColor.VekColorComparer.RgbaAndHsl.GetHashCode(color2);

        // Assert
        h1.Should().NotBe(h2);
    }

    [Test]
    public void GetHashCode_Both_ShouldDiffer_ForCompletelyDifferentColors()
    {
        // Arrange
        var color1 = new VekColor("#ff0000");
        var color2 = new VekColor("#0000ff");

        // Act
        var h1 = VekColor.VekColorComparer.RgbaAndHsl.GetHashCode(color1);
        var h2 = VekColor.VekColorComparer.RgbaAndHsl.GetHashCode(color2);

        // Assert
        h1.Should().NotBe(h2);
    }

    [Test]
    [TestCaseSource(nameof(AllComparers))]
    public void GetHashCode_NullObject_ShouldBeZero(VekColor.VekColorComparer comparer)
    {
        // Documented contract: the comparer hashes null to 0 regardless of mode.
        comparer.GetHashCode(null).Should().Be(0);
    }

    [Test]
    public void GetHashCode_ShouldUseFallback_WhenComparisonIsInvalid()
    {
        // Arrange
        var comparer = ConstructInvalidComparer();

        var color = new VekColor(1, 2, 3, 4);

        // Act
        var hash = comparer.GetHashCode(color);

        // Assert
        hash.Should().Be(color.GetHashCode(), because: "fallback: vekColor.GetHashCode()");
    }

    private static VekColor.VekColorComparer ConstructInvalidComparer()
    {
        var invalidComparison = (VekColorComparison)(-1);
        var comparer = (VekColor.VekColorComparer)typeof(VekColor.VekColorComparer)
            .GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                [typeof(VekColorComparison)],
                modifiers: null
            )!
            .Invoke([invalidComparison]);

        return comparer;
    }

    private static IEnumerable<VekColor.VekColorComparer> AllComparers()
    {
        yield return VekColor.VekColorComparer.Rgba;
        yield return VekColor.VekColorComparer.Hsl;
        yield return VekColor.VekColorComparer.RgbaAndHsl;
    }
}
