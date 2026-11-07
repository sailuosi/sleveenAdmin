namespace SleveenAdmin.System.Org.Dtos;

public sealed class SysOrgInput
{
    public Guid? Id { get; set; }

    public Guid? Pid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public int OrderNo { get; set; }
}
