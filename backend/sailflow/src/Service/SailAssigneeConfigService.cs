using System.Security.Claims;
using System.Xml.Linq;
using sailflow.service;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.UnitScope;

namespace SleveenAdmin.Sailflow.Service;

public sealed class SailAssigneeNodeConfig
{
    public string ActivityId { get; set; } = "";
    public string ActivityName { get; set; } = "";
    public string Strategy { get; set; } = "users";
    public string? ConfigJson { get; set; }
}

public sealed class SailAssigneeConfigBundle
{
    public string ProcessId { get; set; } = "";
    public string Version { get; set; } = "1";
    public string AuthType { get; set; } = "";
    public List<SailAssigneeNodeConfig> Nodes { get; set; } = new();
}

public sealed class SailAssigneeConfigSaveRequest
{
    public string? ProcessId { get; set; }
    public string? Version { get; set; }
    public List<SailAssigneeNodeConfig>? Nodes { get; set; }
}

/// <summary>单位流程审批人配置（读写 BPMN 用户任务扩展）。</summary>
[Scoped]
public class SailAssigneeConfigService
{
    public static readonly string[] AvailableStrategies = ["users", "role", "unitMap", "jobRelative", "deptBoss"];

    private readonly IWorkflowService _wf;
    private readonly SailUnitAccess _access;

    public SailAssigneeConfigService(IWorkflowService wf, SailUnitAccess access)
    {
        _wf = wf;
        _access = access;
    }

    public SailAssigneeConfigBundle Get(ClaimsPrincipal? user, string processId, string version)
    {
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        var process = _wf.GetProcessByVersion(processId, ver)
                      ?? throw new InvalidOperationException("process not found");
        _access.RequireProcessAccess(user, process, write: false);
        var auth = _access.GetProcessAuth(processId, ver);

        if (!SailAuthScope.EqualsType(auth.AuthType, SailAuthScope.Unit)
            && !_access.IsPlatformAdmin(user))
            throw new SailUnitAccessException("审批人配置仅支持单位流程");

        return new SailAssigneeConfigBundle
        {
            ProcessId = processId,
            Version = ver,
            AuthType = auth.AuthType ?? "",
            Nodes = ParseUserTasks(process.XmlContent)
        };
    }

    public SailAssigneeConfigBundle Save(ClaimsPrincipal? user, SailAssigneeConfigSaveRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.ProcessId))
            throw new ArgumentException("ProcessId required");
        var ver = string.IsNullOrWhiteSpace(req.Version) ? "1" : req.Version!.Trim();
        var process = _wf.GetProcessByVersion(req.ProcessId!, ver)
                      ?? throw new InvalidOperationException("process not found");
        _access.RequireProcessAccess(user, process, write: true);
        var auth = _access.GetProcessAuth(req.ProcessId!, ver);

        if (!SailAuthScope.EqualsType(auth.AuthType, SailAuthScope.Unit)
            && !_access.IsPlatformAdmin(user))
            throw new SailUnitAccessException("审批人配置仅支持单位流程");

        process.XmlContent = ApplyNodeConfigs(process.XmlContent ?? "", req.Nodes ?? new List<SailAssigneeNodeConfig>());
        _wf.UpdateProcess(process);
        return Get(user, req.ProcessId!, ver);
    }

    static List<SailAssigneeNodeConfig> ParseUserTasks(string? xml)
    {
        var list = new List<SailAssigneeNodeConfig>();
        if (string.IsNullOrWhiteSpace(xml)) return list;
        try
        {
            var doc = XDocument.Parse(xml);
            foreach (var task in doc.Descendants().Where(e => e.Name.LocalName == "userTask"))
            {
                var id = (string?)task.Attribute("id") ?? "";
                var name = (string?)task.Attribute("name") ?? id;
                var strategy = (string?)task.Attributes().FirstOrDefault(a => a.Name.LocalName == "assigneeStrategy")
                               ?? "users";
                var cfg = (string?)task.Attributes().FirstOrDefault(a => a.Name.LocalName == "assigneeConfig");
                list.Add(new SailAssigneeNodeConfig
                {
                    ActivityId = id,
                    ActivityName = name,
                    Strategy = strategy,
                    ConfigJson = cfg
                });
            }
        }
        catch
        {
            // ignore
        }
        return list;
    }

    static string ApplyNodeConfigs(string xml, List<SailAssigneeNodeConfig> nodes)
    {
        if (string.IsNullOrWhiteSpace(xml) || nodes.Count == 0) return xml;
        var doc = XDocument.Parse(xml);
        var map = nodes.Where(n => !string.IsNullOrWhiteSpace(n.ActivityId))
            .ToDictionary(n => n.ActivityId, n => n, StringComparer.OrdinalIgnoreCase);

        foreach (var task in doc.Descendants().Where(e => e.Name.LocalName == "userTask"))
        {
            var id = (string?)task.Attribute("id") ?? "";
            if (!map.TryGetValue(id, out var cfg)) continue;
            if (!AvailableStrategies.Contains(cfg.Strategy, StringComparer.OrdinalIgnoreCase))
                continue;
            SetOrRemove(task, "assigneeStrategy", cfg.Strategy);
            SetOrRemove(task, "assigneeConfig", string.IsNullOrWhiteSpace(cfg.ConfigJson) ? null : cfg.ConfigJson);
        }

        return doc.ToString(SaveOptions.DisableFormatting);
    }

    static void SetOrRemove(XElement el, string localName, string? value)
    {
        var attr = el.Attributes().FirstOrDefault(a => a.Name.LocalName == localName);
        if (string.IsNullOrWhiteSpace(value))
        {
            attr?.Remove();
            return;
        }

        if (attr == null)
            el.SetAttributeValue(XName.Get(localName, el.Name.NamespaceName), value);
        else
            attr.Value = value;
    }
}
