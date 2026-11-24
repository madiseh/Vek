using System.Web;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using NUnit.Framework;
using Vek.UnitTests.TestComponents.Dialog;

namespace Vek.UnitTests.Components
{
    [TestFixture]
    // Shares DialogRender's static OnInitializedCount and exercises async inline-dialog timing that
    // deadlocks/flakes under NUnit parallel execution. Kept serial by design (accepted limitation); see #13188 / #13297.
    [NonParallelizable]
    public class DialogTests : BunitTest
    {
        /// <summary>
        /// Testing lifecycle of dialogs in DialogProvider
        /// </summary>
        [Test]
        public async Task Lifecycle()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;
            await comp.InvokeAsync(async () =>
            {
                dialogReference = await service.ShowAsync<DialogRender>();
                var result1 = await dialogReference.Result;
                //The second Dialog is added here, but the first dialog is still in the _dialogs collection of the DialogProvider, as only the result task was set to completion.
                //So DialogProvider will render again with 2 dialogs, but 1 is completed. This one needs to be excluded from rendering to prevent double initialize with no params.
                dialogReference = await service.ShowAsync<DialogRender>();
                var result2 = await dialogReference.Result;
            });
            DialogRender.OnInitializedCount.Should().Be(2);
            //Reset global value
            DialogRender.OnInitializedCount = 0;
        }

