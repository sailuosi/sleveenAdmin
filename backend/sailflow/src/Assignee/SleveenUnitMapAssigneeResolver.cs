using System.Data;
using mooSQL.data;
using sailflow.assignee;
using sailflow.assignee.Builtin;
using sailflow.engine.Common;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.Saiflow.Assignee;

/// <summary>
/// R3：覆盖内置 UnitMap —— 按宿主 <c>SysOrg</c>/<c>SysUser</c> 查人。
/// 身份键输出为 <c>SysUser.Account</c>（用户编码），非 GUID。
/// unitKey 支持 OrgId(Guid) 或 Org.Code。
/// </summary>
public class SleveenUnitMapAssigneeResolver : UnitMapAssigneeResolver
{
    /// <summary>主数据库位（与 MooRepository&lt;SysUser&gt; 一致，非 sailflow 库）。</summary>
    public int MasterDbPosition { get; set; } = MooSqlSetup.DefaultPosition;

    protected override IEnumerable<string> LookupUsers(AssigneeResolveContext ctx, string unitKey)
    {
        if (string.IsNullOrWhiteSpace(unitKey))
            yield break;

        // 先读 Params.maps（设计器显式映射优先）
        var fromMap = ConditionBag.MapLookup(ctx.Config, unitKey).ToList();
        if (fromMap.Count > 0)
        {
            foreach (var id in fromMap)
                yield return id;
            yield break;
        }

        foreach (var account in LookupAccountsByOrg(unitKey))
            yield return account;
    }

    IEnumerable<string> LookupAccountsByOrg(string unitKey)
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

        Guid? orgId = null;
        if (Guid.TryParse(unitKey, out var gid))
            orgId = gid;
        else
        {
            var orgRows = kit.clear()
                .setTable("SysOrg")
                .select("So_Id")
                .where("So_Code", unitKey)
                .query();
            if (orgRows.Rows.Count > 0)
                orgId = orgRows.Rows[0].Field<Guid>("So_Id");
        }

        if (orgId == null)
            yield break;

        var userRows = kit.clear()
            .setTable("SysUser")
            .select("Su_Account")
            .where("Su_OrgId", orgId.Value)
            .where("Su_Status", 1)
            .query();

        for (var i = 0; i < userRows.Rows.Count; i++)
        {
            var account = Convert.ToString(userRows.Rows[i]["Su_Account"]);
            if (!string.IsNullOrWhiteSpace(account))
                yield return account!;
        }
    }
}
