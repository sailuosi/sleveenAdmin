namespace SleveenAdmin.System.Menu.Service;

public sealed class BizSysMenuTreeNode
{
    public Guid Id { get; set; }

    public Guid? Pid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Type { get; set; }

    public int OrderNo { get; set; }

    public List<BizSysMenuTreeNode> Children { get; set; } = [];
}