        /// <summary>
        /// Opening and closing a simple dialog
        /// </summary>
        [Test]
        public async Task Simple()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;
            // open simple test dialog
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>());
            dialogReference.Should().NotBe(null);
            comp.Find("div.vek-dialog-container").Should().NotBe(null);
            comp.Find("p.vek-typography").TrimmedText().Should().Be("Wabalabadubdub!");
            // close by click outside the dialog
            comp.Find("div.vek-overlay").Click();
            comp.Markup.Trim().Should().BeEmpty();
            var result = await dialogReference.Result;
            result.Canceled.Should().BeTrue();
            // open simple test dialog
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>());
            // close by click on cancel button
            comp.FindAll("button")[0].Click();
            result = await dialogReference.Result;
            result.Canceled.Should().BeTrue();
            // open simple test dialog
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>());
            // close by click on ok button
            comp.FindAll("button")[1].Click();
            result = await dialogReference.Result;
            result.Canceled.Should().BeFalse();

            //create 2 instances and dismiss all
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>());
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>());
            var cont = comp.FindAll("div.vek-dialog-container");
            cont.Count.Should().Be(2);
            await comp.InvokeAsync(() => comp.Instance.DismissAll());
            cont = comp.FindAll("div.vek-dialog-container");
            cont.Count.Should().Be(0);

            // Close by using default close method
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>());
            comp.FindAll("button")[2].Click();
            result = await dialogReference.Result;
            result.Data.Should().BeNull();
            result.DataType.Should().BeNull();
            result.Canceled.Should().BeFalse();
        }

        /// <summary>
        /// The title-less ShowAsync overloads forward options and parameters like their titled counterparts.
        /// </summary>
        [Test]
        public async Task ShowAsync_TitlelessOverloads_ForwardOptionsAndParameters()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;

            var options = new DialogOptions { FullWidth = true };

            // ShowAsync<T>(DialogOptions) applies the options with no title.
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>(options));
            dialogReference.Should().NotBe(null);
            comp.Find("div.vek-dialog-container").Should().NotBe(null);
            comp.FindAll(".vek-dialog-width-full").Should().NotBeEmpty();
            await comp.InvokeAsync(() => comp.Instance.DismissAll());

            // ShowAsync<T>(DialogParameters, DialogOptions) applies both with no title.
            var parameters = new DialogParameters<DialogWithParameters> { { x => x.TestValue, "test" } };
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithParameters>(parameters, options));
            dialogReference.Should().NotBe(null);
            comp.FindComponent<VekInput<string>>().Instance.ReadText.Should().Be("test");
            comp.FindAll(".vek-dialog-width-full").Should().NotBeEmpty();
        }

        /// <summary>
        /// Opening and closing dialogs via navigation.
        /// </summary>
        [Test]
        public async Task CloseOnNavigationTest()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var dialogService = Context.Services.GetRequiredService<IDialogService>();
            dialogService.Should().NotBe(null);
            var navigationManager = Context.Services.GetRequiredService<NavigationManager>();
            navigationManager.Should().NotBe(null);

            //create 2 instances and dismiss all except for one with CloseOnNavigation = false
            var closeOnNavigationOptions = new DialogOptions
            {
                CloseOnNavigation = false
            };
            await comp.InvokeAsync(async () => _ = await dialogService.ShowAsync<DialogOkCancel>("test", options: closeOnNavigationOptions));
            await comp.InvokeAsync(async () => _ = await dialogService.ShowAsync<DialogOkCancel>());
            var cont = comp.FindAll("div.vek-dialog-container");
            cont.Count.Should().Be(2);

            var uri = new Uri(navigationManager.Uri);
            var query = HttpUtility.ParseQueryString(uri.Query);
            query["query"] = Guid.NewGuid().ToString();
            var newUri = $"{uri.GetLeftPart(UriPartial.Path)}?{query}";
            await comp.InvokeAsync(() => navigationManager.NavigateTo(newUri));

            cont = comp.FindAll("div.vek-dialog-container");
            cont.Count.Should().Be(1);
        }

        /// <summary>
        /// <para>Opening and closing an inline dialog. Click on open will open the inlined dialog.</para>
        /// <para>
        /// Note: this test uses two different components, one containing the dialog provider and
        /// one containing the open button and the inline dialog
        /// </para>
        /// </summary>
        [Test]
        public async Task InlineDialog()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            // displaying the component with the inline dialog only renders the open button
            var comp1 = Context.Render<TestInlineDialog>();
            comp1.FindComponents<VekButton>().Count.Should().Be(1);
            // open the dialog
            comp1.Find("button").Click();
            await comp1.WaitForAssertionAsync(() =>
                comp.Find("div.vek-dialog-container").Should().NotBe(null)
            );
            comp.Find("p.vek-typography").TrimmedText().Should().Be("Wabalabadubdub!");
            comp.Find("div.vek-dialog").GetAttribute("class").Should().Contain("vek-dialog-width-full");
            // close by click on ok button
            comp.Find("button").Click();
            comp.Markup.Trim().Should().BeEmpty();
        }

        /// <summary>
        /// https://github.com/madiseh/vek/issues/4098
        /// https://github.com/madiseh/vek/issues/8746
        /// </summary>
        [Test]
        public async Task InlineDialogShowMethod()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);

            var comp1 = Context.Render<InlineDialogShowMethod>();

            comp.Markup.Should().NotContain("Here be dragons");

            // open the dialog
            comp1.Find(".open-dialog-button").Click();
            await comp1.WaitForAssertionAsync(() => comp.Find("div.vek-dialog-container").Should().NotBe(null));

            comp.Markup.Should().Contain("Here be dragons");

            // close the dialog
            comp.Find(".close-dialog-button").Click();

            // messagebox should have opened
            comp.Markup.Should().Contain("dialog was successfully closed");

            // close by click on ok button
            comp.Find(".vek-message-box button").Click();

            await comp.WaitForAssertionAsync(() => comp.Markup.Trim().Should().BeEmpty(), timeout: TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Check where a dialog is opened, canceled and reopened.
        /// </summary>
        /// <remarks>
        /// https://github.com/madiseh/vek/issues/11789
        /// </remarks>
        [Test]
        public async Task InlineDialog_OpenCancelOpen()
        {
            // Arrange

            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<VekDialogProvider>();
            var sup = Context.Render<InlineDialogShowMethodTest>();

            // Assert : Initial state, dialog should be closed

            comp.FindAll(".vek-dialog-content").Should().BeEmpty();

            // Act : Open the dialog

            await sup.Find(".open-dialog-button").ClickAsync();

            // Assert : Dialog should be open

            comp.Find(".vek-dialog-content").InnerHtml.Trim().Should().NotBeEmpty();

            // Act : Cancel by click outside

            await comp.Find("div.vek-overlay-dialog").ClickAsync();

            // Assert : Dialog should be closed

            comp.FindAll(".vek-dialog-content").Should().BeEmpty();

            // Act : Reopen the dialog

            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await sup.Find(".open-dialog-button").ClickAsync();

            // Assert : Dialog should be open

            comp.Find(".vek-dialog-content").InnerHtml.Trim().Should().NotBeEmpty();
        }

        /// <summary>
        /// Nested dialogs should not appear unless manually shown
        /// </summary>
        [Test]
        public async Task NestedInlineDialog()
        {
            var provider = Context.Render<VekDialogProvider>();
            provider.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            // displaying the component with the inline dialog only renders the open button
            var comp = Context.Render<TestNestedInlineDialog>();
            comp.FindComponents<VekButton>().Count.Should().Be(1);
            // open the dialog
            comp.Find("button").Click();
            await comp.WaitForAssertionAsync(() =>
                provider.Find("div.vek-dialog-container").Should().NotBe(null)
            );
            provider.Find("p.vek-typography").TrimmedText().Should().Be("Scorpiany!");
            provider.FindComponents<VekText>().Count.Should().Be(2); //counts both the dialog header and the text in our test component

            provider.Find("button").Click(); //open nested dialog
            await comp.WaitForAssertionAsync(() =>
                provider.Find(".nested").Should().NotBe(null)
            );

            provider.FindAll("p.vek-typography")[1].TrimmedText().Should().Be("Nested dialog!");
            provider.FindComponents<VekText>().Count.Should().Be(4); //now we have another VekText
        }

        /// <summary>
        /// Click outside the dialog (or any other method) must update the Visible parameter two-way binding on close
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task InlineDialog_Should_UpdateIsVisibleOnClose()
        {
            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var comp1 = Context.Render<InlineDialogIsVisibleStateTest>();
            await comp1.InvokeAsync(() => comp1.Find("button").Click());
            await comp.WaitForAssertionAsync(() => comp.FindAll("div.vek-dialog-container").Count.Should().Be(1));
            await comp.InvokeAsync(() => comp.WaitForElement("div.vek-overlay").Click());
            await comp.WaitForAssertionAsync(() => comp.Markup.Trim().Should().BeEmpty(), TimeSpan.FromSeconds(5));
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            await comp1.InvokeAsync(() => comp1.Find("button").Click());
            await comp.WaitForAssertionAsync(() => comp.FindAll("div.vek-dialog-container").Count.Should().Be(1),
                TimeSpan.FromSeconds(5));
            await comp.InvokeAsync(() => comp.WaitForElement("div.vek-overlay").Click());
            await comp.WaitForAssertionAsync(() => comp.Markup.Trim().Should().BeEmpty(), TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Closing a bound inline dialog outside an event handler re-renders the parent synchronously and must not throw or lose the result.
        /// </summary>
        [Test]
        public async Task InlineDialog_CloseAsync_OutsideEventHandler()
        {
            var provider = Context.Render<VekDialogProvider>();
            var comp = Context.Render<InlineDialogIsVisibleStateTest>();
            await comp.Find("button").ClickAsync();
            await provider.WaitForAssertionAsync(() => provider.FindAll("div.vek-dialog-container").Count.Should().Be(1));
            var dialog = comp.FindComponent<VekDialog>();
            var reference = await comp.InvokeAsync(() => dialog.Instance.ShowAsync());

            await comp.InvokeAsync(() => dialog.Instance.CloseAsync(DialogResult.Ok(true)));

            var result = await reference.Result;
            result.Canceled.Should().BeFalse();
            await provider.WaitForAssertionAsync(() => provider.Markup.Trim().Should().BeEmpty());
        }

        /// <summary>
        /// Based on bug report #3701 #3687
        /// Dialog inline should not be closed after any event inside
        /// </summary>
        [Test]
        public async Task InlineDialogShouldNotCloseAfterStateHasChanged()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            // displaying the component with the inline dialog only renders the open button
            var comp1 = Context.Render<TestInlineDialog>();
            // open the dialog
            comp1.Find("button").Click();
            // rate star
            comp.Find("span.vek-rating-item").FirstElementChild.Click();
            // check if is still opened
            await comp.WaitForAssertionAsync(() => comp.Find("div.vek-dialog-container").Should().NotBe(null), TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// An inline dialog closed and reopened in one dispatcher turn stays open (#13917).
        /// </summary>
        [Test]
        public async Task InlineDialogShouldStayOpenWhenReopenedImmediately()
        {
            var provider = Context.Render<VekDialogProvider>();
            var host = Context.Render<InlineDialogReopenTest>();
            await host.Find("button").ClickAsync();
            await provider.WaitForAssertionAsync(() => provider.FindAll("div.vek-dialog-container").Should().ContainSingle());

            await host.InvokeAsync(host.Instance.CloseAndReopen);

            // The first dialog's close callback hops through the thread pool.
            // Without the wait for the reopened content, the assertions below can run before it lands and miss the bug.
            await provider.InvokeAsync(() => { });
            await provider.WaitForAssertionAsync(() => provider.Find(".dialog-item").TextContent.Should().Be("Item 2"));
            provider.FindAll("div.vek-dialog-container").Should().ContainSingle();
            host.Instance.Visible.Should().BeTrue();
        }

        /// <summary>
        /// Based on bug report by Porkopek:
        /// Updating values that are referenced in TitleContent render fragment won't result in an update of the dialog title
        /// when they change. This is solved by allowing the user to call ForceRender() on DialogInstance
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task DialogShouldUpdateTitleContent()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;
            // open simple test dialog
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogThatUpdatesItsTitle>());
            dialogReference.Should().NotBe(null);
            //comp.Find("div.vek-dialog-container").Should().NotBe(null);
            comp.Find("div.vek-dialog-content").TrimmedText().Should().Be("Body:");
            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title:");
            // click on ok button should set title and content
            comp.FindAll("button")[1].Click();
            comp.Find("div.vek-dialog-content").TrimmedText().Should().Be("Body: Test123");
            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title: Test123");
        }

        /// <summary>
        /// A test that ensures parameters are not overwritten when dialog is updated
        /// </summary>
        [Test]
        public async Task DialogShouldNotOverwriteParameters()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;

            var parameters = new DialogParameters<DialogWithParameters>
            {
                { x => x.TestValue, "test" },
                { x => x.ColorTest, Color.Error } // !! comment me !!
            };

            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithParameters>(string.Empty, parameters));
            dialogReference.Should().NotBe(null);

            var textField = comp.FindComponent<VekInput<string>>().Instance;
            textField.ReadText.Should().Be("test");

            comp.Find("input").Change("new_test");
            comp.Find("input").Blur();
            textField.ReadText.Should().Be("new_test");

            comp.FindAll("button")[0].Click();

            ((DialogWithParameters)dialogReference.Dialog).TestValue.Should().Be("new_test");
            ((DialogWithParameters)dialogReference.Dialog).ParametersSetCounter.Should().Be(1);
            textField.ReadText.Should().Be("new_test");
        }

        [Test]
        public async Task PassingEventCallbackToDialogViaParameters()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);

            var testComp = Context.Render<DialogWithEventCallbackTest>();
            // open dialog
            testComp.Find("button").Click();
            // in the opened dialog find the text field
            var tf = comp.FindComponent<VekTextField<string>>();
            tf.Find("input").Input("User input ...");
            // the user input should be passed out of the dialog into the outer component and displayed there.
            await testComp.WaitForAssertionAsync(() =>
                testComp.Find("p").TextContent.Trim().Should().Be("Search Text:  User input ...")
            );
        }

        [Test]
        public async Task CustomDialogService()
        {
            //Remove default IDialogService so we can provide our custom implementation
            //This is not necessary in normal cases, you would rather just not register all services in the beginning, but the test environment requires here to do so.
            Context.Services.Remove(Context.Services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IDialogService)));
            //Register our custom dialog service implementation as the new service instance behind IDialogService
            Context.Services.AddScoped<IDialogService>(sp => new CustomDialogService());

            //Render our dialog provider and make sure everything is fine
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();

            //Try to get the current service instance for the type IDialogService and make sure it is our custom implementation
            var service = Context.Services.GetService<IDialogService>();
            service.Should().NotBe(null);
            service.Should().BeAssignableTo(typeof(CustomDialogService));

            //Show the dialog, create reference and make sure it is our custom dialog reference implementation
            IDialogReference dialogReference = null;
            //The type of the dialog does not really matter, for the sake of laziness I will re-use this existing one
            await comp.InvokeAsync(async () => dialogReference = await service?.ShowAsync<DialogThatUpdatesItsTitle>());
            dialogReference.Should().NotBe(null);
            dialogReference.Should().BeAssignableTo(typeof(CustomDialogReference));

            //After above checks have passed, we can safely cast the generic reference into our specific implementation
            var customDialogReference = (CustomDialogReference)dialogReference;
            //The custom property should be false by default otherwise the rest of the test logic would be incorrect
            customDialogReference.AllowDismiss.Should().BeFalse();

            //Dialog should not be closable through backdrop click
            comp.Find("div.vek-overlay").Click();
            await comp.WaitForAssertionAsync(() => comp.Markup.Trim().Should().NotBeEmpty(), TimeSpan.FromSeconds(5));

            //Allow dismiss
            customDialogReference.AllowDismiss = true;

            //Dialog should now be closable through backdrop click
            comp.Find("div.vek-overlay").Click();
            await comp.WaitForAssertionAsync(() => comp.Markup.Trim().Should().BeEmpty(), TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Getting return value from dialog
        /// </summary>
        [Test]
        public async Task DialogShouldReturnTheReturnValue()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;
            // open dialog
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithReturnValue>());
            dialogReference.Should().NotBe(null);
            // close by click on cancel button
            comp.FindAll("button")[0].Click();
            var rv = await dialogReference.GetReturnValueAsync<string>();
            rv.Should().BeNull();
            // open dialog
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithReturnValue>());
            // close by click on ok button
            comp.FindAll("button")[1].Click();
            rv = await dialogReference.GetReturnValueAsync<string>();
            rv.Should().Be("Closed via OK");
        }

        [Test]
        public async Task DialogKeyboardNavigation()
        {
            var keyInterceptorService = Context.AddKeyInterceptorService();
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;
            //dialog with clickable backdrop
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>(string.Empty, new DialogOptions() { CloseOnEscapeKey = true }));
            dialogReference.Should().NotBe(null);
            var dialog1 = (DialogOkCancel)dialogReference.Dialog!;
            var dialogInstance1 = dialog1.VekDialog.GetDialogContainer();
            comp.Markup.Trim().Should().NotBeEmpty();

            await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(dialogInstance1.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown", }));
            comp.Markup.Trim().Should().BeEmpty();
            //dialog with disabled backdrop click
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>(string.Empty, new DialogOptions() { CloseOnEscapeKey = false }));
            dialogReference.Should().NotBe(null);
            var dialog2 = (DialogOkCancel)dialogReference.Dialog!;
            var dialogInstance2 = dialog2.VekDialog.GetDialogContainer();
            comp.Markup.Trim().Should().NotBeEmpty();
            await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(dialogInstance2.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown", }));
            comp.Markup.Trim().Should().NotBeEmpty();
        }

        [Test]
        public async Task DialogKeyboardEvents()
        {
            var keyInterceptorService = Context.AddKeyInterceptorService();
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;
            //dialog with clickable backdrop
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>(string.Empty, new DialogOptions() { CloseOnEscapeKey = true }));
            dialogReference.Should().NotBe(null);
            var dialog1 = ((DialogOkCancel)dialogReference.Dialog)!;
            var dialogInstance = dialog1.VekDialog.GetDialogContainer();
            dialog1.LastKeyDown.Should().Be(null);
            dialog1.LastKeyUp.Should().Be(null);
            comp.Markup.Trim().Should().NotBeEmpty();

            await comp.InvokeAsync(async () => await keyInterceptorService.OnKeyDown(dialogInstance.ElementId, new KeyboardEventArgs { Key = "Enter", Type = "keydown", }));
            dialog1.LastKeyDown.Key.Should().Be("Enter");
            await comp.InvokeAsync(async () => await keyInterceptorService.OnKeyUp(dialogInstance.ElementId, new KeyboardEventArgs { Key = "Backspace", Type = "keyup", }));
            dialog1.LastKeyUp.Key.Should().Be("Backspace");
            comp.Markup.Trim().Should().NotBeEmpty();
        }

        [Test]
        public async Task DialogHandlesOnBackdropClickEvent()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();

            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;

            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithOnBackdropClickEvent>());
            dialogReference.Should().NotBe(null);
            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title:");

            //Click on backdrop
            comp.Find("div.vek-overlay").Click();

            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title: Backdrop clicked");
        }

        /// <summary>
        /// Open Inline Dialog and the from the inline dialog another normal dialog
        /// while closing the inline dialog.
        /// https://github.com/madiseh/vek/issues/4871
        /// </summary>
        [Test]
        public async Task InlineDialogBug4871()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            // displaying the component with the inline dialog only renders the open button
            var comp1 = Context.Render<TestInlineDialog>();
            comp1.FindComponents<VekButton>().Count.Should().Be(1);
            // open the dialog
            comp1.Find("button").Click();
            await comp1.WaitForAssertionAsync(() =>
                comp.Find("div.vek-dialog-container").Should().NotBe(null)
            );
            comp.Find("p.vek-typography").TrimmedText().Should().Be("Wabalabadubdub!");
            comp.Find("div.vek-dialog").GetAttribute("class").Should().Contain("vek-dialog-width-full");
            // close by click on ok button
            comp.FindAll("button").Last().Click();
            await comp.WaitForAssertionAsync(() => comp.FindComponent<VekMessageBox>());
            var messageBox = comp.FindComponent<VekMessageBox>();
            messageBox.Should().NotBeNull();
            messageBox.Instance.YesText.Should().Be("BUG4871");
        }

        [Test]
        public async Task DialogToggleFullscreenOptions()
        {
            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();

            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            IDialogReference dialogReference = null;

            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogToggleFullscreen>());
            dialogReference.Should().NotBe(null);

            comp.Find("div.vek-dialog").GetAttribute("class").Should().Be("vek-dialog vek-dialog-width-sm");
            comp.Find("button").Click();
            comp.Find("div.vek-dialog").GetAttribute("class").Should().Be("vek-dialog vek-dialog-fullscreen");
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            comp.Find("button").Click();
            comp.Find("div.vek-dialog").GetAttribute("class").Should().Be("vek-dialog vek-dialog-width-sm");
        }

        ///-------------------------------
        /// <summary>
        /// Testing lifecycle of dialogs in dialogprovider
        /// </summary>
        [Test]
        public async Task AsyncLifecycle()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            await comp.InvokeAsync(async () =>
            {
                var dialogReference = await service.ShowAsync<DialogRender>();
                await dialogReference.Result;
                //The second Dialog is added here, but the first dialog is still in the _dialogs collection of the dialogprovider, as only the result task was set to completion.
                //So DialogProvider will render again with 2 dialogs, but 1 is completed. This one needs to be excluded from rendering to prevent double initialize with no params.
                dialogReference = await service.ShowAsync<DialogRender>();
                await dialogReference.Result;
            });
            DialogRender.OnInitializedCount.Should().Be(2);
            //Reset global value
            DialogRender.OnInitializedCount = 0;
        }

        /// <summary>
        /// Reproduce bug from the GitHub issue #10180 :
        /// https://github.com/madiseh/vek/issues/10180
        /// </summary>
        [Test]
        public void OpenAndCloseImmediately()
        {
            // Arrange

            var comp = Context.Render<ComponentThatOpensAndClosesDialog>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBeNull();

            // Act : Open and close immediately

            comp.Render();
        }

        /// <summary>
        /// Opening and closing a simple dialog
        /// </summary>
        [Test]
        public async Task AsyncSimple()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service?.ShowAsync<DialogOkCancel>());
            //open simple test dialog
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);
            comp.Find("div.vek-dialog-container").Should().NotBe(null);
            comp.Find("p.vek-typography").TrimmedText().Should().Be("Wabalabadubdub!");
            //close by click outside the dialog
            comp.Find("div.vek-overlay").Click();
            comp.Markup.Trim().Should().BeEmpty();
            var result = await dialogReference.Result;
            result.Canceled.Should().BeTrue();
            //open simple test dialog
            dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogOkCancel>());
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            dialogReference = await dialogReferenceLazy.Value;
            //close by click on cancel button
            comp.FindAll("button")[0].Click();
            result = await dialogReference.Result;
            result.Canceled.Should().BeTrue();
            //open simple test dialog
            dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogOkCancel>());
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            dialogReference = await dialogReferenceLazy.Value;
            //close by click on ok button
            comp.FindAll("button")[1].Click();
            result = result = await dialogReference.Result;
            result.Canceled.Should().BeFalse();

            //create 2 instances and dismiss all
            dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogOkCancel>());
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogOkCancel>());
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var cont = comp.FindAll("div.vek-dialog-container");
            cont.Count.Should().Be(2);
            await comp.InvokeAsync(() => comp.Instance.DismissAll());
            cont = comp.FindAll("div.vek-dialog-container");
            cont.Count.Should().Be(0);
        }

        /// <summary>
        /// Based on bug report #3128
        /// Dialog Class parameters should be honored for inline dialog
        /// </summary>
        [Test]
        public async Task InlineAsyncDialogShouldHonorClass()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(service.ShowAsync<TestInlineDialog>);
            // open simple test dialog
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            await comp.WaitForAssertionAsync(() => dialogReference.Should().NotBe(null));
            comp.Find("button").Click();
            await comp.WaitForAssertionAsync(() => comp.Find("div.vek-dialog").ClassList.Should().Contain("test-class"));
            comp.Find("div.vek-dialog-content").ClassList.Should().NotContain("test-class");
            comp.Find("div.vek-dialog-content").ClassList.Should().Contain("content-class");
            // check if tag is ok
            var dialogInstance = comp.FindComponent<VekDialog>().Instance;
            dialogInstance.Tag.Should().Be("test-tag");
        }

        /// <summary>
        /// Based on bug report by Porkopek:
        /// Updating values that are referenced in TitleContent render fragment won't result in an update of the dialog title
        /// when they change. This is solved by allowing the user to call ForceRender() on DialogInstance
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AsyncDialogShouldUpdateTitleContent()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(service.ShowAsync<DialogThatUpdatesItsTitle>);
            // open simple test dialog
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);
            //comp.Find("div.vek-dialog-container").Should().NotBe(null);
            comp.Find("div.vek-dialog-content").TrimmedText().Should().Be("Body:");
            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title:");
            // click on ok button should set title and content
            comp.FindAll("button")[1].Click();
            comp.Find("div.vek-dialog-content").TrimmedText().Should().Be("Body: Test123");
            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title: Test123");
        }

        /// <summary>
        /// A test that ensures parameters are not overwritten when dialog is updated
        /// </summary>
        [Test]
        public async Task AsyncDialogShouldNotOverwriteParameters()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);

            var parameters = new DialogParameters<DialogWithParameters>
            {
                { x => x.TestValue, "test" },
                { x => x.ColorTest, Color.Error } // !! comment me !!
            };

            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogWithParameters>(string.Empty, parameters));
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);

            var textField = comp.FindComponent<VekInput<string>>().Instance;
            textField.ReadText.Should().Be("test");

            comp.Find("input").Change("new_test");
            comp.Find("input").Blur();
            textField.ReadText.Should().Be("new_test");

            comp.FindAll("button")[0].Click();

            ((DialogWithParameters)dialogReference.Dialog).TestValue.Should().Be("new_test");
            ((DialogWithParameters)dialogReference.Dialog).ParametersSetCounter.Should().Be(1);
            textField.ReadText.Should().Be("new_test");
        }

        /// <summary>
        /// Based on bug report #1385
        /// Dialog Class parameters should be honored
        /// </summary>
        [Test]
        public async Task AsyncDialogShouldHonorClass()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(service.ShowAsync<DialogOkCancel>);
            // open simple test dialog
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);
            comp.Find("div.vek-dialog").ClassList.Should().Contain("test-class");
            comp.Find("div.vek-dialog-content").ClassList.Should().NotContain("test-class");
            comp.Find("div.vek-dialog-content").ClassList.Should().Contain("content-class");
        }

        [Test]
        public async Task AsyncCustomDialogService()
        {
            //Remove default IDialogService so we can provide our custom implementation
            //This is not necessary in normal cases, you would rather just not register all services in the beginning, but the test environment requires here to do so.
            Context.Services.Remove(Context.Services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IDialogService)));
            //Register our custom dialog service implementation as the new service instance behind IDialogService
            Context.Services.AddScoped<IDialogService>(sp => new CustomDialogService());

            //Render our dialog provider and make sure everything is fine
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();

            //Try to get the current service instance for the type IDialogService and make sure it is our custom implementation
            var service = Context.Services.GetService<IDialogService>();
            service.Should().NotBe(null);
            service.Should().BeAssignableTo(typeof(CustomDialogService));

            //Show the dialog, create reference and make sure it is our custom dialog reference implementation
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service?.ShowAsync<DialogThatUpdatesItsTitle>());
            //The type of the dialog does not really matter, for the sake of laziness I will re-use this existing one
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);
            dialogReference.Should().BeAssignableTo(typeof(CustomDialogReference));

            //After above checks have passed, we can safely cast the generic reference into our specific implementation
            var customDialogReference = (CustomDialogReference)dialogReference;
            //The custom property should be false by default otherwise the rest of the test logic would be incorrect
            customDialogReference.AllowDismiss.Should().BeFalse();

            //Dialog should not be closable through backdrop click
            comp.Find("div.vek-overlay").Click();
            await comp.WaitForAssertionAsync(() => comp.Markup.Trim().Should().NotBeEmpty(), TimeSpan.FromSeconds(5));

            //Allow dismiss
            customDialogReference.AllowDismiss = true;

            //Dialog should now be closable through backdrop click
            comp.Find("div.vek-overlay").Click();
            await comp.WaitForAssertionAsync(() => comp.Markup.Trim().Should().BeEmpty(), TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Getting return value from dialog
        /// </summary>
        [Test]
        public async Task AsyncDialogShouldReturnTheReturnValue()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(service.ShowAsync<DialogWithReturnValue>);
            // open dialog
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);
            // close by click on cancel button
            comp.FindAll("button")[0].Click();
            var rv = await dialogReference.GetReturnValueAsync<string>();
            rv.Should().BeNull();
            // open dialog
            dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogWithReturnValue>());
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            // close by click on ok button
            comp.FindAll("button")[1].Click();
            dialogReference = await dialogReferenceLazy.Value;
            rv = await dialogReference.GetReturnValueAsync<string>();
            rv.Should().Be("Closed via OK");
        }

        [Test]
        public async Task AsyncDialogKeyboardNavigation()
        {
            var keyInterceptorService = Context.AddKeyInterceptorService();
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogOkCancel>(string.Empty, new DialogOptions() { CloseOnEscapeKey = true }));
            //dialog with clickable backdrop
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            dialogReferenceLazy.Value.Should().NotBe(null);
            var dialogReference = await dialogReferenceLazy.Value;
            var dialog1 = (DialogOkCancel)dialogReference.Dialog!;
            var dialogInstance1 = dialog1.VekDialog.GetDialogContainer();
            comp.Markup.Trim().Should().NotBeEmpty();
            await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(dialogInstance1.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown", }));
            comp.Markup.Trim().Should().BeEmpty();
            //dialog with disabled backdrop click
            dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogOkCancel>(string.Empty, new DialogOptions() { CloseOnEscapeKey = false }));
            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);
            var dialog2 = (DialogOkCancel)dialogReference.Dialog!;
            var dialogInstance2 = dialog2.VekDialog.GetDialogContainer();
            comp.Markup.Trim().Should().NotBeEmpty();
            await comp.InvokeAsync(() => keyInterceptorService.OnKeyDown(dialogInstance2.ElementId, new KeyboardEventArgs { Key = "Escape", Type = "keydown", }));
            comp.Markup.Trim().Should().NotBeEmpty();
        }

        [Test]
        public async Task AsyncDialogHandlesOnBackdropClickEvent()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();

            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);

            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(service.ShowAsync<DialogWithOnBackdropClickEvent>);

            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);
            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title:");

            //Click on backdrop
            comp.Find("div.vek-overlay").Click();

            comp.Find("div.vek-dialog-title").TrimmedText().Should().Be("Title: Backdrop clicked");
        }

        [Test]
        public async Task AsyncDialogToggleFullscreenOptions()
        {
            var timeProvider = Context.AddFakeTimeProvider();
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();

            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(service.ShowAsync<DialogToggleFullscreen>);

            await comp.InvokeAsync(() => dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);

            comp.Find("div.vek-dialog").GetAttribute("class").Should().Be("vek-dialog vek-dialog-width-sm");
            comp.Find("button").Click();
            comp.Find("div.vek-dialog").GetAttribute("class").Should().Be("vek-dialog vek-dialog-fullscreen");
            timeProvider.Advance(TimeSpan.FromMilliseconds(301));
            comp.Find("button").Click();
            comp.Find("div.vek-dialog").GetAttribute("class").Should().Be("vek-dialog vek-dialog-width-sm");
        }

        [Test]
        public async Task ShowAsyncRenderComplete()
        {
            var comp = Context.Render<VekDialogProvider>();

            var service = Context.Services.GetRequiredService<IDialogService>();

            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(service.ShowAsync<DialogOkCancel>);

            await comp.InvokeAsync(async () => await dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;

            var result = await dialogReference.RenderCompleteTaskCompletionSource.Task;

            result.Should().Be(true);
        }

        [Test]
        public void MockIDialogReferenceShouldWork()
        {
            Func<IDialogReference> createMock = Mock.Of<IDialogReference>;
            createMock.Should().NotThrow();
        }

        [Test]
        public async Task AsyncDialogParametersGenericShouldPassParameters()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);

            var parameters = new DialogParameters<DialogWithParameters>
            {
                { x => x.TestValue, "test" },
                { x => x.ColorTest, Color.Error }
            };
            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogWithParameters>(string.Empty, parameters));
            await comp.InvokeAsync(async () => await dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);

            var textField = comp.FindComponent<VekInput<string>>().Instance;
            textField.ReadText.Should().Be("test");
        }

        [Test]
        public async Task ShowAsyncWithParametersOnly_ShouldPassParameters()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);

            var parameters = new DialogParameters<DialogWithParameters>
            {
                { x => x.TestValue, "test" },
                { x => x.ColorTest, Color.Error }
            };
            IDialogReference dialogReference = null!;
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithParameters>(parameters));
            dialogReference.Should().NotBe(null);

            var textField = comp.FindComponent<VekInput<string>>().Instance;
            textField.ReadText.Should().Be("test");
            ((DialogWithParameters)dialogReference.Dialog).ColorTest.Should().Be(Color.Error);
        }

        [Test]
        public async Task ShowGeneric_ShouldProvideDefaultOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Custom title"));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Markup.Should().Contain("Custom title");
            dialogInstance.Instance.Options.Position.Should().BeNull();
            dialogInstance.Instance.Options.MaxWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackdropClick.Should().BeNull();
            dialogInstance.Instance.Options.CloseOnEscapeKey.Should().BeNull();
            dialogInstance.Instance.Options.NoHeader.Should().BeNull();
            dialogInstance.Instance.Options.CloseButton.Should().BeNull();
            dialogInstance.Instance.Options.FullWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackgroundClass.Should().BeNull();
        }

        [Test]
        public async Task ShowGeneric_ShouldProvideCorrectOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Custom title", new DialogOptions { CloseButton = true }));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Instance.Options.CloseButton.Should().BeTrue();
        }

        [Test]
        public async Task Show_ShouldRenderComponent()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync(typeof(DialogOkCancel)));

            // Assert
            provider.FindComponents<DialogOkCancel>().Should().HaveCount(1);
        }

        [Test]
        public async Task Show_ShouldProvideDefaultOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync(typeof(DialogOkCancel), "Custom title"));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Markup.Should().Contain("Custom title");
            dialogInstance.Instance.Options.Position.Should().BeNull();
            dialogInstance.Instance.Options.MaxWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackdropClick.Should().BeNull();
            dialogInstance.Instance.Options.CloseOnEscapeKey.Should().BeNull();
            dialogInstance.Instance.Options.NoHeader.Should().BeNull();
            dialogInstance.Instance.Options.CloseButton.Should().BeNull();
            dialogInstance.Instance.Options.FullWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackgroundClass.Should().BeNull();
        }

        [Test]
        public async Task Show_ShouldProvideCorrectOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync(typeof(DialogOkCancel), "Custom title", new DialogOptions { CloseButton = true }));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Instance.Options.CloseButton.Should().BeTrue();
        }

        [Test]
        public async Task Show_ShouldPassDialogParametersToDialog()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () =>
                await service.ShowAsync(typeof(DialogWithActionsClass), "Custom title", new DialogParameters<DialogWithActionsClass> { { x => x.ActionsClass, "custom-class" } }));

            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.FindAll(".custom-class").Should().HaveCount(1);
        }

        [Test]
        public async Task ShowGenericAsync_ShouldProvideDefaultOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Custom title"));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Markup.Should().Contain("Custom title");
            dialogInstance.Instance.Options.Position.Should().BeNull();
            dialogInstance.Instance.Options.MaxWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackdropClick.Should().BeNull();
            dialogInstance.Instance.Options.CloseOnEscapeKey.Should().BeNull();
            dialogInstance.Instance.Options.NoHeader.Should().BeNull();
            dialogInstance.Instance.Options.CloseButton.Should().BeNull();
            dialogInstance.Instance.Options.FullWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackgroundClass.Should().BeNull();
        }

        [Test]
        public async Task ShowGenericAsync_ShouldProvideCorrectOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Custom title", new DialogOptions { CloseButton = true }));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Instance.Options.CloseButton.Should().BeTrue();
        }

        [Test]
        public async Task Close_ShouldCloseDialogAndInvokeOnDialogCloseRequested()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            var reference = service.CreateReference();
            var invoked = false;
            Type type = null;
            service.OnDialogCloseRequested += (_, result) =>
            {
                invoked = true;
                type = result.DataType;
            };

            // Act
            await provider.InvokeAsync(() => service.Close(reference));

            // Assert
            invoked.Should().BeTrue();
            type.Should().BeNull();
        }

        [Test]
        public async Task ShowAsync_FromNonUiThreadShouldAddInstance()
        {
            // Arrange
            var service = Context.Services.GetRequiredService<IDialogService>();
            _ = Context.Render<VekDialogProvider>();
            var invoked = false;
            service.DialogInstanceAddedAsync += _ =>
            {
                invoked = true;

                return Task.CompletedTask;
            };

            // Act
            await Task.Factory.StartNew(() => service.ShowAsync(typeof(DialogOkCancel))).ConfigureAwait(false);

            // Assert
            invoked.Should().BeTrue();
        }

        [Test]
        public async Task ShowAsync_ShouldProvideDefaultOption()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync(typeof(DialogOkCancel)));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Instance.Options.Position.Should().BeNull();
            dialogInstance.Instance.Options.MaxWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackdropClick.Should().BeNull();
            dialogInstance.Instance.Options.CloseOnEscapeKey.Should().BeNull();
            dialogInstance.Instance.Options.NoHeader.Should().BeNull();
            dialogInstance.Instance.Options.CloseButton.Should().BeNull();
            dialogInstance.Instance.Options.FullWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackgroundClass.Should().BeNull();
        }

        [Test]
        public async Task ShowAsync_ShouldProvideDefaultOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync(typeof(DialogOkCancel), "Custom title"));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Markup.Should().Contain("Custom title");
            dialogInstance.Instance.Options.Position.Should().BeNull();
            dialogInstance.Instance.Options.MaxWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackdropClick.Should().BeNull();
            dialogInstance.Instance.Options.CloseOnEscapeKey.Should().BeNull();
            dialogInstance.Instance.Options.NoHeader.Should().BeNull();
            dialogInstance.Instance.Options.CloseButton.Should().BeNull();
            dialogInstance.Instance.Options.FullWidth.Should().BeNull();
            dialogInstance.Instance.Options.BackgroundClass.Should().BeNull();
        }

        [Test]
        public async Task ShowAsync_ShouldProvideCorrectOptions_WhenOverloadIsCalled()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(async () => await service.ShowAsync(typeof(DialogOkCancel), "Custom title", new DialogOptions { CloseButton = true }));
            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.Instance.Options.CloseButton.Should().BeTrue();
        }

        [Test]
        public async Task ShowAsync_ShouldPassDialogParametersToDialog()
        {
            // Arrange
            var provider = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act
            await provider.InvokeAsync(() =>
                service.ShowAsync(typeof(DialogWithActionsClass), "Custom title", new DialogParameters<DialogWithActionsClass> { { x => x.ActionsClass, "custom-class" } }));

            var dialogInstance = provider.FindComponent<VekDialogContainer>();

            // Assert
            dialogInstance.FindAll(".custom-class").Should().HaveCount(1);
        }

        [TestCase("", false, "vek-dialog-content")]
        [TestCase("", true, "vek-dialog-content vek-dialog-no-side-padding")]
        [TestCase("my-class", false, "vek-dialog-content my-class")]
        [TestCase("my-class", true, "vek-dialog-content vek-dialog-no-side-padding my-class")]
        public async Task DialogWithContentClassValueShouldRenderExpectedClassname(string contentClass, bool disablePadding, string expectedClassname)
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBeNull();
            IDialogReference dialogReference = null;

            var parameters = new DialogParameters<DialogWithContentClass>
            {
                { x => x.ContentClass, contentClass },
                { x => x.Gutters, !disablePadding }
            };

            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithContentClass>(string.Empty, parameters));
            dialogReference.Should().NotBeNull();

            comp.Find("div.vek-dialog-content").GetAttribute("class").Should().Be(expectedClassname);
        }

        /// <summary>
        /// Enables the accessible movement and resize controls when requested.
        /// </summary>
        [Test]
        public async Task DraggableAndResizableOptionsRenderAccessibleInteractionHandles()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            var options = new DialogOptions { Draggable = true, Resizable = true };

            await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("Adjustable dialog", options));

            var dialog = comp.Find("[role='dialog']");
            dialog.ClassList.Should().Contain("vek-dialog-draggable");
            dialog.ClassList.Should().Contain("vek-dialog-resizable");

            var header = comp.Find(".vek-dialog-title");
            header.GetAttribute("role").Should().Be("group");
            header.GetAttribute("tabindex").Should().Be("0");
            header.GetAttribute("aria-label").Should().Be("Move dialog with arrow keys");

            var handles = comp.FindAll(".vek-dialog-resize-handle");
            handles.Count.Should().Be(8);
            handles.Select(handle => handle.GetAttribute("aria-label")).Should().OnlyContain(label => !string.IsNullOrWhiteSpace(label));
        }

        [TestCase("", "vek-dialog-actions")]
        [TestCase("my-class", "vek-dialog-actions my-class")]
        public async Task DialogWithActionsClassValueShouldRenderExpectedClassname(string actionsClass, string expectedClassname)
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBeNull();
            IDialogReference dialogReference = null;

            var parameters = new DialogParameters<DialogWithActionsClass>
            {
                { x => x.ActionsClass, actionsClass }
            };

            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithActionsClass>(string.Empty, parameters));
            dialogReference.Should().NotBeNull();

            comp.Find("div.vek-dialog-actions").GetAttribute("class").Should().Be(expectedClassname);
        }

        [TestCase("", "vek-dialog-title")]
        [TestCase("my-title-class my-second-class", "vek-dialog-title my-title-class my-second-class")]
        public async Task DialogWithTitleClassValueShouldRenderExpectedClassname(string titleClass, string expectedClassname)
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBeNull();
            IDialogReference dialogReference = null;

            var parameters = new DialogParameters<DialogWithTitleClass>
            {
                { x => x.TitleClass, titleClass }
            };

            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithTitleClass>(string.Empty, parameters));
            dialogReference.Should().NotBeNull();

            comp.Find("div.vek-dialog-title").GetAttribute("class").Should().Be(expectedClassname);
        }

        [Test]
        public async Task DialogWithTitleShouldLinkDialogToTitleForAssistiveTechnologies()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Dialog title"));

            var dialog = comp.Find("div[role='dialog']");
            var labelledBy = dialog.GetAttribute("aria-labelledby");
            labelledBy.Should().NotBeNullOrWhiteSpace();

            var title = comp.Find("div.vek-dialog-title");
            title.GetAttribute("id").Should().Be(labelledBy);
            title.TrimmedText().Should().Contain("Dialog title");
        }

        [Test]
        public async Task DialogWithoutHeaderShouldNotRenderAriaLabelledBy()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>(string.Empty, new DialogOptions { NoHeader = true }));

            var dialog = comp.Find("div[role='dialog']");
            dialog.GetAttribute("aria-labelledby").Should().BeNull();
            comp.FindAll("div.vek-dialog-title").Should().BeEmpty();
        }

        [Test]
        public async Task DialogWithNestedDialogOptionShouldNotReset()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();
            var service = Context.Services.GetRequiredService<IDialogService>();

            var dialogReferenceLazy = new Lazy<Task<IDialogReference>>(() => service.ShowAsync<DialogOptionMutation>("Simple Dialog"));
            await comp.InvokeAsync(async () => await dialogReferenceLazy.Value);
            var dialogReference = await dialogReferenceLazy.Value;
            dialogReference.Should().NotBe(null);

            var closeButton = () => comp.Find("button.vek-button-close");
            // Close button should be visible
            closeButton().Should().NotBeNull();
            var button = comp.Find("#show_messagebox_btn");
            // Open nested message box
            button.Click();

            var messageBox = () => comp.Find("div.vek-message-box");

            // close message box by clicking on yes button.
            messageBox().Should().NotBeNull();
            comp.FindAll(".vek-dialog-actions button")[1].Click();
            messageBox.Should().Throw<ElementNotFoundException>();

            // Close button should still be visible
            closeButton().Should().NotBeNull();
        }

        /// <summary>
        /// Ensures dialog will only be shown once
        /// </summary>
        [Test]
        public async Task DuplicateDialog()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            service.Should().NotBe(null);
            var comp1 = Context.Render<InlineDialogDuplicateTest>();
            // open the dialog
            comp1.Find("button").Click();
            await Task.Delay(1000);
            comp.FindComponents<VekDialog>().Count.Should().Be(1);
        }

        [Test]
        public async Task Dialog_TitleContent_And_ProgressBar_ShouldUpdate()
        {
            var comp = Context.Render<VekDialogProvider>();
            comp.Markup.Trim().Should().BeEmpty();

            var service = Context.Services.GetRequiredService<IDialogService>();

            var comp1 = Context.Render<InlineDialogTitleRefreshTest>();

            comp1.Find("button").Click();

            var dialog = comp.Find("div.vek-dialog");

            dialog.Should().NotBeNull();
            dialog.TextContent.Trim().Should().Be("Initial state"); // Initial title
            comp.FindComponent<VekProgressLinear>().Instance.Value.Should().Be(0); // Initial progress value - 0

            // Wait for mid-progress (simulate that loop is progressing)
            await Task.Delay(1000);
            //comp.Render();

            dialog = comp.Find("div.vek-dialog");
            dialog.TextContent.Should().Contain("Progress"); //change from "Initial state" to "Progress"

            var progressComponent = comp.FindComponent<VekProgressLinear>();
            var progressValue = progressComponent.Instance.Value;
            progressValue.Should().BeGreaterThan(0).And.BeLessThan(100);

            // Wait for dialog to close and state reset
            await Task.Delay(3000); // Ensure loop finishes
            comp.Render();

            // Assert the dialog is now hidden
            comp.Markup.Trim().Should().NotContain("vek-dialog");
        }

        [Test]
        public async Task HandleBackgroundClickAsync_Should_Call_RefocusDialogAsync_When_BackdropClick_Disabled()
        {
            // Arrange
            IDialogReference dialogReference = null;

            var jsRuntimeMock = new Mock<IJSRuntime>();

            jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("Blazor._internal.domWrapper.focus", It.IsAny<object[]>()));

            Context.Services.AddSingleton(jsRuntimeMock.Object);

            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            var dialogOptions = new DialogOptions { BackdropClick = false, CloseOnEscapeKey = true };

            //Act
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogOkCancel>("Test Dialog True", dialogOptions));

            jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("Blazor._internal.domWrapper.focus",
                It.Is<object[]>(args =>
                    args.Length == 2 &&
                    args[0] is ElementReference &&
                    args[1] is bool)),
                Times.AtMost(1)); // Focus should be called once when dialog is opened (FocusTrap)

            comp.Find("div.vek-dialog-container").Should().NotBeNull(); // Dialog is open
            comp.Find("div.vek-overlay").Click(); // Simulate a click on the backdrop

            //Assert
            comp.Find("div.vek-dialog-container").Should().NotBeNull(); // Dialog should still be open

            jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("Blazor._internal.domWrapper.focus",
                It.Is<object[]>(args =>
                    args.Length == 2 &&
                    args[0] is ElementReference &&
                    args[1] is bool)),
                Times.AtMost(2)); // Focus should be called once when dialog is opened and once when backdrop is clicked (refouus)
        }

        [Test]
        public async Task HandleBackgroundClickAsync_Should_Call_OnBackdropClick_And_RefocusDialog_When_Delegate_Defined()
        {
            // Arrange
            var jsRuntimeMock = new Mock<IJSRuntime>();

            jsRuntimeMock.Setup(x => x.InvokeAsync<IJSVoidResult>("Blazor._internal.domWrapper.focus", It.IsAny<object[]>()));

            Context.Services.AddSingleton(jsRuntimeMock.Object);

            var comp = Context.Render<VekDialogProvider>();

            // Create a dialog component with OnBackdropClick delegate
            var dialogComponent = Context.Render<DialogCloseOnEscapeTest>();

            // Act
            await dialogComponent.FindComponent<VekButton>().Find("button").ClickAsync(new MouseEventArgs()); // Open the dialog

            jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("Blazor._internal.domWrapper.focus",
                It.Is<object[]>(args =>
                    args.Length == 2 &&
                    args[0] is ElementReference &&
                    args[1] is bool)),
                Times.AtMost(1)); // Focus should be called once when dialog is opened

            comp.Find("div.vek-dialog-container").Should().NotBeNull(); // Dialog is open

            await comp.Find("div.vek-overlay").ClickAsync(new MouseEventArgs()); // Simulate a click on the backdrop

            // Assert
            comp.Find("div.vek-dialog-container").Should().NotBeNull(); // Dialog should still be open (not closed)

            jsRuntimeMock.Verify(x => x.InvokeAsync<IJSVoidResult>("Blazor._internal.domWrapper.focus",
                It.Is<object[]>(args =>
                    args.Length == 2 &&
                    args[0] is ElementReference &&
                    args[1] is bool)),
                Times.AtMost(2)); // Focus should be called once when dialog opens and once for refocus after backdrop click
        }

        /// <summary>
        /// Test that DefaultFocus property on VekDialogProvider is properly cascaded to dialogs
        /// </summary>
        [Test]
        public async Task DialogProvider_DefaultFocus_ShouldCascadeToDialogs()
        {
            // Arrange - Create a DialogProvider with DefaultFocus set to None
            var comp = Context.Render<VekDialogProvider>(parameters => parameters
                .Add(p => p.DefaultFocus, DefaultFocus.None));
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act - Show a dialog without explicitly setting DefaultFocus
            IDialogReference dialogReference = null;
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithDefaultFocusTest>());

            // Assert - Verify the dialog is shown
            dialogReference.Should().NotBeNull();
            comp.Find("div.vek-dialog-container").Should().NotBeNull();

            // Close the dialog
            await comp.InvokeAsync(() => service.Close(dialogReference));
        }

        /// <summary>
        /// Test that VekDialog respects explicit DefaultFocus parameter over global setting
        /// </summary>
        [Test]
        public async Task Dialog_ExplicitDefaultFocus_ShouldOverrideGlobalSetting()
        {
            // Arrange - Create a DialogProvider with DefaultFocus set to None
            var comp = Context.Render<VekDialogProvider>(parameters => parameters
                .Add(p => p.DefaultFocus, DefaultFocus.None));
            var service = Context.Services.GetRequiredService<IDialogService>();

            // Act - Show a dialog WITH explicitly setting DefaultFocus to FirstChild
            var dialogParameters = new DialogParameters
            {
                [nameof(VekDialog.DefaultFocus)] = DefaultFocus.FirstChild
            };
            IDialogReference dialogReference = null;
            await comp.InvokeAsync(async () => dialogReference = await service.ShowAsync<DialogWithDefaultFocusTest>(title: "Test", parameters: dialogParameters));

            // Assert - Verify the dialog is shown
            dialogReference.Should().NotBeNull();
            comp.Find("div.vek-dialog-container").Should().NotBeNull();

            // Close the dialog
            await comp.InvokeAsync(() => service.Close(dialogReference));
        }

        /// <summary>
        /// InjectOptions() should set the options of the calling IDialogReference.
        /// </summary>
        [Test]
        public async Task InjectOptions_ShouldNotBeNull()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();

            var reference = await service.ShowAsync<DialogOkCancel>();

            reference.InjectOptions(new DialogOptions());
            reference.Options.Should().NotBe(null);
        }

        /// <summary>
        /// SetOptions() should set the options of the dialog reference that corresponds to the given id.
        /// </summary>
        [Test]
        public async Task SetOptions_ShouldNotBeNull()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();
            var provider = Context.Render<VekDialogProvider>();

            var reference = await service.ShowAsync<DialogOkCancel>();

            provider.Instance.SetOptions(reference.Id, new DialogOptions());
            reference.Options.Should().NotBe(null);
        }

        /// <summary>
        /// ShouldDismissOnNavigation() should return true if the dialog reference's options CloseOnNavigation is set to true.
        /// </summary>
        [Test]
        public async Task ShouldDismissOnNavigation_ShouldBeTrue()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();
            var provider = Context.Render<VekDialogProvider>();

            var options = new DialogOptions
            {
                CloseOnNavigation = true
            };
            var reference = await service.ShowAsync<DialogOkCancel>("test", options: options);

            provider.Instance.ShouldDismissOnNavigation(reference, "/test").Should().BeTrue();
        }

        /// <summary>
        /// ShouldDismissOnNavigation() should return false if the dialog reference's options CloseOnNavigation is set to false.
        /// </summary>
        [Test]
        public async Task ShouldDismissOnNavigation_ShouldBeFalse()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();
            var provider = Context.Render<VekDialogProvider>();

            var reference = await service.ShowAsync<DialogOkCancel>();
            var options = new DialogOptions
            {
                CloseOnNavigation = false
            };
            reference.InjectOptions(options);

            provider.Instance.ShouldDismissOnNavigation(reference, "/test").Should().BeFalse();
        }

        /// <summary>
        /// HasRouteChanged() should return true if the dialog reference's options
        /// CloseOnNavigation is set to null and the absolute path has changed.
        /// </summary>
        [Test]
        public async Task HasRouteChanged_ShouldBeTrueWhenCloseOnNavigationIsNullAndRouteChanged()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();
            var navigationManager = Context.Services.GetRequiredService<NavigationManager>();
            var provider = Context.Render<VekDialogProvider>();

            var currentRoute = navigationManager.ToAbsoluteUri($"/test/{Guid.NewGuid()}").ToString();
            navigationManager.NavigateTo(currentRoute);

            var reference = await service.ShowAsync<DialogOkCancel>();
            var options = new DialogOptions
            {
                CloseOnNavigation = null
            };
            reference.InjectOptions(options);

            var changedRoute = navigationManager.ToAbsoluteUri($"{currentRoute}/{Guid.NewGuid()}").AbsolutePath.TrimEnd('/');
            provider.Instance.HasRouteChanged(changedRoute).Should().BeTrue();
        }

        /// <summary>
        /// HasRouteChanged() should return false if the dialog reference's options
        /// CloseOnNavigation is set to null and only the query or fragment has changed.
        /// </summary>
        [Test]
        public async Task HasRouteChanged_ShouldBeFalseWhenCloseOnNavigationIsNullAndQueryOrFragmentChanged()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();
            var navigationManager = Context.Services.GetRequiredService<NavigationManager>();
            var provider = Context.Render<VekDialogProvider>();

            var currentRoute = navigationManager.ToAbsoluteUri($"/test/{Guid.NewGuid()}").ToString();
            navigationManager.NavigateTo(currentRoute);

            var reference = await service.ShowAsync<DialogOkCancel>();
            var options = new DialogOptions
            {
                CloseOnNavigation = null
            };
            reference.InjectOptions(options);

            var changedQuery = navigationManager.ToAbsoluteUri($"{currentRoute}?query={Guid.NewGuid()}").AbsolutePath;
            provider.Instance.HasRouteChanged(changedQuery).Should().BeFalse();

            var changedFragment = navigationManager.ToAbsoluteUri($"{currentRoute}#{Guid.NewGuid()}").AbsolutePath;
            provider.Instance.HasRouteChanged(changedFragment).Should().BeFalse();
        }

        /// <summary>
        /// HasRouteChanged() should return true if the dialog reference's options is set to null
        /// and the absolute path has changed.
        /// </summary>
        [Test]
        public async Task HasRouteChanged_ShouldBeTrueWhenOptionsIsNullAndRouteChanged()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();
            var navigationManager = Context.Services.GetRequiredService<NavigationManager>();
            var provider = Context.Render<VekDialogProvider>();

            var currentRoute = navigationManager.ToAbsoluteUri($"/test/{Guid.NewGuid()}").ToString();
            navigationManager.NavigateTo(currentRoute);

            var reference = await service.ShowAsync<DialogOkCancel>();
            reference.InjectOptions(null);

            var changedRoute = navigationManager.ToAbsoluteUri($"{currentRoute}/{Guid.NewGuid()}").AbsolutePath.TrimEnd('/');
            provider.Instance.HasRouteChanged(changedRoute).Should().BeTrue();
        }

        /// <summary>
        /// HasRouteChanged() should return false if the dialog reference's options is set to null
        /// and only the query or fragment has changed.
        /// </summary>
        [Test]
        public async Task HasRouteChanged_ShouldBeFalseWhenOptionsIsNullAndQueryOrFragmentChanged()
        {
            var service = Context.Services.GetRequiredService<IDialogService>();
            var navigationManager = Context.Services.GetRequiredService<NavigationManager>();
            var provider = Context.Render<VekDialogProvider>();

            var currentRoute = navigationManager.ToAbsoluteUri($"/test/{Guid.NewGuid()}").ToString();
            navigationManager.NavigateTo(currentRoute);

            var reference = await service.ShowAsync<DialogOkCancel>();
            reference.InjectOptions(null);

            var changedQuery = navigationManager.ToAbsoluteUri($"{currentRoute}?query={Guid.NewGuid()}").AbsolutePath;
            provider.Instance.HasRouteChanged(changedQuery).Should().BeFalse();

            var changedFragment = navigationManager.ToAbsoluteUri($"{currentRoute}#{Guid.NewGuid()}").AbsolutePath;
            provider.Instance.HasRouteChanged(changedFragment).Should().BeFalse();
        }

        /// <summary>
        /// Closing an earlier dialog must not re-instantiate later dialogs that remain open.
        /// Reproduces #13231 (closing a parent dialog reopens/re-renders a still-open child dialog).
        /// </summary>
        [Test]
        public async Task ClosingEarlierDialog_ShouldNotReinstantiateLaterDialogs()
        {
            DialogReinstantiationInner.InitCount = 0;
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            IDialogReference first = null;
            IDialogReference second = null;
            await comp.InvokeAsync(async () => first = await service.ShowAsync<DialogOkCancel>("First"));
            await comp.InvokeAsync(async () => second = await service.ShowAsync<DialogReinstantiationInner>("Second"));

            // Both dialogs are open and the inner dialog has been constructed exactly once.
            comp.FindAll("div.vek-dialog-container").Count.Should().Be(2);
            DialogReinstantiationInner.InitCount.Should().Be(1);

            // Close the FIRST (earlier) dialog while the second remains open.
            await comp.InvokeAsync(() => first.Close());

            // The second dialog must still be open and must NOT have been reconstructed.
            comp.FindAll("div.vek-dialog-container").Count.Should().Be(1);
            DialogReinstantiationInner.InitCount.Should().Be(1);

            second.Should().NotBeNull();
        }

        /// <summary>
        /// A dialog is aria-modal and is named by its visible title (#13609).
        /// </summary>
        [Test]
        public async Task DialogShouldExposeModalSemanticsForAssistiveTechnologies()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Dialog title"));

            var dialog = comp.Find("div[role='dialog']");
            dialog.GetAttribute("aria-modal").Should().Be("true");
            // The visible title already names the dialog, so no redundant aria-label is emitted.
            dialog.HasAttribute("aria-label").Should().BeFalse();
        }

        /// <summary>
        /// A dialog without a header falls back to an aria-label from its title (#13609).
        /// </summary>
        [Test]
        public async Task DialogWithoutHeaderShouldFallBackToAriaLabelFromTitle()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>("Dialog title", new DialogOptions { NoHeader = true }));

            var dialog = comp.Find("div[role='dialog']");
            dialog.GetAttribute("aria-labelledby").Should().BeNull();
            dialog.GetAttribute("aria-label").Should().Be("Dialog title");
        }

        /// <summary>
        /// A dialog with neither a header nor a title gets no empty aria-label (#13609).
        /// </summary>
        [Test]
        public async Task DialogWithoutHeaderOrTitleShouldNotEmitEmptyAriaLabel()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(async () => await service.ShowAsync<DialogOkCancel>(string.Empty, new DialogOptions { NoHeader = true }));

            var dialog = comp.Find("div[role='dialog']");
            dialog.HasAttribute("aria-labelledby").Should().BeFalse();
            dialog.HasAttribute("aria-label").Should().BeFalse();
        }

        /// <summary>
        /// An inline dialog shown through the parameterless ShowAsync has no title, so a hidden header leaves it unnamed unless AriaLabel names it (#13609).
        /// </summary>
        [Test]
        public async Task InlineDialogWithoutHeaderShouldUseAriaLabel()
        {
            var provider = Context.Render<VekDialogProvider>();
            var comp = Context.Render<InlineDialogAriaLabelTest>(parameters => parameters.Add(p => p.AriaLabel, "Confirm removal"));

            await comp.Find("button").ClickAsync();

            await provider.WaitForAssertionAsync(() =>
            {
                var dialog = provider.Find("div[role='dialog']");
                dialog.GetAttribute("aria-modal").Should().Be("true");
                dialog.GetAttribute("aria-label").Should().Be("Confirm removal");
                dialog.HasAttribute("aria-labelledby").Should().BeFalse();
            });
        }

        /// <summary>
        /// AriaLabelledBy names the dialog by an element inside its content and suppresses the text fallback.
        /// </summary>
        [Test]
        public async Task InlineDialogShouldUseAriaLabelledBy()
        {
            var provider = Context.Render<VekDialogProvider>();
            var comp = Context.Render<InlineDialogAriaLabelTest>(parameters => parameters
                .Add(p => p.AriaLabel, "Ignored")
                .Add(p => p.AriaLabelledBy, "custom-heading"));

            await comp.Find("button").ClickAsync();

            await provider.WaitForAssertionAsync(() =>
            {
                var dialog = provider.Find("div[role='dialog']");
                dialog.GetAttribute("aria-labelledby").Should().Be("custom-heading");
                dialog.HasAttribute("aria-label").Should().BeFalse();
                provider.Find("#custom-heading").TextContent.Trim().Should().Be("Remove this item?");
            });
        }

        /// <summary>
        /// An inline dialog with a hidden header and no name of its own stays unlabelled rather than getting an empty attribute.
        /// </summary>
        [Test]
        public async Task InlineDialogWithoutHeaderOrNameShouldNotEmitEmptyAttributes()
        {
            var provider = Context.Render<VekDialogProvider>();
            var comp = Context.Render<InlineDialogAriaLabelTest>();

            await comp.Find("button").ClickAsync();

            await provider.WaitForAssertionAsync(() =>
            {
                var dialog = provider.Find("div[role='dialog']");
                dialog.HasAttribute("aria-label").Should().BeFalse();
                dialog.HasAttribute("aria-labelledby").Should().BeFalse();
            });
        }

        /// <summary>
        /// An explicit AriaLabel replaces the title bar as the name of a service-shown dialog.
        /// </summary>
        [Test]
        public async Task DialogAriaLabelShouldReplaceTitleBarName()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(async () => await service.ShowAsync<DialogAriaLabelContent>("Dialog title"));

            await comp.WaitForAssertionAsync(() =>
            {
                var dialog = comp.Find("div[role='dialog']");
                dialog.GetAttribute("aria-label").Should().Be("Spoken name");
                dialog.HasAttribute("aria-labelledby").Should().BeFalse();
                comp.Find("div.vek-dialog-title").TrimmedText().Should().Contain("Dialog title");
            });
        }

        [Test]
        public async Task MaximizeButton_ShouldNotRenderByDefault()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("Standard dialog"));

            comp.FindAll("button.vek-button-maximize").Should().BeEmpty();
            comp.Find("div.vek-dialog").ClassList.Should().NotContain("vek-dialog-fullscreen");
        }

        [Test]
        public async Task MaximizeButton_ClickShouldToggleFullscreenAndRestore()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            var options = new DialogOptions
            {
                ShowMaximizeButton = true,
                CloseButton = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            };

            await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("Resizable window", options));

            var dialog = comp.Find("div.vek-dialog");
            dialog.ClassList.Should().Contain("vek-dialog-width-md");
            dialog.ClassList.Should().Contain("vek-dialog-width-full");
            dialog.ClassList.Should().NotContain("vek-dialog-fullscreen");

            var maxButton = comp.Find("button.vek-button-maximize");
            maxButton.GetAttribute("aria-label").Should().Be("Maximize");

            await maxButton.ClickAsync();

            dialog = comp.Find("div.vek-dialog");
            dialog.ClassList.Should().Contain("vek-dialog-fullscreen");
            dialog.ClassList.Should().NotContain("vek-dialog-width-md");
            dialog.ClassList.Should().NotContain("vek-dialog-width-full");
            comp.Find("button.vek-button-maximize").GetAttribute("aria-label").Should().Be("Restore");

            await comp.Find("button.vek-button-maximize").ClickAsync();

            dialog = comp.Find("div.vek-dialog");
            dialog.ClassList.Should().NotContain("vek-dialog-fullscreen");
            dialog.ClassList.Should().Contain("vek-dialog-width-md");
            dialog.ClassList.Should().Contain("vek-dialog-width-full");
            comp.Find("button.vek-button-maximize").GetAttribute("aria-label").Should().Be("Maximize");
        }

        [Test]
        public async Task HeaderDoubleClick_ShouldToggleMaximizeOnlyWhenShowMaximizeButtonEnabled()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("No maximize"));
            await comp.Find("div.vek-dialog-title").DoubleClickAsync();
            comp.Find("div.vek-dialog").ClassList.Should().NotContain("vek-dialog-fullscreen");

            await comp.InvokeAsync(() => comp.Instance.DismissAll());

            await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("With maximize", new DialogOptions { ShowMaximizeButton = true }));
            await comp.Find("div.vek-dialog-title").DoubleClickAsync();
            comp.Find("div.vek-dialog").ClassList.Should().Contain("vek-dialog-fullscreen");

            await comp.Find("div.vek-dialog-title").DoubleClickAsync();
            comp.Find("div.vek-dialog").ClassList.Should().NotContain("vek-dialog-fullscreen");
        }

        [Test]
        public async Task MaximizeAndRestore_ShouldPreservePreviousBoundsAndToggleInteractionHandles()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            var options = new DialogOptions
            {
                ShowMaximizeButton = true,
                Draggable = true,
                Resizable = true
            };

            var reference = await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("Bounded dialog", options));
            var container = comp.FindComponent<VekDialogContainer>();

            await comp.InvokeAsync(() => container.Instance.SetDialogBoundsAsync(120, 80, 420, 300));

            var dialog = comp.Find("div.vek-dialog");
            var style = dialog.GetAttribute("style");
            style.Should().Contain("left:120px");
            style.Should().Contain("top:80px");
            style.Should().Contain("width:420px");
            style.Should().Contain("height:300px");
            comp.FindAll(".vek-dialog-resize-handle").Count.Should().Be(8);

            await comp.Find("button.vek-button-maximize").ClickAsync();

            dialog = comp.Find("div.vek-dialog");
            dialog.ClassList.Should().Contain("vek-dialog-fullscreen");
            dialog.ClassList.Should().NotContain("vek-dialog-draggable");
            dialog.ClassList.Should().NotContain("vek-dialog-resizable");
            (dialog.GetAttribute("style") ?? string.Empty).Should().NotContain("left:120px");
            (dialog.GetAttribute("style") ?? string.Empty).Should().NotContain("width:420px");
            comp.FindAll(".vek-dialog-resize-handle").Should().BeEmpty();
            reference.Options!.FullScreen.Should().BeTrue();

            await comp.Find("button.vek-button-maximize").ClickAsync();

            dialog = comp.Find("div.vek-dialog");
            dialog.ClassList.Should().NotContain("vek-dialog-fullscreen");
            dialog.ClassList.Should().Contain("vek-dialog-draggable");
            dialog.ClassList.Should().Contain("vek-dialog-resizable");
            style = dialog.GetAttribute("style");
            style.Should().Contain("left:120px");
            style.Should().Contain("top:80px");
            style.Should().Contain("width:420px");
            style.Should().Contain("height:300px");
            comp.FindAll(".vek-dialog-resize-handle").Count.Should().Be(8);
            reference.Options!.FullScreen.Should().BeFalse();
        }

        [Test]
        public async Task VekDialogParameters_ShouldControlShowMaximizeButtonAndMaximizedState()
        {
            var comp = Context.Render<VekDialogProvider>();
            var service = Context.Services.GetRequiredService<IDialogService>();
            var parameters = new DialogParameters<DialogMaximizeTest>
            {
                { x => x.ShowMaximizeButton, true },
                { x => x.Maximized, true }
            };

            await comp.InvokeAsync(() => service.ShowAsync<DialogMaximizeTest>("Parameter dialog", parameters));

            comp.Find("div.vek-dialog").ClassList.Should().Contain("vek-dialog-fullscreen");
            comp.Find("button.vek-button-maximize").GetAttribute("aria-label").Should().Be("Restore");
            comp.Find(".maximized-state").TrimmedText().Should().Be("True");

            await comp.Find("button.vek-button-maximize").ClickAsync();

            comp.Find("div.vek-dialog").ClassList.Should().NotContain("vek-dialog-fullscreen");
            comp.Find("button.vek-button-maximize").GetAttribute("aria-label").Should().Be("Maximize");
            comp.Find(".maximized-state").TrimmedText().Should().Be("False");
        }

        [Test]
        public async Task VekDialogProvider_GlobalShowMaximizeButton_ShouldApplyToDialogs()
        {
            var comp = Context.Render<VekDialogProvider>(parameters => parameters.Add(p => p.ShowMaximizeButton, true));
            var service = Context.Services.GetRequiredService<IDialogService>();

            await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("Global maximize"));

            comp.FindAll("button.vek-button-maximize").Count.Should().Be(1);

            await comp.InvokeAsync(() => comp.Instance.DismissAll());
            await comp.InvokeAsync(() => service.ShowAsync<DialogOkCancel>("Hidden header", new DialogOptions { NoHeader = true }));

            comp.FindAll("button.vek-button-maximize").Should().BeEmpty();
        }

        [Test]
        public async Task InlineDialog_ShowMaximizeButtonAndBindMaximized_ShouldSync()
        {
            var provider = Context.Render<VekDialogProvider>();
            var comp = Context.Render<InlineDialogMaximizeTest>();

            await comp.Find(".open-inline-maximize-btn").ClickAsync();
            await provider.WaitForAssertionAsync(() => provider.Find("button.vek-button-maximize").Should().NotBeNull());

            await provider.Find("button.vek-button-maximize").ClickAsync();

            provider.Find("div.vek-dialog").ClassList.Should().Contain("vek-dialog-fullscreen");
            comp.Find(".inline-maximized-value").TrimmedText().Should().Be("True");

            await provider.Find("button.vek-button-maximize").ClickAsync();

            provider.Find("div.vek-dialog").ClassList.Should().NotContain("vek-dialog-fullscreen");
            comp.Find(".inline-maximized-value").TrimmedText().Should().Be("False");
        }

    }
    internal class CustomDialogService : DialogService
    {
        public override IDialogReference CreateReference() => new CustomDialogReference(Guid.NewGuid(), this);
    }

    internal class CustomDialogReference : DialogReference
    {
        public bool AllowDismiss { get; set; }

        public CustomDialogReference(Guid dialogInstanceId, IDialogService dialogService) : base(dialogInstanceId, dialogService) { }

        public override bool Dismiss(DialogResult result)
        {
            return AllowDismiss;
        }
    }
}
