using System.Security.Claims;
using mooSQL.data;
using sailflow.data;
using sailflow.hosting;
using sailflow.service;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;
using SleveenAdmin.Sailflow.UnitScope;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>流程编辑：定义字段 + sf_process_meta 产品扩展 + Auth* 归属。</summary>
[Scoped]
public class SailProcessMetaService
{
    private readonly SailClient _sail;
    private readonly IWorkflowService _wf;
    private readonly SailUnitAccess _access;

    public SailProcessMetaService(SailClient sail, IWorkflowService wf, SailUnitAccess access)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
        _wf = wf ?? throw new ArgumentNullException(nameof(wf));
        _access = access ?? throw new ArgumentNullException(nameof(access));
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

    public ProcessEditBundle GetBundle(string processId, string version, ClaimsPrincipal? user = null)
    {
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        var process = _wf.GetProcessByVersion(processId, ver);
        if (process == null)
            throw new InvalidOperationException("process not found");
        if (user != null)
            _access.RequireProcessAccess(user, process, write: false);
        var meta = GetMeta(processId, ver) ?? CreateDefaultMeta(processId, ver);
        var hasBpmn = !string.IsNullOrWhiteSpace(process.XmlContent)
                      && process.XmlContent.IndexOf("<process", StringComparison.OrdinalIgnoreCase) >= 0;
        var auth = SailProcessAuthStore.Read(_sail, processId, ver);
        return new ProcessEditBundle
        {
            Process = process,
            Meta = meta,
            HasBpmn = hasBpmn,
            AuthType = auth.AuthType,
            AuthCode = auth.AuthCode,
            AuthId = auth.AuthId,
            AuthLevel = auth.AuthLevel
        };
    }

