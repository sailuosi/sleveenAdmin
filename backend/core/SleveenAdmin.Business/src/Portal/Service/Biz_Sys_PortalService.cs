using System.Data;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysPortalService : BizServiceBase<BizSysPortalRow>
{
    private readonly SysPortalCommonService _portalCommon;

    public BizSysPortalService(SysPortalCommonService portalCommon) => _portalCommon = portalCommon;

    public override string BizName => "Biz_Sys_Portal";

    protected override string TableName => PortalSchema.TablePortal;

    protected override string OidColumn => "Sp_Id";

    protected override string SoftDeleteColumn => "Sp_Deleted";

    protected override string CreatedAtColumn => "Sp_CreateTime";

    protected override string UpdatedAtColumn => "Sp_UpdateTime";

    protected override string? OidAlias => "spId";

    protected override string DefaultOrderBy => "mtb.Sp_CreateTime desc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        LoadDictItems(
            "SysPtTask", "SysShareStatus", "SysPtCellTheme", "SysPtDisplay", "SysAppletTitle",
            "SysAppletMany", "SysAppletType", "SysApplteTool", "SysCellDisabled");

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.P_Name", "mtb.P_Key", "mtb.P_Note"], kw);
    }

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            var col = key switch
            {
                "spPortPageId" or "SpPortPageId" => "Sp_PortPageId",
                "spOrgId" or "SpOrgId" => "Sp_OrgId",
                "P_Task" or "PTask" => "P_Task",
                "P_Key" or "PKey" => "P_Key",
                "P_Name" or "PName" => "P_Name",
                "P_IsOn" or "PIsOn" => "P_IsOn",
                _ => IsSafeSqlIdent(key) ? key : null
            };
            if (col is null) continue;
            if (col is "P_Name" or "P_Key")
                kit.whereLike("mtb." + col, value);
            else
                kit.where("mtb." + col, value);
        }
    }

    public override PagedResult<BizSysPortalRow> LoadPageList(BizQueryInput input)
    {
        var page = input.ResolvePage();
        var size = input.ResolvePageSize();
        var kit = Db.useSQL();
        kit.select("mtb.*,p.PP_Title as PP_Title,o.So_Name as OrgName")
            .from($"{TableName} as mtb")
            .join($"left join {PortalSchema.TablePortPage} p on mtb.Sp_PortPageId=p.Spp_Id")
            .join($"left join {PortalSchema.TableSysOrg} o on mtb.Sp_OrgId=o.So_Id");
        WhenQueryWhere(kit, input);
        var total = kit.count();
        var order = string.IsNullOrWhiteSpace(input.OrderBy) ? DefaultOrderBy : input.OrderBy!;
        var dt = kit.setPage(size, page).orderBy(order).query();
        var records = MaterializeList(dt);
        return PagedResult<BizSysPortalRow>.Create(records, page, size, total);
    }

    protected override void OnAfterSave(BizSysPortalRow row) =>
        _portalCommon.InvalidatePortalConfig(row.Id, row.Key);

    protected override void OnAfterDelete(Guid id) =>
        _portalCommon.InvalidatePortalConfig(id);
}
