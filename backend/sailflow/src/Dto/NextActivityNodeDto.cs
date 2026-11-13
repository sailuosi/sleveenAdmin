using sailflow.engine.Common;
using sailflow.hub.Resource;

namespace SleveenAdmin.Sailflow.Dto;

/// <summary>下一节点 + 选人元数据（瘦身 Users，避免全员回填卡死）。</summary>
public class NextActivityNodeDto
{
    public string? ActivityId { get; set; }
    public string? ActivityName { get; set; }
    public ActivityTypeEnum ActivityType { get; set; }
    public string? ActivityCode { get; set; }
    public string? ActivityUrl { get; set; }
    public string? MyProperties { get; set; }
    public IList<Role>? Roles { get; set; }
    public IList<User>? Users { get; set; }
    public IList<Partaker>? Partakers { get; set; }
    public bool IsSkipTo { get; set; }
    public ReceiverTypeEnum ReceiverType { get; set; }
    public bool Disabled { get; set; }
    public bool Required { get; set; }
    public string? ReasonCode { get; set; }

    /// <summary>bound | anyselect | search</summary>
    public string CandidateMode { get; set; } = "anyselect";

    public int CandidateCount { get; set; }

    public CandidateFilterDto? CandidateFilter { get; set; }

    /// <summary>HostUserSearch | InlineList | RoleUserSearch</summary>
    public string? CandidateSelectorType { get; set; }

    public string? CandidateSelectorParams { get; set; }

    public static NextActivityNodeDto FromNodeView(NodeView node)
    {
        return new NextActivityNodeDto
        {
            ActivityId = node.ActivityId,
            ActivityName = node.ActivityName,
            ActivityType = node.ActivityType,
            ActivityCode = node.ActivityCode,
            ActivityUrl = node.ActivityUrl,
            MyProperties = node.MyProperties,
            Roles = node.Roles,
            Users = node.Users,
            Partakers = node.Partakers,
            IsSkipTo = node.IsSkipTo,
            ReceiverType = node.ReceiverType,
            Disabled = node.Disabled,
            Required = node.Required,
            ReasonCode = node.ReasonCode
        };
    }
}

public class CandidateFilterDto
{
    public string[]? RoleIds { get; set; }
    public int? ReceiverType { get; set; }
}

/// <summary>QueryNextActivityCandidates 入参。</summary>
public class QueryNextActivityCandidatesRequest
{
    public string? ProcessId { get; set; }
    public string? Version { get; set; }
    public string? ActivityId { get; set; }
    public string? TaskId { get; set; }
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
    public string[]? RoleIds { get; set; }
    /// <summary>bound | anyselect | search；缺省 anyselect</summary>
    public string? CandidateMode { get; set; }
    public string? CandidateSelectorType { get; set; }
}

public class QueryNextActivityCandidatesResult
{
    public List<User> Users { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
