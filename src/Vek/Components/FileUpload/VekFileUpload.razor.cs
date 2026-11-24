// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Vek.Resources;
using Vek.State;
using Vek.Utilities;

namespace Vek
{

    /// <summary>
    /// A form component that lets users upload one or more files.
    /// </summary>
    /// <typeparam name="T">Use <see cref="IBrowserFile"/> for a single file or <see cref="IReadOnlyList{IBrowserFile}">IReadOnlyList&lt;IBrowserFile&gt;</see> for multiple files.</typeparam>
    public partial class VekFileUpload<T> : VekFormComponent<T, string>
    {
        private readonly ParameterState<T?> _filesState;
        private readonly ParameterState<bool> _draggingState;
        private readonly ParameterState<bool> _showQueueState;
        private readonly ParameterState<bool> _autoUploadState;
        private readonly ParameterState<IReadOnlyList<FileUploadItem>?> _queueState;
        private readonly CancellationTokenSource _lifetimeCancellation = new();
        private readonly CancellationToken _lifetimeToken;

        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        [Inject]
        private InternalVekLocalizer Localizer { get; set; } = null!;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public VekFileUpload()
        {
            _lifetimeToken = _lifetimeCancellation.Token;

            using var registerScope = CreateRegisterScope();
            _filesState = registerScope.RegisterParameter<T?>(nameof(Files))
                .WithParameter(() => Files)
                .WithEventCallback(() => FilesChanged)
                .WithChangeHandler(OnFilesParameterChangedAsync);
            _draggingState = registerScope.RegisterParameter<bool>(nameof(Dragging))
                .WithParameter(() => Dragging)
                .WithEventCallback(() => DraggingChanged);
            _showQueueState = registerScope.RegisterParameter<bool>(nameof(ShowQueue))
                .WithParameter(() => ShowQueue);
            _autoUploadState = registerScope.RegisterParameter<bool>(nameof(AutoUpload))
                .WithParameter(() => AutoUpload);
            _queueState = registerScope.RegisterParameter<IReadOnlyList<FileUploadItem>?>(nameof(Queue))
                .WithParameter(() => Queue)
                .WithEventCallback(() => QueueChanged)
                .WithChangeHandler(OnQueueParameterChangedAsync);
        }

        private readonly string _id = Identifier.Create();
        private readonly List<string> _validationErrors = [];
        private readonly Dictionary<IBrowserFile, string> _imagePreviewUrls = new(ReferenceEqualityComparer.Instance);
        private readonly List<FileUploadItem> _queueItems = [];
        private readonly Dictionary<string, CancellationTokenSource> _activeUploadTokens = new(StringComparer.Ordinal);
        private readonly HashSet<string> _canceledItemIds = new(StringComparer.Ordinal);
        private Func<FileUploadChunkEventArgs, CancellationToken, Task>? _lastUploadChunkHandler;
        private bool _isSyncingQueue;
        private bool _hasExternalQueue;

        protected string Classname =>
            new CssBuilder("vek-file-upload")
                .AddClass(Class)
                .Build();

        protected string DragClass =>
            new CssBuilder("vek-file-upload-dragarea")
                .AddClass("vek-file-upload-dragarea-clickable", !GetDisabledState())
                .AddClass("vek-border-primary", _draggingState.Value)
                .Build();

        protected string InputClasses =>
            new CssBuilder(InputClass)
                .AddClass("vek-file-upload-dragover", DragAndDrop && (Hidden || CustomContent == null))
                .Build();

        /// <summary>
        /// The selected file or files.
        /// </summary>
        /// <remarks>
        /// When <c>T</c> is <see cref="IBrowserFile" />, a single file is provided.<br />
        /// When <c>T</c> is <see cref="IReadOnlyList{IBrowserFile}">IReadOnlyList&lt;IBrowserFile&gt;</see>, multiple files are provided.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public T? Files { get; set; }

        /// <summary>
        /// Occurs when <see cref="Files"/> has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<T?> FilesChanged { get; set; }

        /// <summary>
        /// Occurs when <see cref="Dragging"/> has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<bool> DraggingChanged { get; set; }

        /// <summary>
        /// Occurs when the user selects or drops files.
        /// </summary>
        /// <remarks>
        /// Not raised when <see cref="SuppressOnChangeWhenInvalid"/> is <c>true</c> and validation fails.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<InputFileChangeEventArgs> OnFilesChanged { get; set; }

        /// <summary>
        /// The maximum number of bytes read for each upload chunk.
        /// </summary>
        /// <remarks>
        /// Defaults to 2 MB.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public int ChunkSizeInBytes { get; set; } = 2 * 1024 * 1024;

        /// <summary>
        /// The number of retries for a chunk when its upload handler reports a transient failure.
        /// </summary>
        /// <remarks>
        /// Defaults to 3 retries. Set to 0 to disable retries.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public int MaxChunkRetries { get; set; } = 3;

        /// <summary>
        /// Occurs after an upload chunk succeeds and progress advances.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<FileUploadProgressEventArgs> OnUploadProgress { get; set; }

        /// <summary>
        /// Displays the interactive upload queue table with individual progress bars and cancel/retry actions.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. Ignored when <see cref="SelectedTemplate"/> is supplied.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FileUpload.Appearance)]
        public bool ShowQueue { get; set; }

