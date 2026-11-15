using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Vek.Extensions;
using Vek.UnitTests.TestComponents.Stepper;
using NUnit.Framework;

#nullable enable
namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class StepperTests : BunitTest
    {
        [Test]
        public void StepperContent_ShouldDisplayActiveStepContent()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.SecondaryText, "a");
                    step.Add(x => x.Class, "step-a");
                    step.Add(x => x.Style, "fontsize:11px");
                    step.Add(x => x.ChildContent, builder => builder.AddMarkupContent(0, "step 1"));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.SecondaryText, "b");
                    step.Add(x => x.ChildContent, builder => builder.AddMarkupContent(0, "step 2"));
                    step.Add(x => x.Class, "step-b");
                    step.Add(x => x.Style, "fontsize:12px");
                });
            });
            // check the steps
            stepper.Instance.Steps.Count.Should().Be(2);
            stepper.Instance.Steps.All(x => ((VekStep)x).Parent == stepper.Instance).Should().Be(true);
            stepper.Instance.Steps[0].Title.Should().Be("A");
            stepper.Instance.Steps[0].IsActive.Should().Be(true);
            stepper.Instance.Steps[1].Title.Should().Be("B");
            stepper.Instance.Steps[1].IsActive.Should().Be(false);
            // check the DOM
            stepper.FindAll(".vek-step-label-icon")[0].TextContent.Trimmed().Should().Be("1");
            stepper.FindAll(".vek-step-label-content .vek-typography-subtitle2")[0].TextContent.Trimmed().Should().Be("A");
            stepper.FindAll(".vek-step-label-content .vek-typography-caption")[0].TextContent.Trimmed().Should().Be("a");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            stepper.FindAll(".vek-step-label-content .vek-typography-body2")[0].TextContent.Trimmed().Should().Be("B");
            stepper.FindAll(".vek-step-label-content .vek-typography-caption")[1].TextContent.Trimmed().Should().Be("b");
            stepper.Find(".vek-stepper-content").GetAttribute("class").Should().Contain("step-a");
            stepper.Find(".vek-stepper-content").GetAttribute("style").Should().Contain("fontsize:11px");
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
        }

        [Test]
        public void ActionsClass_AppliesCustomClass_Issue13478()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.ActionsClass, "sticky-actions");
            });

            stepper.Find(".vek-stepper-actions").ClassList.Should().Contain("sticky-actions");
        }

        [Test]
        public async Task StepperStepContext_ShouldBeAvailableInsideChildContent()
        {
            VekStepContext? firstStepContext = null;
            VekStepContext? secondStepContext = null;

            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, builder =>
                    {
                        builder.OpenComponent<StepContextRecorder>(0);
                        builder.AddAttribute(1, nameof(StepContextRecorder.ActiveMarkup), "step-1-active");
                        builder.AddAttribute(2, nameof(StepContextRecorder.Capture), (Action<VekStepContext?>)(context => firstStepContext = context));
                        builder.CloseComponent();
                    });
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, builder =>
                    {
                        builder.OpenComponent<StepContextRecorder>(0);
                        builder.AddAttribute(1, nameof(StepContextRecorder.ActiveMarkup), "step-2-active");
                        builder.AddAttribute(2, nameof(StepContextRecorder.Capture), (Action<VekStepContext?>)(context => secondStepContext = context));
                        builder.CloseComponent();
                    });
                });
            });

            firstStepContext.Should().NotBeNull();
            firstStepContext!.Step.Should().Be(stepper.Instance.Steps[0]);
            firstStepContext.IsActive.Should().BeTrue();
            secondStepContext.Should().BeNull();
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Be("step-1-active");

            await stepper.FindAll(".vek-step")[1].ClickAsync();

            secondStepContext.Should().NotBeNull();
            secondStepContext!.Step.Should().Be(stepper.Instance.Steps[1]);
            secondStepContext.IsActive.Should().BeTrue();
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Be("step-2-active");
        }

        [Test]
        public async Task Stepper_ShouldDisplayContentOfActiveStep()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.SecondaryText, "a");
                    step.Add(x => x.Class, "step-a");
                    step.Add(x => x.Style, "fontsize:11px");
                    step.Add(x => x.ChildContent, builder => builder.AddMarkupContent(0, "step 1"));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.SecondaryText, "b");
                    step.Add(x => x.ChildContent, builder => builder.AddMarkupContent(0, "step 2"));
                    step.Add(x => x.Class, "step-b");
                    step.Add(x => x.Style, "fontsize:12px");
                });
            });
            // check the stepper content
            stepper.Find(".vek-stepper-content").GetAttribute("class").Should().Contain("step-a");
            stepper.Find(".vek-stepper-content").GetAttribute("style").Should().Contain("fontsize:11px");
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            await stepper.FindAll(".vek-step")[1].ClickAsync();
            stepper.Find(".vek-stepper-content").GetAttribute("class").Should().Contain("step-b");
            stepper.Find(".vek-stepper-content").GetAttribute("style").Should().Contain("fontsize:12px");
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 2");
            await stepper.FindAll(".vek-step")[0].ClickAsync();
            stepper.Find(".vek-stepper-content").GetAttribute("class").Should().Contain("step-a");
            stepper.Find(".vek-stepper-content").GetAttribute("style").Should().Contain("fontsize:11px");
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
        }

        [Test]
        public async Task Stepper_ShouldNavigateViaNextAndPrevious()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, false);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, builder => builder.AddMarkupContent(0, "step 1"));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, builder => builder.AddMarkupContent(0, "step 2"));
                });
            });
            // check the stepper content
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(true); // previous
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(false); // next
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(0); // can't complete yet
            // step 1 icon should be "1", step 2 icon should be "2"
            stepper.FindAll(".vek-step-label-icon")[0].TextContent.Trimmed().Should().Be("1");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            await stepper.Find(".vek-stepper-button-next").ClickAsync(); // next
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 2");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(false); // previous
            stepper.FindAll(".vek-stepper-button-next").Count.Should().Be(0); // no next button when completable
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(1);
            // step 1 icon should be a check mark, step 2 icon should be "2"
            stepper.FindAll(".vek-step-label-icon")[0].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            await stepper.Find(".vek-stepper-button-previous").ClickAsync(); // prev
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(true); // previous
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(false); // next
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(0); // can't complete yet
            // step 1 icon should be a check mark, step 2 icon should be "2"
            stepper.FindAll(".vek-step-label-icon")[0].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            await stepper.Find(".vek-stepper-button-next").ClickAsync(); // next
            stepper.FindAll(".vek-stepper-button-next").Count.Should().Be(0); // no next button when completable
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(1); // can complete now
            await stepper.Find(".vek-stepper-button-complete").ClickAsync(); // next
            stepper.FindAll(".vek-stepper-button-next").Count.Should().Be(0); // all done, no need to show the next button
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(0); // all done, no need to show the complete button
            // step 1 and 2 icon should be check marks
            stepper.FindAll(".vek-step-label-icon")[0].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step-label-icon")[1].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
        }

        [Test]
        public void Stepper_ShouldBeginWithFirstEnabledStep()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, true));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, true));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, false));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, false));
            });
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(2);
        }

        [Test]
        public async Task PreviousAndNext_ShouldCorrectlyHandleDisabledSteps()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, true));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, false));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, true));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Disabled, false));
            });

            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(false);
            await stepper.Find(".vek-stepper-button-next").ClickAsync(); // next

            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(3);
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(true);
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(false);
            await stepper.Find(".vek-stepper-button-previous").ClickAsync(); // prev

            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(true);
            await stepper.Find(".vek-stepper-button-previous").ClickAsync(); // prev
        }

        [Test]
        public async Task Stepper_ShouldBeAbleToSkipSkippableSteps()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, false);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.Skippable, true);
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
            });
            // check the stepper content
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(true); // previous
            stepper.Find(".vek-stepper-button-skip").IsDisabled().Should().Be(false); // skippable step
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(false); // next
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(0); // can't complete yet
            await stepper.Find(".vek-stepper-button-skip").ClickAsync(); // skip
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 2");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(false); // previous
            stepper.FindAll(".vek-stepper-button-skip").Count.Should().Be(0); // non-skippable step
            stepper.FindAll(".vek-stepper-button-next").Count.Should().Be(0); // no next button on last step
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(1); // next
            await stepper.Find(".vek-stepper-button-previous").ClickAsync(); // prev
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(true); // previous
            stepper.Find(".vek-stepper-button-skip").IsDisabled().Should().Be(false); // skippable step
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(false); // next
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(0); // can't complete yet, because step 2 is not Completed yet
        }

        [Test]
        public async Task ActiveIndex_ShouldBeTwoWayBindable()
        {
            var comp = Context.Render<StepperTwoWayBindingTestComponent>();
            var stepper1 = comp.FindComponents<VekStepper>()[0];
            var stepper2 = comp.FindComponents<VekStepper>()[1];
            stepper1.Instance.ActiveStep?.Title.Should().Be("A");
            stepper2.Instance.ActiveStep?.Title.Should().Be("X");
            stepper1.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step A");
            stepper2.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step X");
            // next
            await stepper1.Find(".vek-stepper-button-next").ClickAsync();
            stepper1.Instance.ActiveStep?.Title.Should().Be("B");
            stepper2.Instance.ActiveStep?.Title.Should().Be("Y");
            stepper1.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step B");
            stepper2.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step Y");
            // next
            await stepper1.Find(".vek-stepper-button-next").ClickAsync();
            stepper1.Instance.ActiveStep?.Title.Should().Be("C");
            stepper2.Instance.ActiveStep?.Title.Should().Be("Z");
            stepper1.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step C");
            stepper2.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step Z");
            // prev
            await stepper1.Find(".vek-stepper-button-previous").ClickAsync();
            stepper1.Instance.ActiveStep?.Title.Should().Be("B");
            stepper2.Instance.ActiveStep?.Title.Should().Be("Y");
            stepper1.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step B");
            stepper2.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("Step Y");
        }

        [Test]
        public async Task ManipulatingStepsProgrammatically_ShouldUpdateTheUi()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "C");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 3")));
                });
            });

            // this doesn't work on CI?
            // check render count. first render is just setup, second render renders the active step 
            // await stepper.WaitForAssertionAsync(() => stepper.RenderCount.Should().Be(2));

            // disable step 1
            stepper.FindAll(".vek-step")[0].IsDisabled().Should().Be(false);
            await stepper.InvokeAsync(async () => await stepper.Instance.Steps[0].SetDisabledAsync(true));
            stepper.FindAll(".vek-step")[0].IsDisabled().Should().Be(true);
            // fail step 2
            stepper.FindAll(".vek-step")[1].ClassList.Should().NotContain("vek-step-error");
            stepper.FindAll(".vek-step-label-icon")[1].ClassList.Should().NotContain("vek-error");
            stepper.FindAll(".vek-step-label-content")[1].ClassList.Should().NotContain("vek-error-text");
            await stepper.InvokeAsync(async () => await stepper.Instance.Steps[1].SetHasErrorAsync(true));
            stepper.FindAll(".vek-step")[1].ClassList.Should().Contain("vek-step-error");
            stepper.FindAll(".vek-step-label-icon")[1].ClassList.Should().Contain("vek-error");
            stepper.FindAll(".vek-step-label-content")[1].ClassList.Should().Contain("vek-error-text");
            // complete step 3
            stepper.FindAll(".vek-step")[2].ClassList.Should().NotContain("vek-step-completed");
            stepper.FindAll(".vek-step-label-icon")[2].QuerySelectorAll("path").Should().BeEmpty(); // no svg icon if not completed
            await stepper.InvokeAsync(async () => await stepper.Instance.Steps[2].SetCompletedAsync(true));
            stepper.FindAll(".vek-step-label-icon")[2].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step")[2].ClassList.Should().Contain("vek-step-completed");
        }

        [Test]
        public async Task FirstStep_ShouldBeActiveIfActiveIndexNotSet()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "A"));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "B"));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "C"));
            });
            await stepper.WaitForAssertionAsync(() => stepper.Instance.ActiveStep?.Title.Should().Be("A"));
            stepper.FindAll(".vek-step-label-icon")[0].TextContent.Trimmed().Should().Be("1");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            stepper.FindAll(".vek-step-label-icon")[2].TextContent.Trimmed().Should().Be("3");
        }

        [Test]
        public async Task InitialActiveIndex_ShouldBeRespectedIfSet()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.ActiveIndex, 1);
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "A"));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "B"));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "C"));
            });
            await stepper.WaitForAssertionAsync(() => stepper.Instance.ActiveStep?.Title.Should().Be("B"));
        }

        [Test]
        public async Task RemoveStep_ShouldUpdateActiveIndex()
        {
            int activeIndex = 2;
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Bind(x => x.ActiveIndex, activeIndex, newValue => activeIndex = newValue);
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "A"));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "B"));
                self.AddChildContent<VekStep>(step => step.Add(x => x.Title, "C"));
            });
            await stepper.WaitForAssertionAsync(() => stepper.Instance.ActiveStep?.Title.Should().Be("C"));
            activeIndex.Should().Be(2);
            // remove active step C, stepper should fall back to B  
            await stepper.InvokeAsync(async () => await stepper.Instance.RemoveStepAsync((VekStep)stepper.Instance.ActiveStep!));
            stepper.Instance.ActiveStep?.Title.Should().Be("B");
            activeIndex.Should().Be(1);
            // remove step A, stepper should remain on B, active index should fall back to 0  
            await stepper.InvokeAsync(async () => await stepper.Instance.RemoveStepAsync((VekStep)stepper.Instance.Steps[0]));
            stepper.Instance.ActiveStep?.Title.Should().Be("B");
            activeIndex.Should().Be(0);
            // remove active step B, stepper has no more steps, active index should be -1, active step should be null  
            await stepper.InvokeAsync(async () => await stepper.Instance.RemoveStepAsync((VekStep)stepper.Instance.ActiveStep!));
            stepper.Instance.ActiveStep.Should().BeNull();
            activeIndex.Should().Be(-1);
        }

        [Test]
        public async Task AddStep_ShouldUpdateActiveIndexAndStep()
        {
            int activeIndex = -1;
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Bind(x => x.ActiveIndex, activeIndex, newValue => activeIndex = newValue);
            });
            var stepper1 = Context.Render<VekStep>(parameters => parameters.Add(x => x.Title, "X"));
            var stepper2 = Context.Render<VekStep>(parameters => parameters.Add(x => x.Title, "Y"));
            activeIndex.Should().Be(-1);
            // adding a step changes active index to 0
            stepper1.Instance.IsActive.Should().Be(false); // <-- fight partial line coverage
            await stepper.InvokeAsync(async () => await stepper.Instance.AddStepAsync(stepper1.Instance));
            activeIndex.Should().Be(0);
            stepper.Instance.ActiveStep?.Title.Should().Be("X");
            // adding another step won't change active index
            await stepper.InvokeAsync(async () => await stepper.Instance.AddStepAsync(stepper2.Instance));
            activeIndex.Should().Be(0);
            stepper.Instance.ActiveStep?.Title.Should().Be("X");
        }

        [Test]
        public async Task Stepper_ShouldNavigateViaProgrammaticApi()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, false);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
            });
            // check the stepper content
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(true); // previous
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(false); // next
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(0);
            // step 1 icon should be "1", step 2 icon should be "2"
            stepper.FindAll(".vek-step-label-icon")[0].TextContent.Trimmed().Should().Be("1");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync()); // next
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 2");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(false); // previous
            stepper.FindAll(".vek-stepper-button-next").Count.Should().Be(0);
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(1);
            // step 1 icon should be a check mark, step 2 icon should be "2"
            stepper.FindAll(".vek-step-label-icon")[0].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            await stepper.InvokeAsync(async () => await stepper.Instance.PreviousStepAsync());  // prev
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            stepper.Find(".vek-stepper-button-previous").IsDisabled().Should().Be(true); // previous
            stepper.Find(".vek-stepper-button-next").IsDisabled().Should().Be(false); // next
            stepper.FindAll(".vek-stepper-button-complete").Count.Should().Be(0);
            // step 1 icon should be a check mark, step 2 icon should be "2"
            stepper.FindAll(".vek-step-label-icon")[0].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step-label-icon")[1].TextContent.Trimmed().Should().Be("2");
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync()); // next
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync()); // complete also uses the next api!
            // step 1 and 2 icon should be check marks
            stepper.FindAll(".vek-step-label-icon")[0].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step-label-icon")[1].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
        }

        [Test]
        public async Task CompletedContent_ShouldShowUpIfAllStepsAreComplete_Horizontal()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.CompletedContent, markupFactory => markupFactory.AddMarkupContent(0, "voilà"));
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
            });
            // check the stepper content
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 1");
            stepper.FindAll(".vek-stepper-complete").FirstOrDefault()?.TextContent.Trimmed().Should().BeNullOrEmpty();
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync()); // next
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Contain("step 2");
            stepper.FindAll(".vek-stepper-complete").FirstOrDefault()?.TextContent.Trimmed().Should().BeNullOrEmpty();
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync()); // next
            // completed content
            stepper.Find(".vek-stepper-complete").TextContent.Trimmed().Should().Contain("voilà");
        }

        [Test]
        public async Task CompletedContent_ShouldShowUpIfAllStepsAreComplete_Vertical()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.Vertical, true);
                self.Add(x => x.CompletedContent, markupFactory => markupFactory.AddMarkupContent(0, "voilà"));
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
            });
            // check the stepper content
            stepper.FindAll(".vek-stepper-complete").FirstOrDefault()?.TextContent.Trimmed().Should().BeNullOrEmpty();
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync()); // next
            stepper.FindAll(".vek-stepper-complete").FirstOrDefault()?.TextContent.Trimmed().Should().BeNullOrEmpty();
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync()); // next
            // completed content
            stepper.Find(".vek-stepper-complete").TextContent.Trimmed().Should().Contain("voilà");
        }

        [Test]
        public async Task UpdatingStepProperties_ShouldUpdateStepper()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "C");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 3")));
                });
            });

            // disable step 1
            stepper.FindAll(".vek-step")[0].IsDisabled().Should().Be(false);
            await stepper.FindComponents<VekStep>()[0].SetParametersAndRenderAsync(parameters => parameters.Add(x => x.Disabled, true));
            stepper.FindAll(".vek-step")[0].IsDisabled().Should().Be(true);
            // fail step 2
            stepper.FindAll(".vek-step")[1].ClassList.Should().NotContain("vek-step-error");
            stepper.FindAll(".vek-step-label-icon")[1].ClassList.Should().NotContain("vek-error");
            stepper.FindAll(".vek-step-label-content")[1].ClassList.Should().NotContain("vek-error-text");
            await stepper.FindComponents<VekStep>()[1].SetParametersAndRenderAsync(parameters => parameters.Add(x => x.HasError, true));
            stepper.FindAll(".vek-step")[1].ClassList.Should().Contain("vek-step-error");
            stepper.FindAll(".vek-step-label-icon")[1].ClassList.Should().Contain("vek-error");
            stepper.FindAll(".vek-step-label-content")[1].ClassList.Should().Contain("vek-error-text");
            // complete step 3
            stepper.FindAll(".vek-step")[2].ClassList.Should().NotContain("vek-step-completed");
            stepper.FindAll(".vek-step-label-icon")[2].QuerySelectorAll("path").Should().BeEmpty(); // no svg icon if not completed
            await stepper.FindComponents<VekStep>()[2].SetParametersAndRenderAsync(parameters => parameters.Add(x => x.Completed, true));
            stepper.FindAll(".vek-step-label-icon")[2].QuerySelectorAll("path").Last().GetAttribute("d").Should().Be("M9 16.2L4.8 12l-1.4 1.4L9 19 21 7l-1.4-1.4L9 16.2z");
            stepper.FindAll(".vek-step")[2].ClassList.Should().Contain("vek-step-completed");
        }

        /// <summary>
        /// Changing a step's Title or SecondaryText updates the stepper header without needing another render (#13925).
        /// </summary>
        [Test]
        public async Task StepTitleAndSecondaryText_UpdateHeaderInSameRender_Issue13925()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "Hello");
                    step.Add(x => x.SecondaryText, "Greeting");
                });
            });

            stepper.Find(".vek-step-label-content .vek-typography-subtitle2").TextContent.Trim().Should().Be("Hello");
            stepper.Find(".vek-step-label-content .vek-typography-caption").TextContent.Trim().Should().Be("Greeting");

            await stepper.FindComponent<VekStep>().SetParametersAndRenderAsync(parameters => parameters
                .Add(x => x.Title, "Hallo")
                .Add(x => x.SecondaryText, "Begrüßung"));

            stepper.Find(".vek-step-label-content .vek-typography-subtitle2").TextContent.Trim().Should().Be("Hallo");
            stepper.Find(".vek-step-label-content .vek-typography-caption").TextContent.Trim().Should().Be("Begrüßung");
        }

        [Test]
        public async Task StepOnClick_ShouldFireForNonLinearStepper()
        {
            int aClick = 0;
            int bClick = 0;
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.OnClick, () => aClick++);
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.OnClick, () => bClick++);
                });
            });
            await stepper.FindAll(".vek-step")[0].ClickAsync();
            aClick.Should().Be(1);
            bClick.Should().Be(0);
            await stepper.FindAll(".vek-step")[1].ClickAsync();
            aClick.Should().Be(1);
            bClick.Should().Be(1);
        }

        [Test]
        public void ActionContentTemplate_ShouldReplaceTheNavButtons()
        {
            var stepper = Context.Render<VekStepper>();
            stepper.FindAll(".vek-card-actions .vek-button").Count.Should().Be(2, "because no action content defined"); // prev, next
            stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.Tag, "je ne sais pas");
                // this replaces the action buttons prev, skip and next with just text
                self.Add(x => x.ActionContent, stepperRef => (string)stepperRef.Tag!); // <-- here we simulate using the passed in stepper context
            });
            stepper.FindAll(".vek-card-actions .vek-button").Count.Should().Be(0, "because the action content replaces them");
            stepper.Find(".vek-card-actions").InnerHtml?.Trim().Should().Be("je ne sais pas");
        }

        [Test]
        public void ShowReset_ShouldControlResetButtonVisibilty()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.ShowResetButton, true);
            });
            stepper.FindAll(".vek-card-actions .vek-button").Count.Should().Be(3); // reset, previous, next
            stepper.FindAll(".vek-card-actions .vek-stepper-button-reset").Count.Should().Be(1);
            stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.ShowResetButton, false);
            });
            stepper.FindAll(".vek-card-actions .vek-button").Count.Should().Be(2); // previous, next
            stepper.FindAll(".vek-card-actions .vek-stepper-button-reset").Count.Should().Be(0);
        }

        [Test]
        public async Task ResetButton_ShouldResetActiveStep()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.ShowResetButton, true);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "C");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 3")));
                });
            });
            stepper.Instance.GetState(x => x.ActiveIndex).Should().Be(0);
            stepper.Instance.ActiveStep?.Title.Should().Be("A");
            stepper.Instance.Steps[0].Completed.Should().Be(false);
            await stepper.InvokeAsync(async () => await stepper.Instance.NextStepAsync());
            stepper.Instance.ActiveStep?.Title.Should().Be("B");
            stepper.Instance.Steps[0].Completed.Should().Be(true);
            stepper.Instance.GetState(x => x.ActiveIndex).Should().Be(1);
            await stepper.Find(".vek-stepper-button-reset").ClickAsync();
            stepper.Instance.ActiveStep?.Title.Should().Be("A");
            stepper.Instance.Steps[0].Completed.Should().Be(false);
            stepper.Instance.GetState(x => x.ActiveIndex).Should().Be(0);
        }

        [Test]
        public async Task ResetButton_ShouldTriggerResetStepActionOnAllStepsThenActivateFirstStep()
        {
            var cancel = false;
            var actions = new List<StepAction>();
            var index = -1;
            Task OnPreviewInteraction(StepperInteractionEventArgs args)
            {
                actions.Add(args.Action);
                index = args.StepIndex;
                // ReSharper disable once AccessToModifiedClosure
                args.Cancel = cancel;
                return Task.CompletedTask;
            }
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.OnPreviewInteraction, OnPreviewInteraction);
                self.Add(x => x.ShowResetButton, true);
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
            });

            // clicking next sends Complete action requests to get us in a state that reset is a valid click
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            await stepper.Find(".vek-stepper-button-next").ClickAsync();
            index.Should().Be(0);
            actions[0].Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
            await stepper.Find(".vek-stepper-button-next").ClickAsync();
            index.Should().Be(1);
            actions[1].Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(2);

            // check that clicking reset sends Reset StepAction
            await stepper.Find(".vek-stepper-button-reset").ClickAsync();
            actions[2].Should().Be(StepAction.Reset);
            actions[3].Should().Be(StepAction.Reset);
            actions[4].Should().Be(StepAction.Reset);
            actions[5].Should().Be(StepAction.Activate);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
        }

        [Test]
        public async Task ResetButton_ShouldTriggerResetStepActionForSkippedSteps()
        {
            var cancel = false;
            var actions = new List<StepAction>();
            var index = -1;
            Task OnPreviewInteraction(StepperInteractionEventArgs args)
            {
                actions.Add(args.Action);
                index = args.StepIndex;
                args.Cancel = cancel;
                return Task.CompletedTask;
            }
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.OnPreviewInteraction, OnPreviewInteraction);
                self.Add(x => x.ShowResetButton, true);
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step => { step.Add(s => s.Skippable, true); });
                self.AddChildContent<VekStep>(step => { step.Add(s => s.Skippable, true); });
                self.AddChildContent<VekStep>(step => { step.Add(s => s.Skippable, true); });
            });

            // clicking skip sends Skip action requests to get us in a state that reset is a valid click
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            await stepper.Find(".vek-stepper-button-skip").ClickAsync();
            index.Should().Be(0);
            actions[0].Should().Be(StepAction.Skip);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
            await stepper.Find(".vek-stepper-button-skip").ClickAsync();
            index.Should().Be(1);
            actions[1].Should().Be(StepAction.Skip);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(2);

            // check that clicking reset sends Reset StepAction
            await stepper.Find(".vek-stepper-button-reset").ClickAsync();
            actions[2].Should().Be(StepAction.Reset);
            actions[3].Should().Be(StepAction.Reset);
            actions[4].Should().Be(StepAction.Reset);
            actions[5].Should().Be(StepAction.Activate);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
        }

        [Test]
        public async Task NextButton_ShouldTriggerCompleteStepAction()
        {
            var cancel = false;
            var action = StepAction.Reset;
            var index = -1;
            Task OnPreviewInteraction(StepperInteractionEventArgs args)
            {
                action = args.Action;
                index = args.StepIndex;
                // ReSharper disable once AccessToModifiedClosure
                args.Cancel = cancel;
                return Task.CompletedTask;
            }
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.OnPreviewInteraction, OnPreviewInteraction);
                self.Add(x => x.ShowResetButton, true);
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
            });

            // clicking next sends Complete action requests
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            await stepper.Find(".vek-stepper-button-next").ClickAsync();
            index.Should().Be(0);
            action.Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
        }

        [Test]
        public async Task SkipButton_ShouldTriggerSkipStepAction()
        {
            var cancel = false;
            var action = StepAction.Reset;
            var index = -1;
            Task OnPreviewInteraction(StepperInteractionEventArgs args)
            {
                action = args.Action;
                index = args.StepIndex;
                args.Cancel = cancel;
                return Task.CompletedTask;
            }
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.OnPreviewInteraction, OnPreviewInteraction);
                self.Add(x => x.ShowResetButton, true);
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step => { step.Add(s => s.Skippable, true); });
                self.AddChildContent<VekStep>(step => { step.Add(s => s.Skippable, true); });
                self.AddChildContent<VekStep>(step => { step.Add(s => s.Skippable, true); });
            });

            // clicking skip sends Skipped action requests
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            await stepper.Find(".vek-stepper-button-skip").ClickAsync();
            index.Should().Be(0);
            action.Should().Be(StepAction.Skip);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
        }

        [Test]
        public async Task BackButton_ShouldTriggerActivateStepAction()
        {
            var cancel = false;
            var action = StepAction.Reset;
            var index = -1;
            Task OnPreviewInteraction(StepperInteractionEventArgs args)
            {
                action = args.Action;
                index = args.StepIndex;
                // ReSharper disable once AccessToModifiedClosure
                args.Cancel = cancel;
                return Task.CompletedTask;
            }
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.OnPreviewInteraction, OnPreviewInteraction);
                self.Add(x => x.ShowResetButton, true);
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
            });

            // clicking next sends Complete action requests to get us in a state that back is a valid click
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            await stepper.Find(".vek-stepper-button-next").ClickAsync();
            index.Should().Be(0);
            action.Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);

            // check that clicking reset sends Reset StepAction
            await stepper.Find(".vek-stepper-button-previous").ClickAsync();
            index.Should().Be(0);
            action.Should().Be(StepAction.Activate);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
        }

        [Test]
        public async Task Stepper_ControlledNavigation()
        {
            var cancel = false;
            var actions = new List<StepAction>();
            var index = -1;
            Task OnPreviewInteraction(StepperInteractionEventArgs args)
            {
                actions.Add(args.Action);
                index = args.StepIndex;
                // ReSharper disable once AccessToModifiedClosure
                args.Cancel = cancel;
                return Task.CompletedTask;
            }
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.OnPreviewInteraction, OnPreviewInteraction);
                self.Add(x => x.ShowResetButton, true);
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
                self.AddChildContent<VekStep>(step => { });
            });

            // check that clicking next sends Complete action requests
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            await stepper.Find(".vek-stepper-button-next").ClickAsync();
            index.Should().Be(0);
            actions[0].Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
            await stepper.Find(".vek-stepper-button-next").ClickAsync();
            index.Should().Be(1);
            actions[1].Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(2);
            await stepper.Find(".vek-stepper-button-complete").ClickAsync();
            index.Should().Be(2);
            actions[2].Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(2);

            // cancel next
            await stepper.Find(".vek-stepper-button-reset").ClickAsync();
            actions[3].Should().Be(StepAction.Reset); // one reset for each step
            actions[4].Should().Be(StepAction.Reset);
            actions[5].Should().Be(StepAction.Reset);
            actions[6].Should().Be(StepAction.Activate);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            cancel = true; // this should cancel the completion of step 1
            await stepper.Find(".vek-stepper-button-next").ClickAsync();
            actions[7].Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);

            // cancel previous
            cancel = false;
            await stepper.Find(".vek-stepper-button-next").ClickAsync(); // go to step2
            actions[8].Should().Be(StepAction.Complete);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);
            cancel = true; // this should cancel the activation of step 1
            await stepper.Find(".vek-stepper-button-previous").ClickAsync(); // try to go to step1
            index.Should().Be(0);
            actions[9].Should().Be(StepAction.Activate);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(1);

            // cancel jumping via header click
            cancel = false;
            await stepper.Find(".vek-stepper-button-reset").ClickAsync();
            actions[10].Should().Be(StepAction.Reset); // On reset for each step
            actions[11].Should().Be(StepAction.Reset);
            actions[12].Should().Be(StepAction.Reset);
            actions[13].Should().Be(StepAction.Activate);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
            cancel = true; // this should cancel the activation of step 3
            await stepper.FindAll(".vek-step")[2].ClickAsync(); // try to go to step3
            index.Should().Be(2);
            actions[14].Should().Be(StepAction.Activate);
            stepper.Instance.GetState<int>(nameof(VekStepper.ActiveIndex)).Should().Be(0);
        }

        [Test]
        public void HasCompletedClassIfLinear()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.CompletedStepColor, Color.Success);
                self.Add(x => x.CurrentStepColor, Color.Secondary);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Completed, true);
                });
            });

            var stepIcon = stepper.Find(".vek-step-label-icon");

            stepIcon.ClassList.Should().Contain("vek-success");
            stepIcon.ClassList.Should().NotContain("vek-secondary");
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void HasRippleClass(bool ripple, bool hasClass)
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.Ripple, ripple);

                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
            });

            var stepButton = stepper.Find(".vek-step");

            if (hasClass)
            {
                stepButton.ClassList.Should().Contain("vek-ripple");
            }
            else
            {
                stepButton.ClassList.Should().NotContain("vek-ripple");
            }
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void HasClickableClassIfNonLinear(bool nonLinear, bool hasClass)
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, nonLinear);

                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
            });

            var stepButton = stepper.Find(".vek-step");

            if (hasClass)
            {
                stepButton.ClassList.Should().Contain("vek-clickable");
            }
            else
            {
                stepButton.ClassList.Should().NotContain("vek-clickable");
            }
        }

        [Test]
        public async Task Stepper_ShouldOnlySetAriaControlsForRenderedStepPanel()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.Add(x => x.NonLinear, true);
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "Configure Parameters");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 1")));
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "Review & Confirm");
                    step.Add(x => x.ChildContent, StepContent(text => text.AddMarkupContent(0, "step 2")));
                });
            });

            var stepButtons = stepper.FindAll(".vek-step");
            var activePanel = stepper.Find("[role='tabpanel']");

            stepButtons[0].GetAttribute("aria-controls").Should().Be(activePanel.Id);
            stepButtons[1].HasAttribute("aria-controls").Should().BeFalse();
            activePanel.GetAttribute("aria-labelledby").Should().Be(stepButtons[0].Id);

            await stepButtons[1].ClickAsync();

            stepButtons = stepper.FindAll(".vek-step");
            activePanel = stepper.Find("[role='tabpanel']");

            stepButtons[0].HasAttribute("aria-controls").Should().BeFalse();
            stepButtons[1].GetAttribute("aria-controls").Should().Be(activePanel.Id);
            activePanel.GetAttribute("aria-labelledby").Should().Be(stepButtons[1].Id);
        }

        private static RenderFragment StepContent(Action<RenderTreeBuilder> content)
            => builder => content(builder);

        [Test]
        public async Task Stepper_ShouldHandleNullChildContent()
        {
            var stepper = Context.Render<VekStepper>(self =>
            {
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "A");
                });
                self.AddChildContent<VekStep>(step =>
                {
                    step.Add(x => x.Title, "B");
                    step.Add(x => x.ChildContent, builder => builder.AddMarkupContent(0, "step-b"));
                });
            });

            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().BeEmpty();
            await stepper.FindAll(".vek-step")[1].ClickAsync();
            stepper.Find(".vek-stepper-content").TextContent.Trimmed().Should().Be("step-b");
        }

        [Test]
        public void StepContext_NullStepper_Throws()
        {
            Action act = () => _ = new VekStepContext(null!, new VekStep());
            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("stepper");
        }

        [Test]
        public void StepContext_NullStep_Throws()
        {
            Action act = () => _ = new VekStepContext(new VekStepper(), null!);
            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("step");
        }

    }
}
