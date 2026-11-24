using sailflow.utils;

namespace SleveenAdmin.Sailflow.Dto;

/// <summary>预选办理人读写请求（独立 api/WfPreselect）。</summary>
public class PreselectAssigneesRequest
{
    public string ProcessInstanceId { get; set; } = "";

    /// <summary>可选；Clear / Get 单节点时用。</summary>
    public string? ActivityId { get; set; }

    /// <summary>ActivityId → 办理人列表（Replace 时必填）。</summary>
    public IDictionary<string, List<PreselectPerformerDto>>? Performers { get; set; }
}

/// <summary>定义内 Preselect 目标节点。</summary>
public class PreselectTargetDto
{
    public string ActivityId { get; set; } = "";
    public string ActivityName { get; set; } = "";
    public bool Required { get; set; }
    public bool AllowMulti { get; set; } = true;
    public string? ParamsJson { get; set; }
}

/// <summary>办理预览：下一跳是否已可由预选表覆盖选人。</summary>
public class PreselectNextProbeDto
{
    public string ActivityId { get; set; } = "";
    public bool IsPreselectTarget { get; set; }
    public bool HasPerformers { get; set; }
    public bool SkipManualPick { get; set; }
    public List<PreselectPerformerDto> Performers { get; set; } = new();
}

public class PreselectPerformerDto
{
    public string UserId { get; set; } = "";
    public string? UserName { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? GroupRole { get; set; }
    public string? UnitId { get; set; }
    public string? UnitCode { get; set; }
    public string? UnitName { get; set; }
    /// <summary>票权；默认 1。</summary>
    public decimal Weight { get; set; } = 1m;

    public Performer ToPerformer() =>
        new(UserId?.Trim() ?? "", string.IsNullOrWhiteSpace(UserName) ? UserId?.Trim() ?? "" : UserName.Trim())
        {
            GroupId = GroupId,
            GroupName = GroupName,
            GroupRole = GroupRole,
            UnitId = UnitId,
            UnitCode = UnitCode,
            UnitName = UnitName,
            Weight = Weight > 0 ? Weight : 1m
        };

    public static PreselectPerformerDto From(Performer p) =>
        new()
        {
            UserId = p.UserId,
            UserName = p.UserName,
            GroupId = p.GroupId,
            GroupName = p.GroupName,
            GroupRole = p.GroupRole,
            UnitId = p.UnitId,
            UnitCode = p.UnitCode,
            UnitName = p.UnitName,
            Weight = p.Weight > 0 ? p.Weight : 1m
        };
}
