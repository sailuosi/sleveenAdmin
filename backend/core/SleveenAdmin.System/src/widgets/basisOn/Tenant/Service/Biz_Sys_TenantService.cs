using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.System.Tenant.Service;

public sealed class BizSysTenantService : BizServiceBase<BizSysTenantRow>
{
    public override string BizName => "Biz_Sys_Tenant";

    protected override string TableName => "SysTenant";

    protected override string OidColumn => "St_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => "St_CreateTime";

    protected override string UpdatedAtColumn => string.Empty;

    protected override string DefaultOrderBy => "mtb.St_CreateTime desc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.St_Name", "mtb.St_Code"], kw);
    }

    protected override void OnBeforeSave(BizSysTenantRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
            throw new ArgumentException("租户名称不能为空");
        if (string.IsNullOrWhiteSpace(row.Code))
            throw new ArgumentException("租户编码不能为空");
        row.Name = row.Name.Trim();
        row.Code = row.Code.Trim();

        var kit = Db.useSQL().from(TableName).where("St_Code", row.Code);
        if (row.Id != Guid.Empty)
            kit.whereFormat("St_Id <> {0}", row.Id);
        if (kit.count() > 0)
            throw new InvalidOperationException("租户编码已存在");
    }

    public List<BizSysTenantRow> LoadAll()
    {
        var dt = Db.useSQL().select("mtb.*").from($"{TableName} as mtb")
            .orderBy("mtb.St_Name asc").query();
        return MaterializeList(dt);
    }
}
