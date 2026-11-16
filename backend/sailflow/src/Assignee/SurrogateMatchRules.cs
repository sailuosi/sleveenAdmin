using SleveenAdmin.Sailflow.Entity;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>委托代批生效裁决（纯函数，供 Filter / 单测）。</summary>
public static class SurrogateMatchRules
{
    public static bool InWindow(SailSurrogateEntity row, DateTime nowUtc)
    {
        if (row == null) return false;
        if (row.Enabled != 1) return false;
        if (row.StartAt.HasValue && row.StartAt.Value > nowUtc) return false;
        if (row.EndAt.HasValue && row.EndAt.Value < nowUtc) return false;
        return true;
    }

    public static bool IsSelfDelegate(string grantorId, string agentId) =>
        !string.IsNullOrWhiteSpace(grantorId)
        && string.Equals(grantorId.Trim(), agentId?.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 精确 ProcessId 优先；否则全局空 ProcessId。同作用域取 UpdatedAt/CreatedAt 最新。
    /// </summary>
    public static SailSurrogateEntity? PickActive(
        IEnumerable<SailSurrogateEntity>? rows,
        string processId,
        DateTime nowUtc)
    {
        if (rows == null) return null;
        var scoped = rows.Where(r => InWindow(r, nowUtc)).ToList();
        if (scoped.Count == 0) return null;

        SailSurrogateEntity? PickLatest(IEnumerable<SailSurrogateEntity> bag) =>
            bag
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt ?? DateTime.MinValue)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(processId))
        {
            var exact = PickLatest(scoped.Where(r =>
                string.Equals(r.ProcessId?.Trim(), processId.Trim(), StringComparison.OrdinalIgnoreCase)));
            if (exact != null)
                return exact;
        }

        return PickLatest(scoped.Where(r => string.IsNullOrWhiteSpace(r.ProcessId)));
    }
}
