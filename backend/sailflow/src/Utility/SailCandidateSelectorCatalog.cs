using System.Xml;

namespace SleveenAdmin.Sailflow.Utility;

/// <summary>解析节点 sf:CandidateSelector（运行时选人器类型/参数）。</summary>
public static class SailCandidateSelectorCatalog
{
    public static (string? Type, string? Params) ReadFromXml(string? xml, string? activityId)
    {
        if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(activityId))
            return (null, null);
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
            nsmgr.AddNamespace("sf", "http://www.saiflow.com/schema/sf");

            XmlNode? activity =
                doc.SelectSingleNode($"//*[@id='{EscapeXPath(activityId)}']") ??
                doc.SelectSingleNode($"//bpmn:userTask[@id='{EscapeXPath(activityId)}']", nsmgr) ??
                doc.SelectSingleNode($"//bpmn:task[@id='{EscapeXPath(activityId)}']", nsmgr);
            if (activity == null) return (null, null);

            XmlNode? sel =
                activity.SelectSingleNode(".//*[local-name()='CandidateSelector']");
            if (sel == null) return (null, null);

            var type = sel.Attributes?["type"]?.Value
                       ?? sel.Attributes?["sf:type"]?.Value;
            var parms = sel.Attributes?["params"]?.Value
                        ?? sel.Attributes?["sf:params"]?.Value;
            return (
                string.IsNullOrWhiteSpace(type) ? null : type.Trim(),
                string.IsNullOrWhiteSpace(parms) ? null : parms.Trim()
            );
        }
        catch
        {
            return (null, null);
        }
    }

    static string EscapeXPath(string id) => id.Replace("'", "&apos;");
}
