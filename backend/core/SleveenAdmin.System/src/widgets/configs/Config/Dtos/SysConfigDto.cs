namespace SleveenAdmin.System.Config.Dtos;

public sealed class SysConfigDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string? Remark { get; set; }
}
