using System.Data;
using mooSQL.data;
using sailflow.assignee;
using sailflow.assignee.Builtin;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.Saiflow.Assignee;

/// <summary>
/// R3：覆盖 JobRelative。宿主尚无独立 HR 岗位编制表：
/// ① Params.maps[jobCode] / jobCode@unitId → Account 列表（建模内联）；
/// ② 可选：AuthVisitorBag.Code = jobCode 时展开账号成员（与 RoleGroup 共用访客袋，作轻量「岗」占位）；
/// 真 HR 编制就绪后在此改查编制表即可。
/// </summary>
public class SleveenJobRelativeAssigneeResolver : JobRelativeAssigneeResolver
{
    public int MasterDbPosition { get; set; } = MooSqlSetup.DefaultPosition;

    protected override IEnumerable<string> LookupByJob(
        AssigneeResolveContext ctx,
        string jobCode,
        string? unitId)
    {
        if (string.IsNullOrWhiteSpace(jobCode))
            yield break;

        var fromMap = false;
        foreach (var id in ConditionBag.MapLookup(ctx.Config, jobCode))
        {
            if (string.IsNullOrWhiteSpace(id)) continue;
            fromMap = true;
            yield return id;
        }

        if (!string.IsNullOrWhiteSpace(unitId))
        {
            var compound = jobCode + "@" + unitId;
            foreach (var id in ConditionBag.MapLookup(ctx.Config, compound))
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                fromMap = true;
                yield return id;
            }
        }

        if (fromMap)
            yield break;

        foreach (var account in LookupVisitorBagAccounts(jobCode.Trim()))
            yield return account;
    }

    IEnumerable<string> LookupVisitorBagAccounts(string jobCode)
    {
        SQLBuilder kit;
        try
        {
            kit = DBCash.useSQL(MasterDbPosition);
        }
        catch
        {
            yield break;
        }

        var bag = kit.clear()
            .setTable("AuthVisitorBag")
            .select("Avb_Id")
            .where("Avb_Code", jobCode)
            .where("(Avb_IsDeleted=0 or Avb_IsDeleted is null)")
            .query();
        if (bag.Rows.Count == 0)
            yield break;

        var bagId = bag.Rows[0].Field<Guid>("Avb_Id");
        var items = kit.clear()
            .setTable("AuthVisitorItem")
            .select("Avi_Type,Avi_Code")
            .where("Avi_VisitorBagId", bagId)
            .where("(Avi_IsOn=1 or Avi_IsOn is null)")
            .where("Avi_Type", "1")
            .query();
        for (var i = 0; i < items.Rows.Count; i++)
        {
            var account = Convert.ToString(items.Rows[i]["Avi_Code"]);
            if (!string.IsNullOrWhiteSpace(account))
                yield return account!;
        }
    }
}
