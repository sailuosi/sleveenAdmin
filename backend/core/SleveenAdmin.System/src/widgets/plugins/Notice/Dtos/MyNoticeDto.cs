namespace SleveenAdmin.System.Notice.Dtos;

public sealed class MyNoticeDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime? PublishTime { get; set; }

    public DateTime CreateTime { get; set; }
}
