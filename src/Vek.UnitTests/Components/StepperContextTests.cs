using NUnit.Framework;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    public class StepperContextTests
    {
        [Test]
        public void StepContext_NullStepper_Throws()
        {
            Assert.That(() => _ = new VekStepContext(null!, new VekStep()),
                Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("stepper"));
        }

        [Test]
        public void StepContext_NullStep_Throws()
        {
            Assert.That(() => _ = new VekStepContext(new VekStepper(), null!),
                Throws.TypeOf<ArgumentNullException>().With.Property("ParamName").EqualTo("step"));
        }
    }
}
