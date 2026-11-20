using System.Globalization;
using AwesomeAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Vek.Resources;
using NUnit.Framework;

namespace Vek.UnitTests.Services.Localization;

#nullable enable
[TestFixture(Description = "Similar to InternalVekLocalizerTests but more narrow focusing on IgnoreDefaultEnglish cases.")]
public class DefaultLocalizationInterceptorTests
{
    [Test]
    public void NullVekLocalizer_IgnoreDefaultEnglishTrue_ReturnsDefaultEnglish()
    {
        // Even though IgnoreDefaultEnglish is "true", we still should get default English localization
        // Because we didn't provide custom VekLocalizer, so it will fall back.

        // Arrange
        var defaultLocalizationInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer: null)
        {
            IgnoreDefaultEnglish = true
        };
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationInterceptor);
        var resourceManager = LanguageResource.ResourceManager;
        var resourceSet = resourceManager
            .GetResourceSet(CultureInfo.InvariantCulture, true, true)
            .ToEnumerable()
            .ToDictionary(x => (string)x.Key, x => x.Value?.ToString(), StringComparer.Ordinal);

        // Act & Result
        foreach (var resource in resourceSet)
        {
            var translation = internalVekLocalizer[resource.Key];

            translation.Should().Be(resource.Value);
        }
    }

    [Test]
    [SetUICulture("en-US")]
    public void CustomVekLocalizer_IgnoreDefaultEnglishFalse_EnglishUICulture_ReturnsDefaultEnglish()
    {
        // When IgnoreDefaultEnglish is "false" and CurrentUICulture is "en", we return default English localization
        // Even when custom VekLocalizer is provided

        // Arrange
        var vekLocalizer = new Mock<VekLocalizer> { CallBase = true };
        vekLocalizer.Setup(mock => mock[LanguageResource.VekDataGrid_IsEmpty]).Returns(new LocalizedString(LanguageResource.VekDataGrid_IsEmpty, "XXX", resourceNotFound: false));
        var defaultLocalizationInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer.Object)
        {
            IgnoreDefaultEnglish = false
        };
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationInterceptor);

        // Act
        var result = internalVekLocalizer[LanguageResource.VekDataGrid_IsEmpty];

        // Assert
        result.Should().Be(LanguageResource.ResourceManager.GetString(LanguageResource.VekDataGrid_IsEmpty), "We default to english despite VekLocalizer, the value should be the one from the LanguageResource.");
    }

    [Test]
    [SetUICulture("fr-FR")]
    public void CustomVekLocalizer_IgnoreDefaultEnglishFalse_NonEnglishUICulture_ReturnsCustomLocalization()
    {
        // When IgnoreDefaultEnglish is "false" and CurrentUICulture is NOT "en", we return localization provided from custom VekLocalizer

        // Arrange
        var vekLocalizer = new Mock<VekLocalizer> { CallBase = true };
        vekLocalizer.Setup(mock => mock[LanguageResource.VekDataGrid_IsEmpty]).Returns(new LocalizedString(LanguageResource.VekDataGrid_IsEmpty, "XXX", resourceNotFound: false));
        var defaultLocalizationInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer.Object)
        {
            IgnoreDefaultEnglish = false
        };
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationInterceptor);

        // Act
        var result = internalVekLocalizer[LanguageResource.VekDataGrid_IsEmpty];

        // Assert
        result.Should().Be("XXX", "The UICulture is not English therefore the value is the one from the Mock.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    [SetUICulture("fr-FR")]
    public void CustomVekLocalizer_IgnoreDefaultEnglishFalse_TemplateTranslation_ReturnsFormattedString(bool ignoreDefaultEnglish)
    {
        // Regardless of the IgnoreDefaultEnglish setting, the custom localization should be returned.
        // This is because custom VekLocalizer was provided, and we have an existing key.

        // Arrange
        var vekLocalizer = new Mock<VekLocalizer> { CallBase = true };
        vekLocalizer.Setup(mock => mock["TemplateString"]).Returns(new LocalizedString("TemplateString", "Bonjour {0}!", resourceNotFound: false));
        var defaultLocalizationInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer.Object)
        {
            IgnoreDefaultEnglish = ignoreDefaultEnglish
        };
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationInterceptor);

        // Act
        var result = internalVekLocalizer["TemplateString", "le monde"];

        // Assert
        result.Should().Be("Bonjour le monde!", "The value should be the template string with the provided parameter.");
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    [SetUICulture("en-US")]
    public void CustomVekLocalizer_DefaultLocalizationFallback_TemplateTranslation_ReturnsFormattedString(bool ignoreDefaultEnglish)
    {
        // Regardless of the IgnoreDefaultEnglish setting, the default English localization should be returned.
        // Although a custom VekLocalizer is provided, the key does not exist, resulting in resourceNotFound being "true".
        // Consequently, this causes a fallback to the default English localization.

        // Arrange
        var vekLocalizer = new Mock<VekLocalizer> { CallBase = true };
        var defaultLocalizationInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer.Object)
        {
            IgnoreDefaultEnglish = ignoreDefaultEnglish
        };
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationInterceptor);

        // Act

        var result = internalVekLocalizer[LanguageResource.VekBaseDatePicker_PrevMonth, "2024"];

        // Assert
        var expectedValue = GetResourceString(LanguageResource.VekBaseDatePicker_PrevMonth, "2024");
        result.Should().Be(expectedValue);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    [SetUICulture("en-US")]
    public void CustomVekLocalizer_DefaultLocalizationFallback_KeyTranslation_ReturnsString(bool ignoreDefaultEnglish)
    {
        // Regardless of the IgnoreDefaultEnglish setting, the default English localization should be returned.
        // Although a custom VekLocalizer is provided, the key does not exist, resulting in resourceNotFound being "true".
        // Consequently, this causes a fallback to the default English localization.

        // Arrange
        var vekLocalizer = new Mock<VekLocalizer> { CallBase = true };
        var defaultLocalizationInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer.Object)
        {
            IgnoreDefaultEnglish = ignoreDefaultEnglish
        };
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationInterceptor);

        // Act
        var result = internalVekLocalizer[LanguageResource.VekColorPicker_ModeSwitch];

        // Assert
        var expectedValue = GetResourceString(LanguageResource.VekColorPicker_ModeSwitch);
        result.Should().Be(expectedValue);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    [SetUICulture("en-US")]
    public void NullVekLocalizer_DefaultLocalizationFallback_TemplateTranslation_ReturnsFormattedString(bool ignoreDefaultEnglish)
    {
        // Regardless of the IgnoreDefaultEnglish setting, the default English localization should be returned.
        // This is because no custom VekLocalizer was provided, causing a fallback to the default English localization even when IgnoreDefaultEnglish is "true".

        // Arrange
        var defaultLocalizationIInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer: null)
        {
            IgnoreDefaultEnglish = ignoreDefaultEnglish
        };
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationIInterceptor);

        // Act
        var result = internalVekLocalizer[LanguageResource.VekBaseDatePicker_PrevMonth, "2024"];

        // Assert
        var expectedValue = GetResourceString(LanguageResource.VekBaseDatePicker_PrevMonth, "2024");
        result.Should().Be(expectedValue, "The value should be the template string with the provided parameter.");
    }

    [Test]
    [NonParallelizable]
    [SetUICulture("sv-SE")]
    public void DefaultEnglishLookup_NonEnglishUICulture_DoesNotProbeSatelliteAssembly()
    {
        // Regression test for #13461. Reading the built-in English resources under a non-English UI culture must not make the ResourceManager probe for a (non-existent) Vek.resources satellite assembly.
        // On the Blazor WebAssembly runtime each probe throws a first-chance FileNotFoundException; on the runtime used by the test host the same probe surfaces as an AssemblyResolve request.
        // A value-only assertion is not enough: the English fallback resolves correctly either way, so the test asserts that the probe never happens.

        // Arrange
        var defaultLocalizationInterceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer: null);
        var internalVekLocalizer = new InternalVekLocalizer(defaultLocalizationInterceptor);
        var satelliteProbes = 0;
        ResolveEventHandler handler = (_, args) =>
        {
            if (args.Name.StartsWith("Vek.resources", StringComparison.OrdinalIgnoreCase))
            {
                Interlocked.Increment(ref satelliteProbes);
            }

            return null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += handler;

        // Act
        LocalizedString result;
        string value;
        try
        {
            // ResourceNotFound is read from the interceptor because InternalVekLocalizer's indexer returns the value only.
            result = defaultLocalizationInterceptor.Handle(LanguageResource.VekDataGrid_Clear);
            value = internalVekLocalizer[LanguageResource.VekDataGrid_Clear];
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= handler;
        }

        // Assert
        satelliteProbes.Should().Be(0, "the invariant-culture lookup must not probe for a culture-specific satellite assembly");
        result.ResourceNotFound.Should().BeFalse();
        value.Should().Be("Clear", "the built-in English fallback is still returned under a non-English UI culture");
    }

    /// <summary>
    /// Repeated lookups of a built-in key are served from the cache and stay on the invariant value.
    /// </summary>
    [Test]
    [NonParallelizable]
    [SetUICulture("sv-SE")]
    public void DefaultEnglishLookup_RepeatedReads_StayOnTheInvariantValue()
    {
        // Arrange
        var interceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer: null);

        // Act
        var first = interceptor.Handle(LanguageResource.VekDataGrid_Clear);
        var second = interceptor.Handle(LanguageResource.VekDataGrid_Clear);

        // Assert
        first.ResourceNotFound.Should().BeFalse();
        second.ResourceNotFound.Should().BeFalse();
        second.Value.Should().Be(first.Value).And.Be("Clear");
    }

    /// <summary>
    /// A key that is not a resource must keep reporting itself as missing, so it is never served from the cache.
    /// </summary>
    /// <remarks>
    /// Callers pass arbitrary strings through this path, such as a conversion exception message, so caching a miss would let the cache grow without bound.
    /// </remarks>
    [Test]
    public void UnknownKey_RepeatedReads_KeepReportingResourceNotFound()
    {
        // Arrange
        var interceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer: null);
        const string UnknownKey = "This is not a resource key, it is an exception message.";

        // Act
        var first = interceptor.Handle(UnknownKey);
        var second = interceptor.Handle(UnknownKey);

        // Assert
        first.ResourceNotFound.Should().BeTrue();
        second.ResourceNotFound.Should().BeTrue();
        second.Value.Should().Be(UnknownKey);
    }

    private static string GetResourceString(string key, params object[] parameters)
    {
        var resourceString = LanguageResource.GetResourceString(key) ?? string.Empty;

        return string.Format(CultureInfo.CurrentUICulture, resourceString, parameters);
    }
}
