using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysAppletCataService : BizServiceBase<BizSysAppletCataRow>
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
}
