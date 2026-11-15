using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.System.SysPost.Service;

public sealed class BizSysPostService : BizServiceBase<BizSysPostRow>
{
    public override string BizName => "Biz_Sys_Post";
    protected override string TableName => "Sys_Post";
    protected override string OidColumn => "P_Id";
    protected override bool UseSoftDelete => true;
    protected override string SoftDeleteColumn => "P_Deleted";
    protected override string CreatedAtColumn => "P_Created";
    protected override string UpdatedAtColumn => "P_Updated";
    protected override string DefaultOrderBy => "mtb.P_Order asc, mtb.P_Created desc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["HH_DOrgSrc"] =
            [
                new BizDictItem { Value = PostOrgSrc.AdminOrg, Label = "行政组织" },
                new BizDictItem { Value = PostOrgSrc.Place, Label = "位置" }
            ],
            ["Sys_PostOrgSrc"] =
            [
                new BizDictItem { Value = PostOrgSrc.AdminOrg, Label = "行政组织" },
                new BizDictItem { Value = PostOrgSrc.Place, Label = "位置" }
            ]
        };

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.P_Name", "mtb.P_OrgName", "mtb.P_Card"], input.Keyword.Trim());
    }

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            if (key is "userId" or "P_UserId" or "HH_User_FK")
            {
                if (Guid.TryParse(value, out var userId)) kit.whereGuid("mtb.P_UserId", userId);
                continue;
            }
            if (key is "orgSrc" or "P_OrgSrc" or "DW_OrgSrc")
            {
                kit.where("mtb.P_OrgSrc", value);
                continue;
            }
            if (!IsSafeSqlIdent(key)) continue;
            kit.where("mtb." + key, value);
        }
    }

    protected override void OnBeforeSave(BizSysPostRow row)
    {
        if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
        row.IsOn ??= true;
        row.Deleted ??= false;
        row.OrgSrc = string.IsNullOrWhiteSpace(row.OrgSrc) ? PostOrgSrc.AdminOrg : row.OrgSrc!.Trim();
    }
}

public sealed class BizSysPostTypeService : BizServiceBase<BizSysPostTypeRow>
{
    public override string BizName => "Biz_Sys_PostType";
    protected override string TableName => "Sys_PostType";
    protected override string OidColumn => "Pt_Id";
    protected override bool UseSoftDelete => false;
    protected override string CreatedAtColumn => "Pt_Created";
    protected override string UpdatedAtColumn => string.Empty;
    protected override string DefaultOrderBy => "mtb.Pt_Order asc, mtb.Pt_Created desc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["HH_DOrgSrc"] =
            [
                new BizDictItem { Value = PostOrgSrc.AdminOrg, Label = "行政组织" },
                new BizDictItem { Value = PostOrgSrc.Place, Label = "位置" }
            ]
        };

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.Pt_Title", "mtb.Pt_Note"], input.Keyword.Trim());
    }

    protected override void OnBeforeSave(BizSysPostTypeRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Title))
            throw new ArgumentException("岗位类别标题不能为空");
        if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
        row.IsOn ??= true;
        row.Title = row.Title!.Trim();
    }
}

public sealed class BizSysPostScopeService : BizServiceBase<BizSysPostScopeRow>
{
    public override string BizName => "Biz_Sys_PostScope";
    protected override string TableName => "Sys_PostScope";
    protected override string OidColumn => "Ps_Id";
    protected override bool UseSoftDelete => false;
    protected override string CreatedAtColumn => string.Empty;
    protected override string UpdatedAtColumn => string.Empty;
    protected override string DefaultOrderBy => "mtb.Ps_Id desc";

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            if (key is "postId" or "Ps_PostId" or "HH_DivWorker_FK")
            {
                if (Guid.TryParse(value, out var postId)) kit.whereGuid("mtb.Ps_PostId", postId);
                continue;
            }
            if (key is "userId" or "Ps_UserId" or "HH_User_FK")
            {
                if (Guid.TryParse(value, out var userId)) kit.whereGuid("mtb.Ps_UserId", userId);
                continue;
            }
            if (!IsSafeSqlIdent(key)) continue;
            kit.where("mtb." + key, value);
        }
    }

    public override PagedResult<BizSysPostScopeRow> LoadPageList(BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = Db.useSQL();
        kit.select("mtb.*,t.Pt_Title as Pt_Title,t.Pt_IsOn as Pt_IsOn,t.Pt_Cust as Pt_Cust,t.Pt_OSrc as Pt_OSrc")
            .from("Sys_PostScope as mtb")
            .join("left join Sys_PostType t on mtb.Ps_PostTypeId=t.Pt_Id");
        WhenQueryWhere(kit, input);
        var total = kit.count();
        var order = string.IsNullOrWhiteSpace(input.OrderBy) ? DefaultOrderBy : input.OrderBy!;
        var dt = kit.setPage(size, page).orderBy(order).query();
        return PagedResult<BizSysPostScopeRow>.Create(MaterializeList(dt), page, size, total);
    }

    protected override void OnBeforeSave(BizSysPostScopeRow row)
    {
        if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
        if (row.PostTypeId == null || row.PostTypeId == Guid.Empty)
            throw new ArgumentException("岗位类别不能为空");
        if (row.PostId == null || row.PostId == Guid.Empty)
            throw new ArgumentException("岗位不能为空");
    }
}

