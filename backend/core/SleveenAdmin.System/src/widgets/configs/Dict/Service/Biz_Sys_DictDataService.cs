using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.System.Dict.Service;

public sealed class BizSysDictDataService : BizServiceBase<BizSysDictDataRow>
{
    public override string BizName => "Biz_Sys_DictData";

    protected override string TableName => "SysDictData";

    protected override string OidColumn => "Sdd_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => string.Empty;

    protected override string UpdatedAtColumn => string.Empty;

    protected override string DefaultOrderBy => "mtb.Sdd_OrderNo asc";

    protected override void ApplyFilters(SQLBuilder kit, BizQueryInput input)
    {
        if (input.Filters is null) return;
        foreach (var (key, value) in input.Filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) continue;
            var col = key switch
            {
                "typeId" or "TypeId" => "Sdd_TypeId",
                _ => IsSafeSqlIdent(key) ? key : null
            };
            if (col is null) continue;
            kit.where("mtb." + col, value);
        }
    }

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Sdd_Label", "mtb.Sdd_Value"], kw);
    }

    protected override void OnBeforeSave(BizSysDictDataRow row)
    {
        if (row.TypeId == Guid.Empty)
            throw new ArgumentException("字典类型不能为空");
        if (string.IsNullOrWhiteSpace(row.Label) || string.IsNullOrWhiteSpace(row.Value))
            throw new ArgumentException("字典标签和值不能为空");
        row.Label = row.Label.Trim();
        row.Value = row.Value.Trim();

        if (Db.useSQL().from("SysDictType").whereGuid("Sdt_Id", row.TypeId).count() == 0)
            throw new InvalidOperationException("字典类型不存在");
    }
}
