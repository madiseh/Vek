using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class SkeletonTests : BunitTest
    {
        [Test]
        public async Task SkeletonStyleIsAppliedAfterInitialRendering()
        {
            var comp = Context.Render<VekSkeleton>();

            var skeleton = comp.Instance;
            var span = comp.Find("span");

            var skeletonClasses = comp.Find(".vek-skeleton");
            // check initial state
            span.Attributes.GetNamedItem("style")?.Value.Should().BeNullOrEmpty();
            skeleton.Style.Should().BeNullOrEmpty();
            skeletonClasses.ClassList.Should().ContainInOrder(new[] { "vek-skeleton", "vek-skeleton-text", "vek-skeleton-pulse" });

            // add new style and check if it was applied
            await comp.SetParametersAndRenderAsync(p => p.Add(x => x.Style, "background:blue"));
            span = comp.Find("span");
            span.Attributes.GetNamedItem("style")?.Value.Should().Be("background:blue;");
        }

        [Test]
        public async Task SkeletonHeightIsAppliedAfterInitialRendering()
        {
            var comp = Context.Render<VekSkeleton>(p => p.Add(x => x.Height, "100px"));

            var skeleton = comp.Instance;
            var span = comp.Find("span");

            var skeletonClasses = comp.Find(".vek-skeleton");
            // check initial state
            span.Attributes.GetNamedItem("style")?.Value.Should().Be("height:100px;");
            skeleton.Height.Should().Be("100px");
            skeletonClasses.ClassList.Should().ContainInOrder(new[] { "vek-skeleton", "vek-skeleton-text", "vek-skeleton-pulse" });

            // add new style and check if it was applied
            await comp.SetParametersAndRenderAsync(p => p.Add(x => x.Height, "50px"));
            span = comp.Find("span");
            span.Attributes.GetNamedItem("style")?.Value.Should().Be("height:50px;");
        }

        [Test]
        public async Task SkeletonWidthIsAppliedAfterInitialRendering()
        {
            var comp = Context.Render<VekSkeleton>(p => p.Add(x => x.Width, "300px"));

            var skeleton = comp.Instance;
            var span = comp.Find("span");

            var skeletonClasses = comp.Find(".vek-skeleton");
            // check initial state
            span.Attributes.GetNamedItem("style")?.Value.Should().Be("width:300px;");
            skeleton.Width.Should().Be("300px");
            skeletonClasses.ClassList.Should().ContainInOrder(new[] { "vek-skeleton", "vek-skeleton-text", "vek-skeleton-pulse" });

            // add new style and check if it was applied
            await comp.SetParametersAndRenderAsync(p => p.Add(x => x.Width, "500px"));
            span = comp.Find("span");
            span.Attributes.GetNamedItem("style")?.Value.Should().Be("width:500px;");
        }

        /// <summary>
        /// Skeletons expose a loading name and busy state that user attributes can override.
        /// </summary>
        [Test]
        public void SkeletonShouldExposeAccessibleLoadingState()
        {
            var comp = Context.Render<VekSkeleton>();
            var skeleton = comp.Find("span.vek-skeleton");
            skeleton.GetAttribute("aria-busy").Should().Be("true");
            skeleton.GetAttribute("aria-label").Should().Be("Loading");

            var overridden = Context.Render<VekSkeleton>(parameters => parameters
                .AddUnmatched("aria-busy", "false")
                .AddUnmatched("aria-label", "Loading profile card"));
            var overriddenSkeleton = overridden.Find("span.vek-skeleton");
            overriddenSkeleton.GetAttribute("aria-busy").Should().Be("false");
            overriddenSkeleton.GetAttribute("aria-label").Should().Be("Loading profile card");
        }

        /// <summary>
        /// Card skeletons support the shimmer animation and a caller-defined corner radius.
        /// </summary>
        [Test]
        public void SkeletonCardShapeSupportsShimmerAndBorderRadius()
        {
            var comp = Context.Render<VekSkeleton>(parameters => parameters
                .Add(x => x.SkeletonType, SkeletonType.Card)
                .Add(x => x.Animation, Animation.Shimmer)
                .Add(x => x.BorderRadius, "12px"));

            var skeleton = comp.Find("span.vek-skeleton");
            skeleton.ClassList.Should().Contain("vek-skeleton-card");
            skeleton.ClassList.Should().Contain("vek-skeleton-shimmer");
            skeleton.GetAttribute("style").Should().Be("border-radius:12px;");
        }
    }
}
