namespace SleveenAdmin.System.Monitor.Dtos;

public sealed class RedisKeyValueDto
{
    public string Key { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Value { get; set; }

    public long? TtlSeconds { get; set; }
}
