using AwesomeAssertions;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using NUnit.Framework;

namespace Vek.UnitTests.Services;

[TestFixture]
public class ScrollManagerTests
{
    private Mock<IJSRuntime> _runtimeMock;
    private ScrollManager _service;

    [SetUp]
    public void SetUp()
    {
        _runtimeMock = new Mock<IJSRuntime>(MockBehavior.Strict);
        _service = new ScrollManager(_runtimeMock.Object);
    }

    [Test]
    public async Task ScrollToAsync_CallsJsWithExpectedArguments()
    {
        SetupVoidInvocation("vekScrollManager.scrollTo", args =>
            args.Length == 4 &&
            (args[0] as string) == "#list" &&
            (int)args[1] == 10 &&
            (int)args[2] == 20 &&
            (args[3] as string) == "smooth");

        await _service.ScrollToAsync("#list", 10, 20, ScrollBehavior.Smooth);

        _runtimeMock.VerifyAll();
    }

    [Test]
    public async Task ScrollIntoViewAsync_CallsJsWithExpectedArguments()
    {
        SetupVoidInvocation("vekScrollManager.scrollIntoView", args =>
            args.Length == 2 &&
            (args[0] as string) == ".target" &&
            (args[1] as string) == "auto");

        await _service.ScrollIntoViewAsync(".target", ScrollBehavior.Auto);

        _runtimeMock.VerifyAll();
    }

    [Test]
    public async Task ScrollToTopAsync_CallsScrollToWithZeroOffsets()
    {
        SetupVoidInvocation("vekScrollManager.scrollTo", args =>
            args.Length == 4 &&
            args[0] == null &&
            (int)args[1] == 0 &&
            (int)args[2] == 0 &&
            (args[3] as string) == "auto");

        await _service.ScrollToTopAsync(null);

        _runtimeMock.VerifyAll();
    }

    [Test]
    public async Task ScrollToBottomAsync_CallsJsWithExpectedArguments()
    {
        SetupVoidInvocation("vekScrollManager.scrollToBottom", args =>
            args.Length == 2 &&
            (args[0] as string) == "messages" &&
            (args[1] as string) == "smooth");

        await _service.ScrollToBottomAsync("messages", ScrollBehavior.Smooth);

        _runtimeMock.VerifyAll();
    }

    [Test]
    public async Task ScrollToYearAsync_CallsJsWithExpectedArguments()
    {
        SetupVoidInvocation("vekScrollManager.scrollToYear", args =>
            args.Length == 1 &&
            (args[0] as string) == "year-2026");

        await _service.ScrollToYearAsync("year-2026");

        _runtimeMock.VerifyAll();
    }

    [Test]
    public async Task ScrollToListItemAsync_CallsJsWithExpectedArguments()
    {
        SetupVoidInvocation("vekScrollManager.scrollToListItem", args =>
            args.Length == 1 &&
            (args[0] as string) == "item-42");

        await _service.ScrollToListItemAsync("item-42");

        _runtimeMock.VerifyAll();
    }

    [Test]
    public async Task LockAndUnlockScroll_CallJsWithExpectedArguments()
    {
        SetupVoidInvocation("vekScrollManager.lockScroll", args =>
            args.Length == 2 &&
            (args[0] as string) == "#dialog" &&
            (args[1] as string) == "locked");
        SetupVoidInvocation("vekScrollManager.unlockScroll", args =>
            args.Length == 2 &&
            (args[0] as string) == "#dialog" &&
            (args[1] as string) == "locked");

        await _service.LockScrollAsync("#dialog", "locked");
        await _service.UnlockScrollAsync("#dialog", "locked");

        _runtimeMock.VerifyAll();
    }

    [Test]
    public async Task LockScrollAsync_SwallowsJsRuntimeError()
    {
        // Lock runs eagerly on overlay/dialog open, so a missing or disconnected script must degrade instead of tearing down the circuit.
        // This mirrors UnlockScrollAsync; the pair are symmetric.
        SetupThrowingInvocation("vekScrollManager.lockScroll", new JSDisconnectedException("disconnected"));

        var act = async () => await _service.LockScrollAsync("#dialog", "locked");

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task UnlockScrollAsync_SwallowsJsRuntimeError()
    {
        // Unlock runs during teardown when the circuit may already be gone, so it must not throw.
        SetupThrowingInvocation("vekScrollManager.unlockScroll", new JSDisconnectedException("disconnected"));

        var act = async () => await _service.UnlockScrollAsync("#dialog", "locked");

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task ScrollToVirtualizedItemAsync_CallsJsWithExpectedArguments()
    {
        SetupVoidInvocation("vekScrollManager.scrollToVirtualizedItem", args =>
            args.Length == 5 &&
            (args[0] as string) == "table" &&
            (int)args[1] == 7 &&
            (double)args[2] == 42.5 &&
            (args[3] as string) == "row-7" &&
            (args[4] as string) == "smooth");

        await _service.ScrollToVirtualizedItemAsync("table", 7, 42.5, "row-7", ScrollBehavior.Smooth);

        _runtimeMock.VerifyAll();
    }

    private void SetupVoidInvocation(string identifier, Func<object[], bool> argumentMatcher)
    {
        _runtimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.Is<object[]>(args => argumentMatcher(args))))
            .ReturnsAsync(Mock.Of<IJSVoidResult>())
            .Verifiable();
    }

    private void SetupThrowingInvocation(string identifier, Exception exception)
    {
        _runtimeMock
            .Setup(x => x.InvokeAsync<IJSVoidResult>(identifier, It.IsAny<object[]>()))
            .Throws(exception);
    }
}
