namespace SleveenAdmin.System.Notice.Dtos;

public sealed class SysNoticeInput
{
    public Guid? Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    /// <summary>1=公告 2=站内信</summary>
    public int Type { get; set; }

    public List<Guid>? TargetUserIds { get; set; }
}
