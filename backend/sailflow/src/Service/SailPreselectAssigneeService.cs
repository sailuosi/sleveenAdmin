using sailflow.utils;
using sailflow.service;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Assignee;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>预选办理人产品门面（独立于 Wf2Xml）。</summary>
[Scoped]
public class SailPreselectAssigneeService
{
    readonly SleveenPreselectAssigneeStore _store;
    readonly IWorkflowService _wf;

    public SailPreselectAssigneeService(SleveenPreselectAssigneeStore store, IWorkflowService wf)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _wf = wf ?? throw new ArgumentNullException(nameof(wf));
    }

    public PerformerList Load(string processInstanceId, string activityId) =>
        _store.Load(processInstanceId, activityId);

    public void Replace(string processInstanceId, string activityId, PerformerList performers) =>
        _store.Replace(processInstanceId, activityId, performers);

    public void Clear(string processInstanceId, string activityId) =>
        _store.Clear(processInstanceId, activityId);

    public void ClearAll(string processInstanceId) =>
        _store.ClearAll(processInstanceId);

    public int PersistFromPerformerMap(
        string? processInstanceId,
        IDictionary<string, PerformerList>? map)
    {
        if (string.IsNullOrWhiteSpace(processInstanceId) || map == null || map.Count == 0)
            return 0;

        var n = 0;
        foreach (var kv in map)
        {
            if (string.IsNullOrWhiteSpace(kv.Key))
                continue;
            _store.Replace(processInstanceId, kv.Key.Trim(), kv.Value ?? new PerformerList());
            n++;
        }

        return n;
    }

    /// <summary>扫描流程定义中 Type=Preselect 的用户任务。</summary>
    public IList<PreselectTargetDto> ListTargets(string? processId, string? version)
    {
        if (string.IsNullOrWhiteSpace(processId))
            return Array.Empty<PreselectTargetDto>();

        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version.Trim();
        var file = _wf.GetProcessFile(processId.Trim(), ver);
        var xml = file?.XmlContent;
        return SailPreselectTargetCatalog.ReadFromXml(xml)
            .Select(t => new PreselectTargetDto
            {
                ActivityId = t.ActivityId,
                ActivityName = t.ActivityName,
                Required = t.Required,
                AllowMulti = t.AllowMulti,
                ParamsJson = t.ParamsJson
            })
            .ToList();
    }

    /// <summary>
    /// 办理条：下一跳若为 Preselect 且表中已有人 → SkipManualPick。
    /// </summary>
    public PreselectNextProbeDto ProbeNext(
        string? processId,
        string? version,
        string? processInstanceId,
        string? activityId)
    {
        var dto = new PreselectNextProbeDto
        {
            ActivityId = activityId?.Trim() ?? ""
        };
        if (string.IsNullOrWhiteSpace(activityId))
            return dto;

        var targets = ListTargets(processId, version);
        var hit = targets.FirstOrDefault(t =>
            string.Equals(t.ActivityId, activityId.Trim(), StringComparison.OrdinalIgnoreCase));
        if (hit == null)
            return dto;

        dto.IsPreselectTarget = true;
        if (string.IsNullOrWhiteSpace(processInstanceId))
            return dto;

        var list = _store.Load(processInstanceId.Trim(), activityId.Trim());
        dto.Performers = list.Select(PreselectPerformerDto.From).ToList();
        dto.HasPerformers = dto.Performers.Count > 0;
        dto.SkipManualPick = dto.HasPerformers;
        return dto;
    }
}
