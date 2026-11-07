using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace SleveenAdmin.Saiflow.Utility;

/// <summary>节点产品策略（sf:NodePolicy.json）解析；引擎不解释，Host/办理台消费。</summary>
public sealed class SailNodePolicyDto
{
    public int ApproveType { get; set; } = 1;
    public int RejectHandlerType { get; set; } = 1;
    public string ReturnNodeId { get; set; } = "";
    public int AssignEmptyHandlerType { get; set; } = 2;
    public List<string> AssignEmptyUserIds { get; set; } = new();
    public int AssignStartUserHandlerType { get; set; } = 1;

    /// <summary>
    /// 同人=3 换人：主管道 Resolve Type（如 deptBoss / RoleGroup），走 Registry。
    /// </summary>
    public string AssignStartResolveType { get; set; } = "";

    /// <summary>同人=3 换人：Resolve Params JSON（与 sf:assigneeResolve @params 同形）。</summary>
    public string AssignStartResolveParams { get; set; } = "";

    public bool SignEnable { get; set; }
    public bool ReasonRequire { get; set; }
    public string SkipExpression { get; set; } = "";
    public bool TimeoutHandlerEnable { get; set; }
    public int TimeoutHandlerType { get; set; } = 1;
    public int TimeDuration { get; set; } = 6;
    public int TimeUnit { get; set; } = 2;
    public int MaxRemindCount { get; set; } = 1;

    /// <summary>§J 维①：Prev | Starter | History</summary>
    public string SendBackTarget { get; set; } = "Starter";

    /// <summary>§J 维②：ForwardNormal | ReturnToSender</summary>
    public string ResubmitMode { get; set; } = "ReturnToSender";

    /// <summary>§C 字段编辑性：field / title / permission(editable|readonly|hidden|masked)。</summary>
    public List<SailFieldPermissionItem> FieldsPermission { get; set; } = new();
}

public sealed class SailFieldPermissionItem
{
    public string Field { get; set; } = "";
    public string Title { get; set; } = "";
    /// <summary>editable | readonly | hidden | masked（旧 READ/WRITE/NONE 由 Catalog 归一）。</summary>
    public string Permission { get; set; } = "readonly";
}

public static class SailNodePolicyCatalog
{
    static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static SailNodePolicyDto Defaults { get; } = new();

    public static SailNodePolicyDto Merge(SailNodePolicyDto? raw)
    {
        var d = Defaults;
        if (raw == null) return Clone(d);

        return new SailNodePolicyDto
        {
            ApproveType = raw.ApproveType != 0 ? raw.ApproveType : d.ApproveType,
            RejectHandlerType = raw.RejectHandlerType != 0 ? raw.RejectHandlerType : d.RejectHandlerType,
            ReturnNodeId = raw.ReturnNodeId ?? d.ReturnNodeId,
            AssignEmptyHandlerType = raw.AssignEmptyHandlerType != 0
                ? raw.AssignEmptyHandlerType
                : d.AssignEmptyHandlerType,
            AssignEmptyUserIds = raw.AssignEmptyUserIds?.Count > 0
                ? raw.AssignEmptyUserIds
                : new List<string>(),
            AssignStartUserHandlerType = raw.AssignStartUserHandlerType != 0
                ? raw.AssignStartUserHandlerType
                : d.AssignStartUserHandlerType,
            AssignStartResolveType = !string.IsNullOrWhiteSpace(raw.AssignStartResolveType)
                ? raw.AssignStartResolveType.Trim()
                : d.AssignStartResolveType,
            AssignStartResolveParams = raw.AssignStartResolveParams ?? d.AssignStartResolveParams,
            SignEnable = raw.SignEnable,
            ReasonRequire = raw.ReasonRequire,
            SkipExpression = raw.SkipExpression ?? "",
            TimeoutHandlerEnable = raw.TimeoutHandlerEnable,
            TimeoutHandlerType = raw.TimeoutHandlerType != 0 ? raw.TimeoutHandlerType : d.TimeoutHandlerType,
            TimeDuration = raw.TimeDuration > 0 ? raw.TimeDuration : d.TimeDuration,
            TimeUnit = raw.TimeUnit != 0 ? raw.TimeUnit : d.TimeUnit,
            MaxRemindCount = raw.MaxRemindCount > 0 ? raw.MaxRemindCount : d.MaxRemindCount,
            SendBackTarget = NormalizeTarget(raw.SendBackTarget),
            ResubmitMode = NormalizeResubmit(raw.ResubmitMode),
            FieldsPermission = NormalizeFields(raw.FieldsPermission)
        };
    }

    public static SailNodePolicyDto ParseForActivity(string? xml, string? activityId)
    {
        if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(activityId))
            return Merge(null);

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
            nsmgr.AddNamespace("sf", "http://www.slickflow.com/schema/sf");

