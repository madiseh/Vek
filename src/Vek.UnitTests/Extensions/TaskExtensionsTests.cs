using System.Diagnostics;
using AwesomeAssertions;
using NUnit.Framework;

namespace Vek.UnitTests.Extensions
{

    [TestFixture]
    [NonParallelizable]
    public class TaskExtensionsTests
    {
        private Action<Exception> _originalExceptionHandler = null!;

        [SetUp]
        public void SetUp()
        {
            _originalExceptionHandler = VekGlobal.UnhandledExceptionHandler;
        }

        [TearDown]
        public void TearDown()
        {
            VekGlobal.UnhandledExceptionHandler = _originalExceptionHandler;
        }

        private async Task AsyncTaskExceptionGenerator(string errorMessage)
        {
            await Task.Delay(10);
            throw new Exception(errorMessage);
        }

        private async ValueTask AsyncValueTaskExceptionGenerator(string errorMessage)
        {
            await Task.Delay(10);
            throw new Exception(errorMessage);
        }

        private async ValueTask<TValue> AsyncValueTaskExceptionGenerator<TValue>(string errorMessage)
        {
            await Task.Delay(10);
            throw new Exception(errorMessage);
        }

        [Test]
        public async Task Task_AndForget_ShouldForwardExceptionToGlobalHandler()
        {
            string errorMessage = null;
            VekGlobal.UnhandledExceptionHandler = ex => errorMessage = ex.Message;
            var task = AsyncTaskExceptionGenerator("Something bad is about to happen ...");
            task.CatchAndLog();
            var t = Stopwatch.StartNew();
            while (errorMessage == null)
            {
                await Task.Delay(10);
                if (t.Elapsed > TimeSpan.FromSeconds(5))
                {
                    Assert.Fail("The exception wasn't forwarded to the global exception handler in time!");
                }
            }
            errorMessage.Should().Be("Something bad is about to happen ...");
        }

        [Test]
        public async Task ValueTask_AndForget_ShouldForwardExceptionToGlobalHandler()
        {
            string errorMessage = null;
            VekGlobal.UnhandledExceptionHandler = ex => errorMessage = ex.Message;
            var task = AsyncValueTaskExceptionGenerator("Something bad is about to happen ...");
            task.CatchAndLog();
            var t = Stopwatch.StartNew();
            while (errorMessage == null)
            {
                await Task.Delay(10);
                if (t.Elapsed > TimeSpan.FromSeconds(5))
                {
                    Assert.Fail("The exception wasn't forwarded to the global exception handler in time!");
                }
            }
            errorMessage.Should().Be("Something bad is about to happen ...");
        }

        [Test]
        public async Task ValueTask_T_AndForget_ShouldForwardExceptionToGlobalHandler()
        {
            string errorMessage = null;
            VekGlobal.UnhandledExceptionHandler = ex => errorMessage = ex.Message;
            var task = AsyncValueTaskExceptionGenerator<bool>("Something bad is about to happen ...");
            task.CatchAndLog();
            var t = Stopwatch.StartNew();
            while (errorMessage == null)
            {
                await Task.Delay(10);
                if (t.Elapsed > TimeSpan.FromSeconds(5))
                {
                    Assert.Fail("The exception wasn't forwarded to the global exception handler in time!");
                }
            }
            errorMessage.Should().Be("Something bad is about to happen ...");
        }

        [Test]
        public async Task Task_AndForget_ShouldNotFailIfGlobalHandlerIsNull()
        {
            VekGlobal.UnhandledExceptionHandler = null;
            var task = AsyncTaskExceptionGenerator("Something bad is about to happen ...");
            task.CatchAndLog();
            var t = Stopwatch.StartNew();
            while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted))
            {
                await Task.Delay(10);
                if (t.Elapsed > TimeSpan.FromSeconds(5))
                {
                    Assert.Fail("The test task did not end in time, this should not happen!");
                }
            }
        }

        [Test]
        public async Task Task_AndForget_ShouldNotForwardExceptionWhenIgnoreExceptions()
        {
            var handlerInvoked = false;
            VekGlobal.UnhandledExceptionHandler = _ => handlerInvoked = true;
            var task = AsyncTaskExceptionGenerator("Something bad is about to happen ...");
            task.CatchAndLog(ignoreExceptions: true);
            var t = Stopwatch.StartNew();
            while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted))
            {
                await Task.Delay(10);
                if (t.Elapsed > TimeSpan.FromSeconds(5))
                {
                    Assert.Fail("The test task did not end in time, this should not happen!");
                }
            }
            // Give the fire-and-forget continuation a chance to run before asserting the handler stayed untouched.
            await Task.Delay(100);
            handlerInvoked.Should().BeFalse();
        }
    }
}