        /// <summary>
        /// Starts uploading newly queued files automatically after selection when <see cref="UploadChunk"/> is set.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public bool AutoUpload { get; set; }

        /// <summary>
        /// Uploads one chunk to the consumer's destination when running queue uploads or retrying failed items.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public Func<FileUploadChunkEventArgs, CancellationToken, Task>? UploadChunk { get; set; }

        /// <summary>
        /// The tracked upload queue items for the selected files.
        /// </summary>
        [Parameter, ParameterState]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public IReadOnlyList<FileUploadItem>? Queue { get; set; }

        /// <summary>
        /// Occurs when <see cref="Queue"/> has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<IReadOnlyList<FileUploadItem>?> QueueChanged { get; set; }

        /// <summary>
        /// Occurs when a drag operation enters this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<DragEventArgs> OnDragEnter { get; set; }

        /// <summary>
        /// Occurs when files are dropped onto this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<DragEventArgs> OnDrop { get; set; }

        /// <summary>
        /// Occurs when a drag operation leaves this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<DragEventArgs> OnDragLeave { get; set; }

        /// <summary>
        /// Occurs when a drag operation ends for this component.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public EventCallback<DragEventArgs> OnDragEnd { get; set; }

        /// <summary>
        /// Appends new files to the existing selection.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  This applies when <c>T</c> is <see cref="IReadOnlyList{IBrowserFile}">IReadOnlyList&lt;IBrowserFile&gt;</see>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public bool AppendMultipleFiles { get; set; }

        /// <summary>
        /// The custom content used to render the upload UI.
        /// </summary>
        /// <remarks>
        /// The context is the current <see cref="VekFileUpload{T}"/> instance and can be used to call <see cref="OpenFilePickerAsync"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public RenderFragment<VekFileUpload<T>>? CustomContent { get; set; }

        /// <summary>
        /// The template used to render selected files.
        /// </summary>
        /// <remarks>
        /// When <c>null</c>, a default chip list is shown.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Appearance)]
        public RenderFragment<T?>? SelectedTemplate { get; set; }

        /// <summary>
        /// Prevents raising <see cref="OnFilesChanged"/> when validation fails during an upload.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public bool SuppressOnChangeWhenInvalid { get; set; }

        /// <summary>
        /// The accepted file types for the file picker.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c> for any file type.  Multiple file types must be separated by commas (e.g. <c>".png,.jpg"</c>).
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public string? Accept { get; set; }

        /// <summary>
        /// Hides the internal <see cref="InputFile"/> element when full-surface drag and drop is disabled.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. When <see cref="DragAndDrop"/> is <c>true</c>, the input remains present for full-surface dropping.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Appearance)]
        public bool Hidden { get; set; } = true;

        /// <summary>
        /// Enables full-surface file dropping inside the component.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. Files can also be selected with the browse button; multiple files are supported when <c>T</c> is <see cref="IReadOnlyList{IBrowserFile}">IReadOnlyList&lt;IBrowserFile&gt;</see>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public bool DragAndDrop { get; set; }

        /// <summary>
        /// Indicates whether a drag operation is active.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, the internal <see cref="InputFile"/> element is shown to capture the drop, which temporarily overrides <see cref="Hidden"/>.  When <c>false</c>, visibility follows <see cref="Hidden"/> again.  Drop-related events reset it to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public bool Dragging { get; set; }

        /// <summary>
        /// The CSS classes applied to the internal <see cref="InputFile"/> element.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  These classes apply when <see cref="Hidden"/> is <c>false</c>.  Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Appearance)]
        public string? InputClass { get; set; }

        /// <summary>
        /// The CSS styles applied to the internal <see cref="InputFile"/> element.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  These styles apply when <see cref="Hidden"/> is <c>false</c>.  Prefer <see cref="InputClass"/>.
        /// </remarks>
        [Obsolete("Prefer the InputClass property with CSS https://github.com/madiseh/vek/issues/12047")]
        [Parameter]
        [Category(CategoryTypes.FileUpload.Appearance)]
        public string? InputStyle { get; set; }

        /// <summary>
        /// The maximum number of files retrieved per selection.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>10</c>.  This does not limit the total number of files; enforce overall limits in <see cref="FilesChanged"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public int MaximumFileCount { get; set; } = 10;

        /// <summary>
        /// The maximum allowed file size in bytes.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c> (no limit).  Files exceeding this limit are rejected and a validation error is added.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public long? MaxFileSize { get; set; }

        /// <summary>
        /// The file name extensions accepted by this upload control.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c> (no extension restriction). Values may include or omit the leading period.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public IReadOnlyList<string>? AllowedExtensions { get; set; }

        /// <summary>
        /// The MIME types accepted by this upload control.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c> (no MIME type restriction).
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public IReadOnlyList<string>? AllowedMimeTypes { get; set; }

        /// <summary>
        /// Prevents the user from interacting with this component.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FileUpload.Behavior)]
        public bool Disabled { get; set; }

        [CascadingParameter(Name = "ParentDisabled")]
        private bool ParentDisabled { get; set; }

        [CascadingParameter(Name = "ParentReadOnly")]
        private bool ParentReadOnly { get; set; }

