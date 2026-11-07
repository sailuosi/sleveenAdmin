namespace SleveenAdmin.System.Org.Dtos;

public sealed class SysOrgDto
{
    public Guid Id { get; set; }

    public Guid? Pid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string? ClassCode { get; set; }

    public int OrderNo { get; set; }

    public DateTime CreateTime { get; set; }

    public List<SysOrgDto> Children { get; set; } = [];
}
