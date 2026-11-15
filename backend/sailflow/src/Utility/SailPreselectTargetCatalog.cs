using System.Text.Json;
using System.Xml;

namespace SleveenAdmin.Sailflow.Utility;

/// <summary>从 BPMN 扫描 Resolve Type=<c>Preselect</c> 的用户任务。</summary>
public static class SailPreselectTargetCatalog
{
    public sealed class Target
    {
        public string ActivityId { get; set; } = "";
        public string ActivityName { get; set; } = "";
        public bool Required { get; set; }
        public bool AllowMulti { get; set; } = true;
        public string? ParamsJson { get; set; }
    }

    public static IList<Target> ReadFromXml(string? xml)
    {
        var list = new List<Target>();
        if (string.IsNullOrWhiteSpace(xml))
            return list;

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
            nsmgr.AddNamespace("sf", "http://www.saiflow.com/schema/sf");

            var tasks = doc.SelectNodes("//bpmn:userTask | //bpmn:task", nsmgr)
                         ?? doc.SelectNodes("//*[local-name()='userTask' or local-name()='task']");
            if (tasks == null)
                return list;

            foreach (XmlNode task in tasks)
            {
                var id = task.Attributes?["id"]?.Value?.Trim();
                if (string.IsNullOrEmpty(id))
                    continue;

                var resolve = task.SelectSingleNode(".//*[local-name()='assigneeResolve' or local-name()='AssigneeResolve']");
                if (resolve == null)
                    continue;

                var type = resolve.Attributes?["type"]?.Value
                           ?? resolve.Attributes?["sf:type"]?.Value;
                if (!string.Equals(type?.Trim(), "Preselect", StringComparison.OrdinalIgnoreCase))
                    continue;

                var parms = resolve.Attributes?["params"]?.Value
                            ?? resolve.Attributes?["sf:params"]?.Value;
                var (required, allowMulti) = ParseParams(parms);
                var name = task.Attributes?["name"]?.Value?.Trim();
                list.Add(new Target
                {
                    ActivityId = id,
                    ActivityName = string.IsNullOrWhiteSpace(name) ? id : name!,
                    Required = required,
                    AllowMulti = allowMulti,
                    ParamsJson = string.IsNullOrWhiteSpace(parms) ? null : parms.Trim()
                });
            }
        }
        catch
        {
            // 坏 XML → 空列表，由调用方提示
        }

        return list;
    }

    static (bool Required, bool AllowMulti) ParseParams(string? raw)
    {
        var required = false;
        var allowMulti = true;
        if (string.IsNullOrWhiteSpace(raw))
            return (required, allowMulti);
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return (required, allowMulti);
            if (doc.RootElement.TryGetProperty("required", out var r))
                required = r.ValueKind == JsonValueKind.True
                           || (r.ValueKind == JsonValueKind.String
                               && bool.TryParse(r.GetString(), out var rb) && rb);
            if (doc.RootElement.TryGetProperty("allowMulti", out var m))
            {
                if (m.ValueKind == JsonValueKind.False)
                    allowMulti = false;
                else if (m.ValueKind == JsonValueKind.String
                         && bool.TryParse(m.GetString(), out var mb))
                    allowMulti = mb;
            }
        }
        catch
        {
            // ignore
        }

        return (required, allowMulti);
    }
}