        /// <summary>
        /// Returns the filenames for the selected files.
        /// </summary>
        /// <remarks>
        /// When <c>T</c> is <see cref="IBrowserFile" />, a single filename is returned.<br />
        /// When <c>T</c> is <see cref="IReadOnlyList{IBrowserFile}">IReadOnlyList&lt;IBrowserFile&gt;</see>, multiple filenames are returned.
        /// </remarks>
        public IReadOnlyList<string> GetFilenames()
        {
            if (EqualityComparer<T>.Default.Equals(_filesState.Value, default))
            {
                return [];
            }
            return _filesState.Value switch
            {
                IBrowserFile singleFile => [singleFile.Name],
                IReadOnlyList<IBrowserFile> fileList => fileList.Select(f => f.Name).ToList(),
                _ => []
            };
        }

        protected bool GetDisabledState() => Disabled || ParentDisabled || ParentReadOnly;

        private async Task OnDragResetAsync()
        {
            await _draggingState.SetValueAsync(false);
        }

        private async Task OnDropAsync(DragEventArgs args)
        {
            await OnDragResetAsync();
            await OnDrop.InvokeAsync(args);
        }

        private async Task OnDragLeaveAsync(DragEventArgs args)
        {
            await OnDragResetAsync();
            await OnDragLeave.InvokeAsync(args);
        }

        private async Task OnDragEndAsync(DragEventArgs args)
        {
            await OnDragResetAsync();
            await OnDragEnd.InvokeAsync(args);
        }

        private Task OnDragAreaClickAsync()
        {
            if (GetDisabledState())
            {
                return Task.CompletedTask;
            }

            return OpenFilePickerAsync();
        }

        private async Task OnDragEnterAsync(DragEventArgs args)
        {
            if (GetDisabledState())
            {
                return;
            }

            await _draggingState.SetValueAsync(true);
            await OnDragEnter.InvokeAsync(args);
        }

        private Task OnFileChipCloseAsync(string filename)
        {
            if (GetDisabledState())
            {
                return Task.CompletedTask;
            }

            return RemoveFileAsync(filename);
        }

        private int _numberOfActiveFileInputs = 1;
        private string? GetInputClass(int fileInputIndex) => fileInputIndex == _numberOfActiveFileInputs
            ? InputClasses
            : $"{InputClasses} d-none";
        private string GetInputId(int fileInputIndex) => $"{_id}-{fileInputIndex}";
        private string GetActiveInputId() => $"{_id}-{_numberOfActiveFileInputs}";

        /// <summary>
        /// Removes all files with the specified name from <see cref="Files"/>.
        /// </summary>
        /// <param name="filename">The name of the file(s) to remove.</param>
        /// <returns>
        /// <c>true</c> if one or more files were removed; otherwise, <c>false</c>
        /// if no file with the specified name exists in the current selection.
        /// </returns>
        public async Task<bool> RemoveFileAsync(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                Logger.LogWarning("Attempted to remove a file with an invalid filename.");
                return false;
            }

            var current = _filesState.Value;

            switch (current)
            {
                case IBrowserFile singleFile when singleFile.Name == filename:
                    // Single selection with matching name: clear selection
                    await _filesState.SetValueAsync(default);
                    await RevokeImagePreviewsForFilesAsync([singleFile]);
                    await SyncQueueFromFilesAsync(default);
                    return true;

                case IReadOnlyList<IBrowserFile> fileList:
                    {
                        var updatedList = fileList.Where(f => f.Name != filename).ToList();
                        if (updatedList.Count == fileList.Count)
                        {
                            Logger.LogDebug("File '{Filename}' not found in the current selection.", filename);
                            return false;
                        }

                        var removedFiles = fileList.Where(f => f.Name == filename).ToArray();
                        await RevokeImagePreviewsForFilesAsync(removedFiles);

                        T? nextValue = updatedList.Count == 0
                            ? default
                            : (T)(object)updatedList;
                        await _filesState.SetValueAsync(nextValue);
                        await SyncQueueFromFilesAsync(nextValue);

                        return true;
                    }

                case null:
                    Logger.LogDebug("No files are currently selected.");
                    return false;

                default:
                    Logger.LogWarning(
                        "File removal by name is not supported for the current files type '{Type}'.",
                        current.GetType().FullName);
                    return false;
            }
        }

        /// <summary>
        /// Removes the specified file instance from <see cref="Files"/>.
        /// </summary>
        /// <param name="file">The file instance to remove.</param>
        /// <returns>
        /// <c>true</c> if the specified file instance was removed; otherwise, <c>false</c>.
        /// </returns>
        public async Task<bool> RemoveFileAsync(IBrowserFile file)
        {
            if (file is null)
            {
                Logger.LogWarning("Attempted to remove a null file.");
                return false;
            }

            var current = _filesState.Value;

            switch (current)
            {
                case IBrowserFile singleFile when ReferenceEquals(singleFile, file):
                    // Single selection with the same instance: clear selection
                    await _filesState.SetValueAsync(default);
                    await RevokeImagePreviewsForFilesAsync([singleFile]);
                    await SyncQueueFromFilesAsync(default);
                    return true;

                case IReadOnlyList<IBrowserFile> fileList:
                    {
                        // Remove this specific instance (reference-based via List.Remove)
                        var updatedList = fileList.ToList();
                        if (!updatedList.Remove(file))
                        {
                            Logger.LogDebug("The specified file instance was not found in the current selection.");
                            return false;
                        }

                        await RevokeImagePreviewsForFilesAsync([file]);

                        T? nextValue = updatedList.Count == 0
                            ? default
                            : (T)(object)updatedList;
                        await _filesState.SetValueAsync(nextValue);
                        await SyncQueueFromFilesAsync(nextValue);

                        return true;
                    }

                case null:
                    Logger.LogDebug("No files are currently selected.");
                    return false;

                default:
                    Logger.LogWarning(
                        "File removal by instance is not supported for the current files type '{Type}'.",
                        current.GetType().FullName);
                    return false;
            }
        }


