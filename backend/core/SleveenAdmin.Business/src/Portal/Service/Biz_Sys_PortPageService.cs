using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysPortPageService : BizServiceBase
{
    public override string BizName => "Biz_Sys_PortPage";

    protected override string TableName => PortalSchema.TablePortPage;

    protected override string OidColumn => "Spp_Id";

    protected override string SoftDeleteColumn => "Spp_Deleted";

    protected override string CreatedAtColumn => "Spp_CreateTime";

    protected override string UpdatedAtColumn => "Spp_UpdateTime";

    protected override string? OidAlias => "sppId";

    protected override string DefaultOrderBy => "mtb.Spp_CreateTime desc";

    protected override Dictionary<string, object> LoadInitDicts()
    {
        var dicts = LoadDictItems("SysPtCellTheme", "KB_PtPagePose");
        return dicts.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
    }

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.PP_Title", "mtb.PP_Code"], input.Keyword.Trim());
    }

    protected override void ApplyOutputAliases(Dictionary<string, object?> map)
    {
        base.ApplyOutputAliases(map);
        PortalRowAlias.ApplyToMap(map, PortalRowAlias.PortPage);
    }

    protected override IEnumerable<string> AliasKeysToStrip()
    {
        foreach (var k in base.AliasKeysToStrip()) yield return k;
        yield return "spOrgId";
        yield return "PP_OrgOID";
    }

    protected override void OnBeforeSave(Dictionary<string, object?> map)
    {
        if (map.TryGetValue("spOrgId", out var v) && !map.ContainsKey("Spp_OrgId"))
            map["Spp_OrgId"] = v;
        else if (map.TryGetValue("PP_OrgOID", out var v2) && !map.ContainsKey("Spp_OrgId"))
            map["Spp_OrgId"] = v2;
        base.OnBeforeSave(map);
    }
}
