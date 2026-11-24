using mooSQL.data;
using sailflow.assignee;
using sailflow.data;
using sailflow.hosting;
using sailflow.utils;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Entity;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>宿主具化：<c>sf_preselect_assignee</c> → <see cref="IPreselectAssigneeStore"/>。</summary>
[Scoped]
public class SleveenPreselectAssigneeStore : IPreselectAssigneeStore
{
    readonly SailClient _sail;

    public SleveenPreselectAssigneeStore(SailClient sail) =>
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));

    public PerformerList Load(string processInstanceId, string activityId)
    {
        var list = new PerformerList();
        if (string.IsNullOrWhiteSpace(processInstanceId) || string.IsNullOrWhiteSpace(activityId))
            return list;

        var rows = QueryRows(processInstanceId.Trim(), activityId.Trim());
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.UserId))
                continue;
            list.Add(new Performer(row.UserId.Trim(), row.UserName ?? row.UserId)
            {
                GroupId = row.GroupId,
                GroupName = row.GroupName,
                GroupRole = row.GroupRole,
                UnitId = row.UnitId,
                UnitCode = row.UnitCode,
                UnitName = row.UnitName,
                Weight = row.Weight > 0 ? row.Weight : 1m
            });
        }

        return list;
    }

    public void Replace(string processInstanceId, string activityId, PerformerList performers)
    {
        if (string.IsNullOrWhiteSpace(processInstanceId) || string.IsNullOrWhiteSpace(activityId))
            return;

        var pi = processInstanceId.Trim();
        var aid = activityId.Trim();
        Clear(pi, aid);

        if (performers == null || performers.Count == 0)
            return;

        var now = DateTime.Now;
        var rows = new List<SailPreselectAssigneeEntity>();
        var sort = 0;
        foreach (var p in performers)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.UserId))
                continue;
            rows.Add(new SailPreselectAssigneeEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                ProcessInstanceId = pi,
                ActivityId = aid,
                UserId = p.UserId.Trim(),
                UserName = string.IsNullOrWhiteSpace(p.UserName) ? p.UserId.Trim() : p.UserName.Trim(),
                GroupId = NullIfEmpty(p.GroupId),
                GroupName = NullIfEmpty(p.GroupName),
                GroupRole = NullIfEmpty(p.GroupRole),
                UnitId = NullIfEmpty(p.UnitId),
                UnitCode = NullIfEmpty(p.UnitCode),
                UnitName = NullIfEmpty(p.UnitName),
                Weight = p.Weight > 0 ? p.Weight : 1m,
                SortNo = sort++,
                UpdatedAt = now
            });
        }

        if (rows.Count == 0)
            return;

        var repo = _sail.UseRunSQL().useRepo<SailPreselectAssigneeEntity>();
        repo.InsertRange(rows);
    }

    public void Clear(string processInstanceId, string activityId)
    {
        if (string.IsNullOrWhiteSpace(processInstanceId) || string.IsNullOrWhiteSpace(activityId))
            return;

        var pi = processInstanceId.Trim();
        var aid = activityId.Trim();
        var repo = _sail.UseRunSQL().useRepo<SailPreselectAssigneeEntity>();
        repo.Delete(x => x.ProcessInstanceId == pi && x.ActivityId == aid);
    }

    /// <summary>清除某实例下全部预选（可选管理能力）。</summary>
    public void ClearAll(string processInstanceId)
    {
        if (string.IsNullOrWhiteSpace(processInstanceId))
            return;
        var pi = processInstanceId.Trim();
        _sail.UseRunSQL().useRepo<SailPreselectAssigneeEntity>()
            .Delete(x => x.ProcessInstanceId == pi);
    }

    List<SailPreselectAssigneeEntity> QueryRows(string processInstanceId, string activityId)
    {
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailPreselectAssigneeEntity>(out var t);
        clip.where(() => t.ProcessInstanceId, processInstanceId);
        clip.where(() => t.ActivityId, activityId);
        clip.orderBy(() => t.SortNo);
        return clip.select(t).queryList()?.ToList() ?? new List<SailPreselectAssigneeEntity>();
    }

    static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
