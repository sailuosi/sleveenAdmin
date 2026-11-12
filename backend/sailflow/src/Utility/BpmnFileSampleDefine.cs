using System.Xml;
using sailflow.engine.Utility;

namespace SleveenAdmin.Saiflow.Utility;

/// <summary>
/// 任务 3 T3-3b：按模板名加载 samples/bpmn 并重生 process id/name/code（对齐 sfdapi BpmnFileSampleDefine）。
/// </summary>
public static class BpmnFileSampleDefine
{
    private static readonly IDictionary<string, string> BpmnFileDictionary =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["askforleave"] = "samples/bpmn/business/askforleave.bpmn",
            ["contract"] = "samples/bpmn/business/contract.bpmn",
            ["eorder"] = "samples/bpmn/business/eorder.bpmn",
            ["officein"] = "samples/bpmn/business/officein.bpmn",
            ["reimbursement"] = "samples/bpmn/business/reimbursement.bpmn",
            ["warehousing"] = "samples/bpmn/business/warehousing.bpmn",
            ["andsplitmi"] = "samples/bpmn/logic/andsplitmi.bpmn",
            ["complex"] = "samples/bpmn/logic/complex.bpmn",
            ["gateway"] = "samples/bpmn/logic/gateway.bpmn",
            ["multipleinstance"] = "samples/bpmn/logic/multipleinstance.bpmn",
            ["sequence"] = "samples/bpmn/logic/sequence.bpmn",
            ["subprocess"] = "samples/bpmn/logic/subprocess.bpmn"
        };

    public static bool HasTemplate(string templateName) =>
        !string.IsNullOrWhiteSpace(templateName) && BpmnFileDictionary.ContainsKey(templateName.Trim());

    public static IReadOnlyCollection<string> KnownTemplateKeys => BpmnFileDictionary.Keys.ToList();

    public static string RebuildBpmnProcessXmlContentByTemplate(string templateName)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentException("Template name is required.", nameof(templateName));

        var key = templateName.Trim();
        if (!BpmnFileDictionary.TryGetValue(key, out var relativeFilePath))
            throw new ArgumentException($"Template '{templateName}' not found.");

        var fullFilePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativeFilePath));
        if (!File.Exists(fullFilePath))
            throw new FileNotFoundException($"BPMN template file not found: {fullFilePath}");

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(fullFilePath);

        var random1 = RandomSequenceGenerator.GetRandomInt4();
        var random2 = RandomSequenceGenerator.GetRandomInt4();

        var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
        namespaceManager.AddNamespace("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
        namespaceManager.AddNamespace("sf", "http://www.saiflow.com/schema/sf");

        var processNode = xmlDoc.SelectSingleNode("//bpmn:process", namespaceManager)
            ?? xmlDoc.SelectSingleNode("//process");
        if (processNode?.Attributes == null)
            throw new InvalidOperationException("Process element not found in BPMN XML file.");

        SetOrCreateAttr(xmlDoc, processNode, "id", $"process_{random1}_{random2}");
        SetOrCreateAttr(xmlDoc, processNode, "name", $"{key}_{random1}_{random2}");

        var codeValue = $"{key}_code_{random1}_{random2}";
        XmlAttribute? codeAttr = processNode.Attributes["code"];
        if (codeAttr == null)
        {
            foreach (XmlAttribute attr in processNode.Attributes)
            {
                if (attr.LocalName == "code" &&
                    (attr.Prefix == "sf" || attr.NamespaceURI.Contains("slickflow", StringComparison.OrdinalIgnoreCase)))
                {
                    codeAttr = attr;
                    break;
                }
            }
        }

        if (codeAttr != null)
            codeAttr.Value = codeValue;
        else
        {
            var sfCodeAttr = xmlDoc.CreateAttribute("sf", "code", "http://www.saiflow.com/schema/sf");
            sfCodeAttr.Value = codeValue;
            processNode.Attributes.Append(sfCodeAttr);
        }

        using var stringWriter = new StringWriter();
        using (var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
        {
            xmlDoc.WriteTo(xmlWriter);
        }
        return stringWriter.ToString();
    }

    private static void SetOrCreateAttr(XmlDocument doc, XmlNode node, string name, string value)
    {
        if (node.Attributes == null) return;
        if (node.Attributes[name] != null)
            node.Attributes[name]!.Value = value;
        else
        {
            var attr = doc.CreateAttribute(name);
            attr.Value = value;
            node.Attributes.Append(attr);
        }
    }
}
