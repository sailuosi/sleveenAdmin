using mooSQL.data;

namespace SleveenAdmin.System.Notice.DBLayer;

[SooTable("SysNoticeUser", Caption = "通知接收")]
public class SysNoticeUser
{
    [SooColumn("Snu_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Snu_NoticeId", CanBeNull = false)]
    public Guid NoticeId { get; set; }

    [SooColumn("Snu_UserId", CanBeNull = false)]
    public Guid UserId { get; set; }

    [SooColumn("Snu_IsRead", CanBeNull = false)]
    public bool IsRead { get; set; }

    [SooColumn("Snu_ReadTime", CanBeNull = true)]
    public DateTime? ReadTime { get; set; }
}
