using System.Data;
using mooSQL.data;
using sailflow.assignee;
using sailflow.assignee.Builtin;
using sailflow.engine.Common;
using sailflow.hub.Resource;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// R3：RoleGroup 优先按宿主权限主数据展开，再回落引擎资源表并对齐 Account。
/// <para>
/// roleIds 解析顺序：
/// ① <c>AuthVisitorBag</c>（Avb_Code / Avb_Id）→ 展开 VisitorItem；
/// ② <c>AuthDutyItem</c>（Adi_Code / Adi_Id）→ 其 VisitorBag；
/// ③ 引擎 <c>IResourceService.GetUserListByRoles</c> + Account 重映射。
/// </para>
/// VisitorItem：Type=1 账号(Code)；2 组织 OID；3 组织编码前缀。
/// </summary>
public class SleveenRoleGroupAssigneeResolver : RoleGroupAssigneeResolver
{
    public int MasterDbPosition { get; set; } = MooSqlSetup.DefaultPosition;

    public override PerformerList Resolve(AssigneeResolveContext ctx)
    {
        var roleIds = ConfigPairParams.GetStringArray(ctx.Config, "roleIds");
        if (roleIds.Length == 0 && ConfigPairParams.GetBool(ctx.Config, "useActivityRoles"))
            roleIds = LoadActivityRoleIds(ctx);

        if (roleIds.Length == 0)
            return new PerformerList();

        var host = ExpandHostRoles(roleIds);
        if (host.Count > 0)
            return ToPerformers(host);

        var raw = base.Resolve(ctx);
        return SleveenUserIdentity.RemapPerformers(raw);
    }

    /// <summary>下一跳预览：按角色键展开为 User 列表（Account / RealName）。</summary>
    public IList<User> PreviewUsersByRoleIds(string[] roleIds)
    {
        var list = new List<User>();
        if (roleIds == null || roleIds.Length == 0) return list;
        var host = ExpandHostRoles(roleIds);
        if (host.Count == 0) return list;
        var names = SleveenUserIdentity.LoadActiveAccounts();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var account in host)
        {
            if (string.IsNullOrWhiteSpace(account) || !seen.Add(account)) continue;
            var name = names.TryGetValue(account, out var n) ? n : account;
            list.Add(new User { UserId = account, UserName = name });
        }
        return list;
    }

    PerformerList ToPerformers(IEnumerable<string> accounts)
    {
        var names = SleveenUserIdentity.LoadActiveAccounts();
        var list = new PerformerList();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var account in accounts)
        {
            if (string.IsNullOrWhiteSpace(account) || !seen.Add(account))
                continue;
            var name = names.TryGetValue(account, out var n) ? n : account;
            list.Add(new Performer(account, name) { GroupRole = GroupRoles.Member });
        }
        return list;
    }

    List<string> ExpandHostRoles(string[] roleIds)
    {
        var accounts = new List<string>();
        SQLBuilder kit;
        try
        {
            kit = DBCash.useSQL(MasterDbPosition);
        }
        catch
        {
            return accounts;
        }

        foreach (var roleId in roleIds)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                continue;
            var bagId = ResolveVisitorBagId(kit, roleId.Trim());
            if (bagId == null)
                continue;
            accounts.AddRange(ExpandVisitorBag(kit, bagId.Value));
        }

        return accounts;
    }

    static Guid? ResolveVisitorBagId(SQLBuilder kit, string roleKey)
    {
        // AuthVisitorBag by Id / Code
        if (Guid.TryParse(roleKey, out var bagGuid))
        {
            var byId = kit.clear()
                .setTable("AuthVisitorBag")
                .select("Avb_Id")
                .where("Avb_Id", bagGuid)
                .where("(Avb_IsDeleted=0 or Avb_IsDeleted is null)")
                .query();
            if (byId.Rows.Count > 0)
                return bagGuid;
        }

        var byCode = kit.clear()
            .setTable("AuthVisitorBag")
            .select("Avb_Id")
            .where("Avb_Code", roleKey)
            .where("(Avb_IsDeleted=0 or Avb_IsDeleted is null)")
            .query();
        if (byCode.Rows.Count > 0)
            return byCode.Rows[0].Field<Guid>("Avb_Id");

        // AuthDutyItem → VisitorBagId
        if (Guid.TryParse(roleKey, out var dutyGuid))
        {
            var dutyById = kit.clear()
                .setTable("AuthDutyItem")
                .select("Adi_VisitorBagId")
                .where("Adi_Id", dutyGuid)
                .where("(Adi_IsDeleted=0 or Adi_IsDeleted is null)")
                .query();
            if (dutyById.Rows.Count > 0 && dutyById.Rows[0]["Adi_VisitorBagId"] != DBNull.Value)
                return dutyById.Rows[0].Field<Guid>("Adi_VisitorBagId");
        }

        var dutyByCode = kit.clear()
            .setTable("AuthDutyItem")
            .select("Adi_VisitorBagId")
            .where("Adi_Code", roleKey)
            .where("(Adi_IsDeleted=0 or Adi_IsDeleted is null)")
            .query();
        if (dutyByCode.Rows.Count > 0 && dutyByCode.Rows[0]["Adi_VisitorBagId"] != DBNull.Value)
            return dutyByCode.Rows[0].Field<Guid>("Adi_VisitorBagId");

        return null;
    }

    IEnumerable<string> ExpandVisitorBag(SQLBuilder kit, Guid bagId)
    {
        var items = kit.clear()
            .setTable("AuthVisitorItem")
            .select("Avi_Type,Avi_Code,Avi_Oid")
            .where("Avi_VisitorBagId", bagId)
            .where("(Avi_IsOn=1 or Avi_IsOn is null)")
            .query();

        for (var i = 0; i < items.Rows.Count; i++)
        {
            var row = items.Rows[i];
            var type = Convert.ToString(row["Avi_Type"]) ?? "";
            if (type == "1")
            {
                var account = Convert.ToString(row["Avi_Code"]);
                if (!string.IsNullOrWhiteSpace(account))
                    yield return account!;
                continue;
            }

            if (type == "2")
            {
                var oid = Convert.ToString(row["Avi_Oid"]);
                if (Guid.TryParse(oid, out var orgId))
                {
                    foreach (var a in LookupAccountsByOrgId(kit, orgId))
                        yield return a;
                }
                continue;
            }

            if (type == "3")
            {
                var prefix = Convert.ToString(row["Avi_Code"]);
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    foreach (var a in LookupAccountsByOrgCodePrefix(kit, prefix!))
                        yield return a;
                }
            }
        }
    }

    static IEnumerable<string> LookupAccountsByOrgId(SQLBuilder kit, Guid orgId)
    {
        var userRows = kit.clear()
            .setTable("SysUser")
            .select("Su_Account")
            .where("Su_OrgId", orgId)
            .where("Su_Status", 1)
            .query();
        for (var i = 0; i < userRows.Rows.Count; i++)
        {
            var account = Convert.ToString(userRows.Rows[i]["Su_Account"]);
            if (!string.IsNullOrWhiteSpace(account))
                yield return account!;
        }
    }

    static IEnumerable<string> LookupAccountsByOrgCodePrefix(SQLBuilder kit, string prefix)
    {
        var orgRows = kit.clear()
            .setTable("SysOrg")
            .select("So_Id")
            .whereLikeLeft("So_Code", prefix)
            .query();
        for (var i = 0; i < orgRows.Rows.Count; i++)
        {
            var orgId = orgRows.Rows[i].Field<Guid>("So_Id");
            foreach (var a in LookupAccountsByOrgId(kit, orgId))
                yield return a;
        }
    }
}
