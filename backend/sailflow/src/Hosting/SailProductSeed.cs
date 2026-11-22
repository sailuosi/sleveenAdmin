using System.Globalization;
using System.Linq;
using System.Text.Json;
using mooSQL.data;
using sailflow.engine.Entity;
using sailflow.hub.Form;
using SleveenAdmin.Sailflow.Entity;

namespace SleveenAdmin.Sailflow.Hosting;

/// <summary>
/// 产品侧种子：分类 / 表单 / 流程定义 / meta。
/// 数据来源：sleveen-admin App_Data/sailflow.sqlite 快照（samples/seed）。
/// 按业务键幂等；已有行不覆盖（UAT 图 DI 空/零尺寸除外，会回写可渲染 XmlContent）。
/// </summary>
public static class SailProductSeed
{
    public const string CategoryCode = "demo1";
    public const string CategoryId = "6b82cb309d3d412abc67367a842882ff";

    public const string FormId = "6b5a060a-d38d-40f4-818a-4ea4432ec91c";
    public const string FormCode = "askpost";

    public const string HelloProcessId = "sail-seed-hello";
    public const string HelloProcessCode = "SEED_HELLO";

    public const string DemoProcessId = "Process_d334770c0fae4a049030a41bccbc3d23";
    public const string DemoProcessDefId = "ef915570-b9ce-495f-becc-329bf07fefab";
    public const string DemoProcessCode = "demoflow";

    public const string HelloMetaId = "c68deea4fb674764955120471b7db18d";
    public const string DemoMetaId = "afa08ced46164304a1659f08e1ef08fd";

    /// <summary>UAT·G：双人工节点，可退回上一 UserTask。</summary>
    public const string UatGProcessId = "sail-uat-g";
    public const string UatAProcessId = "sail-uat-a";
    public const string UatBProcessId = "sail-uat-b";
    public const string UatCProcessId = "sail-uat-c";

    /// <summary>复杂条件网关（XOR + Inclusive）。</summary>
    public const string ComplexCondProcessId = "sail-seed-complex-cond";
    public const string ComplexCondProcessCode = "SEED_COMPLEX_COND";

    /// <summary>嵌套子流程（subProcess 内含节点）。</summary>
    public const string NestedSubProcessId = "sail-seed-nested-sub";
    public const string NestedSubProcessCode = "SEED_NESTED_SUB";

    /// <summary>引用子流程-子件（被父流程 sf:subInfoes 绑定）。</summary>
    public const string RefChildProcessId = "sail-seed-ref-child";
    public const string RefChildProcessDefId = "c1d2e3f4-a5b6-4789-8abc-def012345601";
    public const string RefChildProcessCode = "SEED_REF_CHILD";

    /// <summary>引用子流程-父流程。</summary>
    public const string RefParentProcessId = "sail-seed-ref-parent";
    public const string RefParentProcessCode = "SEED_REF_PARENT";

    /// <summary>综合：XOR + Parallel + 嵌套子流程 + NodePolicy.skipExpression。</summary>
    public const string ComplexRouteProcessId = "sail-seed-complex-route";
    public const string ComplexRouteProcessCode = "SEED_COMPLEX_ROUTE";

    const string DefaultExtraJson =
        "{\"allowCancelRunningProcess\":true,\"allowWithdrawTask\":false,\"processIdRule\":{\"enable\":false,\"prefix\":\"\",\"infix\":\"\",\"postfix\":\"\",\"length\":5},\"autoApprovalType\":0,\"titleSetting\":{\"enable\":false,\"title\":\"\"},\"summarySetting\":{\"enable\":false,\"summary\":[]},\"processBeforeTriggerSetting\":null,\"processAfterTriggerSetting\":null,\"taskBeforeTriggerSetting\":null,\"taskAfterTriggerSetting\":null,\"printTemplateSetting\":{\"enable\":false,\"template\":\"\"}}";

