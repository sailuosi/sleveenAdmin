using System.Diagnostics;
using mooSQL.data;
using sailflow.assignee;
using sailflow.engine.Client;
using sailflow.engine.Common;
using SleveenAdmin.Sailflow.Entity;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// 委托代批：对 Resolve 原始 Performer 查生效委托，Add 代理人并填 ProxySignId。
/// 不接力；窗外/停用/自委托跳过；ProcessId 精确优先于全局空；同作用域取最新。
/// </summary>
public sealed class SurrogateAssigneeFilter : IAssigneeResolveFilter
{
    readonly SailClient _sail;

    public SurrogateAssigneeFilter(SailClient sail)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
    }

    public void Apply(AssigneeFilterContext ctx)
    {
        if (ctx.Performers == null || ctx.Performers.Count == 0)
            return;

        // 仅原始根席（无 ProxySignId）；不接力已并入的代理人
        var roots = ctx.Performers
            .Where(p => p != null
                        && !string.IsNullOrWhiteSpace(p.UserId)
                        && string.IsNullOrWhiteSpace(p.ProxySignId))
            .ToList();
        if (roots.Count == 0)
            return;

        var processId = ctx.Request?.ProcessId?.Trim() ?? "";
        var now = DateTime.UtcNow;
        var existing = new HashSet<string>(
            ctx.Performers
                .Where(p => !string.IsNullOrWhiteSpace(p?.UserId))
                .Select(p => p!.UserId.Trim()),
            StringComparer.OrdinalIgnoreCase);

        var added = 0;
        try
        {
            using var kit = _sail.UseRunSQL();
            foreach (var root in roots)
            {
                var row = FindActiveSurrogate(kit, root.UserId.Trim(), processId, now);
                if (row == null)
                    continue;
                if (string.IsNullOrWhiteSpace(row.AgentId))
                    continue;
                if (string.Equals(row.AgentId, root.UserId, StringComparison.OrdinalIgnoreCase))
                    continue; // 禁自委托
                if (!existing.Add(row.AgentId.Trim()))
                    continue; // 已在列表中则不重复并入

                var weight = row.Weight > 0 ? row.Weight : 1m;
                ctx.Performers.Add(new Performer(row.AgentId.Trim(), row.AgentName ?? row.AgentId)
                {
                    Weight = weight,
                    ProxySignId = root.UserId.Trim(),
                    GroupId = root.GroupId,
                    GroupName = root.GroupName,
                    GroupRole = root.GroupRole,
                    UnitId = root.UnitId,
                    UnitCode = root.UnitCode,
                    UnitName = root.UnitName
                });
                added++;
            }
        }
        catch (Exception ex)
        {
            Trace.TraceWarning(
                "[Assignee.Host] SurrogateAssigneeFilter failed (skip merge): {0}",
                ex.Message);
            return;
        }

        if (added > 0)
        {
            if (string.Equals(ctx.Source, PerformerSources.Specific, StringComparison.OrdinalIgnoreCase))
                ctx.RewroteSpecific = true;
            Trace.TraceInformation(
                "[Assignee.Host] SurrogateAssigneeFilter added {0} agent(s); ProcessId={1}",
                added, processId);
        }
    }

    /// <summary>
    /// 精确 ProcessId 优先；否则全局空 ProcessId。同作用域取 UpdatedAt/CreatedAt 最新且 enabled=1、窗内。
    /// </summary>
    static SailSurrogateEntity? FindActiveSurrogate(
        SQLBuilder kit,
        string grantorId,
        string processId,
        DateTime nowUtc)
    {
        var clip = kit.useClip();
        clip.from<SailSurrogateEntity>(out var s)
            .where(() => s.GrantorId, grantorId);

        var list = (clip.select(s).queryList() ?? new List<SailSurrogateEntity>()).ToList();
        if (list.Count < 1)
            return null;

        static bool InWindow(SailSurrogateEntity r, DateTime now)
        {
            var flag = r.Enabled;
            if (flag != 1)
                return false;
            if (r.StartAt.HasValue && r.StartAt.Value > now)
                return false;
            if (r.EndAt.HasValue && r.EndAt.Value < now)
                return false;
            return true;
        }

        var scoped = list
            .Where(r => InWindow(r, nowUtc))
            .ToList();
        if (scoped.Count < 1)
            return null;

        SailSurrogateEntity? PickLatest(IEnumerable<SailSurrogateEntity> rows) =>
            rows
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt ?? DateTime.MinValue)
                .ThenByDescending(r => r.Id)
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(processId))
        {
            var exact = PickLatest(scoped.Where(r =>
                string.Equals(r.ProcessId?.Trim(), processId, StringComparison.OrdinalIgnoreCase)));
            if (exact != null)
                return exact;
        }

        return PickLatest(scoped.Where(r => string.IsNullOrWhiteSpace(r.ProcessId)));
    }
}
