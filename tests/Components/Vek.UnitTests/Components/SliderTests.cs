using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.Slider;

#nullable enable
namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class SliderTests : BunitTest
    {
        [Test]
        public void DefaultValues()
        {
            var slider = new VekSlider<double>();

            slider.Min.Should().Be(0.0);
            slider.Max.Should().Be(100.0);
            slider.Step.Should().Be(1.0);
            slider.Value.Should().Be(0.0);

            slider.TickMarks.Should().BeFalse();
            slider.Vertical.Should().BeFalse();
            slider.Disabled.Should().BeFalse();
            slider.Immediate.Should().BeTrue();
            slider.TickMarkLabels.Should().BeNull();

            slider.Color.Should().Be(Color.Primary);

            slider.Variant.Should().Be(Variant.Text);
            slider.Variant.Should().Be(Variant.Text);

            slider.Size.Should().Be(Size.Small);
            slider.Culture.Should().Be(CultureInfo.InvariantCulture);
            slider.ValueLabelFormat.Should().BeNull();
            slider.ValueLabelDisplay.Should().BeNull();
            slider.ValueLabelContent.Should().BeNull();
        }

        /// <summary>
        /// Applies display mode precedence and composite formatting to slider value labels.
        /// </summary>
        [Test]
        public void ValueLabelDisplay_ControlsVisibilityAndCompositeFormat()
        {
            var hidden = Context.Render<VekSlider<decimal>>(x => x.Add(p => p.ValueLabelDisplay, SliderValueLabelDisplay.Never));
            hidden.FindAll(".vek-slider-value-label").Should().BeEmpty();

            var legacy = Context.Render<VekSlider<decimal>>(x =>
            {
                x.Add(p => p.ValueLabel, true);
                x.Add(p => p.Value, 25m);
            });
            legacy.Find(".vek-slider-value-label").TextContent.Should().Be("25");

            var defaultFormat = Context.Render<VekSlider<decimal>>(x =>
            {
                x.Add(p => p.ValueLabelDisplay, SliderValueLabelDisplay.Always);
                x.Add(p => p.Value, 25m);
            });
            defaultFormat.Find(".vek-slider-value-label").TextContent.Should().Be("25");

            var hover = Context.Render<VekSlider<decimal>>(x =>
            {
                x.Add(p => p.ValueLabelDisplay, SliderValueLabelDisplay.OnHover);
                x.Add(p => p.Value, 12.5m);
                x.Add(p => p.ValueLabelFormat, "{0}%");
            });
            hover.Find(".vek-slider-value-label").TextContent.Should().Be("12.5%");
            hover.Find(".vek-slider-value-label").ClassList.Should().NotContain("vek-slider-value-label-always");

            var always = Context.Render<VekSlider<decimal>>(x =>
            {
                x.Add(p => p.ValueLabelDisplay, SliderValueLabelDisplay.Always);
                x.Add(p => p.Value, 25m);
                x.Add(p => p.NullableValue, 25m);
                x.Add(p => p.ValueLabelFormat, "{0:C}");
                x.Add(p => p.Culture, CultureInfo.GetCultureInfo("en-US"));
            });
            always.Find(".vek-slider-value-label").TextContent.Should().Be("$25.00");
            always.Find(".vek-slider-value-label").ClassList.Should().Contain("vek-slider-value-label-always");

            var range = Context.Render<VekRangeSlider<decimal>>(x =>
            {
                x.Add(p => p.ValueStart, 10m);
                x.Add(p => p.ValueEnd, 40m);
                x.Add(p => p.ValueLabelDisplay, SliderValueLabelDisplay.Always);
                x.Add(p => p.ValueLabelFormat, "{0:C}");
                x.Add(p => p.Culture, CultureInfo.GetCultureInfo("en-US"));
            });
            range.FindAll(".vek-slider-value-label").Select(label => label.TextContent).Should().ContainInOrder("$10.00", "$40.00");
        }

        [Test]
        [TestCase(Size.Small, "small")]
        [TestCase(Size.Medium, "medium")]
        [TestCase(Size.Large, "large")]
        public void CheckSizeCssClass(Size size, string expectedSizeClass)
        {
            var comp = Context.Render<VekSlider<int>>(x => x.Add(p => p.Size, size));

            IElement Slider() => comp.Find(".vek-slider");
            Slider().ClassList.Should().ContainInOrder(new[] { "vek-slider", $"vek-slider-{expectedSizeClass}" });
        }

        /// <summary>
        /// Exposes vertical orientation, height, and inversion in the rendered control.
        /// </summary>
        [Test]
        public void Vertical_UsesHeightAndInversion()
        {
            var comp = Context.Render<VekSlider<int>>(x =>
            {
                x.Add(p => p.Vertical, true);
                x.Add(p => p.Height, "18rem");
                x.Add(p => p.Inverted, true);
                x.Add(p => p.Value, 25);
                x.Add(p => p.ValueLabel, true);
            });

            comp.Find(".vek-slider").GetAttribute("style").Should().Contain("--vek-slider-height:18rem");
            comp.Find(".vek-slider").ClassList.Should().Contain("vek-slider-inverted");
            comp.Find(".vek-slider-value-label").GetAttribute("style").Should().Be("top:75%;");
        }

        [Test]
        public void CheckVerticalClass()
        {
            var verticalSliderComponent = Context.Render<VekSlider<int>>(x => x.Add(p => p.Vertical, true));

            IElement VerticalSlider() => verticalSliderComponent.Find(".vek-slider");
            VerticalSlider().ClassList.Should().ContainInOrder(new[] { "vek-slider", "vek-slider-small", "vek-slider-vertical" });

            var horizontalSliderComponent = Context.Render<VekSlider<int>>(x => x.Add(p => p.Vertical, true));

            IElement HorizontalSlider() => horizontalSliderComponent.Find(".vek-slider");
            HorizontalSlider().ClassList.Should().ContainInOrder(new[] { "vek-slider", "vek-slider-small" });
        }

        [Test]
        [TestCase(Size.Small, "small")]
        [TestCase(Size.Medium, "medium")]
        [TestCase(Size.Large, "large")]
        public void CheckInputSizeCssClass(Size size, string expectedSizeClass)
        {
            var comp = Context.Render<VekSlider<int>>(x => x.Add(p => p.Size, size));

            IElement Slider() => comp.Find(".vek-slider");
            Slider().ClassList.Should().ContainInOrder(new[] { "vek-slider", $"vek-slider-{expectedSizeClass}", "vek-slider-primary" });
        }

        [Test]
        [TestCase(Color.Default, "default")]
        [TestCase(Color.Primary, "primary")]
        [TestCase(Color.Secondary, "secondary")]
        [TestCase(Color.Tertiary, "tertiary")]
        [TestCase(Color.Info, "info")]
        [TestCase(Color.Success, "success")]
        [TestCase(Color.Warning, "warning")]
        [TestCase(Color.Error, "error")]
        [TestCase(Color.Dark, "dark")]
        [TestCase(Color.Transparent, "transparent")]
        [TestCase(Color.Inherit, "inherit")]
        [TestCase(Color.Surface, "surface")]
        public void CheckColorCssClass(Color color, string expectedColorClass)
        {
            var comp = Context.Render<VekSlider<int>>(x => x.Add(p => p.Color, color));

            IElement Slider() => comp.Find(".vek-slider");
            Slider().ClassList.Should().ContainInOrder(new[] { "vek-slider", "vek-slider-small", $"vek-slider-{expectedColorClass}" });
        }

        [Test]
        public void GeneralStructure()
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Min, 100.0);
                x.Add(p => p.Max, 200.0);
                x.Add(p => p.Step, 10.0);
                x.Add(p => p.Value, 120.0);

            });

            comp.Nodes.Should().ContainSingle();

            IElement RootElement() => comp.Find(".vek-slider");
            (comp.Nodes.First() as IHtmlElement)?.InnerHtml.Should().Be(RootElement().InnerHtml);

            RootElement().Children.Should().ContainSingle();

            var container = RootElement().Children.First();
            container.ClassList.Should().Contain("vek-slider-container");

            container.Children.Should().HaveCount(1);

            var input = container.Children.ElementAt(0);

            input.ClassList.Should().Contain("vek-slider-input");
            (input as IHtmlInputElement)?.Value.Should().Be("120");

            var expectedAttributes = new Dictionary<string, string>()
            {
                { "min","100" },
                { "max","200" },
                { "step","10" },
            };

            foreach (var item in expectedAttributes)
            {
                input.GetAttribute(item.Key).Should().Be(item.Value);
            }
        }

        [Test]
        public void Structure_WithChildContent()
        {
            var comp = Context.Render<SliderWithContentTest>(x => x.Add(p => p.Text, "my text"));

            comp.Nodes.Should().ContainSingle();

            IElement RootElement() => comp.Find(".vek-slider");

            (comp.Nodes.First() as IHtmlElement)?.InnerHtml.Should().Be(RootElement().InnerHtml);

            RootElement().Children.Should().HaveCount(2);

            var childContent = RootElement().Children.ElementAt(0);
            childContent.TextContent.Should().Be("my text");
            var container = RootElement().Children.ElementAt(1);
            container.ClassList.Should().Contain("vek-slider-container");

            container.Children.Should().HaveCount(1);

            container.Children.ElementAt(0).ClassList.Should().Contain("vek-slider-input");
        }

        [Test]
        public void Structure_WithFilled()
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Value, 20.0);
                x.Add(p => p.Variant, Variant.Filled);
            });

            comp.Nodes.Should().ContainSingle();

            IElement RootElement() => comp.Find(".vek-slider");
            (comp.Nodes.First() as IHtmlElement)?.InnerHtml.Should().Be(RootElement().InnerHtml);

            RootElement().Children.Should().ContainSingle();

            var container = RootElement().Children.First();
            container.ClassList.Should().Contain("vek-slider-container");

            container.Children.Should().HaveCount(2);

            var containerInner = container.Children.ElementAt(0);
            containerInner.ClassList.Should().Contain("vek-slider-inner-container");

            var filling = containerInner.Children.ElementAt(0);
            filling.ClassList.Should().Contain("vek-slider-filled");

            container.Children.ElementAt(1).ClassList.Should().Contain("vek-slider-input");
        }

        [Test]
        public void TickMarksEnabled_ButNoLabels()
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Value, 20.0);
                x.Add(p => p.Step, 25.0);
                x.Add(p => p.TickMarks, true);
            });

            comp.Nodes.Should().ContainSingle();

            IElement RootElement() => comp.Find(".vek-slider");
            (comp.Nodes.First() as IHtmlElement)?.InnerHtml.Should().Be(RootElement().InnerHtml);

            RootElement().Children.Should().ContainSingle();

            var container = RootElement().Children.First();
            container.ClassList.Should().Contain("vek-slider-container");

            container.Children.Should().HaveCount(2);

            var filling = container.Children.ElementAt(0);

            filling.ClassList.Should().Contain("vek-slider-inner-container");
            filling.Children.Should().ContainSingle();

            var tickMarks = filling.Children.First();
            tickMarks.ClassList.Should().Contain("vek-slider-tickmarks");
            tickMarks.Children.Should().HaveCount(5);

            foreach (var item in tickMarks.Children)
            {
                item.ClassList.Should().Contain(new[] { "d-flex", "flex-column", "relative" });
                item.Children.Should().ContainSingle();

                item.Children.First().ClassList.Should().Contain("vek-slider-track-tick");
            }

            container.Children.ElementAt(1).ClassList.Should().Contain("vek-slider-input");
        }

        [Test]
        public void TickMarksEnabled_ButLabels()
        {
            var labels = new[] { "red", "green", "yellow", "blue", "black" };

            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Value, 20.0);
                x.Add(p => p.Step, 25.0);
                x.Add(p => p.TickMarks, true);
                x.Add(p => p.TickMarkLabels, labels);
            });

            comp.Nodes.Should().ContainSingle();

            IElement RootElement() => comp.Find(".vek-slider");
            (comp.Nodes.First() as IHtmlElement)?.InnerHtml.Should().Be(RootElement().InnerHtml);

            RootElement().Children.Should().ContainSingle();

            var container = RootElement().Children.First();
            container.ClassList.Should().Contain("vek-slider-container");

            container.Children.Should().HaveCount(2);

            var filling = container.Children.ElementAt(0);

            filling.ClassList.Should().Contain("vek-slider-inner-container");
            filling.Children.Should().ContainSingle();

            var tickMarks = filling.Children.First();
            tickMarks.ClassList.Should().Contain("vek-slider-tickmarks");
            tickMarks.Children.Should().HaveCount(5);

            var itemCounter = 0;
            foreach (var item in tickMarks.Children)
            {
                item.ClassList.Should().Contain(new[] { "d-flex", "flex-column", "relative" });
                item.Children.Should().HaveCount(2);

                item.Children.First().ClassList.Should().Contain("vek-slider-track-tick");

                var content = item.Children.ElementAt(1);
                content.ClassList.Should().Contain("vek-slider-track-tick-label");
                content.TextContent.Should().Be(labels[itemCounter++]);
            }

            container.Children.ElementAt(1).ClassList.Should().Contain("vek-slider-input");
        }

        /// <summary>
        /// Includes the maximum value and positions marks according to their numeric values when the interval does not divide the range evenly.
        /// </summary>
        [Test]
        public void TickInterval_IncludesAndPositionsMaximumMark()
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Min, 0);
                x.Add(p => p.Max, 100);
                x.Add(p => p.TickInterval, 30);
                x.Add(p => p.ShowTicks, true);
            });

            var marks = comp.FindAll(".vek-slider-tickmark");
            marks.Should().HaveCount(5);
            marks.Select(mark => mark.GetAttribute("style")).Should().Equal(
                "left:0%;",
                "left:30%;",
                "left:60%;",
                "left:90%;",
                "left:100%;");
        }

        /// <summary>
        /// Formats each generated numeric tick value with the supplied label formatter.
        /// </summary>
        [Test]
        public void TickMarkLabelFormatter_FormatsNumericScaleValues()
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Min, 0);
                x.Add(p => p.Max, 100);
                x.Add(p => p.TickInterval, 25);
                x.Add(p => p.ShowTicks, true);
                x.Add(p => p.TickMarkLabelFormatter, value => $"{value:0}%");
            });

            comp.FindAll(".vek-slider-track-tick-label")
                .Select(label => label.TextContent)
                .Should().Equal("0%", "25%", "50%", "75%", "100%");
        }

        /// <summary>
        /// Rounds user input to the nearest configured step before reporting the value change.
        /// </summary>
        [Test]
        public async Task InputValue_SnapsToNearestStep()
        {
            var changedValue = 0.0;
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Min, 100);
                x.Add(p => p.Max, 200);
                x.Add(p => p.Step, 25);
                x.Add(p => p.ValueChanged, value => changedValue = value);
            });

            await comp.Find(".vek-slider-input").InputAsync("164");

            changedValue.Should().Be(175);
            ((IHtmlInputElement)comp.Find(".vek-slider-input")).Value.Should().Be("175");
        }

        [Test]
        [TestCase(0.0, 100.0, 25.0, 5)]
        [TestCase(0.0, 100.0, 10.0, 11)]
        [TestCase(0.0, 100.0, 1.0, 101)]

        [TestCase(100.0, 200.0, 25.0, 5)]
        [TestCase(-200.0, -100.0, 25.0, 5)]
        public void TickMarksEnabled_CheckAmount(double min, double max, double step, int expectedAmount)
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Min, min);
                x.Add(p => p.Max, max);
                x.Add(p => p.Step, step);
                x.Add(p => p.TickMarks, true);
            });

            comp.Nodes.Should().ContainSingle();

            IElement TickMarks() => comp.Find(".vek-slider-tickmarks");
            TickMarks().Children.Should().HaveCount(expectedAmount);
        }

        [Test]
        [TestCase(0.0, 100.0, 0, "0")]
        [TestCase(0.0, 100.0, 20, "20")]
        [TestCase(0.0, 100.0, 22.5, "22.5")]
        [TestCase(0.0, 100.0, 50, "50")]
        [TestCase(0.0, 100.0, 100, "100")]

        [TestCase(0.0, 1.0, 0.0, "0")]
        [TestCase(0.0, 1.0, 0.2, "20")]
        [TestCase(0.0, 1.0, 0.5, "50")]
        [TestCase(0.0, 1.0, 1, "100")]

        [TestCase(1.0, 2.0, 1.0, "0")]
        [TestCase(1.0, 2.0, 1.2, "20")]
        [TestCase(1.0, 2.0, 1.5, "50")]
        [TestCase(1.0, 2.0, 2, "100")]

        [TestCase(-100.0, 100.0, -100.0, "0")]
        [TestCase(-100.0, 100.0, -50, "25")]
        [TestCase(-100.0, 100.0, 0, "50")]
        [TestCase(-100.0, 100.0, 50, "75")]
        [TestCase(-100.0, 100.0, 100, "100")]

        [TestCase(0.0, 100.0, 110, "100")]
        [TestCase(0.0, 100.0, -10, "0")]
        [TestCase(-200.0, -100.0, -90, "100")]
        [TestCase(-200.0, -100.0, -210, "0")]
        public void Percentage(double min, double max, double value, string expectedPercentage)
        {
            var cultures = new[] {
                new CultureInfo("en-us", false),
                new CultureInfo("de-DE", false),
                new CultureInfo("he-IL", false),
                new CultureInfo("ar-ER", false),
            };

            foreach (var culture in cultures)
            {
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;

                var comp = Context.Render<VekSlider<double>>(x =>
                {
                    x.Add(p => p.Max, max);
                    x.Add(p => p.Min, min);
                    x.Add(p => p.Value, value);
                    x.Add(p => p.Variant, Variant.Filled);
                    x.Add(p => p.ValueLabel, true);
                });

                IElement Thumb() => comp.Find(".vek-slider-value-label");
                Thumb().GetAttribute("style").Should().Be($"left:{expectedPercentage}%;");

                IElement Filling() => comp.Find(".vek-slider-filled");
                Filling().GetAttribute("style").Should().Be($"width:{expectedPercentage}%;");
            }
        }

        [Test]
        [TestCase(0.0, 100.0, 50, "50")]
        [TestCase(0.0, 100.0, 25, "25")]
        [TestCase(0.0, 100.0, 0, "0")]
        [TestCase(0.0, 100.0, 100, "100")]
        public void ValueLabelPosition_Rtl(double min, double max, double value, string expectedPercentage)
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Max, max);
                x.Add(p => p.Min, min);
                x.Add(p => p.Value, value);
                x.Add(p => p.ValueLabel, true);
                x.AddCascadingValue("RightToLeft", true);
            });

            IElement ValueLabel() => comp.Find(".vek-slider-value-label");
            ValueLabel().GetAttribute("style").Should().Be($"right:{expectedPercentage}%;");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task CheckInput(bool immediate)
        {
            var comp = Context.Render<VekSlider<double>>(x =>
            {
                x.Add(p => p.Max, 200);
                x.Add(p => p.Min, 100);
                x.Add(p => p.Value, 150);
                x.Add(p => p.Immediate, immediate);
                x.Add(p => p.Variant, Variant.Filled);
            });

            IElement Input() => comp.Find(".vek-slider-input");
            IElement Filling() => comp.Find(".vek-slider-filled");
            var value = "180";

            if (immediate == false)
            {
                Assert.ThrowsAsync<MissingEventHandlerException>(() => Input().InputAsync(value));
                await Input().ChangeAsync(value);
            }
            else
            {
                Assert.ThrowsAsync<MissingEventHandlerException>(() => Input().ChangeAsync(value));
                await Input().InputAsync(value);
            }

            Filling().GetAttribute("style").Should().Be($"width:80%;");
        }

        [Test]
        [TestCase(null)]
        [TestCase(0.0)]
        [TestCase(20.5)]
        [TestCase(75.5)]
        [TestCase(100.0)]
        [SetCulture("en-US")]
        public async Task NullableBinding(double? value)
        {
            var comp = Context.Render<SliderWithNullable>();
            comp.Instance.NullableValue.Should().BeNull();

            IElement Input() => comp.Find(".vek-slider-input");
            await Input().InputAsync(new ChangeEventArgs { Value = value.ToString() });
            comp.Instance.NullableValue.Should().Be(value);
        }

        [Test]
        [TestCase(null)]
        [TestCase(0.0)]
        [TestCase(20.5)]
        [TestCase(75.5)]
        [TestCase(100.0)]
        [SetCulture("en-US")]
        public async Task TwoBindValues1(double? value)
        {
            var comp = Context.Render<SliderWithTwoBindValues>();
            comp.Instance.NullableValue.Should().BeNull();
            comp.Instance.Value.Should().Be(0);

            IElement Input() => comp.Find(".vek-slider-input");
            await Input().InputAsync(new ChangeEventArgs { Value = value.ToString() });
            comp.Instance.NullableValue.Should().Be(value);
            comp.Instance.Value.Should().Be(value.GetValueOrDefault(0));
        }

        [Test]
        public void TwoBindValues2()
        {
            var comp = Context.Render<SliderWithTwoBindValues>();
            comp.Instance.NullableValue.Should().BeNull();
            comp.Instance.Value.Should().Be(0);

            comp.Instance.Value = 1.5;
            comp.Render();
            comp.Instance.NullableValue.Should().Be(1.5);
            comp.Instance.Value.Should().Be(1.5);

            comp.Instance.Value = 0;
            comp.Render();
            comp.Instance.NullableValue.Should().Be(0);
            comp.Instance.Value.Should().Be(0);

            comp.Instance.NullableValue = 75.5;
            comp.Render();
            comp.Instance.NullableValue.Should().Be(75.5);
            comp.Instance.Value.Should().Be(75.5);

            comp.Instance.NullableValue = null;
            comp.Render();
            comp.Instance.NullableValue.Should().Be(null);
            comp.Instance.Value.Should().Be(0);
        }

        [Test]
        [TestCase(0.0, "$0.00")]
        [TestCase(20.5, "$20.50")]
        [TestCase(75.5, "$75.50")]
        [TestCase(100.0, "$100.00")]
        public void CustomCultureAndFormatting(decimal value, string expectedValueLabel)
        {
            var customCulture = (CultureInfo)CultureInfo.GetCultureInfo("en").Clone();
            customCulture.NumberFormat.CurrencySymbol = "$";
            var comp = Context.Render<VekSlider<decimal>>(x =>
            {
                x.Add(p => p.Value, value);
                x.Add(p => p.Step, 0.5m);
                x.Add(p => p.ValueLabel, true);
                x.Add(p => p.Culture, customCulture);
                x.Add(p => p.ValueLabelFormat, "C");
            });

            IElement ValueLabel() => comp.Find(".vek-slider-value-label");
            ValueLabel().TextContent.Should().Be(expectedValueLabel);
        }

        [Test]
        public void CustomValueLabelContent()
        {
            var comp = Context.Render<SliderWithCustomValueLabelContentTest>();
            IElement AlertText() => VekAlert().Find("div.vek-alert-message");
            IRenderedComponent<VekAlert> VekAlert() => comp.FindComponent<VekAlert>();
            AlertText().InnerHtml.Should().Be("20");
        }
    }
}
