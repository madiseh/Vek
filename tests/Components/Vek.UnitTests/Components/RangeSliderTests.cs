using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;

#nullable enable
namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class RangeSliderTests : BunitTest
    {
        [Test]
        public void DefaultValues()
        {
            var slider = new VekRangeSlider<double>();

            slider.Min.Should().Be(0.0);
            slider.Max.Should().Be(100.0);
            slider.Step.Should().Be(1.0);
            slider.MinDistance.Should().Be(0.0);
            slider.ValueStart.Should().Be(0.0);
            slider.ValueEnd.Should().Be(100.0);
            slider.Value.Should().BeNull();

            slider.TickMarks.Should().BeFalse();
            slider.ShowTicks.Should().BeFalse();
            slider.Vertical.Should().BeFalse();
            slider.Disabled.Should().BeFalse();
            slider.Immediate.Should().BeTrue();
            slider.TickMarkLabels.Should().BeNull();

            slider.Color.Should().Be(Color.Primary);
            slider.Size.Should().Be(Size.Small);
            slider.Culture.Should().Be(CultureInfo.InvariantCulture);
            slider.ValueLabel.Should().BeFalse();
            slider.ValueLabelFormat.Should().BeNull();
            slider.ValueLabelContent.Should().BeNull();
            slider.AriaLabelStart.Should().Be("Start value");
            slider.AriaLabelEnd.Should().Be("End value");
        }

        [Test]
        [TestCase(Size.Small, "small")]
        [TestCase(Size.Medium, "medium")]
        [TestCase(Size.Large, "large")]
        public void CheckSizeCssClass(Size size, string expectedSizeClass)
        {
            var comp = Context.Render<VekRangeSlider<int>>(x => x.Add(p => p.Size, size));

            IElement slider = comp.Find(".vek-slider");
            slider.ClassList.Should().ContainInOrder(new[] { "vek-slider", "vek-slider-range", $"vek-slider-{expectedSizeClass}" });
        }

        [Test]
        [TestCase(Color.Default, "default")]
        [TestCase(Color.Primary, "primary")]
        [TestCase(Color.Secondary, "secondary")]
        [TestCase(Color.Success, "success")]
        [TestCase(Color.Error, "error")]
        public void CheckColorCssClass(Color color, string expectedColorClass)
        {
            var comp = Context.Render<VekRangeSlider<int>>(x => x.Add(p => p.Color, color));

            IElement slider = comp.Find(".vek-slider");
            slider.ClassList.Should().ContainInOrder(new[] { "vek-slider", "vek-slider-range", "vek-slider-small", $"vek-slider-{expectedColorClass}" });
        }

        [Test]
        public void CheckVerticalAndDisabledCssClass()
        {
            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Vertical, true)
                .Add(p => p.Disabled, true));

            IElement slider = comp.Find(".vek-slider");
            slider.ClassList.Should().Contain("vek-slider-vertical");
            slider.ClassList.Should().Contain("vek-slider-disabled");
        }

        [Test]
        public void RendersTwoInputsWithAriaAttributes()
        {
            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 10)
                .Add(p => p.Max, 90)
                .Add(p => p.ValueStart, 25)
                .Add(p => p.ValueEnd, 75)
                .Add(p => p.AriaLabelStart, "Lower price")
                .Add(p => p.AriaLabelEnd, "Upper price"));

            var startInput = comp.Find(".vek-slider-input-start");
            startInput.GetAttribute("aria-label").Should().Be("Lower price");
            startInput.GetAttribute("aria-valuenow").Should().Be("25");
            startInput.GetAttribute("aria-valuemin").Should().Be("10");
            startInput.GetAttribute("aria-valuemax").Should().Be("75");
            startInput.GetAttribute("min").Should().Be("10");
            startInput.GetAttribute("max").Should().Be("90");

            var endInput = comp.Find(".vek-slider-input-end");
            endInput.GetAttribute("aria-label").Should().Be("Upper price");
            endInput.GetAttribute("aria-valuenow").Should().Be("75");
            endInput.GetAttribute("aria-valuemin").Should().Be("25");
            endInput.GetAttribute("aria-valuemax").Should().Be("90");
        }

        [Test]
        public void HighlightedRangeTrack_CalculatesPositionAndWidth()
        {
            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.ValueStart, 20)
                .Add(p => p.ValueEnd, 80));

            var filledTrack = comp.Find(".vek-slider-filled");
            filledTrack.GetAttribute("style").Should().Be("left:20%;width:60%;");
        }

        [Test]
        public void HighlightedRangeTrack_RightToLeft()
        {
            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.ValueStart, 30)
                .Add(p => p.ValueEnd, 70)
                .Add(p => p.RightToLeft, true));

            var filledTrack = comp.Find(".vek-slider-filled");
            filledTrack.GetAttribute("style").Should().Be("right:30%;width:40%;");
        }

        [Test]
        public void TwoWayBinding_UpdatesValueStartAndValueEnd()
        {
            int start = 10;
            int end = 90;

            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.ValueStart, start)
                .Add(p => p.ValueStartChanged, v => start = v)
                .Add(p => p.ValueEnd, end)
                .Add(p => p.ValueEndChanged, v => end = v));

            comp.Find(".vek-slider-input-start").Input("25");
            start.Should().Be(25);

            comp.Find(".vek-slider-input-end").Input("75");
            end.Should().Be(75);

            var filledTrack = comp.Find(".vek-slider-filled");
            filledTrack.GetAttribute("style").Should().Be("left:25%;width:50%;");
        }

        [Test]
        public void TwoWayBinding_WithSliderRangeModel()
        {
            SliderRange<int>? range = new(20, 80);

            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.Value, range)
                .Add(p => p.ValueChanged, v => range = v));

            comp.Find(".vek-slider-input-start").Input("35");

            range.Should().NotBeNull();
            range!.Start.Should().Be(35);
            range!.End.Should().Be(80);

            comp.Find(".vek-slider-input-end").Input("65");
            range.Start.Should().Be(35);
            range.End.Should().Be(65);
        }

        [Test]
        public void CollisionPrevention_CannotCross_DefaultMinDistanceZero()
        {
            int start = 30;
            int end = 70;

            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.ValueStart, start)
                .Add(p => p.ValueStartChanged, v => start = v)
                .Add(p => p.ValueEnd, end)
                .Add(p => p.ValueEndChanged, v => end = v));

            // Try to move start past end (70)
            comp.Find(".vek-slider-input-start").Input("85");
            start.Should().Be(70);

            // Try to move end below start (70)
            comp.Find(".vek-slider-input-end").Input("40");
            end.Should().Be(70);
        }

        [Test]
        public void MinDistance_EnforcesIntervalBetweenThumbs()
        {
            int start = 20;
            int end = 80;

            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.MinDistance, 15)
                .Add(p => p.ValueStart, start)
                .Add(p => p.ValueStartChanged, v => start = v)
                .Add(p => p.ValueEnd, end)
                .Add(p => p.ValueEndChanged, v => end = v));

            // End is 80, MinDistance is 15 -> start cannot exceed 65
            comp.Find(".vek-slider-input-start").Input("75");
            start.Should().Be(65);

            // Start is now 65, MinDistance is 15 -> end cannot go below 80
            comp.Find(".vek-slider-input-end").Input("70");
            end.Should().Be(80);
        }

        [Test]
        public void DisabledSlider_PreventsInteraction()
        {
            int start = 20;
            int end = 80;

            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.Disabled, true)
                .Add(p => p.ValueStart, start)
                .Add(p => p.ValueStartChanged, v => start = v)
                .Add(p => p.ValueEnd, end)
                .Add(p => p.ValueEndChanged, v => end = v));

            comp.Find(".vek-slider-input-start").HasAttribute("disabled").Should().BeTrue();
            comp.Find(".vek-slider-input-end").HasAttribute("disabled").Should().BeTrue();

            comp.Find(".vek-slider-input-start").Input("30");
            start.Should().Be(20);

            comp.Find(".vek-slider-input-end").Input("70");
            end.Should().Be(80);
        }

        [Test]
        public void ValueLabels_RenderWhenEnabled()
        {
            var comp = Context.Render<VekRangeSlider<decimal>>(x => x
                .Add(p => p.Min, 0m)
                .Add(p => p.Max, 100m)
                .Add(p => p.ValueStart, 25.5m)
                .Add(p => p.ValueEnd, 75.5m)
                .Add(p => p.ValueLabel, true));

            var labels = comp.FindAll(".vek-slider-value-label");
            labels.Count.Should().Be(2);
            labels[0].TextContent.Trim().Should().Be("25.5");
            labels[1].TextContent.Trim().Should().Be("75.5");
        }

        [Test]
        public void TickMarks_RenderProperly()
        {
            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 10)
                .Add(p => p.Step, 2)
                .Add(p => p.TickMarks, true));

            var ticks = comp.FindAll(".vek-slider-tickmark");
            ticks.Count.Should().Be(6); // 0, 2, 4, 6, 8, 10
        }

        [Test]
        public void SnapToStep_EnforcesStepIncrements()
        {
            int start = 0;
            int end = 100;

            var comp = Context.Render<VekRangeSlider<int>>(x => x
                .Add(p => p.Min, 0)
                .Add(p => p.Max, 100)
                .Add(p => p.Step, 10)
                .Add(p => p.ValueStart, start)
                .Add(p => p.ValueStartChanged, v => start = v)
                .Add(p => p.ValueEnd, end)
                .Add(p => p.ValueEndChanged, v => end = v));

            comp.Find(".vek-slider-input-start").Input("23"); // Should snap to 20
            start.Should().Be(20);

            comp.Find(".vek-slider-input-end").Input("77"); // Should snap to 80
            end.Should().Be(80);
        }

        [Test]
        public void SliderRange_EqualityAndDeconstruction()
        {
            var range1 = new SliderRange<int>(10, 50);
            var range2 = new SliderRange<int>(10, 50);
            var range3 = new SliderRange<int>(20, 60);

            range1.Should().Be(range2);
            range1.Should().NotBe(range3);
            range1.Lower.Should().Be(10);
            range1.Upper.Should().Be(50);
            range1.ToString().Should().Be("[10, 50]");

            var (lower, upper) = range1;
            lower.Should().Be(10);
            upper.Should().Be(50);

            SliderRange<int> fromTuple = (15, 45);
            fromTuple.Start.Should().Be(15);
            fromTuple.End.Should().Be(45);

            (int start, int end) tuple = range1;
            tuple.start.Should().Be(10);
            tuple.end.Should().Be(50);
        }
    }
}
