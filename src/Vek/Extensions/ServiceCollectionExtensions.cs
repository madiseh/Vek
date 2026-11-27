// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Vek.Services
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a Dialog Service as a Scoped instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekDialog(this IServiceCollection services)
        {
            services.TryAddScoped<IDialogService, DialogService>();

            return services;
        }

        /// <summary>
        /// Adds a Snackbar Service as a Scoped instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekSnackbar(this IServiceCollection services)
        {
            services.AddCommonServices();
            services.TryAddScoped<ISnackbar, SnackbarService>();

            return services;
        }

        /// <summary>
        /// Adds a Snackbar Service as a Scoped instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="options">Defines SnackbarConfiguration for this instance.</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekSnackbar(this IServiceCollection services, Action<SnackbarConfiguration> options)
        {
            services.AddVekSnackbar();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds a ResizeListener as a Scoped instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekResizeListener(this IServiceCollection services)
        {
            services.TryAddScoped<IBrowserViewportService, BrowserViewportService>();

            return services;
        }

        /// <summary>
        /// Adds a ResizeListener as a Scoped instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="options">Defines ResizeOptions for this instance</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekResizeListener(this IServiceCollection services, Action<ResizeOptions> options)
        {
            services.AddVekResizeListener();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds a IResizeObserver as a Transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekResizeObserver(this IServiceCollection services)
        {
            services.TryAddTransient<IResizeObserver, ResizeObserver>();

            return services;
        }

        /// <summary>
        /// Adds a IResizeObserver as a Transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="options">Defines ResizeObserverOptions for this instance</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekResizeObserver(this IServiceCollection services, Action<ResizeObserverOptions> options)
        {
            services.AddVekResizeObserver();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds a IResizeObserverFactory as a scoped dependency.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekResizeObserverFactory(this IServiceCollection services)
        {
            services.TryAddScoped<IResizeObserverFactory, ResizeObserverFactory>();

            return services;
        }

        /// <summary>
        /// Adds a IResizeObserverFactory as a scoped dependency.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="options">Defines ResizeObserverOptions for this instance</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekResizeObserverFactory(this IServiceCollection services, Action<ResizeObserverOptions> options)
        {
            services.AddVekResizeObserverFactory();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds IKeyInterceptor as a Transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekKeyInterceptor(this IServiceCollection services)
        {
            services.TryAddScoped<IKeyInterceptorService, KeyInterceptorService>();

            return services;
        }

        /// <summary>
        /// Adds JsEvent as a Transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekJsEvent(this IServiceCollection services)
        {
            services.TryAddTransient<IJsEvent, JsEvent>();
            services.TryAddScoped<IJsEventFactory, JsEventFactory>();

            return services;
        }

        /// <summary>
        /// Adds ScrollManager as a transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static IServiceCollection AddVekScrollManager(this IServiceCollection services)
        {
            services.TryAddTransient<IScrollManager, ScrollManager>();

            return services;
        }

        /// <summary>
        /// Adds ScrollManager as a transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static IServiceCollection AddVekPopoverService(this IServiceCollection services)
        {
            services.AddCommonServices();
            services.TryAddScoped<IPopoverService, PopoverService>();

            return services;
        }

        /// <summary>
        /// Adds ScrollManager as a transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="options">Defines PopoverOptions for the application/user</param>
        public static IServiceCollection AddVekPopoverService(this IServiceCollection services, Action<PopoverOptions> options)
        {
            services.AddVekPopoverService();
            services.Configure(options);

            return services;
        }

        /// <summary>
        /// Adds ScrollListener as a transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static IServiceCollection AddVekScrollListener(this IServiceCollection services)
        {
            services.TryAddTransient<IScrollListener, ScrollListener>();
            services.TryAddScoped<IScrollListenerFactory, ScrollListenerFactory>();

            return services;
        }

        /// <summary>
        /// Adds ScrollSpy as a transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static IServiceCollection AddVekScrollSpy(this IServiceCollection services)
        {
            services.TryAddTransient<IScrollSpy, ScrollSpy>();
            services.TryAddScoped<IScrollSpyFactory, ScrollSpyFactory>();

            return services;
        }

        /// <summary>
        /// Adds JsApi as a transient instance.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static IServiceCollection AddVekJsApi(this IServiceCollection services)
        {
            services.TryAddTransient<IJsApiService, JsApiService>();

            return services;
        }

        /// <summary>
        /// Adds IPointerEventsNoneService as a scoped dependency.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        public static IServiceCollection AddVekPointerEventsNoneService(this IServiceCollection services)
        {
            services.TryAddScoped<IPointerEventsNoneService, PointerEventsNoneService>();

            return services;
        }

        /// <summary>
        /// Adds <see cref="HotkeyManager"/> as a scoped dependency.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekHotkey(this IServiceCollection services)
        {
            services.TryAddScoped<HotkeyManager>();

            return services;
        }

        /// <summary>
        /// Adds the services required for translations.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekLocalization(this IServiceCollection services)
        {
            // Scoped, not transient: the default interceptor builds a ResourceManagerStringLocalizerFactory and its resource cache.
            // Every form component injects the localizer, so a transient registration rebuilt all of that per component instance.
            services.TryAddScoped<ILocalizationInterceptor, DefaultLocalizationInterceptor>();
            services.TryAddScoped<ILocalizationEnumInterceptor, DefaultLocalizationEnumInterceptor>();
            services.TryAddScoped<InternalVekLocalizer>();

            return services;
        }

        /// <summary>
        /// Replaces the default <see cref="ILocalizationInterceptor"/> with custom implementation.
        /// </summary>
        /// <typeparam name="TInterceptor">Custom <see cref="ILocalizationInterceptor"/> implementation.</typeparam>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddLocalizationInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>(this IServiceCollection services) where TInterceptor : class, ILocalizationInterceptor
        {
            services.Replace(ServiceDescriptor.Transient<ILocalizationInterceptor, TInterceptor>());

            return services;
        }

        /// <summary>
        /// Replaces the default <see cref="ILocalizationEnumInterceptor"/> with custom implementation.
        /// </summary>
        /// <typeparam name="TInterceptor">Custom <see cref="ILocalizationEnumInterceptor"/> implementation.</typeparam>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddLocalizationEnumInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>(this IServiceCollection services) where TInterceptor : class, ILocalizationEnumInterceptor
        {
            services.Replace(ServiceDescriptor.Transient<ILocalizationEnumInterceptor, TInterceptor>());

            return services;
        }

        /// <summary>
        /// Replaces the default <see cref="ILocalizationInterceptor"/> with custom implementation.
        /// </summary>
        /// <typeparam name="TInterceptor">Custom <see cref="ILocalizationInterceptor"/> implementation.</typeparam>
        /// <param name="services">IServiceCollection</param>
        /// <param name="implementationFactory">A factory to create new instances of the <see cref="ILocalizationInterceptor"/> implementation.</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddLocalizationInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>(this IServiceCollection services, Func<IServiceProvider, TInterceptor> implementationFactory) where TInterceptor : class, ILocalizationInterceptor
        {
            services.Replace(ServiceDescriptor.Transient<ILocalizationInterceptor>(implementationFactory));

            return services;
        }

        /// <summary>
        /// Replaces the default <see cref="ILocalizationEnumInterceptor"/> with custom implementation.
        /// </summary>
        /// <typeparam name="TInterceptor">Custom <see cref="ILocalizationEnumInterceptor"/> implementation.</typeparam>
        /// <param name="services">IServiceCollection</param>
        /// <param name="implementationFactory">A factory to create new instances of the <see cref="ILocalizationEnumInterceptor"/> implementation.</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddLocalizationEnumInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>(this IServiceCollection services, Func<IServiceProvider, TInterceptor> implementationFactory) where TInterceptor : class, ILocalizationEnumInterceptor
        {
            services.Replace(ServiceDescriptor.Transient<ILocalizationEnumInterceptor>(implementationFactory));

            return services;
        }

        /// <summary>
        /// Adds common services required by Vek components
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekServices(this IServiceCollection services)
        {
            return services
                .AddCommonServices()
                .AddVekDialog()
                .AddVekSnackbar()
                .AddVekResizeListener()
                .AddVekResizeObserver()
                .AddVekResizeObserverFactory()
                .AddVekKeyInterceptor()
                .AddVekJsEvent()
                .AddVekScrollManager()
                .AddVekScrollListener()
                .AddVekJsApi()
                .AddVekScrollSpy()
                .AddVekPopoverService()
                .AddVekPointerEventsNoneService()
                .AddVekHotkey()
                .AddVekLocalization();
        }

        /// <summary>
        /// Adds common services required by Vek components
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configuration">Defines options for all Vek services.</param>
        /// <returns>Continues the IServiceCollection chain.</returns>
        public static IServiceCollection AddVekServices(this IServiceCollection services, Action<VekServicesConfiguration> configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var options = new VekServicesConfiguration();
            configuration(options);

            return services
                .AddCommonServices()
                .AddVekDialog()
                .AddVekSnackbar(snackBarConfiguration =>
                {
                    snackBarConfiguration.ClearAfterNavigation = options.SnackbarConfiguration.ClearAfterNavigation;
                    snackBarConfiguration.MaxDisplayedSnackbars = options.SnackbarConfiguration.MaxDisplayedSnackbars;
                    snackBarConfiguration.NewestOnTop = options.SnackbarConfiguration.NewestOnTop;
                    snackBarConfiguration.PositionClass = options.SnackbarConfiguration.PositionClass;
                    snackBarConfiguration.PreventDuplicates = options.SnackbarConfiguration.PreventDuplicates;
                    snackBarConfiguration.MaximumOpacity = options.SnackbarConfiguration.MaximumOpacity;
                    snackBarConfiguration.ShowTransitionDuration = options.SnackbarConfiguration.ShowTransitionDuration;
                    snackBarConfiguration.VisibleStateDuration = options.SnackbarConfiguration.VisibleStateDuration;
                    snackBarConfiguration.HideTransitionDuration = options.SnackbarConfiguration.HideTransitionDuration;
                    snackBarConfiguration.ShowCloseIcon = options.SnackbarConfiguration.ShowCloseIcon;
                    snackBarConfiguration.RequireInteraction = options.SnackbarConfiguration.RequireInteraction;
                    snackBarConfiguration.BackgroundBlurred = options.SnackbarConfiguration.BackgroundBlurred;
                    snackBarConfiguration.SnackbarVariant = options.SnackbarConfiguration.SnackbarVariant;
                    snackBarConfiguration.IconSize = options.SnackbarConfiguration.IconSize;
                    snackBarConfiguration.NormalIcon = options.SnackbarConfiguration.NormalIcon;
                    snackBarConfiguration.InfoIcon = options.SnackbarConfiguration.InfoIcon;
                    snackBarConfiguration.SuccessIcon = options.SnackbarConfiguration.SuccessIcon;
                    snackBarConfiguration.WarningIcon = options.SnackbarConfiguration.WarningIcon;
                    snackBarConfiguration.ErrorIcon = options.SnackbarConfiguration.ErrorIcon;
                    snackBarConfiguration.HideIcon = options.SnackbarConfiguration.HideIcon;
                })
                .AddVekResizeListener(resizeOptions =>
                {
                    resizeOptions.BreakpointDefinitions = options.ResizeOptions.BreakpointDefinitions;
                    resizeOptions.EnableLogging = options.ResizeOptions.EnableLogging;
                    resizeOptions.NotifyOnBreakpointOnly = options.ResizeOptions.NotifyOnBreakpointOnly;
                    resizeOptions.ReportRate = options.ResizeOptions.ReportRate;
                    resizeOptions.SuppressInitEvent = options.ResizeOptions.SuppressInitEvent;
                })
                .AddVekResizeObserver(observerOptions =>
                {
                    observerOptions.EnableLogging = options.ResizeObserverOptions.EnableLogging;
                    observerOptions.ReportRate = options.ResizeObserverOptions.ReportRate;
                })
                .AddVekResizeObserverFactory(observerOptions =>
                {
                    observerOptions.EnableLogging = options.ResizeObserverOptions.EnableLogging;
                    observerOptions.ReportRate = options.ResizeObserverOptions.ReportRate;
                })
                .AddVekKeyInterceptor()
                .AddVekJsEvent()
                .AddVekScrollManager()
                .AddVekScrollListener()
                .AddVekJsApi()
                .AddVekPopoverService(popoverOptions =>
                {
                    popoverOptions.CheckForPopoverProvider = options.PopoverOptions.CheckForPopoverProvider;
                    popoverOptions.ContainerClass = options.PopoverOptions.ContainerClass;
                    popoverOptions.FlipMargin = options.PopoverOptions.FlipMargin;
                    popoverOptions.QueueDelay = options.PopoverOptions.QueueDelay;
                    popoverOptions.ThrowOnDuplicateProvider = options.PopoverOptions.ThrowOnDuplicateProvider;
                    popoverOptions.OverflowPadding = options.PopoverOptions.OverflowPadding;
                    popoverOptions.ModalOverlay = options.PopoverOptions.ModalOverlay;
                    popoverOptions.OverflowBehavior = options.PopoverOptions.OverflowBehavior;
                    popoverOptions.Delay = options.PopoverOptions.Delay;
                    popoverOptions.Duration = options.PopoverOptions.Duration;
                })
                .AddVekScrollSpy()
                .AddVekPointerEventsNoneService()
                .AddVekHotkey()
                .AddVekLocalization();
        }

        private static IServiceCollection AddCommonServices(this IServiceCollection service)
        {
            service.TryAddSingleton(TimeProvider.System);

            return service;
        }
    }
}