public sealed class BizSysPostTypeRoleService : BizServiceBase<BizSysPostTypeRoleRow>
{
    public override string BizName => "Biz_Sys_PostTypeRole";
    protected override string TableName => "Sys_PostTypeRole";
    protected override string OidColumn => "Pr_Id";
    protected override bool UseSoftDelete => false;
    protected override string CreatedAtColumn => string.Empty;
    protected override string UpdatedAtColumn => string.Empty;
    protected override string DefaultOrderBy => "mtb.Pr_Id desc";

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            if (key is "postTypeId" or "Pr_PostTypeId" or "HH_WorkorType_FK")
            {
                if (Guid.TryParse(value, out var postTypeId)) kit.whereGuid("mtb.Pr_PostTypeId", postTypeId);
                continue;
            }
            if (!IsSafeSqlIdent(key)) continue;
            kit.where("mtb." + key, value);
        }
    }

    public override PagedResult<BizSysPostTypeRoleRow> LoadPageList(BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = Db.useSQL();
        kit.select("mtb.*,d.Adi_Name as Adi_Name,d.Adi_Code as Adi_Code,d.Adi_Idx as Adi_Idx")
            .from("Sys_PostTypeRole as mtb")
            .join("left join AuthDutyItem d on mtb.Pr_DutyItemId=d.Adi_Id");
        WhenQueryWhere(kit, input);
        var total = kit.count();
        var order = string.IsNullOrWhiteSpace(input.OrderBy) ? "d.Adi_Idx asc, mtb.Pr_Id desc" : input.OrderBy!;
        var dt = kit.setPage(size, page).orderBy(order).query();
        return PagedResult<BizSysPostTypeRoleRow>.Create(MaterializeList(dt), page, size, total);
    }

    protected override void OnBeforeSave(BizSysPostTypeRoleRow row)
    {
        if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
        if (row.PostTypeId == null || row.PostTypeId == Guid.Empty)
            throw new ArgumentException("岗位类别不能为空");
        if (row.DutyItemId == null || row.DutyItemId == Guid.Empty)
            throw new ArgumentException("职责不能为空");
    }
}

public sealed class BizSysPostTypeChildService : BizServiceBase<BizSysPostTypeChildRow>
{
    public override string BizName => "Biz_Sys_PostTypeChild";
    protected override string TableName => "Sys_PostTypeChild";
    protected override string OidColumn => "Pc_Id";
    protected override bool UseSoftDelete => false;
    protected override string CreatedAtColumn => string.Empty;
    protected override string UpdatedAtColumn => string.Empty;
    protected override string DefaultOrderBy => "mtb.Pc_Id desc";

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            if (key is "bossId" or "Pc_BossId" or "HH_Boss_FK")
            {
                if (Guid.TryParse(value, out var bossId)) kit.whereGuid("mtb.Pc_BossId", bossId);
                continue;
            }
            if (!IsSafeSqlIdent(key)) continue;
            kit.where("mtb." + key, value);
        }
    }

    public override PagedResult<BizSysPostTypeChildRow> LoadPageList(BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = Db.useSQL();
        kit.select("mtb.*,t.Pt_IsOn as Pt_IsOn,t.Pt_Title as Pt_Title,t.Pt_Note as Pt_Note,t.Pt_Order as Pt_Order")
            .from("Sys_PostTypeChild as mtb")
            .join("left join Sys_PostType t on mtb.Pc_PostTypeId=t.Pt_Id");
        WhenQueryWhere(kit, input);
        var total = kit.count();
        var order = string.IsNullOrWhiteSpace(input.OrderBy) ? "t.Pt_Order asc, mtb.Pc_Id desc" : input.OrderBy!;
        var dt = kit.setPage(size, page).orderBy(order).query();
        return PagedResult<BizSysPostTypeChildRow>.Create(MaterializeList(dt), page, size, total);
    }

    protected override void OnBeforeSave(BizSysPostTypeChildRow row)
    {
        if (row.Id == Guid.Empty) row.Id = Guid.NewGuid();
        if (row.BossId == null || row.BossId == Guid.Empty)
            throw new ArgumentException("父级岗位类别不能为空");
        if (row.PostTypeId == null || row.PostTypeId == Guid.Empty)
            throw new ArgumentException("子岗位类别不能为空");
        if (row.BossId == row.PostTypeId)
            throw new InvalidOperationException("下级岗位类别不能是自己");
    }
}
