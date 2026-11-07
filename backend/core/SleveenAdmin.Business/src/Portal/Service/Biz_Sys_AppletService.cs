using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysAppletService : BizServiceBase
{
    public override string BizName => "Biz_Sys_Applet";

    protected override string TableName => PortalSchema.TableApplet;

    protected override string OidColumn => "Sa_Id";

    protected override string SoftDeleteColumn => "Sa_Deleted";

    protected override string CreatedAtColumn => "Sa_CreateTime";

    protected override string UpdatedAtColumn => "Sa_UpdateTime";

    protected override string? OidAlias => "saId";

    protected override string DefaultOrderBy => "mtb.Sa_CreateTime desc";

    protected override Dictionary<string, object> LoadInitDicts()
    {
        var dicts = LoadDictItems("SysAppletType", "SysPtCellTheme", "SysAppletTitle");
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
                "saAppletCataId" or "SaAppletCataId" => "Sa_AppletCataId",
                _ => IsSafeSqlIdent(key) ? key : null
            };
            if (col is null) continue;
            kit.where("mtb." + col, value);
        }
    }

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.A_Title", "mtb.A_Name", "mtb.A_Key"], input.Keyword.Trim());
    }

    protected override void ApplyOutputAliases(Dictionary<string, object?> map)
    {
        base.ApplyOutputAliases(map);
        PortalRowAlias.ApplyToMap(map, PortalRowAlias.Applet);
    }

    protected override IEnumerable<string> AliasKeysToStrip()
    {
        foreach (var k in base.AliasKeysToStrip()) yield return k;
        yield return "saAppletCataId";
    }

    protected override void OnBeforeSave(Dictionary<string, object?> map)
    {
        if (map.TryGetValue("saAppletCataId", out var v) && !map.ContainsKey("Sa_AppletCataId"))
            map["Sa_AppletCataId"] = v;
        base.OnBeforeSave(map);
    }
}
