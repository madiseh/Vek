// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Http;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NUnit.Framework;
using Vek.Extensions;
using Vek.UnitTests.Dummy;

namespace Vek.UnitTests.Components.FileUpload
{
    [TestFixture]
    public class FileUploadQueueTests : BunitTest
    {
        /// <summary>
        /// Renders the queue table with file names, sizes, progress bars, and cancel actions when ShowQueue is enabled.
        /// </summary>
        [Test]
        public void ShowQueue_RendersFileRowsAndInitialQueuedState()
        {
            var files = new IBrowserFile[]
            {
                new DummyBrowserFile("report.pdf", DateTimeOffset.UnixEpoch, 512, "application/pdf", new byte[512]),
                new DummyBrowserFile("archive.zip", DateTimeOffset.UnixEpoch, 2048, "application/zip", new byte[2048])
            };

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.Files, files));

            var rows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
            rows.Should().HaveCount(2);

            rows[0].QuerySelector(".vek-file-upload-queue-filename").TextContent.Should().Be("report.pdf");
            rows[0].QuerySelector(".vek-file-upload-queue-size").TextContent.Trim().Should().Be("512 B");
            rows[0].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Queued");
            rows[0].QuerySelector("[role='progressbar']").GetAttribute("aria-valuenow").Should().Be("0");
            rows[0].QuerySelector("button.vek-file-upload-queue-cancel").HasAttribute("disabled").Should().BeFalse();

            rows[1].QuerySelector(".vek-file-upload-queue-filename").TextContent.Should().Be("archive.zip");
            rows[1].QuerySelector(".vek-file-upload-queue-size").TextContent.Trim().Should().Be("2 KB");

