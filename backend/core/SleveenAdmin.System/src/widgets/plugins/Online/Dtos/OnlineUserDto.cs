namespace SleveenAdmin.System.Online.Dtos;

public sealed class OnlineUserDto
{
    public string ConnectionId { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Account { get; set; } = string.Empty;

    public string? RealName { get; set; }

    public string? Ip { get; set; }

    public DateTime ConnectedAt { get; set; }
}
