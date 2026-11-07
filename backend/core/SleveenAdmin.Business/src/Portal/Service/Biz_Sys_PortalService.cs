using System.Data;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysPortalService : BizServiceBase
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

    protected override Dictionary<string, object> LoadInitDicts()
    {
        var dicts = LoadDictItems(
            "SysPtTask", "SysShareStatus", "SysPtCellTheme", "SysPtDisplay", "SysAppletTitle",
            "SysAppletMany", "SysAppletType", "SysApplteTool", "SysCellDisabled");
        return dicts.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
    }

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

    public override object LoadPageList(BizQueryInput input)
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
        var records = dt.Rows.Cast<DataRow>().Select(RowToDictionary).ToList();
        return PagedResult<Dictionary<string, object?>>.Create(records, page, size, total);
    }

    protected override void ApplyOutputAliases(Dictionary<string, object?> map)
    {
        base.ApplyOutputAliases(map);
        PortalRowAlias.ApplyToMap(map, PortalRowAlias.Portal);
    }

    protected override IEnumerable<string> AliasKeysToStrip()
    {
        foreach (var k in base.AliasKeysToStrip()) yield return k;
        yield return "spOrgId";
        yield return "spUserId";
        yield return "spPortPageId";
        yield return "spSubjectId";
        yield return "OrgName";
        yield return "PP_Title";
    }

    protected override void OnBeforeSave(Dictionary<string, object?> map)
    {
        MapFk(map, "spOrgId", "Sp_OrgId");
        MapFk(map, "spUserId", "Sp_UserId");
        MapFk(map, "spPortPageId", "Sp_PortPageId");
        MapFk(map, "spSubjectId", "Sp_SubjectId");
        base.OnBeforeSave(map);
    }

    protected override void OnAfterSave(Dictionary<string, object?> map)
    {
        if (TryToGuid(map.GetValueOrDefault(OidColumn), out var id))
            _portalCommon.InvalidatePortalConfig(id, map.GetValueOrDefault("P_Key")?.ToString());
    }

    protected override void OnAfterDelete(Guid id) =>
        _portalCommon.InvalidatePortalConfig(id);

    static void MapFk(Dictionary<string, object?> map, string alias, string col)
    {
        if (map.TryGetValue(alias, out var v) && !map.ContainsKey(col))
            map[col] = v;
    }
}
