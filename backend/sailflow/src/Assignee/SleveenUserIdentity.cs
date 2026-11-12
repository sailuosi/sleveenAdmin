using System.Data;
using mooSQL.data;
using sailflow.engine.Common;
using sailflow.hub.Resource;
using SleveenAdmin.Foundation.Data;
using User = sailflow.hub.Resource.User;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>R3：把引擎/资源侧用户键对齐为宿主 <c>SysUser.Account</c>。</summary>
public static class SleveenUserIdentity
{
    public static int MasterDbPosition { get; set; } = MooSqlSetup.DefaultPosition;

    /// <summary>Account → RealName（仅 Status=1）。</summary>
    public static Dictionary<string, string> LoadActiveAccounts()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var kit = DBCash.useSQL(MasterDbPosition);
            var rows = kit.clear()
                .setTable("SysUser")
                .select("Su_Account,Su_RealName")
                .where("Su_Status", 1)
                .query();
            for (var i = 0; i < rows.Rows.Count; i++)
            {
                var account = Convert.ToString(rows.Rows[i]["Su_Account"]);
                if (string.IsNullOrWhiteSpace(account)) continue;
                var name = Convert.ToString(rows.Rows[i]["Su_RealName"]);
                map[account!] = string.IsNullOrWhiteSpace(name) ? account! : name!;
            }
        }
        catch
        {
            /* 主库未就绪时跳过 */
        }
        return map;
    }

    /// <summary>尝试将任意键解析为 Account（已是 Account / 匹配 RealName）。</summary>
    public static string? ToAccount(string? userId, string? userName, IReadOnlyDictionary<string, string>? accounts = null)
    {
        accounts ??= LoadActiveAccounts();
        if (!string.IsNullOrWhiteSpace(userId) && accounts.ContainsKey(userId))
            return userId;
        if (!string.IsNullOrWhiteSpace(userName) && accounts.ContainsKey(userName))
            return userName;

        if (!string.IsNullOrWhiteSpace(userName))
        {
            foreach (var kv in accounts)
            {
                if (string.Equals(kv.Value, userName, StringComparison.OrdinalIgnoreCase))
                    return kv.Key;
            }
        }

        return null;
    }

    public static void RemapUsers(IList<User>? users)
    {
        if (users == null || users.Count == 0) return;
        var accounts = LoadActiveAccounts();
        foreach (var u in users)
        {
            if (u == null) continue;
            var account = ToAccount(u.UserId, u.UserName, accounts);
            if (string.IsNullOrWhiteSpace(account)) continue;
            u.UserId = account;
            if (accounts.TryGetValue(account!, out var name) && !string.IsNullOrWhiteSpace(name))
                u.UserName = name;
        }
    }

    /// <summary>
    /// 下一跳预览补候选人：先 Account 重映射；仍空则按角色走宿主 RoleGroup 展开；
    /// 再空（典型 ApproverPick）则填入宿主 SysUser 列表，避免办理页选人下拉空白。
    /// </summary>
    public static void EnrichNodeCandidateUsers(NodeView? node)
    {
        if (node == null) return;
        RemapUsers(node.Users);
        if (node.Users is { Count: > 0 }) return;

        var roleIds = node.Roles?
            .Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id))
            .Select(r => r.Id!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();
        if (roleIds.Length > 0)
        {
            var fromRoles = new SleveenRoleGroupAssigneeResolver().PreviewUsersByRoleIds(roleIds);
            if (fromRoles.Count > 0)
            {
                node.Users = fromRoles;
                return;
            }
        }

        var accounts = LoadActiveAccounts();
        if (accounts.Count == 0) return;
        node.Users = accounts
            .Select(kv => new User { UserId = kv.Key, UserName = kv.Value })
            .ToList();
    }

    public static PerformerList RemapPerformers(PerformerList? list)
    {
        var result = new PerformerList();
        if (list == null || list.Count == 0) return result;
        var accounts = LoadActiveAccounts();
        foreach (var p in list)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.UserId)) continue;
            var account = ToAccount(p.UserId, p.UserName, accounts) ?? p.UserId;
            var name = p.UserName;
            if (accounts.TryGetValue(account, out var n) && !string.IsNullOrWhiteSpace(n))
                name = n;
            result.Add(new Performer(account, name ?? account)
            {
                GroupId = p.GroupId,
                GroupName = p.GroupName,
                GroupRole = p.GroupRole,
                UnitId = p.UnitId,
                UnitCode = p.UnitCode,
                UnitName = p.UnitName
            });
        }
        return result;
    }
}