        /// <summary>
        /// Clears the selected files and resets the internal file inputs.
        /// </summary>
        /// <remarks>
        /// This also removes validation errors produced by file size checks.
        /// </remarks>
        public async Task ClearAsync()
        {
            CancelAndClearActiveUploads();
            ValidationErrors.RemoveAll(_validationErrors.Contains);

            _validationErrors.Clear();
            await RevokeImagePreviewsAsync();
            _numberOfActiveFileInputs = 1;

            await NotifyValueChangedAsync(default);
            await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekInput.resetValue", GetActiveInputId());
        }

        /// <summary>
        /// Uploads the currently selected files one chunk at a time using <see cref="UploadChunk"/> or the most recent upload handler.
        /// </summary>
        /// <param name="cancellationToken">Cancels the upload operation.</param>
        public Task UploadFilesAsync(CancellationToken cancellationToken = default)
        {
            var handler = UploadChunk ?? _lastUploadChunkHandler
                ?? throw new InvalidOperationException($"Specify an upload handler via {nameof(UploadChunk)} or pass one to {nameof(UploadFilesAsync)}.");

            return UploadFilesAsync(handler, cancellationToken);
        }

        /// <summary>
        /// Uploads the currently selected files one chunk at a time through the supplied handler.
        /// </summary>
        /// <param name="uploadChunk">Uploads one chunk to the consumer's chosen destination.</param>
        /// <param name="cancellationToken">Cancels the upload operation.</param>
        /// <remarks>
        /// Statusless HTTP connection failures, HTTP 408, 425, 429, 500, 502, 503, 504, I/O failures, and timeouts retry the current chunk up to <see cref="MaxChunkRetries"/> times.
        /// Other HTTP status codes and cancellation exceptions are not retried.
        /// Handlers should use the file name and chunk index to make retries idempotent.
        /// The component cancels the token when disposed, so handlers should pass it to their I/O operations.
        /// </remarks>
        public async Task UploadFilesAsync(
            Func<FileUploadChunkEventArgs, CancellationToken, Task> uploadChunk,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(uploadChunk);

            if (ChunkSizeInBytes <= 0)
                throw new InvalidOperationException($"{nameof(ChunkSizeInBytes)} must be greater than zero.");

            if (MaxChunkRetries < 0)
                throw new InvalidOperationException($"{nameof(MaxChunkRetries)} cannot be negative.");

            _lastUploadChunkHandler = uploadChunk;

            if (_queueItems.Count == 0 && GetSelectedFiles().Count > 0)
                await SyncQueueFromFilesAsync(_filesState.Value);

            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetimeToken);
            var isQueueMode = _showQueueState.Value || _hasExternalQueue;
            var itemsToUpload = isQueueMode
                ? _queueItems.Where(item => item.Status == FileUploadStatus.Queued).ToArray()
                : _queueItems.ToArray();

