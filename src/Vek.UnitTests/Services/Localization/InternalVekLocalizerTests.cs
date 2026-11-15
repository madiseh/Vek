using AwesomeAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Vek.Resources;
using NUnit.Framework;

namespace Vek.UnitTests.Services.Localization;

#nullable enable
[TestFixture]
public class InternalVekLocalizerTests
{
    [Test]
    public void Indexer_ReturnsString_SoBlazorCanCompareItByValue()
    {
        // Blazor only compares parameters by value for known-immutable types, and ChangeDetection.MayHaveChanged returns true for every other reference type.
        // A LocalizedString return would therefore rebuild any component given a localized attribute, on every render of its parent, forever.
        var indexer = typeof(InternalVekLocalizer).GetProperty("Item", new[] { typeof(string), typeof(object[]) });

        indexer.Should().NotBeNull();
        indexer!.PropertyType.Should().Be<string>();
    }

    [Test]
    public void Constructor_WithNullInterceptor_ShouldThrowArgumentNullException()
    {
        // Arrange
        ILocalizationInterceptor? interceptor = null;

        // Act
        var construct = () => new InternalVekLocalizer(interceptor!);

        // Assert
        construct.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithNullEnumInterceptor_ShouldThrowArgumentNullException()
    {
        // Arrange
        var interceptorMock = new Mock<ILocalizationInterceptor>();
        ILocalizationEnumInterceptor? enumInterceptor = null;

        // Act
        var construct = () => new InternalVekLocalizer(interceptorMock.Object, enumInterceptor!);

        // Assert
        construct.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_WithValidInterceptor_ShouldNotThrowException()
    {
        // Arrange
        var interceptorMock = new Mock<ILocalizationInterceptor>();

        // Act
        var construct = () => new InternalVekLocalizer(interceptorMock.Object);

        // Assert
        construct.Should().NotThrow();
    }

    [Test]
    [SetUICulture("en-US")]
    public void CustomLocalizationInterceptor_EnglishUICulture()
    {
        var interceptorMock = new Mock<ILocalizationInterceptor>();
        interceptorMock.Setup(mock => mock.Handle(LanguageResource.VekDataGrid_Clear)).Returns(new LocalizedString(LanguageResource.VekDataGrid_Clear, "Reset", false));
        var internalVekLocalizer = new InternalVekLocalizer(interceptorMock.Object);

        // Act
        var result = internalVekLocalizer[LanguageResource.VekDataGrid_Clear];

        // Assert
        result.Should().Be("Reset");
    }

    [Test]
    [SetUICulture("de-DE")]
    public void CustomLocalizationInterceptor_NonEnglishUICulture()
    {
        // Assert
        var interceptorMock = new Mock<ILocalizationInterceptor>();
        interceptorMock.Setup(mock => mock.Handle(LanguageResource.VekDataGrid_Clear)).Returns(new LocalizedString(LanguageResource.VekDataGrid_Clear, "Reset", false));
        var internalVekLocalizer = new InternalVekLocalizer(interceptorMock.Object);

        // Act
        var result = internalVekLocalizer[LanguageResource.VekDataGrid_Clear];

        result.Should().Be("Reset");
    }

    [Test]
    [SetUICulture("en-US")]
    public void DefaultLocalizationInterceptor_EnglishUICulture()
    {
        // Arrange
        var interceptorMock = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer: null);
        var internalVekLocalizer = new InternalVekLocalizer(interceptorMock);

        // Act & Assert
        internalVekLocalizer[LanguageResource.VekDataGrid_Contains].Should().Be("contains");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsEmpty].Should().Be("is empty");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsNotEmpty].Should().Be("is not empty");
    }

    [Test]
    [SetUICulture("de-DE")]
    public void DefaultLocalizationInterceptor_NonEnglishUICulture()
    {
        // Arrange
        var interceptorMock = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizer: null);
        var internalVekLocalizer = new InternalVekLocalizer(interceptorMock);

        // Act & Assert
        internalVekLocalizer[LanguageResource.VekDataGrid_Contains].Should().Be("contains");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsEmpty].Should().Be("is empty");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsNotEmpty].Should().Be("is not empty");
    }

    [Test]
    [SetUICulture("en-US")]
    public void DefaultLocalizationInterceptor_WithCustomVekLocalizer_EnglishUICulture()
    {
        // Arrange
        var vekLocalizerMock = new Mock<VekLocalizer> { CallBase = true };
        vekLocalizerMock.Setup(mock => mock[LanguageResource.VekDataGrid_IsEmpty]).Returns(new LocalizedString(LanguageResource.VekDataGrid_IsEmpty, "XXX", false));
        vekLocalizerMock.Setup(mock => mock[LanguageResource.VekDataGrid_IsNotEmpty]).Returns(new LocalizedString(LanguageResource.VekDataGrid_IsNotEmpty, "VekDataGrid_IsNotEmpty", true));
        var interceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizerMock.Object);
        var internalVekLocalizer = new InternalVekLocalizer(interceptor);

        // Act & Assert
        internalVekLocalizer[LanguageResource.VekDataGrid_Contains].Should().Be("contains");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsEmpty].Should().Be("is empty");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsNotEmpty].Should().Be("is not empty");
    }

    [Test]
    [SetUICulture("de-DE")]
    public void DefaultLocalizationInterceptor_WithCustomVekLocalizer_NonEnglishUICulture()
    {
        // Arrange
        var vekLocalizerMock = new Mock<VekLocalizer> { CallBase = true };
        vekLocalizerMock.Setup(mock => mock[LanguageResource.VekDataGrid_IsEmpty]).Returns(new LocalizedString(LanguageResource.VekDataGrid_IsEmpty, "XXX", false));
        vekLocalizerMock.Setup(mock => mock[LanguageResource.VekDataGrid_IsNotEmpty]).Returns(new LocalizedString(LanguageResource.VekDataGrid_IsNotEmpty, "VekDataGrid_IsNotEmpty", true));
        var interceptor = new DefaultLocalizationInterceptor(NullLoggerFactory.Instance, vekLocalizerMock.Object);
        var internalVekLocalizer = new InternalVekLocalizer(interceptor);

        // Act & Assert
        internalVekLocalizer[LanguageResource.VekDataGrid_Contains].Should().Be("contains");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsEmpty].Should().Be("XXX");
        internalVekLocalizer[LanguageResource.VekDataGrid_IsNotEmpty].Should().Be("is not empty");
    }
}
