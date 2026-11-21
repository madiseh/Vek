// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.TimePicker
{
    [TestFixture]
    public class VekClockFaceTests : BunitTest
    {
        [Test]
        public void ClockFace_DefaultRender_ShouldHaveExpectedElements()
        {
            var comp = Context.Render<VekClockFace>();

            comp.Find(".vek-clock-face").Should().NotBeNull();
            comp.Find("svg.vek-clock-face-dial").Should().NotBeNull();
            comp.Find(".vek-clock-face-pin").Should().NotBeNull();
            comp.Find(".vek-clock-face-hand").Should().NotBeNull();
            comp.Find(".vek-clock-face-thumb").Should().NotBeNull();

            // Default view is Hours (12-hour)
            var numbers = comp.FindAll(".vek-clock-dial-number");
            numbers.Count.Should().Be(12);
        }

        [Test]
        public void ClockFace_24HourMode_ShouldRenderInnerAndOuterRings()
        {
            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.TimeEditFormat, TimeEditFormat.TwentyFourHour)
                .Add(p => p.Time, new TimeSpan(15, 30, 0)));

            // Outer ring has 12 numbers (1-12), inner ring has 12 numbers (13-24/00)
            var allNumbers = comp.FindAll(".vek-clock-dial-number");
            allNumbers.Count.Should().Be(24);

            var innerNumbers = comp.FindAll(".vek-clock-dial-inner");
            innerNumbers.Count.Should().Be(12);

            // 15 should be selected in inner ring
            var selected = comp.Find(".vek-clock-dial-number.vek-selected");
            selected.Should().NotBeNull();
            selected.GetAttribute("data-value").Should().Be("15");
        }

        [Test]
        public void ClockFace_MinutesView_ShouldRenderTicksAndIncrements()
        {
            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.View, OpenTo.Minutes)
                .Add(p => p.Time, new TimeSpan(10, 25, 0)));

            // 12 numbers (00, 05, ... 55)
            var numbers = comp.FindAll(".vek-clock-dial-number");
            numbers.Count.Should().Be(12);

            // 60 tick marks
            var ticks = comp.FindAll(".vek-clock-dial-tick");
            ticks.Count.Should().Be(60);

            // Selected minute is 25
            var selected = comp.Find(".vek-clock-dial-number.vek-selected");
            selected.Should().NotBeNull();
            selected.GetAttribute("data-value").Should().Be("25");
        }

        [Test]
        public void ClockFace_SecondsView_ShouldRenderTicksAndIncrements()
        {
            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.View, OpenTo.Seconds)
                .Add(p => p.ShowSeconds, true)
                .Add(p => p.Time, new TimeSpan(10, 25, 45)));

            var numbers = comp.FindAll(".vek-clock-dial-number");
            numbers.Count.Should().Be(12);

            var selected = comp.Find(".vek-clock-dial-number.vek-selected");
            selected.Should().NotBeNull();
            selected.GetAttribute("data-value").Should().Be("45");
        }

        [Test]
        public async Task ClockFace_ClickHour_ShouldUpdateAndAutoTransitionToMinutes()
        {
            TimeSpan? selectedTime = new TimeSpan(1, 0, 0);
            OpenTo currentView = OpenTo.Hours;

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.View, currentView)
                .Add(p => p.ViewChanged, (OpenTo v) => currentView = v)
                .Add(p => p.AutoTransition, true));

            // Click hour "4"
            var item4 = comp.Find("[data-value='4']");
            await item4.ClickAsync();

            selectedTime?.Hours.Should().Be(4);
            currentView.Should().Be(OpenTo.Minutes);
        }

        [Test]
        public async Task ClockFace_Click24HourInnerRing_ShouldSelectHourAbove12()
        {
            TimeSpan? selectedTime = new TimeSpan(10, 0, 0);
            OpenTo currentView = OpenTo.Hours;

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.TimeEditFormat, TimeEditFormat.TwentyFourHour)
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.View, currentView)
                .Add(p => p.ViewChanged, (OpenTo v) => currentView = v)
                .Add(p => p.AutoTransition, true));

            // Click inner ring "18" (6 o'clock inner)
            var item18 = comp.Find(".vek-clock-dial-inner[data-value='18']");
            await item18.ClickAsync();

            selectedTime?.Hours.Should().Be(18);
            currentView.Should().Be(OpenTo.Minutes);
        }

        [Test]
        public async Task ClockFace_Click24HourInnerRing00_ShouldSelectHourZero()
        {
            TimeSpan? selectedTime = new TimeSpan(10, 0, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.TimeEditFormat, TimeEditFormat.TwentyFourHour)
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.AutoTransition, false));

            var item00 = comp.Find(".vek-clock-dial-inner[data-value='0']");
            await item00.ClickAsync();

            selectedTime?.Hours.Should().Be(0);
        }

        [Test]
        public async Task ClockFace_PointerDrag_ShouldUpdateSelectedValue()
        {
            TimeSpan? selectedTime = new TimeSpan(0, 0, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.Time, selectedTime)
                .Add(p => p.AmPm, true)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.AutoTransition, false));

            var dial = comp.Find("svg.vek-clock-face-dial");

            // Pointer down at 3 o'clock position (x=232, y=130)
            dial.PointerDown(new PointerEventArgs { OffsetX = 232, OffsetY = 130 });
            comp.Instance.IsDragging.Should().BeTrue();

            // Drag to 6 o'clock position (x=130, y=232)
            dial.PointerMove(new PointerEventArgs { OffsetX = 130, OffsetY = 232 });

            // Pointer up at 6 o'clock
            dial.PointerUp(new PointerEventArgs { OffsetX = 130, OffsetY = 232 });
            comp.Instance.IsDragging.Should().BeFalse();

            selectedTime?.Hours.Should().Be(6);
        }

        [Test]
        public async Task ClockFace_PointerDrag_In24HourMode_ShouldDifferentiateInnerAndOuterRings()
        {
            TimeSpan? selectedTime = new TimeSpan(12, 0, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.TimeEditFormat, TimeEditFormat.TwentyFourHour)
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.AutoTransition, false));

            var dial = comp.Find("svg.vek-clock-face-dial");

            // Pointer on inner ring at 3 o'clock (center=130, innerRadius=68 -> x=198, y=130)
            dial.PointerDown(new PointerEventArgs { OffsetX = 198, OffsetY = 130 });
            dial.PointerUp(new PointerEventArgs { OffsetX = 198, OffsetY = 130 });

            // Inner ring 3 o'clock is 15:00
            selectedTime?.Hours.Should().Be(15);

            // Pointer on outer ring at 3 o'clock (center=130, outerRadius=102 -> x=232, y=130)
            dial.PointerDown(new PointerEventArgs { OffsetX = 232, OffsetY = 130 });
            dial.PointerUp(new PointerEventArgs { OffsetX = 232, OffsetY = 130 });

            // Outer ring 3 o'clock is 3:00
            selectedTime?.Hours.Should().Be(3);
        }

        [Test]
        public async Task ClockFace_KeyboardNavigation_ArrowKeys_ShouldRotateHand()
        {
            TimeSpan? selectedTime = new TimeSpan(3, 0, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.View, OpenTo.Hours));

            var dial = comp.Find("svg.vek-clock-face-dial");

            // ArrowRight -> increases hour to 4
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
            selectedTime?.Hours.Should().Be(4);

            // ArrowUp -> increases hour to 5
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
            selectedTime?.Hours.Should().Be(5);

            // ArrowLeft -> decreases hour to 4
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });
            selectedTime?.Hours.Should().Be(4);

            // ArrowDown -> decreases hour to 3
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
            selectedTime?.Hours.Should().Be(3);
        }

        [Test]
        public async Task ClockFace_KeyboardNavigation_Minutes_ShouldIncrementByMinuteStep()
        {
            TimeSpan? selectedTime = new TimeSpan(3, 10, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.View, OpenTo.Minutes)
                .Add(p => p.MinuteSelectionStep, 5));

            var dial = comp.Find("svg.vek-clock-face-dial");

            // ArrowRight increases by MinuteSelectionStep (5) -> 15
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
            selectedTime?.Minutes.Should().Be(15);

            // ArrowLeft decreases by MinuteSelectionStep (5) -> 10
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });
            selectedTime?.Minutes.Should().Be(10);
        }

        [Test]
        public async Task ClockFace_KeyboardEnter_ShouldTriggerAutoTransition()
        {
            OpenTo currentView = OpenTo.Hours;
            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.View, currentView)
                .Add(p => p.ViewChanged, (OpenTo v) => currentView = v)
                .Add(p => p.AutoTransition, true));

            var dial = comp.Find("svg.vek-clock-face-dial");
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

            currentView.Should().Be(OpenTo.Minutes);
        }

        [Test]
        public void ClockFace_AriaAttributes_ShouldBeSetCorrectly()
        {
            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.Time, new TimeSpan(8, 30, 0))
                .Add(p => p.View, OpenTo.Hours));

            var dial = comp.Find("svg.vek-clock-face-dial");
            dial.GetAttribute("role").Should().Be("slider");
            dial.GetAttribute("aria-label").Should().Be("Select hours");
            dial.GetAttribute("aria-valuenow").Should().Be("8");
            dial.GetAttribute("aria-valuemin").Should().Be("1");
            dial.GetAttribute("aria-valuemax").Should().Be("12");
            dial.GetAttribute("aria-valuetext").Should().Be("8 hours");
        }

        [Test]
        public async Task ClockFace_Disabled_ShouldNotRespondToInteractions()
        {
            TimeSpan? selectedTime = new TimeSpan(2, 0, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.Disabled, true));

            comp.Find(".vek-clock-face.vek-disabled").Should().NotBeNull();
            var dial = comp.Find("svg.vek-clock-face-dial");
            dial.GetAttribute("tabindex").Should().Be("-1");

            // Attempt click
            var item = comp.Find("[data-value='5']");
            await item.ClickAsync();
            selectedTime?.Hours.Should().Be(2);

            // Attempt keyboard
            await dial.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
            selectedTime?.Hours.Should().Be(2);
        }

        [Test]
        public async Task ClockFace_ReadOnly_ShouldNotRespondToInteractions()
        {
            TimeSpan? selectedTime = new TimeSpan(2, 0, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.ReadOnly, true));

            comp.Find(".vek-clock-face.vek-readonly").Should().NotBeNull();

            // Attempt click
            var item = comp.Find("[data-value='5']");
            await item.ClickAsync();
            selectedTime?.Hours.Should().Be(2);
        }

        [Test]
        public async Task ClockFace_ShowHeader_ShouldAllowSwitchingViewsAndAmPm()
        {
            OpenTo currentView = OpenTo.Hours;
            TimeSpan? selectedTime = new TimeSpan(9, 15, 0);

            var comp = Context.Render<VekClockFace>(parameters => parameters
                .Add(p => p.ShowHeader, true)
                .Add(p => p.Time, selectedTime)
                .Add(p => p.TimeChanged, (TimeSpan? t) => selectedTime = t)
                .Add(p => p.View, currentView)
                .Add(p => p.ViewChanged, (OpenTo v) => currentView = v));

            comp.Find(".vek-clock-face-header").Should().NotBeNull();

            // Click minute button in header
            var minuteBtn = comp.Find("button[aria-label='Switch to minutes']");
            await minuteBtn.ClickAsync();
            currentView.Should().Be(OpenTo.Minutes);

            // Click PM button
            var pmBtn = comp.Find("button[aria-label='Set PM']");
            await pmBtn.ClickAsync();
            selectedTime?.Hours.Should().Be(21); // 9 + 12 = 21 (9 PM)
        }
    }
}
