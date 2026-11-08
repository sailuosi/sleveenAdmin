using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysPortPageService : BizServiceBase<BizSysPortPageRow>
{
    public override string BizName => "Biz_Sys_PortPage";

    protected override string TableName => PortalSchema.TablePortPage;

    protected override string OidColumn => "Spp_Id";

    protected override string SoftDeleteColumn => "Spp_Deleted";

    protected override string CreatedAtColumn => "Spp_CreateTime";

    protected override string UpdatedAtColumn => "Spp_UpdateTime";

    protected override string? OidAlias => "sppId";

    protected override string DefaultOrderBy => "mtb.Spp_CreateTime desc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        LoadDictItems("SysPtCellTheme", "KB_PtPagePose");

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.PP_Title", "mtb.PP_Code"], input.Keyword.Trim());
    }
}
