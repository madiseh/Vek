using Microsoft.AspNetCore.Components.Forms;

namespace Vek;

internal enum FileUploadValidationFailure
{
    None,
    FileSize,
    Extension,
    MimeType
}

internal static class FileUploadValidator
{
    public static FileUploadValidationFailure Validate(
        IBrowserFile file,
        long? maxFileSize,
        IReadOnlyList<string>? allowedExtensions,
        IReadOnlyList<string>? allowedMimeTypes)
    {
        if (maxFileSize.HasValue && file.Size > maxFileSize.Value)
            return FileUploadValidationFailure.FileSize;

        if (allowedExtensions is { Count: > 0 })
        {
            var extension = Path.GetExtension(file.Name);
            var extensionIsAllowed = allowedExtensions.Any(allowed =>
            {
                var normalized = allowed.Trim();
                if (normalized.Length == 0)
                    return false;
                return StringComparer.OrdinalIgnoreCase.Equals(
                    normalized[0] == '.' ? normalized : $".{normalized}",
                    extension);
            });

            if (!extensionIsAllowed)
                return FileUploadValidationFailure.Extension;
        }

        if (allowedMimeTypes is { Count: > 0 })
        {
            var mimeType = file.ContentType.Split(';', 2)[0].Trim();
            if (!allowedMimeTypes.Any(allowed => StringComparer.OrdinalIgnoreCase.Equals(allowed.Trim(), mimeType)))
                return FileUploadValidationFailure.MimeType;
        }

        return FileUploadValidationFailure.None;
    }
}
