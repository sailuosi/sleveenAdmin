using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.System.Menu.Service;

public sealed class BizSysMenuService : BizServiceBase<BizSysMenuRow>
{
    public override string BizName => "Biz_Sys_Menu";

    protected override string TableName => "SysMenu";

    protected override string OidColumn => "Sm_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => "Sm_CreateTime";

    protected override string UpdatedAtColumn => string.Empty;

    protected override string DefaultOrderBy => "mtb.Sm_OrderNo asc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Sm_Name", "mtb.Sm_Title"], kw);
    }

    protected override void OnBeforeSave(BizSysMenuRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Title))
            throw new ArgumentException("菜单名称和标题不能为空");
        row.Name = row.Name.Trim();
        row.Title = row.Title.Trim();

        var kit = Db.useSQL().from(TableName).where("Sm_Name", row.Name);
        if (row.Id != Guid.Empty)
            kit.whereFormat("Sm_Id <> {0}", row.Id);
        if (kit.count() > 0)
            throw new InvalidOperationException("菜单路由名已存在");

        if (row.Id == Guid.Empty)
        {
            row.IsKeepAlive ??= true;
            row.Status ??= 1;
        }
    }

    protected override void OnBeforeDelete(Guid id)
    {
        if (Db.useSQL().from(TableName).whereGuid("Sm_Pid", id).count() > 0)
            throw new InvalidOperationException("请先删除子菜单");

        var oid = id.ToString();
        Db.useSQL().setTable("AuthGoodsItem")
            .where("Agi_Type", "1")
            .where("Agi_Oid", oid)
            .doDelete();
    }

    public List<BizSysMenuTreeNode> LoadTree()
    {
        var dt = Db.useSQL().select("mtb.*").from($"{TableName} as mtb")
            .orderBy("mtb.Sm_OrderNo asc").query();
        var all = MaterializeList(dt);
        return BuildTree(all, null);
    }

    static List<BizSysMenuTreeNode> BuildTree(List<BizSysMenuRow> all, Guid? pid) =>
        all.Where(m => m.Pid == pid)
            .Select(m =>
            {
                var node = ToTreeNode(m);
                node.Children = BuildTree(all, m.Id);
                return node;
            }).ToList();

    static BizSysMenuTreeNode ToTreeNode(BizSysMenuRow m) => new()
    {
        Id = m.Id,
        Pid = m.Pid,
        Name = m.Name,
        Title = m.Title,
        Path = m.Path,
        Component = m.Component,
        Icon = m.Icon,
        Type = m.Type,
        OrderNo = m.OrderNo,
        Permission = m.Permission,
        IsHide = m.IsHide,
        CreateTime = m.CreateTime,
        MenuCode = m.MenuCode,
        MenuType = m.MenuType,
        Redirect = m.Redirect,
        OutLink = m.OutLink,
        IsIframe = m.IsIframe ?? false,
        IsKeepAlive = m.IsKeepAlive ?? true,
        IsAffix = m.IsAffix ?? false,
        Status = m.Status is null or <= 0 ? 1 : m.Status.Value,
        Remark = m.Remark,
        ExtPara = m.ExtPara,
        MenuTheme = m.MenuTheme,
        IsApp = m.IsApp ?? false
    };
}