            XmlNode? activity =
                doc.SelectSingleNode($"//*[@id='{EscapeXPath(activityId)}']") ??
                doc.SelectSingleNode($"//bpmn:userTask[@id='{EscapeXPath(activityId)}']", nsmgr) ??
                doc.SelectSingleNode($"//bpmn:task[@id='{EscapeXPath(activityId)}']", nsmgr);

            if (activity == null)
                return Merge(null);

            XmlNode? policy = activity.SelectSingleNode(".//*[local-name()='NodePolicy']");
            if (policy == null)
                return Merge(null);

            var json = policy.Attributes?["json"]?.Value
                       ?? policy.Attributes?["sf:json"]?.Value
                       ?? policy.InnerText?.Trim();
            return ParseJson(json);
        }
        catch
        {
            return ParseJsonRegex(xml!, activityId!);
        }
    }

    public static SailNodePolicyDto ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Merge(null);
        try
        {
            var raw = JsonSerializer.Deserialize<SailNodePolicyDto>(json, JsonOpts);
            return Merge(raw);
        }
        catch
        {
            return Merge(null);
        }
    }

    static SailNodePolicyDto ParseJsonRegex(string xml, string activityId)
    {
        var idEsc = Regex.Escape(activityId);
        var m = Regex.Match(
            xml,
            $@"id=[""']{idEsc}[""'][\s\S]{{0,8000}}?NodePolicy[^>]*json=[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);
        if (!m.Success)
            return Merge(null);
        var raw = m.Groups[1].Value.Replace("&quot;", "\"").Replace("&amp;", "&");
        return ParseJson(raw);
    }

    public static string NormalizeTarget(string? v)
    {
        var s = (v ?? "").Trim();
        if (s.Equals("Prev", StringComparison.OrdinalIgnoreCase)) return "Prev";
        if (s.Equals("History", StringComparison.OrdinalIgnoreCase)) return "History";
        return "Starter";
    }

    public static string NormalizeResubmit(string? v)
    {
        var s = (v ?? "").Trim();
        if (s.Equals("ForwardNormal", StringComparison.OrdinalIgnoreCase)) return "ForwardNormal";
        return "ReturnToSender";
    }

    /// <summary>§C：归一为 editable|readonly|hidden|masked；兼容 READ/WRITE/NONE/DESENSITIZE。</summary>
    public static string NormalizeFieldMode(string? raw)
    {
        var s = (raw ?? "").Trim();
        if (s.Length == 0) return "readonly";
        if (s.Equals("editable", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("WRITE", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("write", StringComparison.OrdinalIgnoreCase))
            return "editable";
        if (s.Equals("hidden", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("NONE", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("none", StringComparison.OrdinalIgnoreCase))
            return "hidden";
        if (s.Equals("masked", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("DESENSITIZE", StringComparison.OrdinalIgnoreCase) ||
            s.Equals("desensitize", StringComparison.OrdinalIgnoreCase))
            return "masked";
        // READ / readonly / 未知 → readonly
        return "readonly";
    }

    public static List<SailFieldPermissionItem> NormalizeFields(List<SailFieldPermissionItem>? raw)
    {
        if (raw == null || raw.Count == 0) return new List<SailFieldPermissionItem>();
        var list = new List<SailFieldPermissionItem>();
        foreach (var item in raw)
        {
            if (item == null) continue;
            var field = (item.Field ?? "").Trim();
            if (field.Length == 0) continue;
            list.Add(new SailFieldPermissionItem
            {
                Field = field,
                Title = item.Title ?? "",
                Permission = NormalizeFieldMode(item.Permission)
            });
        }
        return list;
    }

    static string EscapeXPath(string id) => id.Replace("'", "&apos;");

    static SailNodePolicyDto Clone(SailNodePolicyDto d) => new()
    {
        ApproveType = d.ApproveType,
        RejectHandlerType = d.RejectHandlerType,
        ReturnNodeId = d.ReturnNodeId,
        AssignEmptyHandlerType = d.AssignEmptyHandlerType,
        AssignEmptyUserIds = new List<string>(d.AssignEmptyUserIds),
        AssignStartUserHandlerType = d.AssignStartUserHandlerType,
        AssignStartResolveType = d.AssignStartResolveType,
        AssignStartResolveParams = d.AssignStartResolveParams,
        SignEnable = d.SignEnable,
        ReasonRequire = d.ReasonRequire,
        SkipExpression = d.SkipExpression,
        TimeoutHandlerEnable = d.TimeoutHandlerEnable,
        TimeoutHandlerType = d.TimeoutHandlerType,
        TimeDuration = d.TimeDuration,
        TimeUnit = d.TimeUnit,
        MaxRemindCount = d.MaxRemindCount,
        SendBackTarget = d.SendBackTarget,
        ResubmitMode = d.ResubmitMode,
        FieldsPermission = NormalizeFields(d.FieldsPermission)
    };
}
