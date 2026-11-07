namespace SleveenAdmin.Foundation.Options;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>相对 ContentRoot 或绝对路径。</summary>
    public string RootPath { get; set; } = "uploads";

    public string PublicPathPrefix { get; set; } = "/uploads";

    public int MaxSizeMb { get; set; } = 20;

    public string[] AllowedExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".zip", ".txt"
    ];
}