            foreach (var item in itemsToUpload)
            {
                linkedCancellation.Token.ThrowIfCancellationRequested();

                if (isQueueMode && item.Status != FileUploadStatus.Queued)
                    continue;

                await UploadItemAsync(item, uploadChunk, linkedCancellation.Token, throwOnFailure: !isQueueMode);
            }
        }

        /// <summary>
        /// Cancels a queued or active upload for the specified file entry.
        /// </summary>
        /// <param name="fileId">The identifier of the <see cref="FileUploadItem"/> to cancel.</param>
        public async Task CancelUpload(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return;

            var item = _queueItems.FirstOrDefault(entry => string.Equals(entry.Id, fileId, StringComparison.Ordinal));
            if (item is null || item.Status is not (FileUploadStatus.Queued or FileUploadStatus.Uploading))
                return;

            _canceledItemIds.Add(fileId);

            if (_activeUploadTokens.TryGetValue(fileId, out var itemCts))
            {
                try
                {
                    await itemCts.CancelAsync();
                }
                catch (ObjectDisposedException)
                {
                    // Upload already completed and disposed its token source.
                }
            }

            item.Status = FileUploadStatus.Error;
            item.ErrorMessage = Localizer[LanguageResource.VekFileUpload_UploadCanceled];

            await NotifyQueueChangedAsync();
            if (!_lifetimeToken.IsCancellationRequested)
                await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Retries a failed or canceled upload item in the queue.
        /// </summary>
        /// <param name="fileId">The identifier of the <see cref="FileUploadItem"/> to retry.</param>
        public async Task RetryUpload(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
                return;

            var item = _queueItems.FirstOrDefault(entry => string.Equals(entry.Id, fileId, StringComparison.Ordinal));
            if (item is null || item.Status != FileUploadStatus.Error)
                return;

            item.Status = FileUploadStatus.Queued;
            item.Progress = 0;
            item.BytesUploaded = 0;
            item.ErrorMessage = null;

            var handler = UploadChunk ?? _lastUploadChunkHandler;
            if (handler is null)
            {
                await NotifyQueueChangedAsync();
                if (!_lifetimeToken.IsCancellationRequested)
                    await InvokeAsync(StateHasChanged);
                return;
            }

            if (ChunkSizeInBytes <= 0)
                throw new InvalidOperationException($"{nameof(ChunkSizeInBytes)} must be greater than zero.");

            if (MaxChunkRetries < 0)
                throw new InvalidOperationException($"{nameof(MaxChunkRetries)} cannot be negative.");

            await UploadItemAsync(item, handler, _lifetimeToken, throwOnFailure: false);
        }

        /// <summary>
        /// Removes all successfully uploaded entries from the queue and current selection.
        /// </summary>
        public async Task ClearCompleted()
        {
            var completedItems = _queueItems
                .Where(item => item.Status == FileUploadStatus.Success)
                .ToArray();

            if (completedItems.Length == 0)
                return;

            foreach (var item in completedItems)
            {
                _queueItems.Remove(item);
            }

            await RevokeImagePreviewsForFilesAsync(completedItems.Select(item => item.File));

            _isSyncingQueue = true;
            try
            {
                await UpdateFilesStateFromQueueAsync();
                await NotifyQueueChangedAsync();
            }
            finally
            {
                _isSyncingQueue = false;
            }

            if (!_lifetimeToken.IsCancellationRequested)
                await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Opens the native file picker for the active input.
        /// </summary>
        public async Task OpenFilePickerAsync()
            => await JsRuntime.InvokeVoidAsyncWithErrorHandling("vekFileUpload.openFilePicker", GetActiveInputId());

        protected override async ValueTask DisposeAsyncCore()
        {
            _lifetimeCancellation.Cancel();
            DisposeActiveUploadTokens();
            await RevokeImagePreviewsAsync();
            _lifetimeCancellation.Dispose();
            await base.DisposeAsyncCore();
        }

        private async Task OnChangeAsync(InputFileChangeEventArgs args)
        {
            _numberOfActiveFileInputs++;

            if (GetDisabledState())
                return;

            await ProcessFileChangeAsync(args);
        }

        private async Task ProcessFileChangeAsync(InputFileChangeEventArgs args)
        {
            T? value;
            ValidationErrors.RemoveAll(_validationErrors.Contains);
            _validationErrors.Clear();

            if (args.FileCount > MaximumFileCount)
            {
                // Notify the consumer about the exceeded file count
                _validationErrors.Add(Localizer[LanguageResource.VekFileUpload_MaximumFileCountExceeded, args.FileCount, MaximumFileCount]);
                await RevokeImagePreviewsAsync();
                await NotifyValueChangedAsync(default); // Reset the value to indicate no valid files were processed
                return;
            }

            if (typeof(T) == typeof(IReadOnlyList<IBrowserFile>))
            {
                value = (T?)(object)ProcessMultipleFiles(args.GetMultipleFiles(MaximumFileCount));
            }
            else if (typeof(T) == typeof(IBrowserFile))
            {
                value = (T?)ProcessSingleFile(args.FileCount == 1 ? args.File : null);
            }
            else
            {
                return;
            }

            await UpdateImagePreviewsAsync(args, value);

            await NotifyValueChangedAsync(value);

            if (!ErrorState.Value || !SuppressOnChangeWhenInvalid)
                await OnFilesChanged.InvokeAsync(args);

            if (_autoUploadState.Value &&
                (UploadChunk ?? _lastUploadChunkHandler) is { } autoUploadHandler &&
                _queueItems.Any(item => item.Status == FileUploadStatus.Queued))
            {
                await UploadFilesAsync(autoUploadHandler);
            }
        }

        private IReadOnlyList<IBrowserFile> GetSelectedFiles()
        {
            return _filesState.Value switch
            {
                IBrowserFile file => [file],
                IReadOnlyList<IBrowserFile> files => files,
                null => [],
                _ => throw new InvalidOperationException("File upload supports IBrowserFile or IReadOnlyList<IBrowserFile> values.")
            };
        }

        private IReadOnlyList<IBrowserFile> ProcessMultipleFiles(IReadOnlyCollection<IBrowserFile> files)
        {
            var validFiles = new List<IBrowserFile>();

            foreach (var file in files)
            {
                if (ValidateFile(file))
                {
                    validFiles.Add(file);
                }
            }

            var newFiles = validFiles.AsReadOnly();

            if (AppendMultipleFiles && _filesState.Value is IReadOnlyList<IBrowserFile> oldFiles)
                return oldFiles.Concat(newFiles).ToList().AsReadOnly();

            return newFiles;
        }

        private IBrowserFile? ProcessSingleFile(IBrowserFile? file)
        {
            if (file == null)
                return null;

            if (!ValidateFile(file))
                return null;

            return file;
        }

        private bool ValidateFile(IBrowserFile file)
        {
            var failure = FileUploadValidator.Validate(file, MaxFileSize, AllowedExtensions, AllowedMimeTypes);
            if (failure == FileUploadValidationFailure.None)
                return true;

            var message = failure switch
            {
                FileUploadValidationFailure.FileSize => Localizer[LanguageResource.VekFileUpload_FileSizeError, file.Name, MaxFileSize!.Value.ToString()],
                FileUploadValidationFailure.Extension => Localizer[LanguageResource.VekFileUpload_FileExtensionError, file.Name],
                FileUploadValidationFailure.MimeType => Localizer[LanguageResource.VekFileUpload_FileTypeError, file.Name],
                _ => string.Empty
            };
            _validationErrors.Add(message);
            return false;
        }

        private async Task UpdateImagePreviewsAsync(InputFileChangeEventArgs args, T? value)
        {
            if (!AppendMultipleFiles || value is not IReadOnlyList<IBrowserFile>)
                await RevokeImagePreviewsAsync();

            var selectedFiles = args.GetMultipleFiles(MaximumFileCount);
            var acceptedFiles = GetFilesFromValue(value);
            var previewIndexes = selectedFiles
                .Select((file, index) => (file, index))
                .Where(item => acceptedFiles.Contains(item.file, ReferenceEqualityComparer.Instance)
                    && item.file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.index)
                .ToArray();

            if (previewIndexes.Length == 0)
                return;

            string[] urls;
            try
            {
                urls = await JsRuntime.InvokeAsync<string[]>(
                    "vekFileUpload.createImagePreviews",
                    _lifetimeToken,
                    GetInputId(_numberOfActiveFileInputs - 1),
                    previewIndexes);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            catch (JSException)
            {
                return;
            }

            if (urls is null)
                return;

            for (var index = 0; index < previewIndexes.Length && index < urls.Length; index++)
            {
                var fileIndex = previewIndexes[index];
                if (fileIndex < 0 || fileIndex >= selectedFiles.Count || string.IsNullOrEmpty(urls[index]))
                    continue;
                _imagePreviewUrls[selectedFiles[fileIndex]] = urls[index];
            }
        }

        private static IReadOnlyList<IBrowserFile> GetFilesFromValue(T? value)
        {
            return value switch
            {
                IBrowserFile file => [file],
                IReadOnlyList<IBrowserFile> files => files,
                _ => []
            };
        }

        private async Task RevokeImagePreviewsAsync()
        {
            if (_imagePreviewUrls.Count == 0)
                return;

            var urls = _imagePreviewUrls.Values.ToArray();
            _imagePreviewUrls.Clear();
            try
            {
                await JsRuntime.InvokeVoidAsync("vekFileUpload.revokeImagePreviews", urls);
            }
            catch (InvalidOperationException)
            {
                // The component can be disposed during server-side prerendering, before JS interop is available.
            }
            catch (JSDisconnectedException)
            {
                // A server circuit can disconnect before the component is disposed.
            }
            catch (JSException)
            {
                // The object URLs are scoped to this browser document and disappear when it is closed.
            }
        }

        private string? GetImagePreviewUrl(IBrowserFile file)
            => _imagePreviewUrls.GetValueOrDefault(file);

        private async Task RevokeImagePreviewsForFilesAsync(IEnumerable<IBrowserFile> files)
        {
            var urls = files
                .Where(_imagePreviewUrls.ContainsKey)
                .Select(file =>
                {
                    var url = _imagePreviewUrls[file];
                    _imagePreviewUrls.Remove(file);
                    return url;
                })
                .ToArray();

            if (urls.Length == 0)
                return;

            try
            {
                await JsRuntime.InvokeVoidAsync("vekFileUpload.revokeImagePreviews", urls);
            }
            catch (InvalidOperationException)
            {
                // The component can be disposed during server-side prerendering, before JS interop is available.
            }
            catch (JSDisconnectedException)
            {
                // A server circuit can disconnect before the file is removed.
            }
            catch (JSException)
            {
                // The object URLs are scoped to this browser document and disappear when it is closed.
            }
        }

        protected override void OnInitialized()
        {
            if (!(typeof(T) == typeof(IReadOnlyList<IBrowserFile>) || typeof(T) == typeof(IBrowserFile)))
            {
                Logger.LogWarning("T must be of type {Type1} or {Type2}", typeof(IReadOnlyList<IBrowserFile>), typeof(IBrowserFile));
            }

            base.OnInitialized();
        }

        private async Task NotifyValueChangedAsync(T? value)
        {
            Touched = true;
            await _filesState.SetValueAsync(value);
            await SyncQueueFromFilesAsync(value);
            await BeginValidateAsync();
            FieldChanged(value);
        }

        private async Task OnFilesParameterChangedAsync(ParameterChangedEventArgs<T?> args)
        {
            if (_isSyncingQueue)
                return;

            await SyncQueueFromFilesAsync(args.Value);
        }

        private async Task OnQueueParameterChangedAsync(ParameterChangedEventArgs<IReadOnlyList<FileUploadItem>?> args)
        {
            if (_isSyncingQueue)
                return;

            _hasExternalQueue = args.Value is not null;
            var incoming = args.Value ?? [];
            if (_queueItems.SequenceEqual(incoming))
                return;

            var incomingIds = incoming.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var removed in _queueItems.Where(item => !incomingIds.Contains(item.Id)).ToArray())
            {
                CancelActiveUploadToken(removed.Id);
            }

            _queueItems.Clear();
            _queueItems.AddRange(incoming);

            if (!args.ParameterView.TryGetValue<T?>(nameof(Files), out _))
            {
                _isSyncingQueue = true;
                try
                {
                    await UpdateFilesStateFromQueueAsync();
                }
                finally
                {
                    _isSyncingQueue = false;
                }
            }
        }

        private async Task SyncQueueFromFilesAsync(T? value)
        {
            if (_isSyncingQueue)
                return;

            _isSyncingQueue = true;
            try
            {
                var files = GetFilesFromValue(value);
                var existingByFile = new Dictionary<IBrowserFile, FileUploadItem>(ReferenceEqualityComparer.Instance);
                foreach (var item in _queueItems)
                {
                    existingByFile.TryAdd(item.File, item);
                }

                var retainedSet = new HashSet<IBrowserFile>(files, ReferenceEqualityComparer.Instance);
                foreach (var removed in _queueItems.Where(item => !retainedSet.Contains(item.File)).ToArray())
                {
                    CancelActiveUploadToken(removed.Id);
                }

                _queueItems.Clear();
                foreach (var file in files)
                {
                    _queueItems.Add(existingByFile.TryGetValue(file, out var existing)
                        ? existing
                        : new FileUploadItem(file));
                }

                await NotifyQueueChangedAsync();
            }
            finally
            {
                _isSyncingQueue = false;
            }
        }

        private Task UpdateFilesStateFromQueueAsync()
        {
            if (typeof(T) == typeof(IReadOnlyList<IBrowserFile>))
            {
                var nextList = _queueItems.Count == 0
                    ? default
                    : (T?)(object)_queueItems.Select(item => item.File).ToList().AsReadOnly();
                return _filesState.SetValueAsync(nextList);
            }

            if (typeof(T) == typeof(IBrowserFile))
            {
                var nextFile = _queueItems.Count == 0
                    ? default
                    : (T?)(object)_queueItems[0].File;
                return _filesState.SetValueAsync(nextFile);
            }

            return Task.CompletedTask;
        }

        private Task NotifyQueueChangedAsync()
        {
            IReadOnlyList<FileUploadItem> snapshot = _queueItems.Count == 0 ? [] : _queueItems.ToArray();
            return _queueState.SetValueAsync(snapshot);
        }

        private async Task UploadItemAsync(
            FileUploadItem item,
            Func<FileUploadChunkEventArgs, CancellationToken, Task> uploadChunk,
            CancellationToken outerToken,
            bool throwOnFailure)
        {
            if (item.Status == FileUploadStatus.Uploading || _activeUploadTokens.ContainsKey(item.Id))
                return;

            using var itemCts = CancellationTokenSource.CreateLinkedTokenSource(outerToken, _lifetimeToken);
            _activeUploadTokens[item.Id] = itemCts;
            _canceledItemIds.Remove(item.Id);

            item.Status = FileUploadStatus.Uploading;
            item.Progress = 0;
            item.BytesUploaded = 0;
            item.ErrorMessage = null;
            if (!_lifetimeToken.IsCancellationRequested)
                await InvokeAsync(StateHasChanged);

            try
            {
                await FileUploadChunkManager.UploadAsync(
                    item.File,
                    ChunkSizeInBytes,
                    MaxChunkRetries,
                    uploadChunk,
                    async progress =>
                    {
                        item.BytesUploaded = progress.BytesUploaded;
                        item.Progress = progress.PercentComplete;
                        if (!_lifetimeToken.IsCancellationRequested)
                        {
                            await InvokeAsync(async () =>
                            {
                                await OnUploadProgress.InvokeAsync(progress);
                                StateHasChanged();
                            });
                        }
                    },
                    itemCts.Token);

                item.Status = FileUploadStatus.Success;
                item.Progress = 100;
                item.BytesUploaded = item.Size;
                item.ErrorMessage = null;
            }
            catch (OperationCanceledException)
            {
                var canceledIndividually = _canceledItemIds.Remove(item.Id);
                item.Status = FileUploadStatus.Error;
                item.ErrorMessage = Localizer[LanguageResource.VekFileUpload_UploadCanceled];

                if (!canceledIndividually && (throwOnFailure || outerToken.IsCancellationRequested || _lifetimeToken.IsCancellationRequested))
                    throw;
            }
            catch (Exception exception)
            {
                item.Status = FileUploadStatus.Error;
                item.ErrorMessage = exception.Message;

                if (throwOnFailure)
                    throw;
            }
            finally
            {
                _activeUploadTokens.Remove(item.Id);
                await NotifyQueueChangedAsync();
                if (!_lifetimeToken.IsCancellationRequested)
                    await InvokeAsync(StateHasChanged);
            }
        }

        private void CancelActiveUploadToken(string fileId)
        {
            _canceledItemIds.Add(fileId);
            if (!_activeUploadTokens.Remove(fileId, out var cts))
                return;

            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Upload already completed and disposed its token source.
            }
        }

        private void CancelAndClearActiveUploads()
        {
            foreach (var (fileId, cts) in _activeUploadTokens.ToArray())
            {
                _canceledItemIds.Add(fileId);
                try
                {
                    cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Upload already completed and disposed its token source.
                }
            }

            _activeUploadTokens.Clear();
        }

        private void DisposeActiveUploadTokens()
        {
            foreach (var cts in _activeUploadTokens.Values)
            {
                cts.Dispose();
            }

            _activeUploadTokens.Clear();
            _canceledItemIds.Clear();
        }

        private Task OnCancelUploadClickAsync(string fileId)
            => GetDisabledState() ? Task.CompletedTask : CancelUpload(fileId);

        private Task OnRetryUploadClickAsync(string fileId)
            => GetDisabledState() ? Task.CompletedTask : RetryUpload(fileId);

        private Task OnClearCompletedClickAsync()
            => GetDisabledState() ? Task.CompletedTask : ClearCompleted();

        private bool HasCompletedItems()
            => _queueItems.Any(item => item.Status == FileUploadStatus.Success);

        private string GetQueueSummaryText()
        {
            var completedCount = _queueItems.Count(item => item.Status == FileUploadStatus.Success);
            return Localizer[LanguageResource.VekFileUpload_QueueSummary, completedCount, _queueItems.Count];
        }

        private static string GetQueueRowClass(FileUploadItem item)
            => new CssBuilder("vek-file-upload-queue-row")
                .AddClass($"vek-file-upload-queue-row-{GetStatusModifier(item.Status)}")
                .Build();

        private static string GetProgressBarClass(FileUploadItem item)
            => new CssBuilder("vek-file-upload-queue-progress")
                .AddClass($"vek-file-upload-queue-progress-{GetStatusModifier(item.Status)}")
                .Build();

        private static string GetStatusBadgeClass(FileUploadItem item)
            => new CssBuilder("vek-file-upload-queue-status-badge")
                .AddClass($"vek-file-upload-queue-status-{GetStatusModifier(item.Status)}")
                .Build();

        private static string GetProgressBarFillStyle(FileUploadItem item)
        {
            var percent = Math.Clamp(item.Progress, 0d, 100d);
            return StyleBuilder.Default("width", $"{percent.ToString("0.##", CultureInfo.InvariantCulture)}%").Build();
        }

        private static string GetRoundedProgress(FileUploadItem item)
            => Math.Clamp((int)Math.Round(item.Progress), 0, 100).ToString(CultureInfo.InvariantCulture);

        private string GetProgressAriaText(FileUploadItem item)
            => $"{GetRoundedProgress(item)}% ({GetStatusText(item)})";

        private string GetProgressAriaLabel(FileUploadItem item)
            => Localizer[LanguageResource.VekFileUpload_ProgressLabel, item.Name];

        private string GetCancelAriaLabel(FileUploadItem item)
            => Localizer[LanguageResource.VekFileUpload_CancelFile, item.Name];

        private string GetRetryAriaLabel(FileUploadItem item)
            => Localizer[LanguageResource.VekFileUpload_RetryFile, item.Name];

        private string GetStatusText(FileUploadItem item) => item.Status switch
        {
            FileUploadStatus.Queued => Localizer[LanguageResource.VekFileUpload_StatusQueued],
            FileUploadStatus.Uploading => Localizer[LanguageResource.VekFileUpload_StatusUploading],
            FileUploadStatus.Success => Localizer[LanguageResource.VekFileUpload_StatusSuccess],
            FileUploadStatus.Error => Localizer[LanguageResource.VekFileUpload_StatusError],
            _ => string.Empty
        };

        private static string GetStatusModifier(FileUploadStatus status) => status switch
        {
            FileUploadStatus.Queued => "queued",
            FileUploadStatus.Uploading => "uploading",
            FileUploadStatus.Success => "success",
            FileUploadStatus.Error => "error",
            _ => "queued"
        };

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes.ToString(CultureInfo.InvariantCulture)} B";

            if (bytes < 1024 * 1024)
                return $"{(bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture)} KB";

            if (bytes < 1024L * 1024 * 1024)
                return $"{(bytes / (1024d * 1024d)).ToString("0.##", CultureInfo.InvariantCulture)} MB";

            return $"{(bytes / (1024d * 1024d * 1024d)).ToString("0.##", CultureInfo.InvariantCulture)} GB";
        }

        /// <inheritdoc />
        protected override IConverter<T?, string?> GetDefaultConverter()
        {
            return new DefaultConverter<T>
            {
                Culture = GetCulture,
                Format = GetFormat
            };
        }

        protected internal override T? ReadValue => _filesState.Value;

        protected override async Task SetValueCoreAsync(T? value)
        {
            await _filesState.SetValueAsync(value);
            await SyncQueueFromFilesAsync(value);
        }

        protected override async Task ValidateValue()
        {
            await base.ValidateValue();

            ValidationErrors = [.. ValidationErrors, .. _validationErrors];
            await ErrorState.SetValueAsync(ValidationErrors.Count > 0);
            await ErrorTextState.SetValueAsync(ValidationErrors.FirstOrDefault());
        }

        /// <inheritdoc />
        public override Task ResetValidationAsync()
        {
            _validationErrors.Clear();

            return base.ResetValidationAsync();
        }
    }
}
