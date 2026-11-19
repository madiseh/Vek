using AwesomeAssertions;
using Bunit;
using Vek.UnitTests.TestComponents.AvatarGroup;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class AvatarGroupTests : BunitTest
    {
        [Test]
        public void AvatarGroup()
        {
            var comp = Context.Render<AvatarGroupTest>();
            // select elements needed for the test
            var group = comp.FindComponent<VekAvatarGroup>();
            var avatars = comp.FindAll(".vek-avatar").ToArray();

            // check group
            group.Instance.Max.Should().Be(4);
            group.Instance.Spacing.Should().Be(2);
            group.Instance._avatars.Should().HaveCount(6);

            // check avatars
            avatars.Should().HaveCount(5);  // 4 avatars since max is 4 + 1 that is max avatar
            avatars[0].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[1].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[2].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[3].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[4].ClassList.Should().Contain("vek-avatar-group-max-avatar");
        }

        [Test]
        public void AvatarGroupMaxDefault()
        {
            var comp = Context.Render<AvatarGroupMaxDefaultTest>();
            // select elements needed for the test
            var group = comp.FindComponent<VekAvatarGroup>();
            var avatars = group.FindAll(".vek-avatar").ToArray();

            // check initial group settings
            group.Instance.Max.Should().Be(int.MaxValue); // should default to 2147483647 when not provided
            group.Instance._avatars.Should().HaveCount(7);

            // verify MaxGroupReached is false for each avatar
            foreach (var avatar in group.Instance._avatars)
            {
                group.Instance.MaxGroupReached(avatar).Should().Be(false);
            }

            // check initial avatars
            avatars.Should().HaveCount(7);  // all 7 avatars should display
            avatars[0].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[1].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[2].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[3].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[4].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[5].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[6].ClassList.Should().NotContain("vek-avatar-group-max-avatar");

        }

        [Test]
        public async Task AvatarGroupChangeMax()
        {
            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<AvatarGroupChangeMaxTest>();
            // select elements needed for the test
            var group = comp.FindComponent<VekAvatarGroup>();
            var avatars = comp.FindAll(".vek-avatar").ToArray();

            // check initial group settings
            group.Instance.Max.Should().Be(3);
            group.Instance.Spacing.Should().Be(3);
            group.Instance._avatars.Should().HaveCount(7);

            // check initial avatars
            avatars.Should().HaveCount(4);  // 4 avatars since max is 3 + 1 that is max avatar
            avatars[0].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[1].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[2].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[3].ClassList.Should().Contain("vek-avatar-group-max-avatar");

            // click button to change max avatars to 4
            await comp.Find("#change-max-button").ClickAsync();

            // find all avatars again after click
            avatars = comp.FindAll(".vek-avatar").ToArray();

            // check group max is changed and avatars added/removed
            group.Instance.Max.Should().Be(4);
            avatars.Should().HaveCount(5);
            avatars[0].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[1].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[2].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[3].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[4].ClassList.Should().Contain("vek-avatar-group-max-avatar");

            // click button to change max avatars to 7
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await comp.Find("#change-max-button").ClickAsync();
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await comp.Find("#change-max-button").ClickAsync();
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await comp.Find("#change-max-button").ClickAsync();

            // find all avatars again after click
            avatars = comp.FindAll(".vek-avatar").ToArray();

            // check group max is changed and avatars added/removed
            group.Instance.Max.Should().Be(7);
            avatars.Should().HaveCount(7);
            avatars[0].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[1].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[2].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[3].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[4].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[5].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[6].ClassList.Should().NotContain("vek-avatar-group-max-avatar");

            // click button one more time to set it to 0
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await comp.Find("#change-max-button").ClickAsync();

            // find all avatars again after click
            avatars = comp.FindAll(".vek-avatar").ToArray();

            // check group max is changed and avatars added/removed
            group.Instance.Max.Should().Be(0);
            avatars.Should().HaveCount(1);
            avatars[0].ClassList.Should().Contain("vek-avatar-group-max-avatar");

            // click button one more time to set it to 1
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await comp.Find("#change-max-button").ClickAsync();

            // find all avatars again after click
            avatars = comp.FindAll(".vek-avatar").ToArray();

            // check group max is changed and avatars added/removed
            group.Instance.Max.Should().Be(1);
            avatars.Should().HaveCount(2);
            avatars[0].ClassList.Should().NotContain("vek-avatar-group-max-avatar");
            avatars[1].ClassList.Should().Contain("vek-avatar-group-max-avatar");
        }

        [Test]
        public async Task AvatarGroupRemove()
        {
            var comp = Context.Render<AvatarGroupRemoveTest>();

            await comp.Find("#empty-button").ClickAsync();

            comp.FindComponent<VekAvatarGroup>().Instance._avatars.Count.Should().Be(0);
        }

        [Test]
        public void AvatarGroupMaxAvatarsTemplate()
        {
            var comp = Context.Render<AvatarGroupMaxAvatarsTemplateTest>();

            comp.FindComponent<VekButton>().Should().NotBeNull();

            comp.FindComponent<VekAvatarGroup>().FindAll(".vek-avatar-group-max-avatar").Should().HaveCount(0);
        }
    }
}
