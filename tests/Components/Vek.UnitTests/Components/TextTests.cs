using System.Linq;
using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

[TestFixture]
public class TextTests : BunitTest
{
    [Test]
    public void Defaults_ShouldExposeExpectedParameterValues()
    {
        var comp = Context.Render<VekText>();

        comp.Instance.Typo.Should().Be(Typo.body1);
        comp.Instance.Align.Should().Be(Align.Inherit);
        comp.Instance.Color.Should().Be(Color.Inherit);
        comp.Instance.GutterBottom.Should().BeFalse();
        comp.Instance.Inline.Should().BeFalse();
        comp.Instance.HtmlTag.Should().BeNull();
    }

    [Test]
    public void Defaults_ShouldRenderBody1ParagraphWithoutOptionalClasses()
    {
        var comp = Context.Render<VekText>();

        comp.MarkupMatches("""<p class="vek-typography vek-typography-body1"></p>""");
    }

    /// <summary>
    /// The typography element renders its content.
    /// </summary>
    [Test]
    public void Text_RendersTypographyElementWithContent()
    {
        var comp = Context.Render<VekText>(parameters => parameters.AddChildContent("Flat white"));

        comp.Find("p.vek-typography").TextContent.Should().Be("Flat white");
    }

    [Test]
    public void UserAttributes_ShouldBeSplattedOnTheRootElement()
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .AddUnmatched("data-test", "test-value")
            .AddUnmatched("aria-label", "example"));

        var element = comp.Find(".vek-typography");

        element.GetAttribute("data-test").Should().Be("test-value");
        element.GetAttribute("aria-label").Should().Be("example");
    }

    [TestCase(Align.Inherit, false, null)]
    [TestCase(Align.Left, false, "vek-typography-align-left")]
    [TestCase(Align.Center, false, "vek-typography-align-center")]
    [TestCase(Align.Right, false, "vek-typography-align-right")]
    [TestCase(Align.Justify, false, "vek-typography-align-justify")]
    [TestCase(Align.Start, false, "vek-typography-align-left")]
    [TestCase(Align.End, false, "vek-typography-align-right")]
    [TestCase(Align.Start, true, "vek-typography-align-right")]
    [TestCase(Align.End, true, "vek-typography-align-left")]
    public void Align_ShouldRenderTheExpectedAlignmentClass(Align align, bool rightToLeft, string expectedClass)
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.Align, align)
            .Add(p => p.RightToLeft, rightToLeft));

        var element = comp.Find(".vek-typography");
        var alignmentClasses = element
            .ClassList
            .Where(x => x.StartsWith("vek-typography-align-"))
            .ToArray();

        if (expectedClass is null)
        {
            alignmentClasses.Should().BeEmpty();
        }
        else
        {
            alignmentClasses.Should().Contain(expectedClass);
            alignmentClasses.Should().HaveCount(1);
        }
    }

    [TestCase(Color.Inherit, null)]
    [TestCase(Color.Default, null)]
    [TestCase(Color.Primary, "vek-primary-text")]
    [TestCase(Color.Secondary, "vek-secondary-text")]
    [TestCase(Color.Tertiary, "vek-tertiary-text")]
    [TestCase(Color.Error, "vek-error-text")]
    public void Color_ShouldRenderTheExpectedTextColorClass(Color color, string expectedClass)
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.Color, color));

        var element = comp.Find(".vek-typography");
        var colorClasses = element
            .ClassList
            .Where(x => x.EndsWith("-text"))
            .ToArray();

        if (expectedClass is null)
        {
            colorClasses.Should().BeEmpty();
        }
        else
        {
            colorClasses.Should().Contain(expectedClass);
            colorClasses.Should().HaveCount(1);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GutterBottom_ShouldRenderTheMarginClassOnlyWhenEnabled(bool gutterBottom)
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.GutterBottom, gutterBottom));

        var element = comp.Find(".vek-typography");

        if (gutterBottom)
        {
            element.ClassList.Should().Contain("vek-typography-gutterbottom");
        }
        else
        {
            element.ClassList.Should().NotContain("vek-typography-gutterbottom");
        }
    }

    [Test]
    public void ChildContent_ShouldRenderInsideTheTypographyElement()
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .AddChildContent("Hello, World!"));

        comp.MarkupMatches("""<p class="vek-typography vek-typography-body1">Hello, World!</p>""");
    }

    [TestCase(Typo.inherit, "span", "vek-typography-inherit")]
    [TestCase(Typo.h1, "h1", "vek-typography-h1")]
    [TestCase(Typo.h2, "h2", "vek-typography-h2")]
    [TestCase(Typo.h3, "h3", "vek-typography-h3")]
    [TestCase(Typo.h4, "h4", "vek-typography-h4")]
    [TestCase(Typo.h5, "h5", "vek-typography-h5")]
    [TestCase(Typo.h6, "h6", "vek-typography-h6")]
    [TestCase(Typo.subtitle1, "p", "vek-typography-subtitle1")]
    [TestCase(Typo.subtitle2, "p", "vek-typography-subtitle2")]
    [TestCase(Typo.body1, "p", "vek-typography-body1")]
    [TestCase(Typo.body2, "p", "vek-typography-body2")]
    [TestCase(Typo.button, "span", "vek-typography-button")]
    [TestCase(Typo.caption, "span", "vek-typography-caption")]
    [TestCase(Typo.overline, "span", "vek-typography-overline")]
    public void Typo_ShouldRenderTheExpectedTagAndTypographyClass(Typo typo, string expectedTag, string expectedClass)
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.Typo, typo)
            .AddChildContent("content"));

        var element = comp.Find(".vek-typography");
        var typographyClasses = element
            .ClassList
            .Where(x => x.StartsWith("vek-typography-"))
            .ToArray();

        element.TagName.Should().Be(expectedTag.ToUpperInvariant());
        typographyClasses.Should().Contain(expectedClass);
        typographyClasses.Should().HaveCount(1);
        element.TextContent.Should().Be("content");
    }

    [TestCase(Typo.h1, null, "h1", "vek-typography-h1")]
    [TestCase(Typo.body1, "", "p", "vek-typography-body1")]
    [TestCase(Typo.caption, "p", "p", "vek-typography-caption")]
    [TestCase(Typo.h4, "span", "span", "vek-typography-h4")]
    public void HtmlTag_ShouldOverrideOrFallbackToTheTypoSelectedTag(Typo typo, string htmlTag, string expectedTag, string expectedClass)
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.Typo, typo)
            .Add(p => p.HtmlTag, htmlTag)
            .AddChildContent("content"));

        var element = comp.Find(".vek-typography");
        var typographyClasses = element
            .ClassList
            .Where(x => x.StartsWith("vek-typography-"))
            .ToArray();

        element.TagName.Should().Be(expectedTag.ToUpperInvariant());
        typographyClasses.Should().Contain(expectedClass);
        typographyClasses.Should().HaveCount(1);
        element.TextContent.Should().Be("content");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Inline_ShouldRenderTheDisplayClassOnlyWhenEnabled(bool inline)
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.Inline, inline));

        var element = comp.Find(".vek-typography");

        if (inline)
        {
            element.ClassList.Should().Contain("d-inline");
        }
        else
        {
            element.ClassList.Should().NotContain("d-inline");
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Truncate_ShouldAddNowrapClass(bool truncate)
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.Truncate, truncate));

        var element = comp.Find(".vek-typography");

        if (truncate)
        {
            element.ClassList.Should().Contain("vek-typography-nowrap");
        }
        else
        {
            element.ClassList.Should().NotContain("vek-typography-nowrap");
        }

    }
}
