using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysAppletService : BizServiceBase<BizSysAppletRow>
{
    public override string BizName => "Biz_Sys_Applet";

    protected override string TableName => PortalSchema.TableApplet;

    protected override string OidColumn => "Sa_Id";

    protected override string SoftDeleteColumn => "Sa_Deleted";

    protected override string CreatedAtColumn => "Sa_CreateTime";

    protected override string UpdatedAtColumn => "Sa_UpdateTime";

    protected override string? OidAlias => "saId";

    protected override string DefaultOrderBy => "mtb.Sa_CreateTime desc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        LoadDictItems("SysAppletType", "SysPtCellTheme", "SysAppletTitle");

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
}
