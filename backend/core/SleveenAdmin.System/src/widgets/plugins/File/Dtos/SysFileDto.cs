namespace SleveenAdmin.System.File.Dtos;

public sealed class SysFileDto
{
    public Guid Id { get; set; }

    public string OriginalName { get; set; } = string.Empty;

    public string StoredName { get; set; } = string.Empty;

    public string? Extension { get; set; }

    public long Size { get; set; }

    public string? ContentType { get; set; }

    public string Path { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    /// <summary>兼容个人中心 Admin.NET 风格字段（与 Url/Path 同值或父路径）</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>兼容个人中心 Admin.NET 风格字段（存储文件名）</summary>
    public string Name { get; set; } = string.Empty;

    public Guid? CreateUserId { get; set; }

    public DateTime CreateTime { get; set; }
}
