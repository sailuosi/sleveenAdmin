using Microsoft.Net.Http.Headers;

namespace SleveenAdmin.Sailflow.Utility;

/// <summary>multipart/form-data 解析辅助（对齐 sfdapi MultipartRequestHelper）。</summary>
public static class MultipartRequestHelper
{
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
        if (string.IsNullOrWhiteSpace(boundary.Value))
            throw new InvalidDataException("Missing Content-Type boundary.");

        if (boundary.Length > lengthLimit)
            throw new InvalidDataException($"Boundary length exceeds {lengthLimit}.");

        return boundary.Value!;
    }

    public static bool IsMultipartContentType(string? contentType) =>
        !string.IsNullOrEmpty(contentType)
        && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue? contentDisposition) =>
        contentDisposition != null
        && contentDisposition.DispositionType.Equals("form-data")
        && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
            || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
}