    /// <summary>写入分类、求职表单、Hello/演示流程定义及 meta；返回新插入行数。</summary>
    public static int EnsureAll(SQLBuilder kit)
    {
        ArgumentNullException.ThrowIfNull(kit);
        var n = 0;
        n += EnsureCategory(kit);
        n += EnsureAskPostForm(kit);
        n += EnsureHelloProcess(kit);
        n += EnsureDemoProcess(kit);
        n += EnsureUatProcess(kit, UatGProcessId, "SEED_UAT_G", "UAT G 双节点退回", UatGBpmn());
        n += EnsureUatProcess(kit, UatAProcessId, "SEED_UAT_A", "UAT A 空人同人超时", UatABpmn());
        n += EnsureUatProcess(kit, UatBProcessId, "SEED_UAT_B", "UAT B 终止作废", UatBBpmn());
        n += EnsureUatProcess(kit, UatCProcessId, "SEED_UAT_C", "UAT C 字段权限", UatCBpmn());
        n += EnsureComplexTestProcesses(kit);
        n += EnsureMeta(kit, HelloMetaId, HelloProcessId, "1");
        n += EnsureMeta(kit, DemoMetaId, DemoProcessId, "1");
        n += EnsureMeta(kit, "uatg-meta-000000000000000000000001", UatGProcessId, "1");
        n += EnsureMeta(kit, "uata-meta-000000000000000000000001", UatAProcessId, "1");
        n += EnsureMeta(kit, "uatb-meta-000000000000000000000001", UatBProcessId, "1");
        n += EnsureMeta(kit, "uatc-meta-000000000000000000000001", UatCProcessId, "1");
        n += EnsureMeta(kit, "cplxcond-meta-00000000000000000001", ComplexCondProcessId, "1");
        n += EnsureMeta(kit, "nestedsub-meta-00000000000000000001", NestedSubProcessId, "1");
        n += EnsureMeta(kit, "refchild-meta-000000000000000000001", RefChildProcessId, "1");
        n += EnsureMeta(kit, "refparent-meta-00000000000000000001", RefParentProcessId, "1");
        n += EnsureMeta(kit, "cplxroute-meta-00000000000000000001", ComplexRouteProcessId, "1");
        return n;
    }

    /// <summary>
    /// 复杂流程联调种子：条件网关 / 嵌套子流程 / 引用子流程 / 综合路由。
    /// BPMN 来自 samples/bpmn/logic；按 ProcessId 幂等，已有行不覆盖。
    /// </summary>
    static int EnsureComplexTestProcesses(SQLBuilder kit)
    {
        var n = 0;
        n += EnsureFileProcess(kit, ComplexCondProcessId, ComplexCondProcessCode, "复杂条件网关",
            "bpmn/logic/complexcond.bpmn");
        n += EnsureFileProcess(kit, NestedSubProcessId, NestedSubProcessCode, "嵌套子流程",
            "bpmn/logic/nestedsub.bpmn");
        n += EnsureFileProcess(kit, RefChildProcessId, RefChildProcessCode, "引用子流程-子件",
            "bpmn/logic/refsubchild.bpmn", RefChildProcessDefId);
        n += EnsureFileProcess(kit, RefParentProcessId, RefParentProcessCode, "引用子流程-父流程",
            "bpmn/logic/refsubparent.bpmn");
        n += EnsureFileProcess(kit, ComplexRouteProcessId, ComplexRouteProcessCode, "综合复杂路由",
            "bpmn/logic/complexroute.bpmn");
        return n;
    }

    static int EnsureFileProcess(
        SQLBuilder kit,
        string processId,
        string code,
        string name,
        string relativeUnderSamples,
        string? processDefId = null)
    {
        var repo = kit.useRepo<ProcessEntity>();
        var existing = repo.GetList(t => t.ProcessId == processId).FirstOrDefault();
        if (existing != null)
        {
            if (NeedsDiagramRepair(existing.XmlContent))
            {
                var repaired = ReadSampleText(relativeUnderSamples);
                if (!string.IsNullOrWhiteSpace(repaired))
                {
                    existing.XmlContent = repaired;
                    existing.UpdatedDateTime = DateTime.UtcNow;
                    repo.Update(existing);
                    return 1;
                }
            }
            return 0;
        }

        var xml = ReadSampleText(relativeUnderSamples);
        if (string.IsNullOrWhiteSpace(xml))
            return 0;

        var entity = new ProcessEntity
        {
            ProcessId = processId,
            ProcessName = name,
            ProcessCode = code,
            Version = "1",
            Status = 1,
            AppType = CategoryCode,
            XmlFileName = Path.GetFileName(relativeUnderSamples),
            XmlContent = xml,
            StartType = 0,
            EndType = 0,
            Description = "复杂流程联调种子（可删）",
            CreatedDateTime = DateTime.UtcNow,
            UpdatedDateTime = DateTime.UtcNow
        };
        if (!string.IsNullOrWhiteSpace(processDefId))
            entity.ProcessDefId = processDefId;

        return InsertProcesses(kit, new List<ProcessEntity> { entity });
    }

