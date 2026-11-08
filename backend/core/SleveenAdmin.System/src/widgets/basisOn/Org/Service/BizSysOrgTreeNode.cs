namespace SleveenAdmin.System.Org.Service;

public sealed class BizSysOrgTreeNode
{
    public Guid Id { get; set; }

    public Guid? Pid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public int OrderNo { get; set; }

    public List<BizSysOrgTreeNode> Children { get; set; } = [];
}
