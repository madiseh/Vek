using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Extensions;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
#nullable enable
    [TestFixture]
    public class RatingTests : BunitTest
    {
        /// <summary>
        /// Click should change selected value
        /// </summary>
        [Test]
        public async Task RatingTest1()
        {
            var comp = Context.Render<VekRating>();
            // select elements needed for the test
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");
            IReadOnlyList<IElement> Inputs() => comp.FindAll("input[type=\"radio\"].vek-rating-input");
            // check initial state
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(0);
            RatingItemsSpans().Count.Should().Be(5);
            Inputs().Count.Should().Be(5);

            // click first rating item
            await RatingItemsSpans()[0].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(1);

            // click 2nd rating item
            await RatingItemsSpans()[1].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);

            // click 3rd rating item
            await RatingItemsSpans()[2].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(3);

            // click 4th rating item
            await RatingItemsSpans()[3].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(4);

            // click 5th rating item
            await RatingItemsSpans()[4].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(5);

            await RatingItemsSpans()[1].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);
        }

        /// <summary>
        /// Click already selected item should change selected value to 0
        /// </summary>
        [Test]
        public async Task RatingTest2()
        {
            var comp = Context.Render<VekRating>();
            // select elements needed for the test
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");
            // check initial state
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(0);
            RatingItemsSpans().Count.Should().Be(5);

            // click 2nd rating item
            await RatingItemsSpans()[1].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);

            // click 2nd rating item
            await RatingItemsSpans()[1].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(0);

            // click 2nd rating item
            await RatingItemsSpans()[1].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);

            // click first rating item
            await RatingItemsSpans()[0].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(1);

            // click first rating item
            await RatingItemsSpans()[0].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(0);

            // click first rating item
            await RatingItemsSpans()[0].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(1);
        }

        /// <summary>
        /// Initialized selected value by parameter should equal component selected value
        /// </summary>
        [Test]
        public void RatingTest3()
        {
            var comp = Context.Render<VekRating>(parameters => parameters
                .Add(p => p.SelectedValue, 3));
            // print the generated html
            // check initial state
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(3);
        }

        /// <summary>
        /// Click disabled component don't change SelectedValue
        /// </summary>
        [Test]
        public async Task RatingTest4()
        {
            var comp = Context.Render<VekRating>(parameters => parameters
                .Add(p => p.Disabled, true)
                .Add(p => p.SelectedValue, 2));
            // print the generated html
            // select elements needed for the test
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");
            // check initial state
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);
            RatingItemsSpans().Count.Should().Be(5);

            // click first rating item
            await RatingItemsSpans()[0].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);

            // click 2nd rating item
            await RatingItemsSpans()[1].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);

            // click 3rd rating item
            await RatingItemsSpans()[2].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);

            // click 4th rating item
            await RatingItemsSpans()[3].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);

            // click 5th rating item
            await RatingItemsSpans()[4].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);
        }

        /// <summary>
        /// Initialized MaxValue by parameter should equal rating items count.
        /// </summary>
        [Test]
        public async Task RatingTest5()
        {
            var comp = Context.Render<VekRating>(parameters => parameters
                .Add(p => p.MaxValue, 12));
            // print the generated html
            // select elements needed for the test
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");
            // check initial state
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(0);
            RatingItemsSpans().Count.Should().Be(12);

            await comp.Instance.HandleItemHoveredAsync(6);
            comp.Instance.HoveredValue.Should().Be(6);
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(0);
            comp.Instance.IsRatingHover.Should().Be(true);
        }

        /// <summary>
        /// Initialized EmptyIconColor and FullIconColor by parameter should have the correct colors set.
        /// </summary>
        [Test]
        public async Task RatingTestIconColors()
        {
            var comp = Context.Render<VekRating>(parameters => parameters
                .Add(p => p.SelectedValue, 2)
                .Add(p => p.EmptyIconColor, Color.Tertiary)
                .Add(p => p.FullIconColor, Color.Primary));

            // Select elements needed for the test
            IReadOnlyList<IElement> SvgColors() => comp.FindAll("svg.vek-icon-root");
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");

            // Check initial state
            SvgColors()[0].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[1].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[2].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[3].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[4].ClassName.Should().Contain("vek-tertiary-text");

            comp.Instance.GetState(x => x.SelectedValue).Should().Be(2);
            await RatingItemsSpans()[0].ClickAsync();
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(1);

            SvgColors()[0].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[1].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[2].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[3].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[4].ClassName.Should().Contain("vek-tertiary-text");

            RatingItemsSpans()[2].PointerOver();
            comp.Instance.HoveredValue.Should().Be(3);
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(1);
            comp.Instance.IsRatingHover.Should().Be(true);

            SvgColors()[0].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[1].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[2].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[3].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[4].ClassName.Should().Contain("vek-tertiary-text");
            RatingItemsSpans()[2].ClassName.Should().Contain("vek-rating-item-active");

            RatingItemsSpans()[2].PointerOut();

            await RatingItemsSpans()[4].ClickAsync();
            RatingItemsSpans()[1].PointerOver();
            comp.Instance.HoveredValue.Should().Be(2);
            comp.Instance.GetState(x => x.SelectedValue).Should().Be(5);
            comp.Instance.IsRatingHover.Should().Be(true);

            SvgColors()[0].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[1].ClassName.Should().Contain("vek-primary-text");
            SvgColors()[2].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[3].ClassName.Should().Contain("vek-tertiary-text");
            SvgColors()[4].ClassName.Should().Contain("vek-tertiary-text");
            RatingItemsSpans()[1].ClassName.Should().Contain("vek-rating-item-active");

            RatingItemsSpans()[1].PointerOut();
        }

        [Test]
        public async Task Rating_RippleOffByDefault()
        {
            var comp = Context.Render<VekRating>();
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");

            RatingItemsSpans()[0].ClassName.Should().NotContain("vek-ripple");

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Ripple, true));
            RatingItemsSpans()[0].ClassName.Should().Contain("vek-ripple");
        }

        [Test]
        public async Task Rating_RippleClass_SuppressedWhenReadOnly()
        {
            var comp = Context.Render<VekRating>(parameters => parameters.Add(p => p.Ripple, true));
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");

            RatingItemsSpans()[0].ClassName.Should().Contain("vek-ripple");

            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(p => p.Ripple, true)
                .Add(p => p.ReadOnly, true));
            RatingItemsSpans()[0].ClassName.Should().NotContain("vek-ripple");
        }

        [Test]
        public async Task Rating_RootGetsReadOnlyClassWhenReadOnly()
        {
            var comp = Context.Render<VekRating>();
            comp.Find("span.vek-rating-root").ClassName.Should().NotContain("vek-readonly");

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.ReadOnly, true));
            comp.Find("span.vek-rating-root").ClassName.Should().Contain("vek-readonly");
        }

        [Test]
        public void Rating_ReadOnly_DoesNotActivateItemOnHover()
        {
            var comp = Context.Render<VekRating>(parameters => parameters.Add(p => p.ReadOnly, true));
            IReadOnlyList<IElement> RatingItemsSpans() => comp.FindAll("span.vek-rating-item");

            RatingItemsSpans()[2].PointerOver();
            comp.Instance.HoveredValue.Should().BeNull();
            RatingItemsSpans()[2].ClassName.Should().NotContain("vek-rating-item-active");
        }

        [Test]
        public async Task Rating_KeyboardNavigation()
        {
            var comp = Context.Render<VekRating>(parameters => parameters
                .Add(p => p.MaxValue, 12));
            var item = comp.FindComponent<VekRatingItem>();
            // print the generated html

            await comp.InvokeAsync(() => item.Instance.HandlePointerOutAsync(new PointerEventArgs()));
            await comp.InvokeAsync(() => item.Instance.HandlePointerOverAsync(new PointerEventArgs()));

            await comp.InvokeAsync(() => comp.Instance.SetHoveredValueAsync(15));
            await comp.InvokeAsync(() => item.Instance.SelectIcon());
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.SelectedValue, 12));
            await comp.InvokeAsync(() => comp.Instance.SetHoveredValueAsync(0));
            await comp.InvokeAsync(() => item.Instance.SelectIcon());
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.SelectedValue, 0));

            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(1));

            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(0));
            //ArrowLeft should not decrease when the value is 0
            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(0));

            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight", ShiftKey = true, Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(12));
            //Shift+ArrowKey should not go beyond the max value
            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight", ShiftKey = true, Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(12));

            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft", ShiftKey = true, Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(0));

            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft", ShiftKey = true, Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(0));

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.Disabled, true));
            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight", Type = "keydown", }));
            await comp.WaitForAssertionAsync(() => comp.Instance.GetState(x => x.SelectedValue).Should().Be(0));

            await comp.InvokeAsync(() => item.Instance.HandlePointerOutAsync(new PointerEventArgs()));
            await comp.InvokeAsync(() => item.Instance.HandlePointerOverAsync(new PointerEventArgs()));
        }

        /// <summary>
        /// The rating group is named and its radios drop the unsupported aria-readonly.
        /// </summary>
        [Test]
        public void Rating_ShouldExposeAccessibleNameAndValidRadioAttributes()
        {
            var comp = Context.Render<VekRating>();

            comp.Find("span.vek-rating-root").GetAttribute("aria-label").Should().Be("Rating");
            // aria-readonly is not supported on the radio role; it lives on the radiogroup instead.
            comp.FindAll("input.vek-rating-input").Should().OnlyContain(input => !input.HasAttribute("aria-readonly"));
            comp.Find("span.vek-rating-root").GetAttribute("aria-readonly").Should().Be("false");
        }

        /// <summary>
        /// A user-supplied aria-label replaces the default rating name.
        /// </summary>
        [Test]
        public void Rating_ShouldAllowUserAriaLabelOverride()
        {
            var comp = Context.Render<VekRating>(parameters => parameters.AddUnmatched("aria-label", "Quality"));

            comp.Find("span.vek-rating-root").GetAttribute("aria-label").Should().Be("Quality");
        }

        /// <summary>
        /// Half-star hover previews a fractional score without changing the selected value until clicked.
        /// </summary>
        [Test]
        public async Task Rating_HalfPrecision_PreviewsAndSelectsFractionalValue()
        {
            double? preview = null;
            var comp = Context.Render<VekRating>(parameters => parameters
                .Add(p => p.Precision, RatingPrecision.Half)
                .Add(p => p.Value, 2d)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<double>(this, _ => { }))
                .Add(p => p.HoveredScoreChanged, EventCallback.Factory.Create<double?>(this, value => preview = value)));
            IReadOnlyList<IElement> items() => comp.FindAll("span.vek-rating-item");

            items().Count.Should().Be(10);
            await items()[4].PointerOverAsync();
            preview.Should().Be(2.5);
            comp.Instance.GetState(x => x.Value).Should().Be(2);

            await items()[4].ClickAsync();
            comp.Instance.GetState(x => x.Value).Should().Be(2.5);
            comp.Find("input[value='2.5']").HasAttribute("checked").Should().BeTrue();
        }

        /// <summary>
        /// Half-star precision moves keyboard selection by half steps.
        /// </summary>
        [Test]
        public async Task Rating_HalfPrecision_KeyboardMovesByHalfSteps()
        {
            var comp = Context.Render<VekRating>(parameters => parameters
                .Add(p => p.Precision, RatingPrecision.Half)
                .Add(p => p.Value, 1d)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<double>(this, _ => { })));

            await comp.InvokeAsync(() => comp.Instance.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" }));
            comp.Instance.GetState(x => x.Value).Should().Be(1.5);
        }
    }
}
