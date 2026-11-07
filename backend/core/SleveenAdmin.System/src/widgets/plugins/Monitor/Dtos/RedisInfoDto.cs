namespace SleveenAdmin.System.Monitor.Dtos;

public sealed class RedisInfoDto
{
    public bool Configured { get; set; }

    public bool Connected { get; set; }

    public string? ServerVersion { get; set; }

    public string? EndPoint { get; set; }

    public long? DbSize { get; set; }

    public string? Error { get; set; }
}
