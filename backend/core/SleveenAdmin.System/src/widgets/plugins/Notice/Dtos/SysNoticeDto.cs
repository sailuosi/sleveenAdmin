namespace SleveenAdmin.System.Notice.Dtos;

public sealed class SysNoticeDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int Type { get; set; }

    public int Status { get; set; }

    public Guid? TenantId { get; set; }

    public Guid CreateUserId { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? PublishTime { get; set; }

    public List<Guid> TargetUserIds { get; set; } = [];
}
