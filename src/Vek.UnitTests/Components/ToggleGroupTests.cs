// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AngleSharp.Common;
using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vek.UnitTests.Mocks;
using Vek.UnitTests.TestComponents.ToggleGroup;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class ToggleGroupTests : BunitTest
    {
        [Test]
        public async Task ToggleGroup_Bind()
        {
            var comp = Context.Render<ToggleGroupBindTest>();
            var toggleFirst = comp.FindComponents<VekToggleGroup<string>>().First();
            var toggleSecond = comp.FindComponents<VekToggleGroup<string>>().Last();
            IElement ToggleItem() => comp.FindAll(".vek-toggle-item").GetItemByIndex(1);

            toggleFirst.Instance.Value.Should().BeNull();
            toggleSecond.Instance.Value.Should().BeNull();
            await ToggleItem().ClickAsync();
            toggleFirst.Instance.Value.Should().Be("Item Two");
            toggleSecond.Instance.Value.Should().Be("Item Two");
        }

        [Test]
        public async Task ToggleGroup_CustomFragmentBind()
        {
            var comp = Context.Render<ToggleGroupCustomFragmentTest>();
            var toggleFirst = comp.FindComponents<VekToggleGroup<string>>().First();
            var toggleSecond = comp.FindComponents<VekToggleGroup<string>>().Last();
            IElement ToggleItem() => comp.FindAll(".vek-toggle-item").GetItemByIndex(1);

            toggleFirst.Instance.Value.Should().BeNull();
            toggleSecond.Instance.Value.Should().BeNull();
            await ToggleItem().ClickAsync();
            toggleFirst.Instance.Value.Should().Be("Item Two");
            toggleSecond.Instance.Value.Should().Be("Item Two");
        }

        [Test]
        public async Task ToggleGroup_SelectionMode()
        {
            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<ToggleGroupBindMultiSelectionTest>();
            var group1 = comp.FindComponents<VekToggleGroup<string>>().First();
            var group2 = comp.FindComponents<VekToggleGroup<string>>().Last();
            IElement ToggleItemSecond() => comp.FindAll(".vek-toggle-item").GetItemByIndex(1);
            IElement ToggleItemThird() => comp.FindAll(".vek-toggle-item").GetItemByIndex(2);

            group1.Instance.Values.Should().BeNull();
            group2.Instance.Values.Should().BeNull();
            await ToggleItemSecond().ClickAsync();
            group1.Instance.Values.Should().Contain("Item Two");
            group2.Instance.Values.Should().Contain("Item Two");
            await ToggleItemThird().ClickAsync();
            group1.Instance.Values.Should().BeEquivalentTo("Item Two", "Item Three");
            group2.Instance.Values.Should().Contain("Item Three");
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await ToggleItemSecond().ClickAsync();
            group1.Instance.Values.Should().BeEquivalentTo("Item Three");
            group2.Instance.Values.Should().Contain("Item Three");
        }

        [Test]
        public async Task ToggleGroup_Initialize()
        {
            var comp = Context.Render<ToggleGroupInitializeTest>();
            var toggleFirst = comp.FindComponents<VekToggleGroup<string>>().First();
            var toggleSecond = comp.FindComponents<VekToggleGroup<string>>().Last();

            toggleFirst.Instance.Value.Should().Be("Item Two");
            toggleSecond.Instance.Values.Should().BeEquivalentTo("Item One", "Item Three");

            await comp.Find("#set-single-value").ClickAsync();
            toggleFirst.Instance.Value.Should().Be("Item One");

            await comp.Find("#set-multi-value").ClickAsync();
            toggleSecond.Instance.Values.Should().BeEquivalentTo("Item Two", "Item Three");
        }

        [Test]
        public async Task ToggleGroup_ToggleSelection_InterceptedValue()
        {
            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<ToggleGroupToggleSelectionTest>();
            var toggle = comp.FindComponent<VekToggleGroup<string>>();
            IElement ToggleItem() => comp.FindAll(".vek-toggle-item").GetItemByIndex(0);

            toggle.Instance.Value.Should().BeNull();
            await ToggleItem().ClickAsync();
            toggle.Instance.Value.Should().Be("Item One");
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await ToggleItem().ClickAsync();
            toggle.Instance.Value.Should().BeNull();
        }

        [Test]
        public async Task ToggleGroup_ToggleRemove()
        {
            var comp = Context.Render<ToggleGroupRemoveTest>();
            var toggle = comp.FindComponent<VekToggleGroup<string>>();
            var toggleGroup = toggle.Instance;
            IElement Button() => comp.Find("#remove_btn");

            toggleGroup.GetItems().Count().Should().Be(8);
            await Button().ClickAsync();
            toggleGroup.GetItems().Count().Should().Be(7);
        }

        [Test]
        [TestCase(Size.Small)]
        [TestCase(Size.Medium)]
        [TestCase(Size.Large)]
        public void ToggleGroup_SizeClasses(Size size)
        {
            var comp = Context.Render<VekToggleGroup<string>>(builder =>
            {
                builder.Add(x => x.Size, size);
                builder.AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"));
                builder.AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "b"));
                builder.AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "c"));
            });

            switch (size)
            {
                case Size.Small:
                    comp.FindAll(".vek-toggle-group-size-small").Count.Should().Be(1);
                    comp.FindAll(".vek-toggle-group-size-medium").Count.Should().Be(0);
                    comp.FindAll(".vek-toggle-group-size-large").Count.Should().Be(0);
                    break;
                case Size.Medium:
                    comp.FindAll(".vek-toggle-group-size-small").Count.Should().Be(0);
                    comp.FindAll(".vek-toggle-group-size-medium").Count.Should().Be(1);
                    comp.FindAll(".vek-toggle-group-size-large").Count.Should().Be(0);
                    break;
                case Size.Large:
                    comp.FindAll(".vek-toggle-group-size-small").Count.Should().Be(0);
                    comp.FindAll(".vek-toggle-group-size-medium").Count.Should().Be(0);
                    comp.FindAll(".vek-toggle-group-size-large").Count.Should().Be(1);
                    break;
            }
        }

        [Test(Description = "Ensures the checkmark is a direct descendant of the button label, is using the right name, and correctly contains a custom class definition")]
        public void ToggleGroup_CheckMarkClass()
        {
            var comp = Context.Render<VekToggleGroup<string>>(builder =>
            {
                builder.Add(x => x.CheckMarkClass, "c69");
                builder.Add(x => x.CheckMark, true);
                builder.AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a").Add(x => x.UnselectedIcon, @Icons.Material.Filled.Coronavirus));
            });

            comp.Find(".vek-button-label > .vek-toggle-item-check-icon").ClassList.Should().Contain("c69");
        }

        [Test]
        public void ToggleGroup_ItemRegistration()
        {
            var comp = Context.Render<VekToggleGroup<string>>(builder =>
            {
                builder.AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"));
                builder.AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "b"));
                builder.AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "c"));
            });
            comp.Instance.GetItems().Count().Should().Be(3);
            // re-registering an item won't do nothing
            comp.Instance.Register(comp.Instance.GetItems().First());
            comp.Instance.GetItems().Count().Should().Be(3);
        }

        [Test]
        public void ToggleGroup_SelectionModeWarning()
        {
            var provider = new MockLoggerProvider();
            var logger = provider.CreateLogger(GetType().FullName!) as MockLogger;
            Context.Services.AddLogging(x => x.ClearProviders().AddProvider(provider)); //set up the logging provider
            foreach (var mode in new[] { SelectionMode.SingleSelection, SelectionMode.ToggleSelection })
            {
                Context.Render<VekToggleGroup<string>>(builder =>
                {
                    builder.Add(x => x.SelectionMode, mode);
                    builder.Add(x => x.ValuesChanged, new Action<IEnumerable<string>>(_ => { }));
                });
                logger!.GetEntries().Last().Level.Should().Be(LogLevel.Warning);
                logger.GetEntries().Last().Message.Should().Be($"For SelectionMode {mode} you should bind {nameof(VekToggleGroup<string>.Value)} instead of {nameof(VekToggleGroup<string>.Values)}");
            }
            Context.Render<VekToggleGroup<string>>(builder =>
            {
                builder.Add(x => x.SelectionMode, SelectionMode.MultiSelection);
                builder.Add(x => x.ValueChanged, new Action<string>(_ => { }));
            });
            logger!.GetEntries().Last().Level.Should().Be(LogLevel.Warning);
            logger.GetEntries().Last().Message.Should().Be($"For SelectionMode {SelectionMode.MultiSelection} you should bind {nameof(VekToggleGroup<string>.Values)} instead of {nameof(VekToggleGroup<string>.Value)}");
            logger.GetEntries().Count.Should().Be(3);
            // no warning if both are bound
            Context.Render<VekToggleGroup<string>>(builder =>
            {
                builder.Add(x => x.SelectionMode, SelectionMode.MultiSelection);
                builder.Add(x => x.ValueChanged, new Action<string>(_ => { }));
                builder.Add(x => x.ValuesChanged, new Action<IEnumerable<string>>(_ => { }));
            });
            logger.GetEntries().Count.Should().Be(3);
            // no warning if none are bound
            Context.Render<VekToggleGroup<string>>(builder =>
            {
                builder.Add(x => x.SelectionMode, SelectionMode.MultiSelection);
            });
            logger.GetEntries().Count.Should().Be(3);
        }

        [Test]
        public void ToggleGroup_Disabled()
        {
            var comp = Context.Render<ToggleGroupDisabledTest>();
            var toggleGroups = comp.FindComponents<VekToggleGroup<string>>();
            var disabledToggleGroup = toggleGroups[0];
            var enabledToggleGroup = toggleGroups[1];

            disabledToggleGroup.Instance.Disabled.Should().BeTrue();
            disabledToggleGroup.Find("div.vek-toggle-group").ClassList.Should().Contain("vek-disabled");
            foreach (var item in disabledToggleGroup.FindComponents<VekToggleItem<string>>())
            {
                // If the group is disabled, the group's disabled state overrules the item's disabled state
                item.Find("button.vek-toggle-item").HasAttribute("disabled").Should().BeTrue();
            }

            enabledToggleGroup.Instance.Disabled.Should().BeFalse();
            enabledToggleGroup.Find("div.vek-toggle-group").ClassList.Should().NotContain("vek-disabled");
            foreach (var item in enabledToggleGroup.FindComponents<VekToggleItem<string>>())
            {
                // If the group is enabled, the item's disabled state dominates
                item.Find("button.vek-toggle-item").HasAttribute("disabled").Should().Be(item.Instance.Disabled);
            }
        }

        [Test]
        [TestCase(SelectionMode.SingleSelection, "b")]
        [TestCase(SelectionMode.MultiSelection, "b")]
        [TestCase(SelectionMode.ToggleSelection, "b")]
        public async Task ToggleGroup_SetSelectedFromValues(SelectionMode selMode, string selectedValues)
        {
            // Arrange
            var comp = Context.Render<VekToggleGroup<string>>(parameters => parameters
                .Add(p => p.SelectionMode, selMode)
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "b"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "c"))
                );

            var toggleGroup = comp.Instance;
            var items = toggleGroup.GetItems().ToList();

            // Act
            if (selMode == SelectionMode.MultiSelection)
            {
                await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Values, [selectedValues]));
            }
            else
            {
                await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Value, selectedValues));
            }

            // Assert
            // Verify only the selected item has the selected state
            items.Single(x => x.Value == selectedValues).Selected.Should().BeTrue();
            items.Where(x => x.Value != selectedValues).All(x => !x.Selected).Should().BeTrue();

            // Verify the UI reflects the selection
            comp.FindAll("button.vek-toggle-item-selected").Count.Should().Be(1);
            comp.Find("button.vek-toggle-item-selected").TextContent.Should().Contain(selectedValues);

            // Verify the internal state matches
            if (selMode == SelectionMode.MultiSelection)
            {
                toggleGroup.Values.Should().BeEquivalentTo([selectedValues]);
                toggleGroup.Value.Should().BeNull();
            }
            else
            {
                toggleGroup.Value.Should().Be(selectedValues);
                toggleGroup.Values.Should().BeNull();
            }
        }

        /// <summary>
        /// This test will fail if selection isn't working without someone being subscribed to the ValueChanged event
        /// </summary>
        [Test]
        [TestCase(SelectionMode.SingleSelection)]
        [TestCase(SelectionMode.ToggleSelection)]
        public async Task ToggleGroup_UnselectPreviousValue_OnToggle(SelectionMode selMode)
        {
            var timeProvider = Context.AddFakeTimeProvider();
            // Arrange
            var comp = Context.Render<VekToggleGroup<string>>(parameters => parameters
                .Add(p => p.SelectionMode, selMode)
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "b"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "c"))
            );

            var toggleGroup = comp.Instance;
            var items = toggleGroup.GetItems().ToList();

            for (var i = 0; i < items.Count; i++)
            {
                // Act
                await comp.FindAll(".vek-toggle-item").GetItemByIndex(i).ClickAsync();
                // Assert
                var currentItem = items[i];
                currentItem.Selected.Should().BeTrue();
                items.Except([currentItem]).All(x => !x.Selected).Should().BeTrue();
                if (selMode == SelectionMode.ToggleSelection)
                {
                        timeProvider.Advance(TimeSpan.FromMilliseconds(301));
                    await comp.FindAll(".vek-toggle-item").GetItemByIndex(i).ClickAsync();
                    currentItem.Selected.Should().BeFalse();
                }
            }
        }

        [Test]
        [TestCase(SelectionMode.SingleSelection, "b")]
        [TestCase(SelectionMode.MultiSelection, "b")]
        [TestCase(SelectionMode.ToggleSelection, "b")]
        public async Task ToggleGroup_SetSelectedFromValuesTest_WithAsyncItems(SelectionMode selMode, string selectedValues)
        {
            // Arrange
            var comp = Context.Render<VekToggleGroup<string>>(parameters => parameters
                .Add(p => p.SelectionMode, selMode)
                .AddChildContent<Virtualize<string>>(v =>
                    v.Add(x => x.Items, ["a", "b", "c"])
                        .Add<VekToggleItem<string>, string>(x => x.ItemContent,
                            value => item => item.Add(x => x.Value, value))
                )
            );

            var toggleGroup = comp.Instance;
            var items = toggleGroup.GetItems().ToList();

            // Act
            if (selMode == SelectionMode.MultiSelection)
            {
                await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Values, [selectedValues]));
            }
            else
            {
                await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Value, selectedValues));
            }

            // Assert
            // Verify only the selected item has the selected state
            items.Single(x => x.Value == selectedValues).Selected.Should().BeTrue();
            items.Where(x => x.Value != selectedValues).All(x => !x.Selected).Should().BeTrue();

            // Verify the UI reflects the selection
            comp.FindAll("button.vek-toggle-item-selected").Count.Should().Be(1);
            comp.Find("button.vek-toggle-item-selected").TextContent.Should().Contain(selectedValues);

            // Verify the internal state matches
            if (selMode == SelectionMode.MultiSelection)
            {
                toggleGroup.Values.Should().BeEquivalentTo([selectedValues]);
                toggleGroup.Value.Should().BeNull();
            }
            else
            {
                toggleGroup.Value.Should().Be(selectedValues);
                toggleGroup.Values.Should().BeNull();
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        [Test]
        public void ToggleGroup_RTL(bool isRTL)
        {
            // Arrange
            var comp = Context.Render<VekToggleGroup<string>>(parameters => parameters
                .Add(p => p.RightToLeft, isRTL)
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "b"))
                );

            // Assert
            var group = comp.Find(".vek-toggle-group");

            if (isRTL)
            {
                group.ClassList.Should().Contain("vek-toggle-group-rtl");
            }
            else
            {
                group.ClassList.Should().NotContain("vek-toggle-group-rtl");
            }

        }

        [Test]
        public async Task ToggleGroup_Vertical()
        {
            // Arrange & Act
            var comp = Context.Render<VekToggleGroup<string>>(parameters => parameters
                .Add(p => p.Vertical, true)
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "b"))
                );

            // Assert
            var group = comp.Find(".vek-toggle-group");
            group.ClassList.Should().Contain("vek-toggle-group-vertical");
            group.ClassList.Should().NotContain("vek-toggle-group-horizontal");

            // Act - set to false
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Vertical, false));

            // Assert
            group = comp.Find(".vek-toggle-group");
            group.ClassList.Should().NotContain("vek-toggle-group-vertical");
            group.ClassList.Should().Contain("vek-toggle-group-horizontal");
        }

        [Test]
        public async Task ToggleGroup_FixedContent()
        {
            // Arrange & Act
            var comp = Context.Render<VekToggleGroup<string>>(parameters => parameters
                .Add(p => p.FixedContent, true)
                .Add(p => p.CheckMark, true)
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"))
                );

            // Assert - should have fixed content padding
            comp.Find(".vek-toggle-item").ClassList.Should().Contain("vek-toggle-item-fixed");

            // Act - disable fixed content
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.FixedContent, false));

            // Assert
            comp.Find(".vek-toggle-item").ClassList.Should().NotContain("vek-toggle-item-fixed");
        }

        [Test]
        public async Task ToggleGroup_MultipleSelection()
        {
            // Arrange
            var comp = Context.Render<VekToggleGroup<string>>(parameters => parameters
                .Add(p => p.SelectionMode, SelectionMode.MultiSelection)
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "a"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "b"))
                .AddChildContent<VekToggleItem<string>>(item => item.Add(x => x.Value, "c"))
                );

            var toggleGroup = comp.Instance;
            var items = toggleGroup.GetItems().ToList();

            // Act - select multiple items
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Values, ["a", "c"]));

            // Assert
            // Verify correct items are selected
            items.First(x => x.Value == "a").Selected.Should().BeTrue();
            items.First(x => x.Value == "b").Selected.Should().BeFalse();
            items.First(x => x.Value == "c").Selected.Should().BeTrue();

            // Verify UI shows multiple selections
            var selectedButtons = comp.FindAll("button.vek-toggle-item-selected");
            selectedButtons.Count.Should().Be(2);
            selectedButtons[0].TextContent.Should().Contain("a");
            selectedButtons[1].TextContent.Should().Contain("c");

            // Verify internal state
            toggleGroup.Values.Should().BeEquivalentTo(["a", "c"]);
            toggleGroup.Value.Should().BeNull();

            // Act - deselect an item
            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Values, ["a"]));

            // Assert
            items.First(x => x.Value == "a").Selected.Should().BeTrue();
            items.First(x => x.Value == "b").Selected.Should().BeFalse();
            items.First(x => x.Value == "c").Selected.Should().BeFalse();

            selectedButtons = comp.FindAll("button.vek-toggle-item-selected");
            selectedButtons.Count.Should().Be(1);
            selectedButtons[0].TextContent.Should().Contain("a");

            toggleGroup.Values.Should().BeEquivalentTo(["a"]);
            toggleGroup.Value.Should().BeNull();
        }

        /// <summary>
        /// This test is based on https://github.com/madiseh/vek/issues/11384
        /// When a ToggleGroupItem is clicked, the value should be set or intercepted via the ValueChanged event
        /// This test verifies that both scenarios update the ToggleGroupItem Selected state
        /// </summary>
        [Test]
        public async Task ToggleGroup_ToggleSelection()
        {
            var comp = Context.Render<ToggleGroupInterceptValueTest>();
            IElement GetYesButton() => comp.FindAll(".vek-toggle-group .vek-toggle-item")[0];
            IElement GetNoButton() => comp.FindAll(".vek-toggle-group .vek-toggle-item")[1];
            IElement GetMaybeButton() => comp.FindAll(".vek-toggle-group .vek-toggle-item")[2];

            var toggleGroup = comp.FindComponent<VekToggleGroup<ToggleGroupInterceptValueTest.AttendanceStatus>>();
            // verify 3 ToggleGroupItems
            var nodes = comp.FindAll(".vek-toggle-group .vek-toggle-item");
            nodes.Count.Should().Be(3);
            GetYesButton().ClassList.Should().NotContain("vek-toggle-item-selected");
            GetNoButton().ClassList.Should().Contain("vek-toggle-item-selected");
            GetMaybeButton().ClassList.Should().NotContain("vek-toggle-item-selected");

            // verify initial state
            comp.Instance.UserAttendanceStatus.Should().Be(ToggleGroupInterceptValueTest.AttendanceStatus.Declined);

            var success = comp.Find(".simulate-success input"); // radio button
            var failure = comp.Find(".simulate-failure input"); // radio button

            // start in success mode
            var successStatus = comp.Find(".simulate-success .vek-button-root");
            successStatus.ClassList.Should().Contain("vek-checked");
            var failureStatus = comp.Find(".simulate-failure .vek-button-root");
            failureStatus.ClassList.Should().NotContain("vek-checked");

            // change to yes
            await GetYesButton().ClickAsync();
            await comp.WaitForAssertionAsync(() => GetYesButton().ClassList.Should().Contain("vek-toggle-item-selected"));
            GetNoButton().ClassList.Should().NotContain("vek-toggle-item-selected");
            GetMaybeButton().ClassList.Should().NotContain("vek-toggle-item-selected");
            comp.Instance.UserAttendanceStatus.Should().Be(ToggleGroupInterceptValueTest.AttendanceStatus.Accepted);

            // change to maybe
            await GetMaybeButton().ClickAsync();
            await comp.WaitForAssertionAsync(() => GetYesButton().ClassList.Should().NotContain("vek-toggle-item-selected"));
            GetNoButton().ClassList.Should().NotContain("vek-toggle-item-selected");
            GetMaybeButton().ClassList.Should().Contain("vek-toggle-item-selected");
            comp.Instance.UserAttendanceStatus.Should().Be(ToggleGroupInterceptValueTest.AttendanceStatus.Maybe);

            // simulate failure where it saves last success
            await failure.ClickAsync();

            // click yes with failure enabled to simulate no change
            await GetYesButton().ClickAsync();
            // check value has not changed, should still be maybe
            await comp.WaitForAssertionAsync(() => comp.Instance.UserAttendanceStatus.Should().Be(ToggleGroupInterceptValueTest.AttendanceStatus.Maybe, "Value should not have changed form Maybe"));
            // check selected has not changed, should still be maybe
            GetYesButton().ClassList.Should().NotContain("vek-toggle-item-selected", "Selection should not have changed from maybe.");
            GetNoButton().ClassList.Should().NotContain("vek-toggle-item-selected", "Selection should not have changed from maybe.");
            GetMaybeButton().ClassList.Should().Contain("vek-toggle-item-selected", "Selection should still be maybe.");
        }
    }
}
