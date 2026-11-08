using mooSQL.data;
using SleveenAdmin.Foundation.Biz;

namespace SleveenAdmin.System.Dict.Service;

public sealed class BizSysDictTypeService : BizServiceBase<BizSysDictTypeRow>
{
    public override string BizName => "Biz_Sys_DictType";

    protected override string TableName => "SysDictType";

    protected override string OidColumn => "Sdt_Id";

    protected override bool UseSoftDelete => false;

    protected override string CreatedAtColumn => string.Empty;

    protected override string UpdatedAtColumn => string.Empty;

    protected override string DefaultOrderBy => "mtb.Sdt_Code asc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        var kw = input.Keyword.Trim();
        kit.whereLikes(["mtb.Sdt_Code", "mtb.Sdt_Name"], kw);
    }

    protected override void OnBeforeSave(BizSysDictTypeRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Code) || string.IsNullOrWhiteSpace(row.Name))
            throw new ArgumentException("字典编码和名称不能为空");
        row.Code = row.Code.Trim();
        row.Name = row.Name.Trim();

        var kit = Db.useSQL().from(TableName).where("Sdt_Code", row.Code);
        if (row.Id != Guid.Empty)
            kit.whereFormat("Sdt_Id <> {0}", row.Id);
        if (kit.count() > 0)
            throw new InvalidOperationException("字典编码已存在");
    }

    protected override void OnBeforeDelete(Guid id)
    {
        Db.useSQL().setTable("SysDictData").whereGuid("Sdd_TypeId", id).doDelete();
    }
}
