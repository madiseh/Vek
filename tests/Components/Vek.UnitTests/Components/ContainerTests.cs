using AwesomeAssertions;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class ContainerTests : BunitTest
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GuttersProperty_AddsClass(bool gutters)
        {
            // Arrange
            var component = Context.Render<VekContainer>(builder => builder
                .Add(p => p.Gutters, gutters)
            );

            // Assert
            if (gutters)
            {
                component.Markup.Should().Contain("vek-container--gutters");
            }
            else
            {
                component.Markup.Should().NotContain("vek-container--gutters");
            }
        }
    }
}
