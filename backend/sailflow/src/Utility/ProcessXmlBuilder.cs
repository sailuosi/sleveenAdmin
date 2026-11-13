using System.Xml;
using sailflow.engine.Entity;
using sailflow.engine.Utility;
using sailflow.engine.Xpdl;

namespace SleveenAdmin.Sailflow.Utility;

/// <summary>精简版 InitNewBPMNFile（原 Graph.ProcessXmlBuilder 静态入口）。</summary>
public static class ProcessXmlBuilder
{
    public static ProcessFileEntity InitNewBPMNFile()
    {
        var fileEntity = new ProcessFileEntity();
        var randomNumber = RandomSequenceGenerator.GetRandomInt4();
        fileEntity.ProcessId = $"Process_{randomNumber}";
        fileEntity.ProcessName = $"Process_Name_{randomNumber}";
        fileEntity.ProcessCode = $"Process_Code_{randomNumber}";
        fileEntity.Version = "1";
        fileEntity.Status = 1;
        fileEntity.XmlContent = GenerateXmlContentWithStartNode(
            fileEntity.ProcessId,
            fileEntity.ProcessName,
            fileEntity.ProcessCode,
            fileEntity.Version);
        return fileEntity;
    }

    /// <summary>
    /// BPMN/XML NCName：首字符须为字母或下划线。Guid.ToString() 常以数字开头，
    /// 直接作 process@id 会导致 bpmn-js「no process or collaboration to display」。
    /// </summary>
    public static string ToBpmnElementId(string? processId)
    {
        var id = string.IsNullOrWhiteSpace(processId) ? "Process_1" : processId.Trim();
        var c0 = id[0];
        if (c0 is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_')
            return id;
        return "Process_" + id;
    }

    /// <summary>新建流程主键：保证同时可作为 BPMN process@id（NCName）。</summary>
    public static string NewProcessId() => "Process_" + Guid.NewGuid().ToString("N");

    public static string GenerateXmlContentWithStartNode(
        string processId,
        string processName,
        string processCode,
        string version)
    {
        var bpmnId = ToBpmnElementId(processId);
        var xmlDoc = CreateDocumentRoot();
        var root = xmlDoc.DocumentElement!;

        var processNode = xmlDoc.CreateElement(
            ProcessNamespaceDefine.m_namespace_prefix_bpmn,
            "process",
            ProcessNamespaceDefine.m_namespace_uri_bpmn);
        root.AppendChild(processNode);
        processNode.SetAttribute("id", bpmnId);
        processNode.SetAttribute("name", processName);
        processNode.SetAttribute("isExecutable", "true");
        processNode.SetAttribute("code", ProcessNamespaceDefine.m_namespace_uri_sf, processCode);
        processNode.SetAttribute("version", ProcessNamespaceDefine.m_namespace_uri_sf, version);

        var startNode = xmlDoc.CreateElement(
            ProcessNamespaceDefine.m_namespace_prefix_bpmn,
            "startEvent",
            ProcessNamespaceDefine.m_namespace_uri_bpmn);
        processNode.AppendChild(startNode);
        startNode.SetAttribute("id", "StartEvent_1");
        startNode.SetAttribute("name", "开始");

        var diagramNode = xmlDoc.CreateElement("bpmndi", "BPMNDiagram", ProcessNamespaceDefine.m_namespace_uri_bpmndi);
        root.AppendChild(diagramNode);
        diagramNode.SetAttribute("id", "BPMNDiagram_1");

        var planeNode = xmlDoc.CreateElement("bpmndi", "BPMNPlane", ProcessNamespaceDefine.m_namespace_uri_bpmndi);
        diagramNode.AppendChild(planeNode);
        planeNode.SetAttribute("id", "BPMNPlane_1");
        planeNode.SetAttribute("bpmnElement", bpmnId);

        var shapeNode = xmlDoc.CreateElement("bpmndi", "BPMNShape", ProcessNamespaceDefine.m_namespace_uri_bpmndi);
        planeNode.AppendChild(shapeNode);
        shapeNode.SetAttribute("id", "_BPMNShape_StartEvent_2");
        shapeNode.SetAttribute("bpmnElement", "StartEvent_1");

        var boundsNode = xmlDoc.CreateElement("dc", "Bounds", ProcessNamespaceDefine.m_namespace_uri_dc);
        shapeNode.AppendChild(boundsNode);
        boundsNode.SetAttribute("height", "36.0");
        boundsNode.SetAttribute("width", "36.0");
        boundsNode.SetAttribute("x", "412.0");
        boundsNode.SetAttribute("y", "240.0");

        // 带 name 的开始事件写上 BPMNLabel，避免重开后标签坐标丢失
        var labelNode = xmlDoc.CreateElement("bpmndi", "BPMNLabel", ProcessNamespaceDefine.m_namespace_uri_bpmndi);
        shapeNode.AppendChild(labelNode);
        var labelBounds = xmlDoc.CreateElement("dc", "Bounds", ProcessNamespaceDefine.m_namespace_uri_dc);
        labelNode.AppendChild(labelBounds);
        labelBounds.SetAttribute("x", "420.0");
        labelBounds.SetAttribute("y", "283.0");
        labelBounds.SetAttribute("width", "22.0");
        labelBounds.SetAttribute("height", "14.0");

        return xmlDoc.OuterXml;
    }

    private static XmlDocument CreateDocumentRoot()
    {
        var xmlDoc = new XmlDocument();
        var xmldecl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmldecl);

        var root = xmlDoc.CreateElement(
            ProcessNamespaceDefine.m_namespace_prefix_bpmn,
            "definitions",
            ProcessNamespaceDefine.m_namespace_uri_bpmn);
        xmlDoc.AppendChild(root);

        root.SetAttribute("xmlns:xsi", ProcessNamespaceDefine.m_namespace_uri_xsi);
        root.SetAttribute("xmlns:bpmndi", ProcessNamespaceDefine.m_namespace_uri_bpmndi);
        root.SetAttribute("xmlns:dc", ProcessNamespaceDefine.m_namespace_uri_dc);
        root.SetAttribute("xmlns:di", ProcessNamespaceDefine.m_namespace_uri_di);
        root.SetAttribute("xmlns:sf", ProcessNamespaceDefine.m_namespace_uri_sf);
        root.SetAttribute("xmlns:bpmn", ProcessNamespaceDefine.m_namespace_uri_bpmn);
        root.SetAttribute("id", "bpmn-diagram");
        root.SetAttribute("targetNamespace", ProcessNamespaceDefine.m_namespace_uri_target);
        root.SetAttribute("xsi:schemaLocation", "http://www.omg.org/spec/BPMN/20100524/MODEL BPMN20.xsd");
        return xmlDoc;
    }
}
