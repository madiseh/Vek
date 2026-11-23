using Microsoft.AspNetCore.Components.Forms;

namespace Vek
{
    /// <summary>
    /// Describes upload progress after a file chunk succeeds.
    /// </summary>
    public sealed class FileUploadProgressEventArgs : EventArgs
    {
        internal FileUploadProgressEventArgs(IBrowserFile file, long bytesUploaded, long chunkIndex, long totalChunks, double bytesPerSecond)
        {
            File = file;
            BytesUploaded = bytesUploaded;
            ChunkIndex = chunkIndex;
            TotalChunks = totalChunks;
            BytesPerSecond = bytesPerSecond;
        }

        /// <summary>
        /// The file currently being uploaded.
        /// </summary>
        public IBrowserFile File { get; }

        /// <summary>
        /// The number of bytes successfully uploaded for the current file.
        /// </summary>
        public long BytesUploaded { get; }

        /// <summary>
        /// The zero-based index of the most recently uploaded chunk.
        /// </summary>
        public long ChunkIndex { get; }

        /// <summary>
        /// The number of chunks in the current file.
        /// </summary>
        public long TotalChunks { get; }

        /// <summary>
        /// The percentage uploaded for the current file, from 0 to 100.
        /// </summary>
        public double PercentComplete => File.Size == 0 ? 100 : BytesUploaded * 100d / File.Size;

        /// <summary>
        /// The average upload speed for the current file, in bytes per second.
        /// </summary>
        public double BytesPerSecond { get; }
    }
}
