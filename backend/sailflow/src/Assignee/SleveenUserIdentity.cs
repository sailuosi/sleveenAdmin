using System.Data;
using mooSQL.data;
using sailflow.engine.Common;
using sailflow.hub.Resource;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Sailflow.Dto;
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

    /// <summary>预载 Users 上限；超额改 search 并清空列表。</summary>
    public const int MaxInlineCandidateUsers = 100;

    /// <summary>
    /// 对齐 Account + 角色展开；不再全表回填。
    /// 写出 CandidateMode / Count / Filter / 默认 Selector。
    /// </summary>
    public static void EnrichNodeCandidateUsers(NodeView? node)
    {
        if (node == null) return;
        var dto = NextActivityNodeDto.FromNodeView(node);
        EnrichNextActivityNode(dto);
        node.Users = dto.Users;
    }

    /// <summary>用 BPMN sf:CandidateSelector 覆盖推断出的选人器（若有配置）。</summary>
    public static void ApplyCandidateSelectorOverride(NextActivityNodeDto dto, string? type, string? parms)
    {
        if (dto == null) return;
        if (!string.IsNullOrWhiteSpace(type))
            dto.CandidateSelectorType = type.Trim();
        if (!string.IsNullOrWhiteSpace(parms))
            dto.CandidateSelectorParams = parms.Trim();
        if (string.Equals(dto.CandidateSelectorType, "HostUserSearch", StringComparison.OrdinalIgnoreCase)
            && dto.CandidateMode == "bound"
            && (dto.Users == null || dto.Users.Count == 0))
        {
            dto.CandidateMode = "anyselect";
        }
    }

    public static NextActivityNodeDto EnrichNextActivityNode(NextActivityNodeDto dto)
    {
        var roleIds = dto.Roles?
            .Where(r => r != null && !string.IsNullOrWhiteSpace(r.Id))
            .Select(r => r.Id!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        RemapUsers(dto.Users);

        IList<User>? users = dto.Users;
        if (users == null || users.Count == 0)
        {
            if (roleIds.Length > 0)
            {
                var fromRoles = new SleveenRoleGroupAssigneeResolver().PreviewUsersByRoleIds(roleIds);
                if (fromRoles.Count > 0)
                    users = fromRoles;
            }
        }

        if (users is { Count: > 0 })
        {
            dto.CandidateCount = users.Count;
            dto.CandidateFilter = roleIds.Length > 0
                ? new CandidateFilterDto { RoleIds = roleIds, ReceiverType = (int)dto.ReceiverType }
                : null;
            if (users.Count > MaxInlineCandidateUsers)
            {
                dto.CandidateMode = "search";
                dto.Users = new List<User>();
                dto.CandidateSelectorType = string.IsNullOrWhiteSpace(dto.CandidateSelectorType)
                    ? "RoleUserSearch"
                    : dto.CandidateSelectorType;
                if (string.IsNullOrWhiteSpace(dto.CandidateSelectorParams) && roleIds.Length > 0)
                    dto.CandidateSelectorParams = "{\"roleIds\":" + ToJsonArray(roleIds) + "}";
            }
            else
            {
                dto.CandidateMode = "bound";
                dto.Users = users;
                dto.CandidateSelectorType = string.IsNullOrWhiteSpace(dto.CandidateSelectorType)
                    ? "InlineList"
                    : dto.CandidateSelectorType;
            }
            return dto;
        }

        // 空名单：ApproverPick / 未配角色 → anyselect，禁止全员塞入
        dto.Users = new List<User>();
        dto.CandidateCount = 0;
        dto.CandidateMode = "anyselect";
        dto.CandidateFilter = roleIds.Length > 0
            ? new CandidateFilterDto { RoleIds = roleIds, ReceiverType = (int)dto.ReceiverType }
            : null;
        dto.CandidateSelectorType = string.IsNullOrWhiteSpace(dto.CandidateSelectorType)
            ? "HostUserSearch"
            : dto.CandidateSelectorType;
        if (string.IsNullOrWhiteSpace(dto.CandidateSelectorParams))
            dto.CandidateSelectorParams = "{\"pageSize\":100}";
        return dto;
    }

    static string ToJsonArray(string[] items)
    {
        var parts = items.Select(x => "\"" + (x ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
        return "[" + string.Join(",", parts) + "]";
    }

    /// <summary>按关键字分页查宿主启用账号（Account/RealName）。</summary>
    public static (List<User> Users, int Total) QueryHostUsers(string? keyword, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, MaxInlineCandidateUsers);
        var all = LoadActiveAccounts()
            .Select(kv => new User { UserId = kv.Key, UserName = kv.Value })
            .ToList();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            all = all
                .Where(u =>
                    (u.UserId?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.UserName?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        var total = all.Count;
        var slice = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (slice, total);
    }

    /// <summary>角色范围内关键字分页。</summary>
    public static (List<User> Users, int Total) QueryRoleUsers(string[]? roleIds, string? keyword, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, MaxInlineCandidateUsers);
        var list = roleIds is { Length: > 0 }
            ? new SleveenRoleGroupAssigneeResolver().PreviewUsersByRoleIds(roleIds).ToList()
            : new List<User>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            list = list
                .Where(u =>
                    (u.UserId?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (u.UserName?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        var total = list.Count;
        var slice = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (slice, total);
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
