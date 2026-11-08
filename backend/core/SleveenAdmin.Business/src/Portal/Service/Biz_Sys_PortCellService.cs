using System.Text.Json.Serialization;
using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.Business.Portal.Service;

public sealed class BizSysPortCellService : BizServiceBase<BizSysPortCellRow>
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

    protected override Dictionary<string, List<BizDictItem>> LoadInitDicts() =>
        LoadDictItems("SysAppletMany", "SysPtCellTheme", "SysAppletTitle");

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

    protected override void OnAfterSave(BizSysPortCellRow row)
    {
        if (row.PortalId is Guid pid && pid != Guid.Empty)
            _portalCommon.InvalidatePortalConfig(pid);
    }

    protected override void OnAfterDelete(Guid id)
    {
        var row = Db.useSQL().select("Spc_PortalId").from(TableName).where("Spc_Id", id).queryRow();
        if (row is not null && Guid.TryParse(row["Spc_PortalId"]?.ToString(), out var pid))
            _portalCommon.InvalidatePortalConfig(pid);
    }
}
