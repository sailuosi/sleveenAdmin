using System.Data;
using System.Diagnostics;
using mooSQL.data;
using sailflow.hub.Resource;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Sailflow.Assignee;

namespace SleveenAdmin.Sailflow.Resource;

/// <summary>
/// 宿主具化的 <see cref="IDeptService"/>（经 <c>UseSleveenOrgResource</c> 注入 Locator）：
/// 用 <c>SysOrg</c>/<c>SysUser</c> + AuthVisitorBag 实现职级过滤。
/// 身份键为 <c>SysUser.Account</c>。
/// <list type="bullet">
/// <item>1 Superior — 角色成员 ∩ 上级组织（祖先 So_Pid 链，不含本组织）</item>
/// <item>2 Compeer — 角色成员 ∩ 同组织（排除本人）</item>
/// <item>3 Subordinate — 角色成员 ∩ 下级组织（子孙，不含本组织）</item>
/// </list>
/// </summary>
public sealed class SleveenDeptService : IDeptService
{
    public int MasterDbPosition { get; set; } = MooSqlSetup.DefaultPosition;

    public IList<User> GetUserListByDeptRank(string[] roleIds, string curUserId, int receiverType)
    {
        if (receiverType is < 1 or > 3)
            return new List<User>();
        if (string.IsNullOrWhiteSpace(curUserId) || roleIds == null || roleIds.Length == 0)
            return new List<User>();

        try
        {
            var roleUsers = new SleveenRoleGroupAssigneeResolver { MasterDbPosition = MasterDbPosition }
                .PreviewUsersByRoleIds(roleIds);
            if (roleUsers.Count == 0)
                return new List<User>();

            var kit = DBCash.useSQL(MasterDbPosition);
            var curOrgId = LookupUserOrgId(kit, curUserId.Trim());
            if (curOrgId == null)
            {
                Trace.TraceWarning("[Resource.Host] SleveenDeptService: no org for account={0}", curUserId);
                return new List<User>();
            }

            var orgIndex = LoadOrgIndex(kit);
            var targetOrgs = receiverType switch
            {
                1 => GetAncestorOrgs(orgIndex, curOrgId.Value),
                2 => new HashSet<Guid> { curOrgId.Value },
                3 => GetDescendantOrgs(orgIndex, curOrgId.Value),
                _ => new HashSet<Guid>()
            };

            if (targetOrgs.Count == 0)
                return new List<User>();

            var accounts = roleUsers
                .Select(u => u.UserId)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()!;

            var orgByAccount = LookupUserOrgIds(kit, accounts);
            var names = SleveenUserIdentity.LoadActiveAccounts();
            var result = new List<User>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var account in accounts)
            {
                if (string.Equals(account, curUserId, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!orgByAccount.TryGetValue(account!, out var orgId) || !targetOrgs.Contains(orgId))
                    continue;
                if (!seen.Add(account!))
                    continue;

                var name = names.TryGetValue(account!, out var n) ? n : account!;
                result.Add(new User { UserId = account, UserName = name });
            }

            Trace.TraceInformation(
                "[Resource.Host] SleveenDeptService rank={0} account={1} hit={2}",
                receiverType, curUserId, result.Count);
            return result;
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("[Resource.Host] SleveenDeptService: {0}", ex.Message);
            return new List<User>();
        }
    }

    static Guid? LookupUserOrgId(SQLBuilder kit, string account)
    {
        var map = LookupUserOrgIds(kit, new[] { account });
        return map.TryGetValue(account, out var org) ? org : null;
    }

    static Dictionary<string, Guid> LookupUserOrgIds(SQLBuilder kit, IReadOnlyList<string> accounts)
    {
        var map = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        if (accounts == null || accounts.Count == 0)
            return map;

        var distinct = accounts
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (distinct.Count == 0)
            return map;

        try
        {
            var rows = kit.clear()
                .setTable("SysUser")
                .select("Su_Account,Su_OrgId")
                .whereIn("Su_Account", distinct)
                .where("Su_Status", 1)
                .query();
            for (var i = 0; i < rows.Rows.Count; i++)
            {
                var account = Convert.ToString(rows.Rows[i]["Su_Account"]);
                var orgObj = rows.Rows[i]["Su_OrgId"];
                if (string.IsNullOrWhiteSpace(account) || orgObj == null || orgObj is DBNull)
                    continue;
                if (orgObj is Guid g)
                    map[account!] = g;
            }
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("[Resource.Host] LookupUserOrgIds failed: {0}", ex.Message);
        }

        return map;
    }

    sealed class OrgIndex
    {
        public Dictionary<Guid, Guid?> Parent { get; } = new();
        public Dictionary<Guid, List<Guid>> Children { get; } = new();
    }

    static OrgIndex LoadOrgIndex(SQLBuilder kit)
    {
        var index = new OrgIndex();
        var rows = kit.clear()
            .setTable("SysOrg")
            .select("So_Id,So_Pid")
            .query();
        for (var i = 0; i < rows.Rows.Count; i++)
        {
            var idObj = rows.Rows[i]["So_Id"];
            if (idObj is not Guid id)
                continue;
            Guid? pid = null;
            var pidObj = rows.Rows[i]["So_Pid"];
            if (pidObj is Guid g && g != Guid.Empty)
                pid = g;
            index.Parent[id] = pid;
            if (pid != null)
            {
                if (!index.Children.TryGetValue(pid.Value, out var list))
                {
                    list = new List<Guid>();
                    index.Children[pid.Value] = list;
                }
                list.Add(id);
            }
        }
        return index;
    }

    static HashSet<Guid> GetAncestorOrgs(OrgIndex index, Guid orgId)
    {
        var set = new HashSet<Guid>();
        var current = orgId;
        for (var i = 0; i < 32; i++)
        {
            if (!index.Parent.TryGetValue(current, out var pid) || pid == null)
                break;
            if (!set.Add(pid.Value))
                break;
            current = pid.Value;
        }
        return set;
    }

    static HashSet<Guid> GetDescendantOrgs(OrgIndex index, Guid orgId)
    {
        var set = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(orgId);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (!index.Children.TryGetValue(cur, out var kids))
                continue;
            foreach (var kid in kids)
            {
                if (set.Add(kid))
                    queue.Enqueue(kid);
            }
        }
        return set;
    }
}
