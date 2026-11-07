namespace SleveenAdmin.System.Dict.Dtos;

public sealed class SysDictTypeDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Remark { get; set; }
}
