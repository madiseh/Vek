// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Vek.Services;
using Vek.UnitTests.Mocks;
using NUnit.Framework;

namespace Vek.UnitTests;

#nullable enable
[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddVekDialog_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVekDialog();
        var serviceProvider = services.BuildServiceProvider();
        var dialogService = serviceProvider.GetService<IDialogService>();

        // Assert
        dialogService.Should().NotBeNull();
    }

    [Test]
    public void AddVekSnackBar_ShouldRegisterServices()
    {
        // Arrange
        using var testContext = new BunitContext();
        var services = new ServiceCollection()
            .AddSingleton<NavigationManager>(new BunitNavigationManager(testContext))
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekSnackbar();
        var serviceProvider = services.BuildServiceProvider();
        var snackBarService = serviceProvider.GetService<ISnackbar>();

        // Assert
        snackBarService.Should().NotBeNull();
    }

    [Test]
    public void AddVekSnackBar_ShouldRegisterServices_WithConfigurationAction()
    {
        // Arrange
        using var testContext = new BunitContext();
        var services = new ServiceCollection()
            .AddSingleton<NavigationManager>(new BunitNavigationManager(testContext))
            .AddSingleton<IJSRuntime, MockJsRuntime>();
        SnackbarConfiguration? expectedOptions = null;

        // Act
        services.AddVekSnackbar(options =>
        {
            options.PositionClass = "position_class";
            options.ClearAfterNavigation = true;
            options.ShowCloseIcon = false;
            expectedOptions = options;
        });
        var serviceProvider = services.BuildServiceProvider();
        var snackBarService = serviceProvider.GetService<ISnackbar>();
        var options = serviceProvider.GetRequiredService<IOptions<SnackbarConfiguration>>();
        var actualOptions = options.Value;

        // Assert
        snackBarService.Should().NotBeNull();
        actualOptions.Should().BeSameAs(expectedOptions);
    }

    [Test]
    public void AddVekResizeListener_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekResizeListener();
        var serviceProvider = services.BuildServiceProvider();
        var browserViewportService = serviceProvider.GetService<IBrowserViewportService>();

        // Assert
        browserViewportService.Should().NotBeNull();
    }

    [Test]
    public void AddVekResizeListener_ShouldRegisterServices_WithOptionsAction()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();
        ResizeOptions? expectedOptions = null;

        // Act
        services.AddVekResizeListener(options =>
        {
            options.BreakpointDefinitions = new Dictionary<Breakpoint, int>
            {
                { Breakpoint.Lg, 500 }
            };
            options.EnableLogging = true;
            options.NotifyOnBreakpointOnly = false;
            options.ReportRate = 100;
            options.SuppressInitEvent = false;
            expectedOptions = options;
        });
        var serviceProvider = services.BuildServiceProvider();
        var browserViewportService = serviceProvider.GetService<IBrowserViewportService>();
        var options = serviceProvider.GetRequiredService<IOptions<ResizeOptions>>();
        var actualOptions = options.Value;

        // Assert
        browserViewportService.Should().NotBeNull();
        expectedOptions.Should().NotBeNull();
        actualOptions.Should().BeSameAs(expectedOptions);
    }

    [Test]
    public void AddVekResizeObserver_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekResizeObserver();
        var serviceProvider = services.BuildServiceProvider();
        var resizeObserver = serviceProvider.GetService<IResizeObserver>();

        // Assert
        resizeObserver.Should().NotBeNull();
    }

    [Test]
    public void AddVekResizeObserver_ShouldRegisterServices_WithOptionsAction()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime, MockJsRuntime>();
        ResizeObserverOptions? expectedOptions = null;

        // Act
        services.AddVekResizeObserver(options =>
        {
            options.EnableLogging = true;
            options.ReportRate = 500;
            expectedOptions = options;
        });
        var serviceProvider = services.BuildServiceProvider();
        var resizeObserver = serviceProvider.GetService<IResizeObserver>();
        var options = serviceProvider.GetRequiredService<IOptions<ResizeObserverOptions>>();
        var actualOptions = options.Value;

        // Assert
        resizeObserver.Should().NotBeNull();
        expectedOptions.Should().NotBeNull();
        actualOptions.Should().BeSameAs(expectedOptions);
    }

    [Test]
    public void AddVekResizeObserverFactory_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVekResizeObserverFactory();
        var serviceProvider = services.BuildServiceProvider();
        var resizeObserverFactory = serviceProvider.GetService<IResizeObserverFactory>();

        // Assert
        resizeObserverFactory.Should().NotBeNull();
    }

    [Test]
    public void AddVekResizeObserverFactory_ShouldRegisterServices_WithOptionsAction()
    {
        // Arrange
        var services = new ServiceCollection();
        ResizeObserverOptions? expectedOptions = null;

        // Act
        services.AddVekResizeObserverFactory(options =>
        {
            options.EnableLogging = true;
            options.ReportRate = 500;
            expectedOptions = options;
        });
        var serviceProvider = services.BuildServiceProvider();
        var resizeObserverFactory = serviceProvider.GetService<IResizeObserverFactory>();
        var options = serviceProvider.GetRequiredService<IOptions<ResizeObserverOptions>>();
        var actualOptions = options.Value;

        // Assert
        resizeObserverFactory.Should().NotBeNull();
        expectedOptions.Should().NotBeNull();
        actualOptions.Should().BeSameAs(expectedOptions);
    }

    [Test]
    public void AddVekKeyInterceptor_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekKeyInterceptor();
        var serviceProvider = services.BuildServiceProvider();
        var keyInterceptorService = serviceProvider.GetService<IKeyInterceptorService>();

        // Assert
        keyInterceptorService.Should().NotBeNull();
    }

    [Test]
    public void AddVekJsEvent_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekJsEvent();
        var serviceProvider = services.BuildServiceProvider();
        var jsEvent = serviceProvider.GetService<IJsEvent>();
        var jsEventFactory = serviceProvider.GetService<IJsEventFactory>();

        // Assert
        jsEvent.Should().NotBeNull();
        jsEventFactory.Should().NotBeNull();
    }

    [Test]
    public void AddVekScrollManager_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekScrollManager();
        var serviceProvider = services.BuildServiceProvider();
        var scrollManager = serviceProvider.GetService<IScrollManager>();

        // Assert
        scrollManager.Should().NotBeNull();
    }

    [Test]
    public void AddVekPopoverService_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekPopoverService();
        var serviceProvider = services.BuildServiceProvider();
        var popoverService = serviceProvider.GetService<IPopoverService>();

        // Assert
        popoverService.Should().NotBeNull();
    }

    [Test]
    public void AddVekPopoverService_ShouldRegisterServices_WithOptionsAction()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();
        PopoverOptions? expectedOptions = null;

        // Act
        services.AddVekPopoverService(options =>
        {
            options.QueueDelay = TimeSpan.FromSeconds(5);
            options.ContainerClass = "container_class";
            options.FlipMargin = 100;
            options.OverflowPadding = 0;
            options.ThrowOnDuplicateProvider = false;
            options.ModalOverlay = true;
            options.OverflowBehavior = OverflowBehavior.FlipNever;
            expectedOptions = options;
        });
        var serviceProvider = services.BuildServiceProvider();
        var popoverService = serviceProvider.GetService<IPopoverService>();
        var options = serviceProvider.GetRequiredService<IOptions<PopoverOptions>>();
        var actualOptions = options.Value;

        // Assert
        popoverService.Should().NotBeNull();
        expectedOptions.Should().NotBeNull();
        actualOptions.Should().BeSameAs(expectedOptions);
    }

    [Test]
    public void AddVekScrollListener_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekScrollListener();
        var serviceProvider = services.BuildServiceProvider();
        var scrollListener = serviceProvider.GetService<IScrollListener>();
        var scrollListenerFactory = serviceProvider.GetService<IScrollListenerFactory>();

        // Assert
        scrollListener.Should().NotBeNull();
        scrollListenerFactory.Should().NotBeNull();
    }

    [Test]
    public void AddVekScrollSpy_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekScrollSpy();
        var serviceProvider = services.BuildServiceProvider();
        var scrollSpy = serviceProvider.GetService<IScrollSpy>();
        var scrollSpyFactory = serviceProvider.GetService<IScrollSpyFactory>();

        // Assert
        scrollSpy.Should().NotBeNull();
        scrollSpyFactory.Should().NotBeNull();
    }

    [Test]
    public void AddVekJsApi_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekJsApi();
        var serviceProvider = services.BuildServiceProvider();
        var jsApiService = serviceProvider.GetService<IJsApiService>();

        // Assert
        jsApiService.Should().NotBeNull();
    }

    [Test]
    public void AddVekPointerEventsNoneService_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekPointerEventsNoneService();
        var serviceProvider = services.BuildServiceProvider();
        var pointerEventsNoneService = serviceProvider.GetService<IPointerEventsNoneService>();

        // Assert
        pointerEventsNoneService.Should().NotBeNull();
    }

    [Test]
    public void AddVekLocalization_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekLocalization();
        var serviceProvider = services.BuildServiceProvider();
        var vekLocalizer = serviceProvider.GetService<InternalVekLocalizer>();
        var localizationInterceptor = serviceProvider.GetService<ILocalizationInterceptor>();
        var localizationEnumInterceptor = serviceProvider.GetService<ILocalizationEnumInterceptor>();

        // Assert
        vekLocalizer.Should().NotBeNull();
        localizationInterceptor.Should().NotBeNull();
        localizationEnumInterceptor.Should().NotBeNull();
    }

    /// <summary>
    /// Every form component injects the localizer, and the default interceptor builds a resource reader, so one instance must serve the whole scope (#13519).
    /// </summary>
    [Test]
    public void AddVekLocalization_ShouldRegisterOneLocalizerPerScope()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekLocalization();
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var first = scope.ServiceProvider.GetRequiredService<InternalVekLocalizer>();
        var second = scope.ServiceProvider.GetRequiredService<InternalVekLocalizer>();

        // Assert
        first.Should().BeSameAs(second);
    }

    [Test]
    public void AddVekServices_ShouldRegisterAllServices()
    {
        // Arrange
        using var testContext = new BunitContext();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<NavigationManager>(new BunitNavigationManager(testContext))
            .AddSingleton<IJSRuntime, MockJsRuntime>();

        // Act
        services.AddVekServices();
        var serviceProvider = services.BuildServiceProvider();
        var timeProvider = serviceProvider.GetService<TimeProvider>();
        var dialogService = serviceProvider.GetService<IDialogService>();
        var snackBarService = serviceProvider.GetService<ISnackbar>();
        var browserViewportService = serviceProvider.GetService<IBrowserViewportService>();
        var resizeObserver = serviceProvider.GetService<IResizeObserver>();
        var resizeObserverFactory = serviceProvider.GetService<IResizeObserverFactory>();
        var keyInterceptorService = serviceProvider.GetService<IKeyInterceptorService>();
        var jsEvent = serviceProvider.GetService<IJsEvent>();
        var jsEventFactory = serviceProvider.GetService<IJsEventFactory>();
        var scrollManager = serviceProvider.GetService<IScrollManager>();
        var popoverService = serviceProvider.GetService<IPopoverService>();
        var scrollListener = serviceProvider.GetService<IScrollListener>();
        var scrollListenerFactory = serviceProvider.GetService<IScrollListenerFactory>();
        var scrollSpy = serviceProvider.GetService<IScrollSpy>();
        var scrollSpyFactory = serviceProvider.GetService<IScrollSpyFactory>();
        var jsApiService = serviceProvider.GetService<IJsApiService>();
        var vekLocalizer = serviceProvider.GetService<InternalVekLocalizer>();
        var localizationInterceptor = serviceProvider.GetService<ILocalizationInterceptor>();
        var localizationEnumInterceptor = serviceProvider.GetService<ILocalizationEnumInterceptor>();

        // Assert
        timeProvider.Should().NotBeNull();
        dialogService.Should().NotBeNull();
        snackBarService.Should().NotBeNull();
        browserViewportService.Should().NotBeNull();
        resizeObserver.Should().NotBeNull();
        resizeObserverFactory.Should().NotBeNull();
        keyInterceptorService.Should().NotBeNull();
        jsEvent.Should().NotBeNull();
        jsEventFactory.Should().NotBeNull();
        scrollManager.Should().NotBeNull();
        popoverService.Should().NotBeNull();
        scrollListener.Should().NotBeNull();
        scrollListenerFactory.Should().NotBeNull();
        scrollSpy.Should().NotBeNull();
        scrollSpyFactory.Should().NotBeNull();
        jsApiService.Should().NotBeNull();
        vekLocalizer.Should().NotBeNull();
        localizationInterceptor.Should().NotBeNull();
        localizationEnumInterceptor.Should().NotBeNull();
    }

    [Test]
    public void AddVekServices_ShouldRegisterAllServices_WithOptionsAction()
    {
        // Arrange
        using var testContext = new BunitContext();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<NavigationManager>(new BunitNavigationManager(testContext))
            .AddSingleton<IJSRuntime, MockJsRuntime>();
        VekServicesConfiguration? expectedOptions = null;

        // Act
        services.AddVekServices(options =>
        {
            // SnackBarConfiguration
            options.SnackbarConfiguration.ClearAfterNavigation = true;
            options.SnackbarConfiguration.MaxDisplayedSnackbars = 1;
            options.SnackbarConfiguration.NewestOnTop = true;
            options.SnackbarConfiguration.PositionClass = "position_class";
            options.SnackbarConfiguration.PreventDuplicates = true;
            options.SnackbarConfiguration.MaximumOpacity = 2;
            options.SnackbarConfiguration.ShowTransitionDuration = 3;
            options.SnackbarConfiguration.VisibleStateDuration = 4;
            options.SnackbarConfiguration.HideTransitionDuration = 5;
            options.SnackbarConfiguration.ShowCloseIcon = false;
            options.SnackbarConfiguration.RequireInteraction = true;
            options.SnackbarConfiguration.BackgroundBlurred = true;
            options.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
            options.SnackbarConfiguration.SuccessIcon = @Icons.Material.Outlined.CheckCircleOutline;
            options.SnackbarConfiguration.WarningIcon = @Icons.Material.Outlined.WarningAmber;

            // ResizeOptions
            options.ResizeOptions.BreakpointDefinitions = new Dictionary<Breakpoint, int>
            {
                { Breakpoint.Lg, 500 }
            };
            options.ResizeOptions.EnableLogging = true;
            options.ResizeOptions.NotifyOnBreakpointOnly = false;
            options.ResizeOptions.ReportRate = 100;
            options.ResizeOptions.SuppressInitEvent = false;

            // ResizeObserverOptions
            options.ResizeObserverOptions.EnableLogging = true;
            options.ResizeObserverOptions.ReportRate = 500;

            // PopoverOptions
            options.PopoverOptions.QueueDelay = TimeSpan.FromSeconds(5);
            options.PopoverOptions.ContainerClass = "container_class";
            options.PopoverOptions.FlipMargin = 100;
            options.PopoverOptions.OverflowPadding = 12;
            options.PopoverOptions.ThrowOnDuplicateProvider = false;
            options.PopoverOptions.ModalOverlay = true;
            options.PopoverOptions.OverflowBehavior = OverflowBehavior.FlipNever;
            options.PopoverOptions.Delay = TimeSpan.FromSeconds(1);
            options.PopoverOptions.Duration = TimeSpan.FromSeconds(2);

            expectedOptions = options;
        });
        var serviceProvider = services.BuildServiceProvider();
        var timeProvider = serviceProvider.GetService<TimeProvider>();
        var dialogService = serviceProvider.GetService<IDialogService>();
        var snackBarService = serviceProvider.GetService<ISnackbar>();
        var browserViewportService = serviceProvider.GetService<IBrowserViewportService>();
        var resizeObserver = serviceProvider.GetService<IResizeObserver>();
        var resizeObserverFactory = serviceProvider.GetService<IResizeObserverFactory>();
        var keyInterceptorService = serviceProvider.GetService<IKeyInterceptorService>();
        var jsEvent = serviceProvider.GetService<IJsEvent>();
        var jsEventFactory = serviceProvider.GetService<IJsEventFactory>();
        var scrollManager = serviceProvider.GetService<IScrollManager>();
        var popoverService = serviceProvider.GetService<IPopoverService>();
        var scrollListener = serviceProvider.GetService<IScrollListener>();
        var scrollListenerFactory = serviceProvider.GetService<IScrollListenerFactory>();
        var scrollSpy = serviceProvider.GetService<IScrollSpy>();
        var scrollSpyFactory = serviceProvider.GetService<IScrollSpyFactory>();
        var jsApiService = serviceProvider.GetService<IJsApiService>();
        var vekLocalizer = serviceProvider.GetService<InternalVekLocalizer>();
        var localizationInterceptor = serviceProvider.GetService<ILocalizationInterceptor>();
        var localizationEnumInterceptor = serviceProvider.GetService<ILocalizationEnumInterceptor>();
        var snackBarOptions = serviceProvider.GetRequiredService<IOptions<SnackbarConfiguration>>();
        var resizeOptions = serviceProvider.GetRequiredService<IOptions<ResizeOptions>>();
        var resizeObserverOptions = serviceProvider.GetRequiredService<IOptions<ResizeObserverOptions>>();
        var popoverOptions = serviceProvider.GetRequiredService<IOptions<PopoverOptions>>();
        var actualSnackBarOptions = snackBarOptions.Value;
        var actualResizeOptions = resizeOptions.Value;
        var actualResizeObserverOptions = resizeObserverOptions.Value;
        var actualPopoverOptions = popoverOptions.Value;

        // Assert
        timeProvider.Should().NotBeNull();
        dialogService.Should().NotBeNull();
        snackBarService.Should().NotBeNull();
        browserViewportService.Should().NotBeNull();
        resizeObserver.Should().NotBeNull();
        resizeObserverFactory.Should().NotBeNull();
        keyInterceptorService.Should().NotBeNull();
        jsEvent.Should().NotBeNull();
        jsEventFactory.Should().NotBeNull();
        scrollManager.Should().NotBeNull();
        popoverService.Should().NotBeNull();
        scrollListener.Should().NotBeNull();
        scrollListenerFactory.Should().NotBeNull();
        scrollSpy.Should().NotBeNull();
        scrollSpyFactory.Should().NotBeNull();
        jsApiService.Should().NotBeNull();
        vekLocalizer.Should().NotBeNull();
        localizationInterceptor.Should().NotBeNull();
        localizationEnumInterceptor.Should().NotBeNull();

        // We can't check reference here, instead we need to check each value
        actualPopoverOptions.QueueDelay.Should().Be(expectedOptions!.PopoverOptions.QueueDelay);
        actualPopoverOptions.ContainerClass.Should().Be(expectedOptions.PopoverOptions.ContainerClass);
        actualPopoverOptions.FlipMargin.Should().Be(expectedOptions.PopoverOptions.FlipMargin);
        actualPopoverOptions.OverflowPadding.Should().Be(expectedOptions.PopoverOptions.OverflowPadding);
        actualPopoverOptions.ThrowOnDuplicateProvider.Should().Be(expectedOptions.PopoverOptions.ThrowOnDuplicateProvider);
        actualPopoverOptions.ModalOverlay.Should().Be(expectedOptions.PopoverOptions.ModalOverlay);
        actualPopoverOptions.OverflowBehavior.Should().Be(expectedOptions.PopoverOptions.OverflowBehavior);
        actualPopoverOptions.Delay.Should().Be(expectedOptions.PopoverOptions.Delay);
        actualPopoverOptions.Duration.Should().Be(expectedOptions.PopoverOptions.Duration);

        actualResizeObserverOptions.EnableLogging.Should().Be(expectedOptions.ResizeObserverOptions.EnableLogging);
        actualResizeObserverOptions.ReportRate.Should().Be(expectedOptions.ResizeObserverOptions.ReportRate);

        actualResizeOptions.BreakpointDefinitions.Should().BeSameAs(expectedOptions.ResizeOptions.BreakpointDefinitions);
        actualResizeOptions.EnableLogging.Should().Be(expectedOptions.ResizeOptions.EnableLogging);
        actualResizeOptions.NotifyOnBreakpointOnly.Should().Be(expectedOptions.ResizeOptions.NotifyOnBreakpointOnly);
        actualResizeOptions.ReportRate.Should().Be(expectedOptions.ResizeOptions.ReportRate);
        actualResizeOptions.SuppressInitEvent.Should().Be(expectedOptions.ResizeOptions.SuppressInitEvent);

        actualSnackBarOptions.ClearAfterNavigation.Should().Be(expectedOptions.SnackbarConfiguration.ClearAfterNavigation);
        actualSnackBarOptions.MaxDisplayedSnackbars.Should().Be(expectedOptions.SnackbarConfiguration.MaxDisplayedSnackbars);
        actualSnackBarOptions.NewestOnTop.Should().Be(expectedOptions.SnackbarConfiguration.NewestOnTop);
        actualSnackBarOptions.PositionClass.Should().Be(expectedOptions.SnackbarConfiguration.PositionClass);
        actualSnackBarOptions.PreventDuplicates.Should().Be(expectedOptions.SnackbarConfiguration.PreventDuplicates);
        actualSnackBarOptions.MaximumOpacity.Should().Be(expectedOptions.SnackbarConfiguration.MaximumOpacity);
        actualSnackBarOptions.ShowTransitionDuration.Should().Be(expectedOptions.SnackbarConfiguration.ShowTransitionDuration);
        actualSnackBarOptions.VisibleStateDuration.Should().Be(expectedOptions.SnackbarConfiguration.VisibleStateDuration);
        actualSnackBarOptions.HideTransitionDuration.Should().Be(expectedOptions.SnackbarConfiguration.HideTransitionDuration);
        actualSnackBarOptions.ShowCloseIcon.Should().Be(expectedOptions.SnackbarConfiguration.ShowCloseIcon);
        actualSnackBarOptions.RequireInteraction.Should().Be(expectedOptions.SnackbarConfiguration.RequireInteraction);
        actualSnackBarOptions.BackgroundBlurred.Should().Be(expectedOptions.SnackbarConfiguration.BackgroundBlurred);
        actualSnackBarOptions.SnackbarVariant.Should().Be(expectedOptions.SnackbarConfiguration.SnackbarVariant);
        actualSnackBarOptions.IconSize.Should().Be(expectedOptions.SnackbarConfiguration.IconSize);
        actualSnackBarOptions.NormalIcon.Should().Be(expectedOptions.SnackbarConfiguration.NormalIcon);
        actualSnackBarOptions.InfoIcon.Should().Be(expectedOptions.SnackbarConfiguration.InfoIcon);
        actualSnackBarOptions.SuccessIcon.Should().Be(expectedOptions.SnackbarConfiguration.SuccessIcon);
        actualSnackBarOptions.WarningIcon.Should().Be(expectedOptions.SnackbarConfiguration.WarningIcon);
        actualSnackBarOptions.ErrorIcon.Should().Be(expectedOptions.SnackbarConfiguration.ErrorIcon);
    }

    [Test]
    public void AddVekDialog_WhenServiceAlreadyRegistered_ShouldNotOverwrite()
    {
        // Arrange: registration uses TryAddScoped so a pre-registered service must survive.
        var existing = new StubDialogService();
        var services = new ServiceCollection();
        services.AddSingleton<IDialogService>(existing);

        // Act
        services.AddVekDialog();
        var serviceProvider = services.BuildServiceProvider();
        var dialogService = serviceProvider.GetService<IDialogService>();

        // Assert
        dialogService.Should().BeSameAs(existing);
    }

    [Test]
    public void AddLocalizationInterceptor_ShouldReplaceDefaultInterceptor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddVekLocalization();

        // Act
        services.AddLocalizationInterceptor<StubLocalizationInterceptor>();

        // Assert: Replace keeps a single registration but swaps the implementation.
        var descriptors = services.Where(x => x.ServiceType == typeof(ILocalizationInterceptor)).ToList();
        descriptors.Should().ContainSingle();
        descriptors[0].ImplementationType.Should().Be<StubLocalizationInterceptor>();
    }

    private sealed class StubDialogService : DialogService;

    private sealed class StubLocalizationInterceptor : ILocalizationInterceptor
    {
        public LocalizedString Handle(string key, params object[] arguments) => new(key, key, resourceNotFound: true);
    }
}
