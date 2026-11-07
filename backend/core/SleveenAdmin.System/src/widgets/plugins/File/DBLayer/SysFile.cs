using mooSQL.data;

namespace SleveenAdmin.System.File.DBLayer;

[SooTable("SysFile", Caption = "系统文件")]
public class SysFile
{
    [SooColumn("Sf_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sf_OriginalName", Length = 256, CanBeNull = false)]
    public string OriginalName { get; set; } = string.Empty;

    [SooColumn("Sf_StoredName", Length = 128, CanBeNull = false)]
    public string StoredName { get; set; } = string.Empty;

    [SooColumn("Sf_Extension", Length = 32, CanBeNull = true)]
    public string? Extension { get; set; }

    [SooColumn("Sf_Size", CanBeNull = false)]
    public long Size { get; set; }

    [SooColumn("Sf_ContentType", Length = 128, CanBeNull = true)]
    public string? ContentType { get; set; }

    [SooColumn("Sf_Path", Length = 512, CanBeNull = false)]
    public string Path { get; set; } = string.Empty;

    [SooColumn("Sf_Url", Length = 512, CanBeNull = false)]
    public string Url { get; set; } = string.Empty;

    [SooColumn("Sf_CreateUserId", CanBeNull = true)]
    public Guid? CreateUserId { get; set; }

    [SooColumn("Sf_CreateTime", CanBeNull = false)]
    public DateTime CreateTime { get; set; }
}
