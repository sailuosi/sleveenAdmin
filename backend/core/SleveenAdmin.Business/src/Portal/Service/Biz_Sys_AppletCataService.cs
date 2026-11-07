using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysAppletCataService : BizServiceBase
{
    public override string BizName => "Biz_Sys_AppletCata";

    protected override string TableName => PortalSchema.TableAppletCata;

    protected override string OidColumn => "Sac_Id";

    protected override string SoftDeleteColumn => "Sac_Deleted";

    protected override string CreatedAtColumn => "Sac_CreateTime";

    protected override string UpdatedAtColumn => "Sac_UpdateTime";

    protected override string? OidAlias => "sacId";

    protected override string DefaultOrderBy => "mtb.AC_Idx asc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.AC_Name", "mtb.AC_Code"], input.Keyword.Trim());
    }

    protected override void ApplyOutputAliases(Dictionary<string, object?> map)
    {
        base.ApplyOutputAliases(map);
        PortalRowAlias.ApplyToMap(map, PortalRowAlias.AppletCata);
    }

    protected override IEnumerable<string> AliasKeysToStrip()
    {
        foreach (var k in base.AliasKeysToStrip()) yield return k;
        yield return "sacPid";
    }

    protected override void OnBeforeSave(Dictionary<string, object?> map)
    {
        if (map.TryGetValue("sacPid", out var v) && !map.ContainsKey("Sac_Pid"))
            map["Sac_Pid"] = v;
        base.OnBeforeSave(map);
    }
}
