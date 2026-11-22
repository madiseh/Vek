using AwesomeAssertions;
using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class DialogStackManagerTests
    {
        /// <summary>
        /// Activation moves a dialog to the top without changing the remaining relative order.
        /// </summary>
        [Test]
        public void Activate_MovesDialogToTop()
        {
            var stack = new DialogStackManager();
            var first = Guid.NewGuid();
            var second = Guid.NewGuid();
            var third = Guid.NewGuid();
            stack.Add(first);
            stack.Add(second);
            stack.Add(third);

            stack.Activate(first).Should().BeTrue();
            stack.GetIndex(second).Should().Be(0);
            stack.GetIndex(third).Should().Be(1);
            stack.GetIndex(first).Should().Be(2);
            stack.IsBackdropOwner(first).Should().BeTrue();

            stack.Remove(first);
            stack.IsBackdropOwner(third).Should().BeTrue();
            stack.GetIndex(second).Should().Be(0);
        }
    }
}
