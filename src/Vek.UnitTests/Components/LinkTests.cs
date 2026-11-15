using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.UnitTests.TestComponents.Link;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

[TestFixture]
public class LinkTests : BunitTest
{
    [Test]
    public void DefaultPropertyValues()
    {
        var comp = Context.Render<VekLink>();

        comp.Instance.Color.Should().Be(Color.Primary);
        comp.Instance.Typo.Should().Be(Typo.inherit);
        comp.Instance.Underline.Should().Be(Underline.Hover);
        comp.Instance.Href.Should().BeNull();
        comp.Instance.Target.Should().BeNull();
        comp.Instance.Disabled.Should().BeFalse();
    }

    [Test]
    public void DefaultTypo_ShouldInheritParentTypography()
    {
        var comp = Context.Render<VekLink>();

        var linkElement = comp.Find("a");
        linkElement.GetAttribute("class").Should().NotContain("vek-typography-");
    }

    [Test]
    public void InlineLink_ShouldRenderWithParentTypography()
    {
        var comp = Context.Render<VekText>(parameters => parameters
            .Add(p => p.Typo, Typo.caption)
            .AddChildContent(childBuilder =>
            {
                childBuilder.AddContent(0, "Vek is ");
                childBuilder.OpenComponent<VekLink>(1);
                childBuilder.AddAttribute(2, nameof(VekLink.ChildContent), (RenderFragment)(linkBuilder => linkBuilder.AddContent(0, "Awesome")));
                childBuilder.CloseComponent();
            }));

        var textElement = comp.Find("span.vek-typography");
        textElement.ClassList.Should().Contain("vek-typography-caption");

        var linkElement = textElement.QuerySelector("a.vek-link")!;
        linkElement.ClassList.Should().Contain("vek-typography");
        linkElement.GetAttribute("class").Should().NotContain("vek-typography-");
        linkElement.TextContent.Should().Be("Awesome");

        comp.MarkupMatches("""<span class="vek-typography vek-typography-caption">Vek is <a class="vek-typography vek-link vek-primary-text vek-link-underline-hover">Awesome</a></span>""");
    }

    [Test]
    public void DisabledProperty_DisplaysAsDisabled()
    {
        var comp = Context.Render<VekLink>(parameters => parameters
            .Add(x => x.Href, "#")
            .Add(x => x.Disabled, true));

        var linkElement = comp.Find("a");
        linkElement.GetAttribute("href").Should().BeNullOrEmpty();
        linkElement.GetAttribute("aria-disabled").Should().Be("true");
        linkElement.ClassList.Should().Contain("vek-link-disabled");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ShouldExecute_OnClick(bool disabled)
    {
        var calls = 0;
        var comp = Context.Render<VekLink>(builder => builder
            .Add(p => p.OnClick, e => calls++)
            .Add(p => p.Disabled, disabled)
        );

        await comp.Find("a").ClickAsync(new MouseEventArgs());

        if (disabled)
        {
            calls.Should().Be(0);
        }
        else
        {
            calls.Should().Be(1);
        }
    }

    [Test]
    public void Icons_RenderInMarkup()
    {
        var comp = Context.Render<VekLink>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Material.Filled.Home)
            .Add(p => p.EndIcon, Icons.Material.Filled.OpenInNew)
            .AddChildContent("Link Text"));

        comp.Find(".vek-link-icon-start").Should().NotBeNull();
        comp.Find(".vek-link-icon-end").Should().NotBeNull();
        comp.Markup.Should().Contain("Link Text");
    }

    [Test]
    public void Icons_CustomClass()
    {
        var comp = Context.Render<VekLink>(parameters => parameters
            .Add(p => p.StartIcon, Icons.Material.Filled.Home)
            .Add(p => p.IconClass, "custom-icon-class"));

        var icon = comp.FindComponent<VekIcon>();
        icon.Instance.Class.Should().Contain("custom-icon-class");
        icon.Instance.Class.Should().Contain("vek-link-icon-start");
    }

    [Test]
    public async Task OnClickErrorContentCaughtException()
    {
        var comp = Context.Render<LinkErrorContenCaughtException>();
        IElement AlertText() => VekAlert().Find("div.vek-alert-message");
        IRenderedComponent<VekAlert> VekAlert() => comp.FindComponent<VekAlert>();
        IReadOnlyList<IElement> Links() => comp.FindAll("a.vek-link");
        IElement VekLink() => Links()[0];

        await VekLink().ClickAsync(new MouseEventArgs());

        AlertText().InnerHtml.Should().Be("Oh my! We caught an error and handled it!");
    }

    [TestCase(Color.Primary, "vek-primary-text")]
    [TestCase(Color.Secondary, "vek-secondary-text")]
    [TestCase(Color.Tertiary, "vek-tertiary-text")]
    public void ColorProperty_AppliesCorrectClass(Color color, string expectedClass)
    {
        var comp = Context.Render<VekLink>(builder => builder
            .Add(p => p.Color, color)
        );

        var linkElement = comp.Find("a");
        linkElement.ClassList.Should().Contain(expectedClass);
    }

    [TestCase(Typo.h1, "vek-typography-h1")]
    [TestCase(Typo.subtitle1, "vek-typography-subtitle1")]
    [TestCase(Typo.caption, "vek-typography-caption")]
    public void TypoProperty_AppliesCorrectClass(Typo typo, string expectedClass)
    {
        var comp = Context.Render<VekLink>(builder => builder
            .Add(p => p.Typo, typo)
        );

        var linkElement = comp.Find("a");
        linkElement.ClassList.Should().Contain(expectedClass);
    }

    [TestCase(Underline.None, "vek-link-underline-none")]
    [TestCase(Underline.Hover, "vek-link-underline-hover")]
    [TestCase(Underline.Always, "vek-link-underline-always")]
    public void UnderlineProperty_AppliesCorrectClass(Underline underline, string expectedClass)
    {
        var comp = Context.Render<VekLink>(builder => builder
            .Add(p => p.Underline, underline)
        );

        var linkElement = comp.Find("a");
        linkElement.ClassList.Should().Contain(expectedClass);
    }

    [TestCase("_blank")]
    [TestCase("_self")]
    [TestCase("_parent")]
    [TestCase("_top")]
    public void TargetProperty_AppliesCorrectAttribute(string target)
    {
        var comp = Context.Render<VekLink>(builder => builder
            .Add(p => p.Href, "#")
            .Add(p => p.Target, target)
        );

        var linkElement = comp.Find("a");
        linkElement.GetAttribute("target").Should().Be(target);
    }

    [Test]
    public void ChildContent_IsRenderedCorrectly()
    {
        var comp = Context.Render<VekLink>(builder => builder
            .AddChildContent("<span>Test content</span>")
        );

        var linkElement = comp.Find("a");
        linkElement.InnerHtml.Should().Be("<span>Test content</span>");
    }

    [Test]
    public void UserAttributes_OverrideDefaultAttributes()
    {
        var comp = Context.Render<VekLink>(builder => builder
            .Add(p => p.Href, "#")
            .Add(p => p.Target, "_self")
            .Add(p => p.UserAttributes, new()
            {
                { "target", "custom-target" },
                { "role", "custom-role" },
                { "custom-attribute", "custom-value" }
            })
        );

        var linkElement = comp.Find("a");

        // Verify that user-defined attributes override default attributes, even ones set by properties.
        linkElement.GetAttribute("target").Should().Be("custom-target");
        linkElement.GetAttribute("role").Should().Be("custom-role");
        linkElement.GetAttribute("custom-attribute").Should().Be("custom-value");
    }
}
