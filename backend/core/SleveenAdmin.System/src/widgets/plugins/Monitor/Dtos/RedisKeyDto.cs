namespace SleveenAdmin.System.Monitor.Dtos;

public sealed class RedisKeyDto
{
    public string Key { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public long? TtlSeconds { get; set; }
}
