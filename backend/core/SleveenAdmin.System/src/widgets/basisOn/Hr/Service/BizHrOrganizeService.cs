using mooSQL.data;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.System.Hr.DBLayer;

namespace SleveenAdmin.System.Hr.Service;

[SooTable(Name = "HR_Organize", Alias = "mtb", Type = DBTableType.Select, Caption = "人力组织")]
public class BizHrOrganizeRow : HROrganize
{
}

public sealed class BizHrOrganizeService : BizServiceBase<BizHrOrganizeRow>
{
    public override string BizName => "Biz_HR_Organize";
    protected override string TableName => "HR_Organize";
    protected override string OidColumn => "HR_OrganizeOID";
    protected override bool UseSoftDelete => true;
    protected override string SoftDeleteColumn => "SYS_Deleted";
    protected override string CreatedAtColumn => "SYS_Created";
    protected override string UpdatedAtColumn => "SYS_LAST_UPD";
    protected override string DefaultOrderBy => "mtb.O_Idx asc, mtb.O_OrgName asc";

    protected override void ApplyKeyword(SQLBuilder kit, BizQueryInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Keyword)) return;
        kit.whereLikes(["mtb.O_OrgName", "mtb.O_OrgCode", "mtb.O_OrgNo"], input.Keyword.Trim());
    }

    protected override void OnBeforeSave(BizHrOrganizeRow row)
    {
        if (string.IsNullOrWhiteSpace(row.O_OrgName))
            throw new ArgumentException("组织名称不能为空");
        if (row.HR_OrganizeOID == null || row.HR_OrganizeOID == Guid.Empty)
            row.HR_OrganizeOID = Guid.NewGuid();
        row.O_Active ??= true;
        row.SYS_Deleted ??= false;
    }

    public List<object> LoadTree(Guid? rootId = null)
    {
        var kit = Db.useSQL().select("mtb.*").from("HR_Organize as mtb")
            .whereIsOrNull("mtb.SYS_Deleted", 0)
            .whereIsOrNull("mtb.O_Active", 1)
            .orderBy("mtb.O_Idx asc, mtb.O_OrgName asc");
        var dt = kit.query();
        var all = MaterializeList(dt);
        return BuildTree(all, rootId);
    }

    static List<object> BuildTree(List<BizHrOrganizeRow> all, Guid? pid) =>
        all.Where(o => o.HR_Parent_FK == pid)
            .Select(o => (object)new
            {
                o.HR_OrganizeOID,
                o.HR_Parent_FK,
                o.O_OrgName,
                o.O_OrgNo,
                o.O_OrgCode,
                o.O_ClassCode,
                o.O_Leaf,
                o.O_Lv,
                o.O_Idx,
                children = BuildTree(all, o.HR_OrganizeOID)
            }).ToList();
}
