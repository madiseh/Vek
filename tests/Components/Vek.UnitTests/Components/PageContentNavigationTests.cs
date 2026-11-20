
using AwesomeAssertions;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Vek.Interfaces;
using Vek.Resources;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class PageContentNavigationTests : BunitTest
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Context.Services.Add(new ServiceDescriptor(typeof(IScrollSpyFactory), new MockScrollSpyFactory()));
        }

        [Test]
        public void DefaultValues()
        {
            var comp = Context.Render<VekPageContentNavigation>();

            comp.Instance.ActiveSection.Should().BeNull();
            comp.Instance.Sections.Should().BeEmpty();
            comp.Instance.Headline.Should().Be("");
            comp.Instance.SectionClassSelector.Should().BeNullOrEmpty();

            comp.Nodes.Should().ContainSingle();
            comp.Nodes[0].ChildNodes.Should().BeEmpty();
        }

        [Test]
        public async Task CustomNavMenuTitle()
        {
            var comp = Context.Render<VekPageContentNavigation>(parameters => parameters.Add(x => x.Headline, "Custom title"));

            var section1 = new VekPageContentSection("my section", "my-id");
            var section2 = new VekPageContentSection("my section 2", "my-id-2");

            comp.Instance.AddSection(section1, false);
            comp.Instance.AddSection(section2, false);
            await comp.InvokeAsync(() => ((IVekStateHasChanged)comp.Instance).StateHasChanged());

            comp.RenderCount.Should().Be(2);

            comp.Instance.ActiveSection.Should().BeNull();
            comp.Instance.Sections.Should().BeEquivalentTo(new[] { section1, section2 });

            comp.Nodes.Should().ContainSingle();
            comp.Instance.Headline.Should().Be("Custom title");

            var navMenu = comp.FindComponent<VekNavMenu>();
            navMenu.Instance.UserAttributes.GetValueOrDefault("aria-label").Should().Be("Custom title");

            var headlineItem = comp.FindComponent<VekText>();
            headlineItem.FindAll("p.vek-typography")[0].TextContent.Should().Be("Custom title");

            var navLinks = comp.FindComponents<VekNavLink>();
            navLinks.Should().HaveCount(2);
            navLinks[0].Instance.Class.Should().Be("page-content-navigation-navlink navigation-level-0");

            var firstLinkText = navLinks[0].Find(".vek-nav-link-text");
            firstLinkText.TextContent.Should().Be("my section");

            var secondLinkText = navLinks[1].Find(".vek-nav-link-text");
            secondLinkText.TextContent.Should().Be("my section 2");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task AddSection(bool withUpdate)
        {
            var localizer = Context.Services.GetRequiredService<InternalVekLocalizer>();
            var comp = Context.Render<VekPageContentNavigation>();

            var section1 = new VekPageContentSection("my section", "my-id");
            var section2 = new VekPageContentSection("my section 2", "my-id-2");

            if (withUpdate)
            {
                await comp.InvokeAsync(() => comp.Instance.AddSection(section1, true));
                await comp.InvokeAsync(() => comp.Instance.AddSection(section2, true));
            }
            else
            {
                comp.Instance.AddSection(section1, false);
                comp.Instance.AddSection(section2, false);
                await comp.InvokeAsync(() => ((IVekStateHasChanged)comp.Instance).StateHasChanged());
            }

            comp.RenderCount.Should().Be(withUpdate ? 3 : 2);

            comp.Instance.ActiveSection.Should().BeNull();
            comp.Instance.Sections.Should().BeEquivalentTo(new[] { section1, section2 });

            comp.Nodes.Should().ContainSingle();

            var navMenu = comp.FindComponent<VekNavMenu>();
            navMenu.Instance.UserAttributes.GetValueOrDefault("aria-label").Should().Be(localizer[LanguageResource.VekPageContentNavigation_NavMenu]);

            var headlineItem = comp.FindComponent<VekText>();
            headlineItem.FindAll("p.vek-typography")[0].TextContent.Should().Be(localizer[LanguageResource.VekPageContentNavigation_NavMenu]);

            var navLinks = comp.FindComponents<VekNavLink>();
            navLinks.Should().HaveCount(2);
            navLinks[0].Instance.Class.Should().Be("page-content-navigation-navlink navigation-level-0");

            var firstLinkText = navLinks[0].Find(".vek-nav-link-text");
            firstLinkText.TextContent.Should().Be("my section");

            var secondLinkText = navLinks[1].Find(".vek-nav-link-text");
            secondLinkText.TextContent.Should().Be("my section 2");
        }

        [Test]
        public async Task AddSection_WhichIsActiveByScrollSpy()
        {
            var mockedScrollSpy = new MockScrollSpy
            {
                CenteredSection = "my-id"
            };

            var factory = new MockScrollSpyFactory(mockedScrollSpy);
            Context.Services.Add(new ServiceDescriptor(typeof(IScrollSpyFactory), factory));

            var comp = Context.Render<VekPageContentNavigation>();

            var section1 = new VekPageContentSection("my section", "my-id");
            var section2 = new VekPageContentSection("different section", "my-id-2");

            comp.Instance.AddSection(section1, false);
            comp.Instance.AddSection(section2, false);

            await comp.InvokeAsync(() => ((IVekStateHasChanged)comp.Instance).StateHasChanged());

            comp.RenderCount.Should().Be(2);

            comp.Instance.ActiveSection.Should().Be(section1);
            comp.Instance.Sections.Should().BeEquivalentTo(new[] { section1, section2 });
            comp.Nodes.Should().ContainSingle();

            var navLinks = comp.FindComponents<VekNavLink>();
            navLinks.Should().HaveCount(2);
            navLinks[0].Instance.Class.Should().Be("page-content-navigation-navlink active navigation-level-0");
            var linkText = navLinks[0].Find(".vek-nav-link-text");
            linkText.TextContent.Should().Be("my section");

            navLinks[1].Instance.Class.Should().NotContain("active");
        }

        [Test]
        public async Task AddSection_ActivateDefault()
        {
            var mockedScrollSpy = new MockScrollSpy();

            var factory = new MockScrollSpyFactory(mockedScrollSpy);
            Context.Services.Add(new ServiceDescriptor(typeof(IScrollSpyFactory), factory));

            var comp = Context.Render<VekPageContentNavigation>(p => p.Add(x => x.ActivateFirstSectionAsDefault, true));

            var section1 = new VekPageContentSection("my section", "my-id");
            var section2 = new VekPageContentSection("my section 2", "my-id-2");

            await comp.InvokeAsync(() => comp.Instance.AddSection(section1, true));
            await comp.InvokeAsync(() => comp.Instance.AddSection(section2, true));

            comp.RenderCount.Should().Be(3);

            comp.Instance.ActiveSection.Should().Be(section1);
            comp.Instance.Sections.Should().BeEquivalentTo(new[] { section1, section2 });
            comp.Nodes.Should().ContainSingle();

            var navLinks = comp.FindComponents<VekNavLink>();
            navLinks.Should().HaveCount(2);
            navLinks[0].Instance.Class.Should().Be("page-content-navigation-navlink active navigation-level-0");
            var linkText = navLinks[0].Find(".vek-nav-link-text");

            linkText.TextContent.Should().Be("my section");

            navLinks[1].Instance.Class.Should().NotContain("active");

            mockedScrollSpy.ScrollHistory.Should().BeEquivalentTo("my-id");
        }

        [Test]
        public async Task NavigateBySections()
        {
            var spyMock = new MockScrollSpy();

            var factory = new MockScrollSpyFactory(spyMock);
            Context.Services.Add(new ServiceDescriptor(typeof(IScrollSpyFactory), factory));

            var comp = Context.Render<VekPageContentNavigation>();

            var section1 = new VekPageContentSection("my first section", "my-id1");
            var section2 = new VekPageContentSection("my second section", "my-id2");
            var section3 = new VekPageContentSection("my third section", "my-id3");
            var sections = new[] { section1, section2, section3 };

            comp.Instance.AddSection(section1, false);
            comp.Instance.AddSection(section2, false);
            comp.Instance.AddSection(section3, false);

            await comp.InvokeAsync(() => ((IVekStateHasChanged)comp.Instance).StateHasChanged());

            for (var i = 0; i < 3; i++)
            {
                var navLinks = comp.FindComponents<VekNavLink>();
                await navLinks[i].Find(".vek-nav-link").ClickAsync();

                comp.Instance.ActiveSection.Should().Be(sections[i]);
                navLinks = comp.FindComponents<VekNavLink>();

                for (var j = 0; j < 3; j++)
                {
                    navLinks[j].Instance.Class.Should().Be(i == j ? "page-content-navigation-navlink active navigation-level-0" : "page-content-navigation-navlink navigation-level-0");
                }
            }

            spyMock.ScrollHistory.Should().BeEquivalentTo("my-id1", "my-id2", "my-id3");
        }

        [Test]
        public async Task ObserverScrollAndHandlingEvent()
        {
            var spyMock = new MockScrollSpy();

            var factory = new MockScrollSpyFactory(spyMock);
            Context.Services.Add(new ServiceDescriptor(typeof(IScrollSpyFactory), factory));

            var comp = Context.Render<VekPageContentNavigation>(x => x.Add(y => y.SectionClassSelector, "my-section-class"));

            spyMock.SpyingInitiated.Should().BeTrue();
            spyMock.SpyingClassSelector.Should().Be("my-section-class");

            var section1 = new VekPageContentSection("my first section", "my-id1");
            var section2 = new VekPageContentSection("my second section", "my-id2");
            var section3 = new VekPageContentSection("my third section", "my-id3");

            var sections = new[] { section1, section2, section3 };

            comp.Instance.AddSection(section1, false);
            comp.Instance.AddSection(section2, false);
            comp.Instance.AddSection(section3, false);

            await comp.InvokeAsync(() => ((IVekStateHasChanged)comp.Instance).StateHasChanged());

            //active second section
            await comp.InvokeAsync(() => spyMock.FireScrollSectionSectionCenteredEvent(section2.Id));
            comp.Instance.ActiveSection.Should().Be(section2);

            //active third section
            await comp.InvokeAsync(() => spyMock.FireScrollSectionSectionCenteredEvent(section3.Id));
            comp.Instance.ActiveSection.Should().Be(section3);

            //active first section
            await comp.InvokeAsync(() => spyMock.FireScrollSectionSectionCenteredEvent(section1.Id));
            comp.Instance.ActiveSection.Should().Be(section1);

            //active non existing section
            await comp.InvokeAsync(() => spyMock.FireScrollSectionSectionCenteredEvent("non-existing-section"));
            comp.Instance.ActiveSection.Should().Be(section1);

            //active empty section
            await comp.InvokeAsync(() => spyMock.FireScrollSectionSectionCenteredEvent(null));
            comp.Instance.ActiveSection.Should().Be(section1);
        }

        [Test]
        public async Task HideContentIfOnlyOneSectionIsAdded()
        {
            var comp = Context.Render<VekPageContentNavigation>();

            var section = new VekPageContentSection("my section", "my-id");

            await comp.InvokeAsync(() => comp.Instance.AddSection(section, true));

            comp.RenderCount.Should().Be(2);

            comp.Instance.Sections.Should().BeEquivalentTo(new[] { section });
            comp.Nodes.Should().ContainSingle();

            var navLinks = comp.FindComponents<VekNavLink>();
            navLinks.Should().BeEmpty();
        }
    }
}
