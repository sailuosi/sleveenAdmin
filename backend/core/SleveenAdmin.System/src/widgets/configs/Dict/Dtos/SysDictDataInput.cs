namespace SleveenAdmin.System.Dict.Dtos;

public sealed class SysDictDataInput
{
    public Guid? Id { get; set; }

    public Guid TypeId { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public int OrderNo { get; set; }
}
