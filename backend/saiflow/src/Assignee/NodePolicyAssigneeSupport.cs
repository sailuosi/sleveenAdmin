using System.Collections.Concurrent;
using saiflow.assignee;
using saiflow.assignee.Builtin;
using saiflow.engine.Assignee;
using saiflow.engine.Client;
using saiflow.engine.Service;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Assignee;

/// <summary>NodePolicy → Assignee Filter/Empty 共用装载与选型（R10）。</summary>
internal static class NodePolicyAssigneeSupport
{
    public const string BagApproveType = "approveType";
    public const string BagAssignEmpty = "assignEmptyHandlerType";
    public const string BagAssignStart = "assignStartUserHandlerType";
    public const string BagAssignStartResolveType = "assignStartResolveType";
    public const string BagAssignStartResolveParams = "assignStartResolveParams";
    public const string BagUserIds = "userIds";
    public const string BagAdminUserIds = "adminUserIds";

    /// <summary>processId|version|activityId → 已解析 NodePolicy（避免每 Resolve 重载 XML）。</summary>
    static readonly ConcurrentDictionary<string, SailNodePolicyDto> PolicyCache = new(StringComparer.OrdinalIgnoreCase);

    public static SailNodePolicyDto LoadPolicy(SailClient sail, AssigneeInvokeRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ProcessId))
                return SailNodePolicyCatalog.Merge(null);

            var version = string.IsNullOrWhiteSpace(request.Version) ? "1" : request.Version;
            var activityId = request.ActivityId ?? "";
            var cacheKey = request.ProcessId + "|" + version + "|" + activityId;
            if (PolicyCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var file = new WorkflowService(sail).GetProcessFile(request.ProcessId, version);
            var policy = SailNodePolicyCatalog.ParseForActivity(file?.XmlContent, request.ActivityId);
            PolicyCache[cacheKey] = policy;
            return policy;
        }
        catch
        {
            return SailNodePolicyCatalog.Merge(null);
        }
    }

    /// <summary>流程定义变更后可清缓存（发布钩子可选调用）。</summary>
    public static void InvalidatePolicyCache(string? processId = null, string? version = null)
    {
        if (string.IsNullOrWhiteSpace(processId))
        {
            PolicyCache.Clear();
            return;
        }

        var prefix = processId + "|" + (string.IsNullOrWhiteSpace(version) ? "" : version);
        foreach (var key in PolicyCache.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                || (string.IsNullOrWhiteSpace(version)
                    && key.StartsWith(processId + "|", StringComparison.OrdinalIgnoreCase)))
                PolicyCache.TryRemove(key, out _);
        }
    }

    public static IDictionary<string, object?> BuildPolicyBag(
        SailNodePolicyDto policy,
        IReadOnlyList<string> adminUserIds)
    {
        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            [BagApproveType] = policy.ApproveType,
            [BagAssignEmpty] = policy.AssignEmptyHandlerType,
            [BagAssignStart] = policy.AssignStartUserHandlerType,
            [BagAssignStartResolveType] = policy.AssignStartResolveType ?? "",
            [BagAssignStartResolveParams] = policy.AssignStartResolveParams ?? "",
            [BagUserIds] = (policy.AssignEmptyUserIds ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToArray(),
            [BagAdminUserIds] = adminUserIds.ToArray()
        };
    }

    /// <summary>
    /// 空人选型：approveType 2/3 优先；否则 assignEmptyHandlerType → EmptyActionKeys / 产品码。
    /// </summary>
    public static string SelectEmptyActionKey(IDictionary<string, object?>? bag)
    {
        var approve = GetInt(bag, BagApproveType, 1);
        if (approve == 2)
            return EmptyActionKeys.Skip;
        if (approve == 3)
            return NodePolicyEmptyActionKeys.AutoReject;

        var empty = GetInt(bag, BagAssignEmpty, 2);
        return empty switch
        {
            1 => EmptyActionKeys.Skip,
            3 => EmptyActionKeys.Users,
            4 => EmptyActionKeys.Admin,
            _ => EmptyActionKeys.Reject
        };
    }

    public static int GetInt(IDictionary<string, object?>? bag, string key, int fallback)
    {
        if (bag == null || !bag.TryGetValue(key, out var raw) || raw == null)
            return fallback;
        try
        {
            return Convert.ToInt32(raw);
        }
        catch
        {
            return fallback;
        }
    }

    public static string[] GetStringArray(IDictionary<string, object?>? bag, string key)
    {
        if (bag == null || !bag.TryGetValue(key, out var raw) || raw == null)
            return Array.Empty<string>();
        if (raw is string[] arr)
            return arr.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();
        if (raw is IEnumerable<string> es)
            return es.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToArray();
        if (raw is IEnumerable<object> eo)
            return eo.Select(o => Convert.ToString(o)?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Cast<string>()
                .ToArray();
        return Array.Empty<string>();
    }

    public static string GetString(IDictionary<string, object?>? bag, string key, string fallback = "")
    {
        if (bag == null || !bag.TryGetValue(key, out var raw) || raw == null)
            return fallback;
        var s = Convert.ToString(raw)?.Trim();
        return string.IsNullOrEmpty(s) ? fallback : s!;
    }
}

/// <summary>宿主 EmptyAction 扩展键（引擎 EmptyActionKeys 之外）。</summary>
internal static class NodePolicyEmptyActionKeys
{
    public const string AutoReject = "autoReject";
}
