using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysPortCellLinkService : BizServiceBase<BizSysPortCellLinkRow>
{
    private readonly SysPortalCommonService _portalCommon;

    public BizSysPortCellLinkService(SysPortalCommonService portalCommon) => _portalCommon = portalCommon;

    public override string BizName => "Biz_Sys_PortCellLink";

    protected override string TableName => PortalSchema.TablePortCellLink;

    protected override string OidColumn => "Spcl_Id";

    protected override string SoftDeleteColumn => "Spcl_Deleted";

    protected override string CreatedAtColumn => "Spcl_CreateTime";

    protected override string UpdatedAtColumn => "Spcl_UpdateTime";

    protected override string? OidAlias => "spclId";

    protected override string DefaultOrderBy => "mtb.CL_Idx asc";

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        LoadDictItems(
            "SysAppletType", "SysPtCellTheme", "SysAppletTitle", "SysApplteTool", "SysCellDisabled");

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            var col = key switch
            {
                "spPortalId" or "SpPortalId" => "Spcl_PortalId",
                "spPortCellId" or "SpPortCellId" => "Spcl_PortCellId",
                "saAppletId" or "SaAppletId" => "Spcl_AppletId",
                _ => IsSafeSqlIdent(key) ? key : null
            };
            if (col is null) continue;
            kit.where("mtb." + col, value);
        }
    }

    protected override void OnAfterSave(BizSysPortCellLinkRow row)
    {
        if (row.PortalId is Guid pid && pid != Guid.Empty)
            _portalCommon.InvalidatePortalConfig(pid);
    }

    protected override void OnAfterDelete(Guid id)
    {
        var row = Db.useSQL().select("Spcl_PortalId").from(TableName).where("Spcl_Id", id).queryRow();
        if (row is not null && Guid.TryParse(row["Spcl_PortalId"]?.ToString(), out var pid))
            _portalCommon.InvalidatePortalConfig(pid);
    }
}
