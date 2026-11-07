using mooSQL.data;

namespace SleveenAdmin.System.Notice.DBLayer;

[SooTable("SysNotice", Caption = "????")]
public class SysNotice
{
    [SooColumn("Sn_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sn_Title", Length = 128, CanBeNull = false)]
    public string Title { get; set; } = string.Empty;

    [SooColumn("Sn_Content", Length = 4000, CanBeNull = false)]
    public string Content { get; set; } = string.Empty;

    /// <summary>1=?? 2=???</summary>
    [SooColumn("Sn_Type", CanBeNull = false)]
    public int Type { get; set; }

    /// <summary>1=?? 2=???</summary>
    [SooColumn("Sn_Status", CanBeNull = false)]
    public int Status { get; set; }

    [SooColumn("Sn_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Sn_CreateUserId", CanBeNull = false)]
    public Guid CreateUserId { get; set; }

    [SooColumn("Sn_CreateTime", CanBeNull = false)]
    public DateTime CreateTime { get; set; }

    [SooColumn("Sn_PublishTime", CanBeNull = true)]
    public DateTime? PublishTime { get; set; }
}