    static int EnsureUatProcess(SQLBuilder kit, string processId, string code, string name, string xml)
    {
        var repo = kit.useRepo<ProcessEntity>();
        var existing = repo.GetList(t => t.ProcessId == processId).FirstOrDefault();

        if (existing != null)
        {
            // 旧种子 DI 为空或 Bounds 全 0，设计器不可见；补图时回写 XmlContent
            if (NeedsDiagramRepair(existing.XmlContent))
            {
                existing.XmlContent = xml;
                existing.UpdatedDateTime = DateTime.UtcNow;
                repo.Update(existing);
                return 1;
            }
            return 0;
        }

        return InsertProcesses(kit, new List<ProcessEntity>
        {
            new()
            {
                ProcessId = processId,
                ProcessName = name,
                ProcessCode = code,
                Version = "1",
                Status = 1,
                AppType = CategoryCode,
                XmlFileName = processId + ".bpmn",
                XmlContent = xml,
                StartType = 0,
                EndType = 0,
                Description = "联调验收专用（可删）",
                CreatedDateTime = DateTime.UtcNow
            }
        });
    }

    /// <summary>无 Shape、或 Bounds 宽高为 0 → 设计器画布空白。</summary>
    static bool NeedsDiagramRepair(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return true;
        if (!xml.Contains("BPMNShape", StringComparison.OrdinalIgnoreCase))
            return true;
        return xml.Contains("width=\"0\"", StringComparison.Ordinal)
               || xml.Contains("width='0'", StringComparison.Ordinal);
    }

    static int EnsureCategory(SQLBuilder kit)
    {
        var exists = kit.countBy<SailProcessCategoryEntity>((c, t) =>
        {
            c.where(() => t.Code, CategoryCode);
        });
        if (exists > 0)
            return 0;

        kit.useRepo<SailProcessCategoryEntity>().Insert(new SailProcessCategoryEntity
        {
            Id = CategoryId,
            Code = CategoryCode,
            Name = "演示分类",
            SortOrder = 1,
            Remark = "种子：来自 sailflow.sqlite",
            CreatedAt = DateTime.UtcNow
        });
        return 1;
    }

    static int EnsureAskPostForm(SQLBuilder kit)
    {
        var exists = kit.countBy<FormEntity>((c, t) =>
        {
            c.where(() => t.FormId, FormId);
        });
        if (exists > 0)
            return 0;

        exists = kit.countBy<FormEntity>((c, t) =>
        {
            c.where(() => t.FormCode, FormCode);
        });
        if (exists > 0)
            return 0;

        var form = LoadAskPostForm() ?? BuildFallbackAskPostForm();
        form.FormId = string.IsNullOrWhiteSpace(form.FormId) ? FormId : form.FormId;
        form.FormCode = string.IsNullOrWhiteSpace(form.FormCode) ? FormCode : form.FormCode;
        form.Version ??= "1";
        form.CreatedDate ??= DateTime.UtcNow;
        form.LastUpdatedDate ??= form.CreatedDate;
        kit.useRepo<FormEntity>().Insert(form);
        return 1;
    }

    static int EnsureHelloProcess(SQLBuilder kit)
    {
        var exists = kit.countBy<ProcessEntity>((c, t) =>
        {
            c.where(() => t.ProcessId, HelloProcessId);
        });
        if (exists > 0)
            return 0;

        return InsertProcesses(kit, new List<ProcessEntity>
        {
            new()
            {
                ProcessId = HelloProcessId,
                ProcessName = "Sail Seed Hello",
                ProcessCode = HelloProcessCode,
                Version = "1",
                Status = 1,
                AppType = CategoryCode,
                XmlFileName = "seed-hello.xml",
                XmlContent = MinimalHelloBpmn(),
                StartType = 0,
                EndType = 0,
                Description = "最小种子流程（可删）",
                CreatedDateTime = DateTime.UtcNow
            }
        });
    }

    static int EnsureDemoProcess(SQLBuilder kit)
    {
        var exists = kit.countBy<ProcessEntity>((c, t) =>
        {
            c.where(() => t.ProcessId, DemoProcessId);
        });
        if (exists > 0)
            return 0;

        exists = kit.countBy<ProcessEntity>((c, t) =>
        {
            c.where(() => t.ProcessCode, DemoProcessCode);
        });
        if (exists > 0)
            return 0;

        var xml = ReadSeedText("demoflow.bpmn");
        if (string.IsNullOrWhiteSpace(xml))
            return 0;

        return InsertProcesses(kit, new List<ProcessEntity>
        {
            new()
            {
                ProcessDefId = DemoProcessDefId,
                ProcessId = DemoProcessId,
                ProcessName = "演示流程",
                ProcessCode = DemoProcessCode,
                Version = "1",
                Status = 1,
                AppType = CategoryCode,
                XmlFileName = "demoflow.bpmn",
                XmlContent = xml,
                Icon = "",
                Description = "演示",
                CreatedDateTime = DateTime.UtcNow,
                UpdatedDateTime = DateTime.UtcNow
            }
        });
    }

