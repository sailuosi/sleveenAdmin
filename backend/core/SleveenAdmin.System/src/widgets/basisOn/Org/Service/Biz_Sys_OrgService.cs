using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Bootstrap.Seed;

namespace SleveenAdmin.System.Org.Service;

public sealed class BizSysOrgService : BizServiceBase<BizSysOrgRow>
{
    public override string BizName => "Biz_Sys_Org";

    protected override string TableName => "SysOrg";

    protected override string OidColumn => "So_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => "So_CreateTime";

    protected override string UpdatedAtColumn => string.Empty;

    protected override string DefaultOrderBy => "mtb.So_OrderNo asc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.So_Name", "mtb.So_Code"], kw);
    }

    protected override void OnBeforeSave(BizSysOrgRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
            throw new ArgumentException("组织名称不能为空");
        row.Name = row.Name.Trim();

        if (row.Id != Guid.Empty && row.Pid == row.Id)
            throw new InvalidOperationException("上级组织不能是自己");
    }

    protected override void OnBeforeDelete(Guid id)
    {
        if (id == SeedIds.RootOrgId)
            throw new InvalidOperationException("不能删除根组织");

        if (Db.useSQL().from(TableName).whereGuid("So_Pid", id).count() > 0)
            throw new InvalidOperationException("请先删除下级组织");
    }

    public List<BizSysOrgTreeNode> LoadTree()
    {
        var dt = Db.useSQL().select("mtb.*").from($"{TableName} as mtb")
            .orderBy("mtb.So_OrderNo asc").query();
        var all = MaterializeList(dt);
        return BuildTree(all, null);
    }

    static List<BizSysOrgTreeNode> BuildTree(List<BizSysOrgRow> all, Guid? pid) =>
        all.Where(o => o.Pid == pid)
            .Select(o =>
            {
                var node = new BizSysOrgTreeNode
                {
                    Id = o.Id,
                    Pid = o.Pid,
                    Name = o.Name,
                    Code = o.Code,
                    OrderNo = o.OrderNo,
                    Children = BuildTree(all, o.Id)
                };
                return node;
            }).ToList();
}
