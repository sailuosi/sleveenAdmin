namespace SleveenAdmin.System.Org.Service;

public sealed class BizSysOrgTreeNode
{
    public Guid Id { get; set; }

    public Guid? Pid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    /// <summary>组织层级编码，权限包 Vi_Type=3 前缀匹配用。</summary>
    public string? ClassCode { get; set; }

    public int OrderNo { get; set; }

    public DateTime CreateTime { get; set; }

    public List<BizSysOrgTreeNode> Children { get; set; } = [];
}
