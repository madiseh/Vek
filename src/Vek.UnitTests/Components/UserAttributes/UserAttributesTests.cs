// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using AwesomeAssertions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Vek.Charts;
using Vek.Services;
using NUnit.Framework;

namespace Vek.UnitTests.UserAttributes
{
    [TestFixture]
    public sealed class UserAttributesTests
    {
        static UserAttributesTests()
        {
            Exclude(typeof(VekBooleanInput<>)); // This is the base class of Switch and CheckBox and should be skipped
            Exclude(typeof(VekHidden));         // No need to test
            Exclude(typeof(VekBreakpointProvider)); // just exposing a cascading value, no layout implications
            Exclude(typeof(VekPicker<>));       // Internal component, skip
            Exclude(typeof(VekRadioGroup<>));   // Wrapping component, skip
            Exclude(typeof(VekDragHandle<>));   // Wrapping component, skip
            Exclude(typeof(VekOverlay));        // Sectioned component, skip
            Exclude(typeof(DataGridGroupRow<>));  // Internal component, skip
            Exclude(typeof(DataGridVirtualizeRow<>)); // Internal component, skip
            Exclude(typeof(DataGridFilterRow<>)); // Internal component, skip
            Exclude(typeof(DataGridFilterCell<>)); // Internal component, skip
            Exclude(typeof(BaseRadialChart<,>)); // Internal component, skip
            Exclude(typeof(BaseAxisChart<,>)); // Internal component, skip
        }

        [Test]
        public async Task AllVekComponents_ShouldForwardUserAttributes()
        {
            // Arrange
            await using var testContext = new BunitContext();
            testContext.AddTestServices();
            testContext.Services.Add(new ServiceDescriptor(typeof(IResizeObserver), new MockResizeObserver()));

            var componentFactory = new VekComponentFactory
            {
                UserAttributes = new Dictionary<string, object> { { "data-testid", "test-123" } },
            };

            // Act & Assert
            var vekComponentTypes = GetVekComponentTypes();

            vekComponentTypes.Should().NotBeEmpty();

            // these components do not need to have user attributes
            var excludedComponents = new HashSet<string>()
            {
                nameof(VekPopover), nameof(VekStep), nameof(VekContextualActionBar), nameof(VekHotkey), nameof(VekExitPrompt),
                "Column`1", "FooterCell`1", "HeaderCell`1", "FilterHeaderCell`1", "SelectColumn`1",
                "HierarchyColumn`1", "PropertyColumn`2", "TemplateColumn`1", "VekToggleItem`1", "VekHeatMapCell`1"
            };

            foreach (var componentType in vekComponentTypes)
            {
                if (excludedComponents.Contains(componentType.Name))
                {
                    continue;
                }

                var component = componentFactory.Create(componentType, testContext);
                component.Markup.Should()
                    .NotBeEmpty(because: $"the component {componentType.Name} should at least contain one element");

                var elementsWithUserAttributes = component.FindAll("[data-testid='test-123']");
                elementsWithUserAttributes.Should()
                    .NotBeEmpty(because: $"UserAttributes should be forwarded by component {componentType.Name}");
            }
        }

        private Type[] GetVekComponentTypes()
        {
            return typeof(VekElement).Assembly
                .GetTypes()
                .Where(type => type.IsAssignableTo(typeof(VekComponentBase)) && !type.IsAbstract)
                .Select(type => type.IsGenericType ? type.GetGenericTypeDefinition() : type)
                .Except(_excludedComponents)
                .ToArray();
        }

        private static ConcurrentBag<Type> _excludedComponents = [];
        private static void Exclude(Type componentType) => _excludedComponents.Add(componentType);
    }
}
