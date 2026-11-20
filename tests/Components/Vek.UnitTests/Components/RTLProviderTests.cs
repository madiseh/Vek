using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class RTLProviderTests : BunitTest
    {
        /// <summary>
        /// The provider element carries its class and style.
        /// </summary>
        [Test]
        public void RTLProvider_RendersClassAndStyle()
        {
            var comp = Context.Render<VekRTLProvider>(parameters => parameters
                .Add(x => x.Class, "my-class")
                .Add(x => x.Style, "color:red"));

            comp.MarkupMatches("""<div class="vek-rtl-provider my-class" style="color:red"></div>""");
        }

        /// <summary>
        /// Content inside the provider still receives the cascaded direction.
        /// </summary>
        [TestCase(true, "vek-typography-align-right")]
        [TestCase(false, "vek-typography-align-left")]
        public void RTLProvider_ShouldCascadeDirectionToItsContent(bool rightToLeft, string expectedClass)
        {
            var comp = Context.Render<VekRTLProvider>(parameters => parameters
                .Add(x => x.RightToLeft, rightToLeft)
                .AddChildContent<VekText>(text => text.Add(x => x.Align, Align.Start)));

            comp.Find("p.vek-typography").ClassList.Should().Contain(expectedClass);
        }
    }
}