    static int EnsureMeta(SQLBuilder kit, string metaId, string processId, string version)
    {
        var exists = kit.countBy<SailProcessMetaEntity>((c, t) =>
        {
            c.where(() => t.ProcessId, processId);
            c.where(() => t.Version, version);
        });
        if (exists > 0)
            return 0;

        kit.useRepo<SailProcessMetaEntity>().Insert(new SailProcessMetaEntity
        {
            Id = metaId,
            ProcessId = processId,
            Version = version,
            FormType = 10,
            FormId = FormId,
            FormCustomCreatePath = "",
            FormCustomViewPath = "",
            Visible = 1,
            StartUserType = 0,
            StartUserIds = "[]",
            StartDeptIds = "[]",
            ManagerUserIds = "[]",
            ExtraJson = DefaultExtraJson,
            UpdatedAt = DateTime.UtcNow
        });
        return 1;
    }

    static int InsertProcesses(SQLBuilder kit, List<ProcessEntity> rows)
    {
        var prev = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return kit.insertList(rows);
        }
        finally
        {
            CultureInfo.CurrentCulture = prev;
            Thread.CurrentThread.CurrentCulture = prev;
        }
    }

    static FormEntity? LoadAskPostForm()
    {
        var json = ReadSeedText("askpost.form.json");
        if (string.IsNullOrWhiteSpace(json))
            return null;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        return new FormEntity
        {
            FormId = GetStr(root, "ff_id") ?? FormId,
            FormName = GetStr(root, "ff_name") ?? "求职表单",
            FormCode = GetStr(root, "ff_code") ?? FormCode,
            Version = GetStr(root, "ff_version") ?? "1",
            FieldSummary = GetStr(root, "ff_fld_summary") ?? "[]",
            TemplateContent = GetStr(root, "ff_template") ?? "{}",
            HTMLContent = GetStr(root, "ff_html") ?? "[]",
            Description = GetStr(root, "ff_description") ?? "",
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };
    }

    static FormEntity BuildFallbackAskPostForm() => new()
    {
        FormId = FormId,
        FormName = "求职表单",
        FormCode = FormCode,
        Version = "1",
        FieldSummary = "[\"F9aqmwg7hka8abc\",\"F4vqmwg7hlfwaec\",\"Flbimwg7hmm9ahc\"]",
        TemplateContent =
            "{\"form\":{\"inline\":false,\"hideRequiredAsterisk\":false,\"labelPosition\":\"right\",\"size\":\"default\",\"labelWidth\":\"100px\"},\"resetBtn\":{\"show\":false,\"innerText\":\"重置\"},\"submitBtn\":{\"show\":true,\"innerText\":\"提交\"}}",
        HTMLContent =
            "[\"{\\\"type\\\":\\\"input\\\",\\\"field\\\":\\\"F9aqmwg7hka8abc\\\",\\\"title\\\":\\\"姓名\\\",\\\"info\\\":\\\"\\\",\\\"$required\\\":false,\\\"_fc_id\\\":\\\"id_F320mwg7hka8acc\\\",\\\"name\\\":\\\"ref_Fiebmwg7hka8adc\\\",\\\"display\\\":true,\\\"hidden\\\":false,\\\"_fc_drag_tag\\\":\\\"input\\\"}\",\"{\\\"type\\\":\\\"input\\\",\\\"field\\\":\\\"F4vqmwg7hlfwaec\\\",\\\"title\\\":\\\"简介\\\",\\\"info\\\":\\\"\\\",\\\"$required\\\":false,\\\"props\\\":{\\\"type\\\":\\\"textarea\\\"},\\\"_fc_id\\\":\\\"id_Fxuumwg7hlfwafc\\\",\\\"name\\\":\\\"ref_Fx88mwg7hlfwagc\\\",\\\"display\\\":true,\\\"hidden\\\":false,\\\"_fc_drag_tag\\\":\\\"textarea\\\"}\",\"{\\\"type\\\":\\\"input\\\",\\\"field\\\":\\\"Flbimwg7hmm9ahc\\\",\\\"title\\\":\\\"意向\\\",\\\"info\\\":\\\"\\\",\\\"$required\\\":false,\\\"_fc_id\\\":\\\"id_F9s0mwg7hmm9aic\\\",\\\"name\\\":\\\"ref_F4bomwg7hmm9ajc\\\",\\\"display\\\":true,\\\"hidden\\\":false,\\\"_fc_drag_tag\\\":\\\"input\\\"}\"]",
        Description = "",
        CreatedDate = DateTime.UtcNow,
        LastUpdatedDate = DateTime.UtcNow
    };

    static string? GetStr(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

    static string? ReadSeedText(string fileName)
    {
        foreach (var dir in SeedDirs())
        {
            var path = Path.Combine(dir, fileName);
            if (File.Exists(path))
                return File.ReadAllText(path);
        }
        return null;
    }

    /// <summary>读取 samples 下相对路径（如 bpmn/logic/complexcond.bpmn）。</summary>
    static string? ReadSampleText(string relativeUnderSamples)
    {
        var rel = relativeUnderSamples.Replace('/', Path.DirectorySeparatorChar);
        foreach (var root in SampleRoots())
        {
            var path = Path.Combine(root, rel);
            if (File.Exists(path))
                return File.ReadAllText(path);
        }
        return null;
    }

    static IEnumerable<string> SeedDirs()
    {
        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in SampleRoots())
            AddDir(seen, list, Path.Combine(root, "seed"));
        return list;
    }

    static IEnumerable<string> SampleRoots()
    {
        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDir(seen, list, Path.Combine(AppContext.BaseDirectory, "samples"));
        var asm = typeof(SailProductSeed).Assembly.Location;
        if (!string.IsNullOrWhiteSpace(asm))
            AddDir(seen, list, Path.Combine(Path.GetDirectoryName(asm)!, "samples"));
        AddDir(seen, list, Path.Combine(Directory.GetCurrentDirectory(), "samples"));
        // 开发期：源码树 samples（未 CopyToOutput 时兜底）
        if (!string.IsNullOrWhiteSpace(asm))
        {
            AddDir(seen, list, Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(asm)!, "..", "..", "..", "samples")));
        }
        return list;
    }

    static void AddDir(HashSet<string> seen, List<string> list, string? d)
    {
        if (string.IsNullOrWhiteSpace(d)) return;
        var full = Path.GetFullPath(d);
        if (Directory.Exists(full) && seen.Add(full))
            list.Add(full);
    }

    static string MinimalHelloBpmn() =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<bpmn:definitions xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
        "xmlns:bpmn=\"http://www.omg.org/spec/BPMN/20100524/MODEL\" " +
        "xmlns:bpmndi=\"http://www.omg.org/spec/BPMN/20100524/DI\" " +
        "xmlns:dc=\"http://www.omg.org/spec/DD/20100524/DC\" " +
        "xmlns:di=\"http://www.omg.org/spec/DD/20100524/DI\" " +
        "xmlns:sf=\"http://www.sailflow.com/schema/sf\" " +
        "id=\"Definitions_SailSeed\" targetNamespace=\"http://bpmn.io/schema/bpmn\">" +
        "<bpmn:process id=\"Process_1\" name=\"Sail Seed Hello\" isExecutable=\"true\" " +
        "sf:code=\"SEED_HELLO\" sf:version=\"1\">" +
        "<bpmn:startEvent id=\"StartEvent_1\" name=\"Start\">" +
        "<bpmn:outgoing>Flow_start_task</bpmn:outgoing>" +
        "</bpmn:startEvent>" +
        "<bpmn:userTask id=\"UserTask_1\" name=\"Hello Task\">" +
        "<bpmn:incoming>Flow_start_task</bpmn:incoming>" +
        "<bpmn:outgoing>Flow_task_end</bpmn:outgoing>" +
        "<bpmn:extensionElements>" +
        "<sf:assigneeResolve type=\"ApproverPick\" params=\"\" />" +
        "<sf:leaveJudge type=\"Collaborate\" params=\"{&quot;completeRate&quot;:1}\" />" +
        "</bpmn:extensionElements>" +
        "</bpmn:userTask>" +
        "<bpmn:endEvent id=\"EndEvent_1\" name=\"End\">" +
        "<bpmn:incoming>Flow_task_end</bpmn:incoming>" +
        "</bpmn:endEvent>" +
        "<bpmn:sequenceFlow id=\"Flow_start_task\" sourceRef=\"StartEvent_1\" targetRef=\"UserTask_1\" />" +
        "<bpmn:sequenceFlow id=\"Flow_task_end\" sourceRef=\"UserTask_1\" targetRef=\"EndEvent_1\" />" +
        "</bpmn:process>" +
        "<bpmndi:BPMNDiagram id=\"BPMNDiagram_1\">" +
        "<bpmndi:BPMNPlane id=\"BPMNPlane_1\" bpmnElement=\"Process_1\">" +
        "<bpmndi:BPMNShape id=\"StartEvent_1_di\" bpmnElement=\"StartEvent_1\">" +
        "<dc:Bounds x=\"180\" y=\"160\" width=\"36\" height=\"36\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNShape id=\"UserTask_1_di\" bpmnElement=\"UserTask_1\">" +
        "<dc:Bounds x=\"280\" y=\"138\" width=\"100\" height=\"80\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNShape id=\"EndEvent_1_di\" bpmnElement=\"EndEvent_1\">" +
        "<dc:Bounds x=\"450\" y=\"160\" width=\"36\" height=\"36\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNEdge id=\"Flow_start_task_di\" bpmnElement=\"Flow_start_task\">" +
        "<di:waypoint x=\"216\" y=\"178\" /><di:waypoint x=\"280\" y=\"178\" />" +
        "</bpmndi:BPMNEdge>" +
        "<bpmndi:BPMNEdge id=\"Flow_task_end_di\" bpmnElement=\"Flow_task_end\">" +
        "<di:waypoint x=\"380\" y=\"178\" /><di:waypoint x=\"450\" y=\"178\" />" +
        "</bpmndi:BPMNEdge>" +
        "</bpmndi:BPMNPlane>" +
        "</bpmndi:BPMNDiagram>" +
        "</bpmn:definitions>";

    static string BpmnShell(string processId, string code, string name, string body, string diagramInner) =>
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<bpmn:definitions xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
        "xmlns:bpmn=\"http://www.omg.org/spec/BPMN/20100524/MODEL\" " +
        "xmlns:bpmndi=\"http://www.omg.org/spec/BPMN/20100524/DI\" " +
        "xmlns:dc=\"http://www.omg.org/spec/DD/20100524/DC\" " +
        "xmlns:di=\"http://www.omg.org/spec/DD/20100524/DI\" " +
        "xmlns:sf=\"http://www.sailflow.com/schema/sf\" " +
        "id=\"Definitions_" + processId + "\" targetNamespace=\"http://bpmn.io/schema/bpmn\">" +
        "<bpmn:process id=\"" + processId + "\" name=\"" + name + "\" isExecutable=\"true\" " +
        "sf:code=\"" + code + "\" sf:version=\"1\">" +
        body +
        "</bpmn:process>" +
        "<bpmndi:BPMNDiagram id=\"BPMNDiagram_1\">" +
        "<bpmndi:BPMNPlane id=\"BPMNPlane_1\" bpmnElement=\"" + processId + "\">" +
        diagramInner +
        "</bpmndi:BPMNPlane>" +
        "</bpmndi:BPMNDiagram>" +
        "</bpmn:definitions>";

    /// <summary>Start → UserTask_1 → End（f1/f2），与 MinimalHello 同坐标习惯。</summary>
    static string DiLinearOneTask() =>
        "<bpmndi:BPMNShape id=\"StartEvent_1_di\" bpmnElement=\"StartEvent_1\">" +
        "<dc:Bounds x=\"180\" y=\"160\" width=\"36\" height=\"36\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNShape id=\"UserTask_1_di\" bpmnElement=\"UserTask_1\">" +
        "<dc:Bounds x=\"280\" y=\"138\" width=\"100\" height=\"80\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNShape id=\"EndEvent_1_di\" bpmnElement=\"EndEvent_1\">" +
        "<dc:Bounds x=\"450\" y=\"160\" width=\"36\" height=\"36\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNEdge id=\"f1_di\" bpmnElement=\"f1\">" +
        "<di:waypoint x=\"216\" y=\"178\" /><di:waypoint x=\"280\" y=\"178\" />" +
        "</bpmndi:BPMNEdge>" +
        "<bpmndi:BPMNEdge id=\"f2_di\" bpmnElement=\"f2\">" +
        "<di:waypoint x=\"380\" y=\"178\" /><di:waypoint x=\"450\" y=\"178\" />" +
        "</bpmndi:BPMNEdge>";

    /// <summary>Start → UserTask_A → UserTask_B → End（f1/f2/f3）。</summary>
    static string DiLinearTwoTasks() =>
        "<bpmndi:BPMNShape id=\"StartEvent_1_di\" bpmnElement=\"StartEvent_1\">" +
        "<dc:Bounds x=\"120\" y=\"160\" width=\"36\" height=\"36\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNShape id=\"UserTask_A_di\" bpmnElement=\"UserTask_A\">" +
        "<dc:Bounds x=\"220\" y=\"138\" width=\"100\" height=\"80\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNShape id=\"UserTask_B_di\" bpmnElement=\"UserTask_B\">" +
        "<dc:Bounds x=\"380\" y=\"138\" width=\"100\" height=\"80\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNShape id=\"EndEvent_1_di\" bpmnElement=\"EndEvent_1\">" +
        "<dc:Bounds x=\"550\" y=\"160\" width=\"36\" height=\"36\" />" +
        "</bpmndi:BPMNShape>" +
        "<bpmndi:BPMNEdge id=\"f1_di\" bpmnElement=\"f1\">" +
        "<di:waypoint x=\"156\" y=\"178\" /><di:waypoint x=\"220\" y=\"178\" />" +
        "</bpmndi:BPMNEdge>" +
        "<bpmndi:BPMNEdge id=\"f2_di\" bpmnElement=\"f2\">" +
        "<di:waypoint x=\"320\" y=\"178\" /><di:waypoint x=\"380\" y=\"178\" />" +
        "</bpmndi:BPMNEdge>" +
        "<bpmndi:BPMNEdge id=\"f3_di\" bpmnElement=\"f3\">" +
        "<di:waypoint x=\"480\" y=\"178\" /><di:waypoint x=\"550\" y=\"178\" />" +
        "</bpmndi:BPMNEdge>";

    static string Ext(string assignee, string leave, string? buttonsJson, string? policyJson)
    {
        var sb = "<bpmn:extensionElements>";
        sb += "<sf:assigneeResolve type=\"" + assignee + "\" params=\"\" />";
        sb += "<sf:leaveJudge type=\"" + leave + "\" params=\"{&quot;completeRate&quot;:1}\" />";
        if (!string.IsNullOrWhiteSpace(buttonsJson))
            sb += "<sf:ButtonsSetting json=\"" + buttonsJson.Replace("\"", "&quot;") + "\" />";
        if (!string.IsNullOrWhiteSpace(policyJson))
            sb += "<sf:NodePolicy json=\"" + policyJson.Replace("\"", "&quot;") + "\" />";
        sb += "</bpmn:extensionElements>";
        return sb;
    }

    static string UatGBpmn()
    {
        var pick = "ApproverPick";
        var body =
            "<bpmn:startEvent id=\"StartEvent_1\" name=\"Start\"><bpmn:outgoing>f1</bpmn:outgoing></bpmn:startEvent>" +
            "<bpmn:userTask id=\"UserTask_A\" name=\"一级审批\">" +
            "<bpmn:incoming>f1</bpmn:incoming><bpmn:outgoing>f2</bpmn:outgoing>" +
            Ext(pick, "Collaborate", null, null) +
            "</bpmn:userTask>" +
            "<bpmn:userTask id=\"UserTask_B\" name=\"二级审批\">" +
            "<bpmn:incoming>f2</bpmn:incoming><bpmn:outgoing>f3</bpmn:outgoing>" +
            Ext(pick, "Collaborate", null, null) +
            "</bpmn:userTask>" +
            "<bpmn:endEvent id=\"EndEvent_1\" name=\"End\"><bpmn:incoming>f3</bpmn:incoming></bpmn:endEvent>" +
            "<bpmn:sequenceFlow id=\"f1\" sourceRef=\"StartEvent_1\" targetRef=\"UserTask_A\" />" +
            "<bpmn:sequenceFlow id=\"f2\" sourceRef=\"UserTask_A\" targetRef=\"UserTask_B\" />" +
            "<bpmn:sequenceFlow id=\"f3\" sourceRef=\"UserTask_B\" targetRef=\"EndEvent_1\" />";
        return BpmnShell(UatGProcessId, "SEED_UAT_G", "UAT G 双节点退回", body, DiLinearTwoTasks());
    }

    static string UatABpmn()
    {
        // 空人=2 拒绝推进；短超时启用 Type=2 自动同意（联调可再改库）
        var policy =
            "{\"approveType\":1,\"assignEmptyHandlerType\":2,\"assignStartUserHandlerType\":1," +
            "\"timeoutHandlerEnable\":true,\"timeoutHandlerType\":2,\"timeDuration\":1,\"timeUnit\":1," +
            "\"maxRemindCount\":1,\"signEnable\":false,\"fieldsPermission\":[]}";
        var body =
            "<bpmn:startEvent id=\"StartEvent_1\" name=\"Start\"><bpmn:outgoing>f1</bpmn:outgoing></bpmn:startEvent>" +
            "<bpmn:userTask id=\"UserTask_1\" name=\"策略节点\">" +
            "<bpmn:incoming>f1</bpmn:incoming><bpmn:outgoing>f2</bpmn:outgoing>" +
            Ext("ApproverPick", "Collaborate", null, policy) +
            "</bpmn:userTask>" +
            "<bpmn:endEvent id=\"EndEvent_1\" name=\"End\"><bpmn:incoming>f2</bpmn:incoming></bpmn:endEvent>" +
            "<bpmn:sequenceFlow id=\"f1\" sourceRef=\"StartEvent_1\" targetRef=\"UserTask_1\" />" +
            "<bpmn:sequenceFlow id=\"f2\" sourceRef=\"UserTask_1\" targetRef=\"EndEvent_1\" />";
        return BpmnShell(UatAProcessId, "SEED_UAT_A", "UAT A 空人同人超时", body, DiLinearOneTask());
    }

    static string UatBBpmn()
    {
        var buttons =
            "[{\"id\":\"Approve\",\"enable\":true,\"displayName\":\"通过\"}," +
            "{\"id\":\"Terminate\",\"enable\":true,\"displayName\":\"终止\"}," +
            "{\"id\":\"Discard\",\"enable\":true,\"displayName\":\"作废\"}]";
        var body =
            "<bpmn:startEvent id=\"StartEvent_1\" name=\"Start\"><bpmn:outgoing>f1</bpmn:outgoing></bpmn:startEvent>" +
            "<bpmn:userTask id=\"UserTask_1\" name=\"管理闸\">" +
            "<bpmn:incoming>f1</bpmn:incoming><bpmn:outgoing>f2</bpmn:outgoing>" +
            Ext("ApproverPick", "Collaborate", buttons, null) +
            "</bpmn:userTask>" +
            "<bpmn:endEvent id=\"EndEvent_1\" name=\"End\"><bpmn:incoming>f2</bpmn:incoming></bpmn:endEvent>" +
            "<bpmn:sequenceFlow id=\"f1\" sourceRef=\"StartEvent_1\" targetRef=\"UserTask_1\" />" +
            "<bpmn:sequenceFlow id=\"f2\" sourceRef=\"UserTask_1\" targetRef=\"EndEvent_1\" />";
        return BpmnShell(UatBProcessId, "SEED_UAT_B", "UAT B 终止作废", body, DiLinearOneTask());
    }

    static string UatCBpmn()
    {
        var policy =
            "{\"approveType\":1,\"assignEmptyHandlerType\":2,\"signEnable\":true,\"reasonRequire\":false," +
            "\"fieldsPermission\":[" +
            "{\"field\":\"F9aqmwg7hka8abc\",\"title\":\"姓名\",\"permission\":\"editable\"}," +
            "{\"field\":\"F4vqmwg7hlfwaec\",\"title\":\"简介\",\"permission\":\"readonly\"}," +
            "{\"field\":\"Flbimwg7hmm9ahc\",\"title\":\"意向\",\"permission\":\"hidden\"}," +
            "{\"field\":\"mask_demo\",\"title\":\"脱敏\",\"permission\":\"masked\"}" +
            "]}";
        var body =
            "<bpmn:startEvent id=\"StartEvent_1\" name=\"Start\"><bpmn:outgoing>f1</bpmn:outgoing></bpmn:startEvent>" +
            "<bpmn:userTask id=\"UserTask_1\" name=\"字段权限\">" +
            "<bpmn:incoming>f1</bpmn:incoming><bpmn:outgoing>f2</bpmn:outgoing>" +
            Ext("ApproverPick", "Collaborate", null, policy) +
            "</bpmn:userTask>" +
            "<bpmn:endEvent id=\"EndEvent_1\" name=\"End\"><bpmn:incoming>f2</bpmn:incoming></bpmn:endEvent>" +
            "<bpmn:sequenceFlow id=\"f1\" sourceRef=\"StartEvent_1\" targetRef=\"UserTask_1\" />" +
            "<bpmn:sequenceFlow id=\"f2\" sourceRef=\"UserTask_1\" targetRef=\"EndEvent_1\" />";
        return BpmnShell(UatCProcessId, "SEED_UAT_C", "UAT C 字段权限", body, DiLinearOneTask());
    }
}
