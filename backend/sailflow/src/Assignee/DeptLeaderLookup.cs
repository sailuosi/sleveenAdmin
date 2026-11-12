using System.Data;
using System.Diagnostics;
using mooSQL.data;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// 部门领导查找（供 Type=<c>deptBoss</c> 使用）：按负责人角色袋在发起人组织链上找人；
/// 未配置或未命中时回落同组织其他账号（弱实现）。
/// </summary>
public sealed class DeptLeaderLookup
{
    readonly int _masterDbPosition;
    readonly string[] _leaderRoleIds;

    public DeptLeaderLookup(
        IEnumerable<string>? leaderRoleIds = null,
        int masterDbPosition = MooSqlSetup.DefaultPosition)
    {
        _masterDbPosition = masterDbPosition;
        _leaderRoleIds = (leaderRoleIds ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<string> LeaderRoleIds => _leaderRoleIds;

    /// <summary>返回负责人 Account 列表（去重，不含发起人）。</summary>
    public List<string> FindLeaders(string starterAccount)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(starterAccount))
            return result;

        try
        {
            var kit = DBCash.useSQL(_masterDbPosition);
            var orgId = LookupUserOrgId(kit, starterAccount);
            if (orgId == null)
                return result;

            var orgChain = BuildOrgChain(kit, orgId.Value);

            if (_leaderRoleIds.Length > 0)
            {
                var byRole = ExpandLeaderRoles(kit)
                    .Where(a => !string.Equals(a, starterAccount, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var orgByAccount = LookupUserOrgIds(kit, byRole);
                foreach (var account in byRole)
                {
                    if (orgByAccount.TryGetValue(account, out var uOrg) && orgChain.Contains(uOrg))
                        result.Add(account);
                }

                if (result.Count > 0)
                {
                    Trace.TraceInformation(
                        "[Assignee.Host] DeptLeader via roles={0} hit={1} starter={2}",
                        string.Join(",", _leaderRoleIds), result.Count, starterAccount);
                    return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }
            }

            // 弱回落：同组织其他账号取一人
            var peers = LookupAccountsInOrg(kit, orgId.Value)
                .Where(a => !string.Equals(a, starterAccount, StringComparison.OrdinalIgnoreCase))
                .Take(1)
                .ToList();
            if (peers.Count > 0)
            {
                Trace.TraceInformation(
                    "[Assignee.Host] DeptLeader fallback peer org={0} starter={1}",
                    orgId, starterAccount);
            }

            return peers;
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("[Assignee.Host] DeptLeaderLookup: {0}", ex.Message);
            return result;
        }
    }

    List<string> ExpandLeaderRoles(SQLBuilder kit)
    {
        // 复用 RoleGroup 宿主展开（VisitorBag / Duty）
        var rg = new SleveenRoleGroupAssigneeResolver { MasterDbPosition = _masterDbPosition };
        var users = rg.PreviewUsersByRoleIds(_leaderRoleIds);
        return users.Select(u => u.UserId)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()!;
    }

    static Guid? LookupUserOrgId(SQLBuilder kit, string account)
    {
        var map = LookupUserOrgIds(kit, new[] { account });
        return map.TryGetValue(account, out var org) ? org : null;
    }

    /// <summary>批量 Account → OrgId，避免领导人逐条查库。</summary>
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

        // mooSQL whereIn；失败则逐条回落
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

            return map;
        }
        catch (Exception ex)
        {
            Trace.TraceWarning("[Assignee.Host] LookupUserOrgIds batch failed, fallback: {0}", ex.Message);
            foreach (var account in distinct)
            {
                var rows = kit.clear()
                    .setTable("SysUser")
                    .select("Su_OrgId")
                    .where("Su_Account", account)
                    .where("Su_Status", 1)
                    .query();
                if (rows.Rows.Count == 0) continue;
                var orgObj = rows.Rows[0]["Su_OrgId"];
                if (orgObj is Guid g)
                    map[account] = g;
            }

            return map;
        }
    }

    static HashSet<Guid> BuildOrgChain(SQLBuilder kit, Guid orgId)
    {
        var set = new HashSet<Guid> { orgId };
        var current = orgId;
        for (var i = 0; i < 16; i++)
        {
            var rows = kit.clear()
                .setTable("SysOrg")
                .select("So_Pid")
                .where("So_Id", current)
                .query();
            if (rows.Rows.Count == 0) break;
            var pidObj = rows.Rows[0]["So_Pid"];
            if (pidObj == null || pidObj is DBNull) break;
            if (pidObj is not Guid pid || pid == Guid.Empty) break;
            if (!set.Add(pid)) break;
            current = pid;
        }

        return set;
    }

    static IEnumerable<string> LookupAccountsInOrg(SQLBuilder kit, Guid orgId)
    {
        var rows = kit.clear()
            .setTable("SysUser")
            .select("Su_Account")
            .where("Su_OrgId", orgId)
            .where("Su_Status", 1)
            .query();
        for (var i = 0; i < rows.Rows.Count; i++)
        {
            var account = Convert.ToString(rows.Rows[i]["Su_Account"]);
            if (!string.IsNullOrWhiteSpace(account))
                yield return account!;
        }
    }
}
