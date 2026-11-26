using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.Dialog;

namespace Vek.UnitTests.Components.Dialog
{
    [TestFixture]
    public class DialogAnimationTests : BunitTest
    {
        [Test]
        public void DialogAnimation_DefaultClass_IsZoomCssFallback()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            _ = comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>());

            var dialog = comp.Find(".vek-dialog");
            dialog.ClassList.Should().NotContain("vek-dialog-animation-fade");
            dialog.ClassList.Should().NotContain("vek-dialog-sheet-mobile");
        }

        [Test]
        [TestCase(DialogAnimation.Fade, "vek-dialog-animation-fade")]
        [TestCase(DialogAnimation.Zoom, "vek-dialog-animation-zoom")]
        [TestCase(DialogAnimation.SlideUp, "vek-dialog-animation-slideup")]
        [TestCase(DialogAnimation.SlideDown, "vek-dialog-animation-slidedown")]
        public void DialogAnimation_WithOptions_AppliesCorrectClass(DialogAnimation animation, string expectedClass)
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            var options = new DialogOptions { Animation = animation };
            _ = comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Title", options));

            var dialog = comp.Find(".vek-dialog");
            dialog.ClassList.Should().Contain(expectedClass);
        }

        [Test]
        public void DialogAnimation_WithNone_DoesNotApplyAnimationClass()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            var options = new DialogOptions { Animation = DialogAnimation.None };
            _ = comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Title", options));

            var dialog = comp.Find(".vek-dialog");
            dialog.ClassList.Should().NotContain("vek-dialog-animation-zoom");
            dialog.ClassList.Should().NotContain("vek-dialog-animation-fade");
        }

        [Test]
        public void BottomSheetOnMobile_WithOptions_AppliesSheetClasses()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            var options = new DialogOptions { BottomSheetOnMobile = true };
            _ = comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Sheet Dialog", options));

            var container = comp.Find(".vek-dialog-container");
            var dialog = comp.Find(".vek-dialog");

            container.ClassList.Should().Contain("vek-dialog-container-sheet-mobile");
            dialog.ClassList.Should().Contain("vek-dialog-sheet-mobile");
        }

        [Test]
        public void GlobalDialogOptions_SetsAnimationAndBottomSheetDefaults()
        {
            var comp = Context.Render<VekDialogProvider>(parameters => parameters
                .Add(p => p.Animation, DialogAnimation.SlideUp)
                .Add(p => p.BottomSheetOnMobile, true));

            var service = Context.Services.GetRequiredService<IDialogService>();
            _ = comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>());

            var container = comp.Find(".vek-dialog-container");
            var dialog = comp.Find(".vek-dialog");

            container.ClassList.Should().Contain("vek-dialog-container-sheet-mobile");
            dialog.ClassList.Should().Contain("vek-dialog-sheet-mobile");
            dialog.ClassList.Should().Contain("vek-dialog-animation-slideup");
        }

        [Test]
        public void InstanceOptions_OverrideGlobalDefaults()
        {
            var comp = Context.Render<VekDialogProvider>(parameters => parameters
                .Add(p => p.Animation, DialogAnimation.SlideDown)
                .Add(p => p.BottomSheetOnMobile, true));

            var service = Context.Services.GetRequiredService<IDialogService>();
            var options = new DialogOptions
            {
                Animation = DialogAnimation.Fade,
                BottomSheetOnMobile = false
            };
            _ = comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Custom Dialog", options));

            var container = comp.Find(".vek-dialog-container");
            var dialog = comp.Find(".vek-dialog");

            container.ClassList.Should().NotContain("vek-dialog-container-sheet-mobile");
            dialog.ClassList.Should().NotContain("vek-dialog-sheet-mobile");
            dialog.ClassList.Should().Contain("vek-dialog-animation-fade");
        }
    }
}
