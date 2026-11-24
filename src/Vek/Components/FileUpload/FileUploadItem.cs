// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components.Forms;

namespace Vek;

/// <summary>
/// Represents the current transfer state of a queued upload item.
/// </summary>
public enum FileUploadStatus
{
    /// <summary>
    /// The file is waiting in the queue to be uploaded.
    /// </summary>
    Queued,

    /// <summary>
    /// The file is actively uploading.
    /// </summary>
    Uploading,

    /// <summary>
    /// The file upload completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The file upload failed or was canceled.
    /// </summary>
    Error
}

/// <summary>
/// Tracks an individual file entry, upload status, and progress in a <see cref="VekFileUpload{T}"/> queue.
/// </summary>
public sealed class FileUploadItem
{
    public FileUploadItem(IBrowserFile file, string? id = null)
    {
        ArgumentNullException.ThrowIfNull(file);
        File = file;
        Id = string.IsNullOrWhiteSpace(id) ? Identifier.Create("file") : id;
    }

    /// <summary>
    /// The unique identifier for this queued file entry.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The underlying browser file for this queue entry.
    /// </summary>
    public IBrowserFile File { get; }

    /// <summary>
    /// The file name displayed in the upload queue.
    /// </summary>
    public string Name => File.Name;

    /// <summary>
    /// The file size in bytes.
    /// </summary>
    public long Size => File.Size;

    /// <summary>
    /// The current upload status of this file.
    /// </summary>
    public FileUploadStatus Status { get; set; } = FileUploadStatus.Queued;

    /// <summary>
    /// The completion percentage of this file upload, from 0 to 100.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// The number of bytes uploaded for this file.
    /// </summary>
    public long BytesUploaded { get; set; }

    /// <summary>
    /// The error message recorded when the upload fails or is canceled.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
