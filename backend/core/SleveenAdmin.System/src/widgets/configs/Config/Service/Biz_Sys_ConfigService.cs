using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Config.DBLayer;
using SleveenAdmin.System.Config.Dtos;

namespace SleveenAdmin.System.Config.Service;

public sealed class BizSysConfigService : BizServiceBase<BizSysConfigRow>
{
    public override string BizName => "Biz_Sys_Config";

    protected override string TableName => "SysConfig";

    protected override string OidColumn => "Sc_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => string.Empty;

    protected override string UpdatedAtColumn => string.Empty;

    protected override string DefaultOrderBy => "mtb.Sc_Code asc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Sc_Code", "mtb.Sc_Remark"], kw);
    }

    protected override void OnBeforeSave(BizSysConfigRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Code))
            throw new ArgumentException("配置编码不能为空");
        row.Code = row.Code.Trim();

        var kit = Db.useSQL().from(TableName).where("Sc_Code", row.Code);
        if (row.Id != Guid.Empty)
            kit.whereFormat("Sc_Id <> {0}", row.Id);
        if (kit.count() > 0)
            throw new InvalidOperationException("配置编码已存在");
    }

    /// <summary>运行时按编码读取（保留给 ConfigController）。</summary>
    public SysConfigDto? GetByCode(string code)
    {
        var row = Db.useSQL().select("mtb.*").from($"{TableName} as mtb")
            .where("mtb.Sc_Code", code).queryRow();
        if (row is null) return null;
        var entity = MaterializeRow(row);
        return new SysConfigDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Value = entity.Value,
            Remark = entity.Remark
        };
    }
}
