using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Forms;

namespace Vek
{
    internal static class FileUploadChunkManager
    {
        private static readonly TimeSpan RetryBaseDelay = TimeSpan.FromMilliseconds(100);

        internal static async Task UploadAsync(
            IBrowserFile file,
            int chunkSizeInBytes,
            int maxRetries,
            Func<FileUploadChunkEventArgs, CancellationToken, Task> uploadChunk,
            Func<FileUploadProgressEventArgs, Task> onProgress,
            CancellationToken cancellationToken)
        {
            var totalChunks = file.Size == 0
                ? 1
                : (file.Size / chunkSizeInBytes) + (file.Size % chunkSizeInBytes == 0 ? 0 : 1);
            var elapsedStart = Stopwatch.GetTimestamp();
            long bytesUploaded = 0;

            // The InputFile stream reads browser-side slices on demand, so memory use stays bounded to one chunk.
            await using var stream = file.OpenReadStream(Math.Max(file.Size, 1), cancellationToken);

            for (long chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var offset = chunkIndex * chunkSizeInBytes;
                var chunkLength = file.Size == 0
                    ? 0
                    : (int)Math.Min(chunkSizeInBytes, file.Size - offset);
                var data = new byte[chunkLength];
                var read = 0;

                while (read < data.Length)
                {
                    var readCount = await stream.ReadAsync(data.AsMemory(read), cancellationToken);
                    if (readCount == 0)
                        throw new EndOfStreamException($"The file '{file.Name}' ended before the expected chunk data was read.");

                    read += readCount;
                }

                for (var attempt = 1; ; attempt++)
                {
                    var chunk = new FileUploadChunkEventArgs(
                        file,
                        chunkIndex,
                        totalChunks,
                        offset,
                        data,
                        chunkIndex == totalChunks - 1,
                        attempt);

                    try
                    {
                        await uploadChunk(chunk, cancellationToken);
                        break;
                    }
                    catch (Exception exception) when (
                        attempt <= maxRetries &&
                        IsTransient(exception, cancellationToken))
                    {
                        await Task.Delay(GetRetryDelay(attempt), cancellationToken);
                    }
                }

                bytesUploaded += data.Length;
                var elapsedSeconds = Stopwatch.GetElapsedTime(elapsedStart).TotalSeconds;
                var bytesPerSecond = elapsedSeconds > 0 ? bytesUploaded / elapsedSeconds : 0;
                await onProgress(new FileUploadProgressEventArgs(
                    file,
                    bytesUploaded,
                    chunkIndex,
                    totalChunks,
                    bytesPerSecond));
            }
        }

        private static bool IsTransient(Exception exception, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            if (exception is OperationCanceledException)
                return false;

            return exception switch
            {
                HttpRequestException { StatusCode: null } => true,
                HttpRequestException { StatusCode: not null } requestException => IsTransientStatusCode(requestException.StatusCode.Value),
                IOException or TimeoutException => true,
                _ => false
            };
        }

        private static bool IsTransientStatusCode(HttpStatusCode statusCode)
        {
            return (int)statusCode is 408 or 425 or 429 or 500 or 502 or 503 or 504;
        }

        private static TimeSpan GetRetryDelay(int attempt)
        {
            var multiplier = 1 << Math.Min(attempt - 1, 4);
            return TimeSpan.FromMilliseconds(Math.Min(RetryBaseDelay.TotalMilliseconds * multiplier, 1600));
        }
    }
}
