// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace Vek.UnitTests.UserAttributes
{
    internal sealed class VekComponentFactory
    {
        private readonly ConcurrentDictionary<Type, Func<BunitContext, IRenderedComponent<IComponent>>> _customFactories = new();

        public VekComponentFactory()
        {
            // Add a custom create function for components that cannot be created automatically.
            // These include components that require certain attributes/prerequisites to be set before rendering anything.
            RegisterCustomFactoryFor<VekBreadcrumbs>(builder => builder
                .Add(x => x.Items, [new("text", "href")]));

            RegisterCustomFactoryFor<VekCarouselItem>((builder, testContext) => builder
                .Add(x => x.Parent, testContext.Render<VekCarousel<string>>(attributes => attributes
                        .Add(x => x.SelectedIndex, 0))
                    .Instance));

            RegisterCustomFactoryFor<VekDialog>((builder, testContext) => builder
                .AddCascadingValue(testContext.Render<VekDialogContainer>().Instance));

            RegisterCustomFactoryFor<VekElement>(builder => builder.Add(x => x.HtmlTag, "div"));

            RegisterCustomFactoryFor<VekMessageBox>((builder, testContext) => builder
                .AddCascadingValue(testContext.Render<VekDialogContainer>().Instance));

            RegisterCustomFactoryFor<VekOverlay>(builder => builder.Add(x => x.Visible, true));

            RegisterCustomFactoryFor<VekHighlighter>(builder => builder
                .Add(x => x.Text, "Hello world")
                .Add(x => x.HighlightedText, "Hello"));

            RegisterCustomFactoryFor<VekTabPanel>((builder, testContext) => builder
                .AddCascadingValue(testContext.Render<VekTabs>(attributes => attributes
                        .Add(x => x.KeepPanelsAlive, true))
                    .Instance));
        }

        public Dictionary<string, object> UserAttributes { get; set; } = null;

        public IRenderedComponent<IComponent> Create(Type componentType, BunitContext testContext)
        {
            if (_customFactories.TryGetValue(componentType, out var factory))
            {
                return factory(testContext);
            }

            factory = BuildDefaultFactory(componentType)
                ?? throw new InvalidOperationException($"Failed to create default factory for component {componentType.Name}");

            return factory(testContext);
        }

        private Func<BunitContext, IRenderedComponent<IComponent>> BuildDefaultFactory(Type componentType)
        {
            // Use string as generic type parameter for generic components
            if (componentType.IsGenericType)
            {
                var genericArgs = componentType.GetGenericArguments();
                var constraints = genericArgs.SelectMany(arg => arg.GetGenericParameterConstraints()).Distinct().ToArray();
                var hasINumberConstraint = constraints.Any(constraint => constraint.GetInterfaces().Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(INumberBase<>)));
                if (hasINumberConstraint)
                {
                    componentType = componentType.MakeGenericType(componentType.GetGenericArguments().Select(_ => typeof(int)).ToArray());
                }
                else
                {
                    componentType = componentType.MakeGenericType(componentType.GetGenericArguments().Select(_ => typeof(string)).ToArray());
                }
            }

            var defaultFactoryMethod = typeof(VekComponentFactory)
                .GetMethod(nameof(DefaultFactory), BindingFlags.Instance | BindingFlags.NonPublic)
                ?.MakeGenericMethod(componentType);

            return defaultFactoryMethod != null
                ? testContext => defaultFactoryMethod.Invoke(this, [testContext]) as IRenderedComponent<IComponent>
                : null;
        }

        private IRenderedComponent<TComponent> DefaultFactory<TComponent>(BunitContext testContext)
            where TComponent : VekComponentBase
            => testContext.Render<TComponent>(builder => ApplyAdditionalParameters(builder));

        private void RegisterCustomFactoryFor<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>> parameterBuilder)
            where TComponent : VekComponentBase
            => _customFactories.TryAdd(typeof(TComponent), testContext => testContext
                .Render<TComponent>(builder => parameterBuilder(ApplyAdditionalParameters(builder))));

        private void RegisterCustomFactoryFor<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>, BunitContext> parameterBuilder)
            where TComponent : VekComponentBase
            => _customFactories.TryAdd(typeof(TComponent), testContext => testContext
                .Render<TComponent>(builder => parameterBuilder(ApplyAdditionalParameters(builder), testContext)));

        private ComponentParameterCollectionBuilder<TComponent> ApplyAdditionalParameters<TComponent>(ComponentParameterCollectionBuilder<TComponent> builder)
            where TComponent : VekComponentBase
        {
            if (UserAttributes != null)
            {
                builder = builder.Add(x => x.UserAttributes, UserAttributes);
            }

            return builder;
        }
    }
}