    public ProcessEditBundle Save(ProcessEditSaveRequest req, ClaimsPrincipal? user = null)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrWhiteSpace(req.ProcessName))
            throw new ArgumentException("ProcessName required");
        if (string.IsNullOrWhiteSpace(req.ProcessCode))
            throw new ArgumentException("ProcessCode required");

        EnsureTable();
        var ver = string.IsNullOrWhiteSpace(req.Version) ? "1" : req.Version!.Trim();
        ValidateAuthFields(req, user);
        ProcessEntity process;

        if (string.IsNullOrWhiteSpace(req.ProcessId))
        {
            // ProcessId 须为合法 BPMN NCName（不可数字开头），否则设计器 import 失败
            var processId = ProcessXmlBuilder.NewProcessId();
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
            if (user != null)
            {
                var preview = BuildAuthBag(processId, ver, req, existing: null);
                _access.RequireAccess(user, preview.AuthType, preview.AuthId, preview.AuthCode, write: true);
                EnsureBusinessTypeUnique(req.BusinessType, preview.AuthType, preview.AuthId, excludeProcessId: null);
            }
            _wf.CreateProcess(entity);
            process = _wf.GetProcessByVersion(processId, ver)
                      ?? throw new InvalidOperationException("create process failed");
            WriteAuth(process.ProcessId, ver, req, existing: null);
        }
        else
        {
            process = _wf.GetProcessByVersion(req.ProcessId!, ver)
                      ?? throw new InvalidOperationException("process not found");
            if (user != null)
                _access.RequireProcessAccess(user, process, write: true);
            process.ProcessName = req.ProcessName!.Trim();
            // ProcessCode 创建后不可改（对齐 n_flowable key）
            process.Description = req.Description ?? "";
            process.AppType = req.AppType ?? "";
            process.Icon = req.Icon ?? "";
            if (req.Status.HasValue) process.Status = req.Status;
            var existingAuth = SailProcessAuthStore.Read(_sail, process.ProcessId, ver);
            var nextAuth = BuildAuthBag(process.ProcessId, ver, req, existingAuth);
            if (user != null)
                _access.RequireAccess(user, nextAuth.AuthType, nextAuth.AuthId, nextAuth.AuthCode, write: true);
            EnsureBusinessTypeUnique(req.BusinessType, nextAuth.AuthType, nextAuth.AuthId, process.ProcessId);
            _wf.UpdateProcess(process);
            process = _wf.GetProcessByVersion(req.ProcessId!, ver)!;
            SailProcessAuthStore.Write(_sail, nextAuth);
        }

        var meta = UpsertMeta(process.ProcessId, ver, req);
        var hasBpmn = !string.IsNullOrWhiteSpace(process.XmlContent);
        var auth = SailProcessAuthStore.Read(_sail, process.ProcessId, ver);
        return new ProcessEditBundle
        {
            Process = process,
            Meta = meta,
            HasBpmn = hasBpmn,
            AuthType = auth.AuthType,
            AuthCode = auth.AuthCode,
            AuthId = auth.AuthId,
            AuthLevel = auth.AuthLevel
        };
    }

    void WriteAuth(string processId, string version, ProcessEditSaveRequest req, SailProcessAuthBag? existing)
    {
        SailProcessAuthStore.Write(_sail, BuildAuthBag(processId, version, req, existing));
    }

    static SailProcessAuthBag BuildAuthBag(
        string processId,
        string version,
        ProcessEditSaveRequest req,
        SailProcessAuthBag? existing)
    {
        var auth = existing ?? new SailProcessAuthBag { ProcessId = processId, Version = version };
        auth.ProcessId = processId;
        auth.Version = version;
        if (req.AuthType != null) auth.AuthType = req.AuthType.Trim();
        if (req.AuthCode != null) auth.AuthCode = req.AuthCode.Trim();
        if (req.AuthId != null) auth.AuthId = req.AuthId.Trim();
        if (req.AuthLevel.HasValue) auth.AuthLevel = req.AuthLevel;

        if (SailAuthScope.EqualsType(auth.AuthType, SailAuthScope.Unit)
            && string.IsNullOrWhiteSpace(auth.AuthId))
            throw new ArgumentException("UNIT 归属须填写 AuthId（单位 Id）");

        if (SailAuthScope.EqualsType(auth.AuthType, SailAuthScope.Public)
            || SailAuthScope.EqualsType(auth.AuthType, SailAuthScope.Template))
            auth.AuthId = "";

        return auth;
    }

    void ValidateAuthFields(ProcessEditSaveRequest req, ClaimsPrincipal? user)
    {
        if (string.IsNullOrWhiteSpace(req.AuthType)) return;
        if (!SailAuthScope.IsKnown(req.AuthType) && !SailAuthScope.IsUnassigned(req.AuthType))
            throw new ArgumentException("AuthType 须为 PUBLIC / UNIT / TEMPLATE 或空");

        if (SailAuthScope.EqualsType(req.AuthType, SailAuthScope.Unit)
            && user != null
            && !_access.CanManageUnit(user, req.AuthId)
            && !_access.IsPlatformAdmin(user))
            throw new SailUnitAccessException("无权为该单位创建或修改流程");

        if ((SailAuthScope.EqualsType(req.AuthType, SailAuthScope.Public)
             || SailAuthScope.EqualsType(req.AuthType, SailAuthScope.Template))
            && user != null
            && !_access.IsPlatformAdmin(user))
            throw new SailUnitAccessException("仅平台管理员可维护 PUBLIC / TEMPLATE");
    }

    void EnsureBusinessTypeUnique(string? businessType, string? authType, string? authId, string? excludeProcessId)
    {
        var bt = (businessType ?? "").Trim();
        if (string.IsNullOrWhiteSpace(bt)) return;

        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailProcessMetaEntity>(out var m);
        clip.where(() => m.BusinessType, bt);
        var metas = clip.select(m).queryList();
        foreach (var meta in metas)
        {
            if (!string.IsNullOrWhiteSpace(excludeProcessId)
                && string.Equals(meta.ProcessId, excludeProcessId, StringComparison.OrdinalIgnoreCase))
                continue;
            var otherAuth = SailProcessAuthStore.Read(_sail, meta.ProcessId, meta.Version);
            var sameLayer = string.Equals(otherAuth.AuthType ?? "", authType ?? "", StringComparison.OrdinalIgnoreCase)
                            && string.Equals(otherAuth.AuthId ?? "", authId ?? "", StringComparison.OrdinalIgnoreCase);
            if (sameLayer)
                throw new InvalidOperationException($"同层业务标识已存在：{bt}");
        }
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
        if (req.BusinessType != null)
            m.BusinessType = req.BusinessType.Trim();
    }

    private static SailProcessMetaEntity CreateDefaultMeta(string processId, string version) => new()
    {
        Id = Guid.NewGuid().ToString("N"),
        ProcessId = processId,
        Version = version,
        FormType = 10,
        Visible = 1,
        BusinessType = "",
        StartUserIds = "[]",
        StartDeptIds = "[]",
        ManagerUserIds = "[]",
        ExtraJson = "{}"
    };
}