            comp.Find(".vek-file-upload-queue-summary").TextContent.Trim().Should().Be("0 of 2 uploaded");
            comp.Find("button.vek-file-upload-queue-clear").HasAttribute("disabled").Should().BeTrue();
        }

        /// <summary>
        /// Uploads queued files sequentially, updates individual progress bars, and enables ClearCompleted upon success.
        /// </summary>
        [Test]
        public async Task UploadFilesAsync_InQueueMode_UpdatesProgressAndMarksSuccess()
        {
            var files = new IBrowserFile[]
            {
                new DummyBrowserFile("first.txt", DateTimeOffset.UnixEpoch, 4, "text/plain", [1, 2, 3, 4]),
                new DummyBrowserFile("second.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [5, 6])
            };

            var progressGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var resumeSecondChunk = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.ChunkSizeInBytes, 2)
                .Add(p => p.Files, files));

            var uploadTask = comp.InvokeAsync(() => comp.Instance.UploadFilesAsync(async (chunk, ct) =>
            {
                if (chunk.File.Name == "first.txt" && chunk.ChunkIndex == 1)
                {
                    progressGate.TrySetResult();
                    await resumeSecondChunk.Task.WaitAsync(NUnit.Framework.TestContext.CurrentContext.CancellationToken);
                    ct.ThrowIfCancellationRequested();
                }
            }));

            await progressGate.Task.WaitAsync(NUnit.Framework.TestContext.CurrentContext.CancellationToken);

            comp.WaitForAssertion(() =>
            {
                var rows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
                rows[0].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Uploading");
                rows[0].QuerySelector("[role='progressbar']").GetAttribute("aria-valuenow").Should().Be("50");
                rows[1].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Queued");
            });

            resumeSecondChunk.TrySetResult();
            await uploadTask;

            var finalRows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
            finalRows[0].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Success");
            finalRows[0].QuerySelector("[role='progressbar']").GetAttribute("aria-valuenow").Should().Be("100");
            finalRows[1].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Success");
            finalRows[1].QuerySelector("[role='progressbar']").GetAttribute("aria-valuenow").Should().Be("100");

            comp.Find(".vek-file-upload-queue-summary").TextContent.Trim().Should().Be("2 of 2 uploaded");
            comp.Find("button.vek-file-upload-queue-clear").HasAttribute("disabled").Should().BeFalse();
        }

        /// <summary>
        /// Canceling an active upload aborts its request token, marks the item as Error, and allows subsequent queued files to upload.
        /// </summary>
        [Test]
        public async Task CancelUpload_AbortsActiveItemAndContinuesRemainingQueue()
        {
            var files = new IBrowserFile[]
            {
                new DummyBrowserFile("active.bin", DateTimeOffset.UnixEpoch, 4, "application/octet-stream", [1, 2, 3, 4]),
                new DummyBrowserFile("next.bin", DateTimeOffset.UnixEpoch, 2, "application/octet-stream", [5, 6])
            };

            var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var firstCanceled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.ChunkSizeInBytes, 4)
                .Add(p => p.Files, files));

            var uploadTask = comp.InvokeAsync(() => comp.Instance.UploadFilesAsync(async (chunk, ct) =>
            {
                if (chunk.File.Name == "active.bin")
                {
                    firstStarted.TrySetResult();
                    using var reg = ct.Register(() => firstCanceled.TrySetResult());
                    await firstCanceled.Task.WaitAsync(NUnit.Framework.TestContext.CurrentContext.CancellationToken);
                    ct.ThrowIfCancellationRequested();
                }
            }));

            await firstStarted.Task.WaitAsync(NUnit.Framework.TestContext.CurrentContext.CancellationToken);

            var cancelButton = comp.Find("tbody tr.vek-file-upload-queue-row button.vek-file-upload-queue-cancel");
            await cancelButton.ClickAsync();

            await uploadTask;

            var rows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
            rows[0].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Error");
            rows[0].QuerySelector(".vek-file-upload-queue-error-text").TextContent.Trim().Should().Be("Upload canceled.");
            rows[0].QuerySelector("button.vek-file-upload-queue-retry").Should().NotBeNull();

            rows[1].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Success");
            comp.Find(".vek-file-upload-queue-summary").TextContent.Trim().Should().Be("1 of 2 uploaded");
        }

        /// <summary>
        /// Canceling a queued item before its turn marks it as Error and skips uploading it.
        /// </summary>
        [Test]
        public async Task CancelUpload_OnQueuedItem_SkipsUploadingThatItem()
        {
            var files = new IBrowserFile[]
            {
                new DummyBrowserFile("one.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [1, 2]),
                new DummyBrowserFile("two.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [3, 4])
            };

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.Files, files));

            var secondId = comp.Instance.GetState(x => x.Queue)[1].Id;
            await comp.InvokeAsync(() => comp.Instance.CancelUpload(secondId));

            var uploadedNames = new List<string>();
            await comp.InvokeAsync(() => comp.Instance.UploadFilesAsync((chunk, _) =>
            {
                uploadedNames.Add(chunk.File.Name);
                return Task.CompletedTask;
            }));

            uploadedNames.Should().Equal("one.txt");
            var rows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
            rows[0].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Success");
            rows[1].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Error");
        }

        /// <summary>
        /// Retrying a failed item re-uploads only that item and updates its status to Success.
        /// </summary>
        [Test]
        public async Task RetryUpload_ReuploadsFailedItemOnly()
        {
            var files = new IBrowserFile[]
            {
                new DummyBrowserFile("ok.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [1, 2]),
                new DummyBrowserFile("flaky.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [3, 4])
            };

            var attemptByFile = new Dictionary<string, int>(StringComparer.Ordinal);
            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.MaxChunkRetries, 0)
                .Add(p => p.Files, files));

            await comp.InvokeAsync(() => comp.Instance.UploadFilesAsync((chunk, _) =>
            {
                attemptByFile[chunk.File.Name] = attemptByFile.GetValueOrDefault(chunk.File.Name) + 1;
                if (chunk.File.Name == "flaky.txt" && attemptByFile["flaky.txt"] == 1)
                    throw new HttpRequestException("Bad request.", null, HttpStatusCode.BadRequest);

                return Task.CompletedTask;
            }));

            var rows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
            rows[0].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Success");
            rows[1].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Error");
            rows[1].QuerySelector(".vek-file-upload-queue-error-text").TextContent.Trim().Should().Be("Bad request.");

            await comp.FindAll("tbody tr.vek-file-upload-queue-row button.vek-file-upload-queue-retry")[0].ClickAsync();

            attemptByFile["ok.txt"].Should().Be(1);
            attemptByFile["flaky.txt"].Should().Be(2);

            rows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
            rows[1].QuerySelector(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Success");
            rows[1].QuerySelector("[role='progressbar']").GetAttribute("aria-valuenow").Should().Be("100");
            comp.Find(".vek-file-upload-queue-summary").TextContent.Trim().Should().Be("2 of 2 uploaded");
        }

        /// <summary>
        /// RetryUpload without a registered handler resets a failed item back to Queued.
        /// </summary>
        [Test]
        public async Task RetryUpload_WithoutHandler_ResetsItemToQueued()
        {
            var file = new DummyBrowserFile("manual.txt", DateTimeOffset.UnixEpoch, 4, "text/plain", [1, 2, 3, 4]);
            var item = new FileUploadItem(file, "manual-1")
            {
                Status = FileUploadStatus.Error,
                Progress = 25,
                BytesUploaded = 1,
                ErrorMessage = "Network error"
            };

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.Queue, [item]));

            await comp.InvokeAsync(() => comp.Instance.RetryUpload("manual-1"));

            item.Status.Should().Be(FileUploadStatus.Queued);
            item.Progress.Should().Be(0);
            item.BytesUploaded.Should().Be(0);
            item.ErrorMessage.Should().BeNull();
            comp.Find(".vek-file-upload-queue-status-badge").TextContent.Trim().Should().Be("Queued");
        }

        /// <summary>
        /// ClearCompleted removes only completed items from the queue and synchronizes Files.
        /// </summary>
        [Test]
        public async Task ClearCompleted_RemovesSuccessfulItemsAndKeepsFailedOrQueuedItems()
        {
            IReadOnlyList<IBrowserFile> boundFiles =
            [
                new DummyBrowserFile("done.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [1, 2]),
                new DummyBrowserFile("failed.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [3, 4])
            ];

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.MaxChunkRetries, 0)
                .Add(p => p.Files, boundFiles)
                .Add(p => p.FilesChanged, EventCallback.Factory.Create<IReadOnlyList<IBrowserFile>>(this, value => boundFiles = value)));

            await comp.InvokeAsync(() => comp.Instance.UploadFilesAsync((chunk, _) =>
            {
                if (chunk.File.Name == "failed.txt")
                    throw new InvalidOperationException("Server rejected file.");

                return Task.CompletedTask;
            }));

            await comp.Find("button.vek-file-upload-queue-clear").ClickAsync();

            var rows = comp.FindAll("tbody tr.vek-file-upload-queue-row");
            rows.Should().HaveCount(1);
            rows[0].QuerySelector(".vek-file-upload-queue-filename").TextContent.Should().Be("failed.txt");
            boundFiles.Should().ContainSingle(f => f.Name == "failed.txt");
        }

        /// <summary>
        /// ClearCompleted clears Files to null when all queue entries have succeeded.
        /// </summary>
        [Test]
        public async Task ClearCompleted_WhenAllSucceeded_ClearsQueueAndFiles()
        {
            IReadOnlyList<IBrowserFile> boundFiles =
            [
                new DummyBrowserFile("only.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [1, 2])
            ];

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.Files, boundFiles)
                .Add(p => p.FilesChanged, EventCallback.Factory.Create<IReadOnlyList<IBrowserFile>>(this, value => boundFiles = value)));

            await comp.InvokeAsync(() => comp.Instance.UploadFilesAsync((_, _) => Task.CompletedTask));
            await comp.InvokeAsync(() => comp.Instance.ClearCompleted());

            comp.FindAll("tbody tr.vek-file-upload-queue-row").Should().BeEmpty();
            boundFiles.Should().BeNull();
        }

        /// <summary>
        /// Disabled state disables queue action buttons and ignores user clicks on cancel, retry, and clear completed.
        /// </summary>
        [Test]
        public async Task Disabled_DisablesQueueActionButtons()
        {
            var queued = new FileUploadItem(new DummyBrowserFile("q.txt", DateTimeOffset.UnixEpoch, 10, "text/plain", []), "q1")
            {
                Status = FileUploadStatus.Queued
            };
            var failed = new FileUploadItem(new DummyBrowserFile("f.txt", DateTimeOffset.UnixEpoch, 10, "text/plain", []), "f1")
            {
                Status = FileUploadStatus.Error,
                ErrorMessage = "Failed"
            };
            var succeeded = new FileUploadItem(new DummyBrowserFile("s.txt", DateTimeOffset.UnixEpoch, 10, "text/plain", []), "s1")
            {
                Status = FileUploadStatus.Success,
                Progress = 100
            };

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.Disabled, true)
                .Add(p => p.Queue, [queued, failed, succeeded]));

            comp.Find("button.vek-file-upload-queue-cancel").HasAttribute("disabled").Should().BeTrue();
            comp.Find("button.vek-file-upload-queue-retry").HasAttribute("disabled").Should().BeTrue();
            comp.Find("button.vek-file-upload-queue-clear").HasAttribute("disabled").Should().BeTrue();

            await comp.Find("button.vek-file-upload-queue-cancel").ClickAsync();
            await comp.Find("button.vek-file-upload-queue-retry").ClickAsync();
            await comp.Find("button.vek-file-upload-queue-clear").ClickAsync();

            queued.Status.Should().Be(FileUploadStatus.Queued);
            failed.Status.Should().Be(FileUploadStatus.Error);
            comp.FindAll("tbody tr.vek-file-upload-queue-row").Should().HaveCount(3);
        }

        /// <summary>
        /// AutoUpload starts uploading queued files immediately upon file selection when UploadChunk is configured.
        /// </summary>
        [Test]
        public async Task AutoUpload_UploadsSelectedFilesAutomatically()
        {
            var uploaded = new List<string>();
            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true)
                .Add(p => p.AutoUpload, true)
                .Add(p => p.UploadChunk, (chunk, _) =>
                {
                    uploaded.Add(chunk.File.Name);
                    return Task.CompletedTask;
                }));

            var input = comp.FindComponent<InputFile>();
            await comp.InvokeAsync(() => input.Instance.OnChange.InvokeAsync(new InputFileChangeEventArgs(
            [
                new DummyBrowserFile("auto1.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [1, 2]),
                new DummyBrowserFile("auto2.txt", DateTimeOffset.UnixEpoch, 2, "text/plain", [3, 4])
            ])));

            uploaded.Should().Equal("auto1.txt", "auto2.txt");
            comp.FindAll(".vek-file-upload-queue-status-success").Should().HaveCount(2);
        }

        /// <summary>
        /// Boundary calls for unknown IDs, empty queues, and null FileUploadItem arguments behave safely.
        /// </summary>
        [Test]
        public async Task QueueMethods_HandleBoundaryInputsSafely()
        {
            Action createNull = () => _ = new FileUploadItem(null);
            createNull.Should().Throw<ArgumentNullException>();

            var comp = Context.Render<VekFileUpload<IReadOnlyList<IBrowserFile>>>(parameters => parameters
                .Add(p => p.ShowQueue, true));

            await comp.InvokeAsync(() => comp.Instance.CancelUpload(string.Empty));
            await comp.InvokeAsync(() => comp.Instance.CancelUpload("nonexistent"));
            await comp.InvokeAsync(() => comp.Instance.RetryUpload(string.Empty));
            await comp.InvokeAsync(() => comp.Instance.RetryUpload("nonexistent"));
            await comp.InvokeAsync(() => comp.Instance.ClearCompleted());

            comp.FindAll("tbody tr.vek-file-upload-queue-row").Should().BeEmpty();
        }
    }
}
