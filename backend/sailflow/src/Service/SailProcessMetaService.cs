using mooSQL.data;
using sailflow.engine.Entity;
using sailflow.engine.Client;
using sailflow.engine.Service;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>流程编辑：定义字段 + sf_process_meta 产品扩展。</summary>
[Scoped]
public class SailProcessMetaService
{
    private readonly SailClient _sail;
    private readonly IWorkflowService _wf;

    public SailProcessMetaService(SailClient sail, IWorkflowService wf)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
        _wf = wf ?? throw new ArgumentNullException(nameof(wf));
    }

    public void EnsureTable() => SailHostSchemaInitializer.EnsureHostTables(_sail);

    public SailProcessMetaEntity? GetMeta(string processId, string version)
    {
        EnsureTable();
        if (string.IsNullOrWhiteSpace(processId)) return null;
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailProcessMetaEntity>(out var m);
        clip.where(() => m.ProcessId, processId);
        clip.where(() => m.Version, ver);
        return clip.select(m).queryList().FirstOrDefault();
    }

    public ProcessEditBundle GetBundle(string processId, string version)
    {
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        var process = _wf.GetProcessByVersion(processId, ver);
        if (process == null)
            throw new InvalidOperationException("process not found");
        var meta = GetMeta(processId, ver) ?? CreateDefaultMeta(processId, ver);
        var hasBpmn = !string.IsNullOrWhiteSpace(process.XmlContent)
                      && process.XmlContent.IndexOf("<process", StringComparison.OrdinalIgnoreCase) >= 0;
        return new ProcessEditBundle
        {
            Process = process,
            Meta = meta,
            HasBpmn = hasBpmn
        };
    }

    public ProcessEditBundle Save(ProcessEditSaveRequest req)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrWhiteSpace(req.ProcessName))
            throw new ArgumentException("ProcessName required");
        if (string.IsNullOrWhiteSpace(req.ProcessCode))
            throw new ArgumentException("ProcessCode required");

        EnsureTable();
        var ver = string.IsNullOrWhiteSpace(req.Version) ? "1" : req.Version!.Trim();
        ProcessEntity process;

        if (string.IsNullOrWhiteSpace(req.ProcessId))
        {
            var processId = Guid.NewGuid().ToString();
            var fileEntity = new ProcessFileEntity
            {
                ProcessId = processId,
                ProcessName = req.ProcessName!.Trim(),
                ProcessCode = req.ProcessCode!.Trim(),
                Version = ver,
                Status = req.Status ?? 0,
                Description = req.Description ?? ""
            };
            fileEntity.XmlContent = ProcessXmlBuilder.GenerateXmlContentWithStartNode(
                processId, fileEntity.ProcessName, fileEntity.ProcessCode, ver);

            var entity = new ProcessEntity
            {
                ProcessId = processId,
                ProcessName = fileEntity.ProcessName,
                ProcessCode = fileEntity.ProcessCode,
                Version = ver,
                Status = fileEntity.Status,
                Description = fileEntity.Description,
                XmlContent = fileEntity.XmlContent,
                AppType = req.AppType ?? "",
                Icon = req.Icon ?? ""
            };
            _wf.CreateProcess(entity);
            process = _wf.GetProcessByVersion(processId, ver)
                      ?? throw new InvalidOperationException("create process failed");
        }
        else
        {
            process = _wf.GetProcessByVersion(req.ProcessId!, ver)
                      ?? throw new InvalidOperationException("process not found");
            process.ProcessName = req.ProcessName!.Trim();
            // ProcessCode 创建后不可改（对齐 n_flowable key）
            process.Description = req.Description ?? "";
            process.AppType = req.AppType ?? "";
            process.Icon = req.Icon ?? "";
            if (req.Status.HasValue) process.Status = req.Status;
            _wf.UpdateProcess(process);
            process = _wf.GetProcessByVersion(req.ProcessId!, ver)!;
        }

        var meta = UpsertMeta(process.ProcessId, ver, req);
        var hasBpmn = !string.IsNullOrWhiteSpace(process.XmlContent);
        return new ProcessEditBundle { Process = process, Meta = meta, HasBpmn = hasBpmn };
    }

    private SailProcessMetaEntity UpsertMeta(string processId, string version, ProcessEditSaveRequest req)
    {
        var existing = GetMeta(processId, version);
        var repo = _sail.UseRunSQL().useRepo<SailProcessMetaEntity>();
        if (existing == null)
        {
            existing = CreateDefaultMeta(processId, version);
            ApplyMetaFields(existing, req);
            existing.UpdatedAt = DateTime.Now;
            repo.Insert(existing);
            return existing;
        }

        ApplyMetaFields(existing, req);
        existing.UpdatedAt = DateTime.Now;
        repo.Update(existing);
        return existing;
    }

    private static void ApplyMetaFields(SailProcessMetaEntity m, ProcessEditSaveRequest req)
    {
        m.FormType = req.FormType;
        m.FormId = req.FormId ?? "";
        m.FormCustomCreatePath = req.FormCustomCreatePath ?? "";
        m.FormCustomViewPath = req.FormCustomViewPath ?? "";
        m.Visible = req.Visible;
        m.StartUserType = req.StartUserType;
        m.StartUserIds = string.IsNullOrWhiteSpace(req.StartUserIds) ? "[]" : req.StartUserIds!;
        m.StartDeptIds = string.IsNullOrWhiteSpace(req.StartDeptIds) ? "[]" : req.StartDeptIds!;
        m.ManagerUserIds = string.IsNullOrWhiteSpace(req.ManagerUserIds) ? "[]" : req.ManagerUserIds!;
        m.ExtraJson = string.IsNullOrWhiteSpace(req.ExtraJson) ? "{}" : req.ExtraJson!;
    }

    private static SailProcessMetaEntity CreateDefaultMeta(string processId, string version) => new()
    {
        Id = Guid.NewGuid().ToString("N"),
        ProcessId = processId,
        Version = version,
        FormType = 10,
        Visible = 1,
        StartUserIds = "[]",
        StartDeptIds = "[]",
        ManagerUserIds = "[]",
        ExtraJson = "{}"
    };
}
