using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysPortCellService : BizServiceBase
{
    private readonly SysPortalCommonService _portalCommon;

    public BizSysPortCellService(SysPortalCommonService portalCommon) => _portalCommon = portalCommon;

    public override string BizName => "Biz_Sys_PortCell";

    protected override string TableName => PortalSchema.TablePortCell;

    protected override string OidColumn => "Spc_Id";

    protected override string SoftDeleteColumn => "Spc_Deleted";

    protected override string CreatedAtColumn => "Spc_CreateTime";

    protected override string UpdatedAtColumn => "Spc_UpdateTime";

    protected override string? OidAlias => "spcId";

    protected override string DefaultOrderBy => "mtb.PC_Idx asc";

    protected override Dictionary<string, object> LoadInitDicts()
    {
        var dicts = LoadDictItems("SysAppletMany", "SysPtCellTheme", "SysAppletTitle");
        return dicts.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
    }

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            var col = key switch
            {
                "spPortalId" or "SpPortalId" or "Spc_PortalId" => "Spc_PortalId",
                _ => IsSafeSqlIdent(key) ? key : null
            };
            if (col is null) continue;
            kit.where("mtb." + col, value);
        }
    }

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.PC_Name", "mtb.PC_Key"], input.Keyword.Trim());
    }

    protected override void ApplyOutputAliases(Dictionary<string, object?> map)
    {
        base.ApplyOutputAliases(map);
        PortalRowAlias.ApplyToMap(map, PortalRowAlias.Cell);
    }

    protected override IEnumerable<string> AliasKeysToStrip()
    {
        foreach (var k in base.AliasKeysToStrip()) yield return k;
        yield return "spPortalId";
    }

    protected override void OnBeforeSave(Dictionary<string, object?> map)
    {
        if (map.TryGetValue("spPortalId", out var v) && !map.ContainsKey("Spc_PortalId"))
            map["Spc_PortalId"] = v;
        base.OnBeforeSave(map);
    }

    protected override void OnAfterSave(Dictionary<string, object?> map)
    {
        if (TryToGuid(map.GetValueOrDefault("Spc_PortalId"), out var pid))
            _portalCommon.InvalidatePortalConfig(pid);
    }

    protected override void OnAfterDelete(Guid id)
    {
        var row = Db.useSQL().select("Spc_PortalId").from(TableName).where("Spc_Id", id).queryRow();
        if (row is not null && Guid.TryParse(row["Spc_PortalId"]?.ToString(), out var pid))
            _portalCommon.InvalidatePortalConfig(pid);
    }
}
