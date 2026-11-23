using Microsoft.AspNetCore.Components.Forms;

namespace Vek
{
    /// <summary>
    /// Describes one file chunk passed to a FileUpload chunk handler.
    /// </summary>
    public sealed class FileUploadChunkEventArgs : EventArgs
    {
        internal FileUploadChunkEventArgs(IBrowserFile file, long chunkIndex, long totalChunks, long offset, ReadOnlyMemory<byte> data, bool isLastChunk, int attempt)
        {
            File = file;
            ChunkIndex = chunkIndex;
            TotalChunks = totalChunks;
            Offset = offset;
            Data = data;
            IsLastChunk = isLastChunk;
            Attempt = attempt;
        }

        /// <summary>
        /// The selected file that contains this chunk.
        /// </summary>
        public IBrowserFile File { get; }

        /// <summary>
        /// The zero-based position of this chunk within the file.
        /// </summary>
        public long ChunkIndex { get; }

        /// <summary>
        /// The number of chunks in the file.
        /// </summary>
        public long TotalChunks { get; }

        /// <summary>
        /// The zero-based byte offset of this chunk within the file.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// The bytes to pass to the upload destination.
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// Indicates that this is the final chunk of the file.
        /// </summary>
        public bool IsLastChunk { get; }

        /// <summary>
        /// The one-based attempt number for this chunk.
        /// </summary>
        public int Attempt { get; }
    }
}
