using System.Security.Claims;
using sailflow.data;
using sailflow.service;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.UnitScope;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Service;

public sealed class SailCopyToUnitRequest
{
    public string? SourceProcessId { get; set; }
    public string? SourceVersion { get; set; }
    public string? TargetUnitId { get; set; }
    public string? ProcessName { get; set; }
    public string? ProcessCode { get; set; }
}

/// <summary>从 TEMPLATE/PUBLIC 复制为单位草稿（AuthType=UNIT）。</summary>
[Scoped]
public class SailCopyToUnitService
{
    private readonly IWorkflowService _wf;
    private readonly SailProcessMetaService _meta;
    private readonly SailUnitAccess _access;

    public SailCopyToUnitService(IWorkflowService wf, SailProcessMetaService meta, SailUnitAccess access)
    {
        _wf = wf;
        _meta = meta;
        _access = access;
    }

    public ProcessEditBundle Copy(ClaimsPrincipal? user, SailCopyToUnitRequest req)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrWhiteSpace(req.SourceProcessId))
            throw new ArgumentException("SourceProcessId required");
        if (string.IsNullOrWhiteSpace(req.TargetUnitId))
            throw new ArgumentException("TargetUnitId required");
        if (!_access.CanManageUnit(user, req.TargetUnitId))
            throw new SailUnitAccessException("无权为目标单位创建流程草稿");

        var srcVer = string.IsNullOrWhiteSpace(req.SourceVersion) ? "1" : req.SourceVersion!.Trim();
        var source = _wf.GetProcessByVersion(req.SourceProcessId!, srcVer)
                     ?? throw new InvalidOperationException("source process not found");

        _access.RequireProcessAccess(user, source, write: false);
        var srcAuth = _access.GetProcessAuth(source.ProcessId, srcVer);

        if (!SailAuthScope.EqualsType(srcAuth.AuthType, SailAuthScope.Template)
            && !SailAuthScope.EqualsType(srcAuth.AuthType, SailAuthScope.Public))
            throw new InvalidOperationException("仅可从 TEMPLATE 或 PUBLIC 流程创建单位草稿");

        if (source.Status != 1)
            throw new InvalidOperationException("源流程须已启用后方可复制");

        var srcMeta = _meta.GetMeta(source.ProcessId, srcVer);
        var processId = ProcessXmlBuilder.NewProcessId();
        var name = string.IsNullOrWhiteSpace(req.ProcessName)
            ? $"{source.ProcessName}-单位草稿"
            : req.ProcessName!.Trim();
        var unitKey = req.TargetUnitId!.Replace("-", "");
        var code = string.IsNullOrWhiteSpace(req.ProcessCode)
            ? $"{source.ProcessCode}_u_{unitKey[..Math.Min(8, unitKey.Length)]}"
            : req.ProcessCode!.Trim();

        var xml = source.XmlContent ?? "";
        if (string.IsNullOrWhiteSpace(xml))
            xml = ProcessXmlBuilder.GenerateXmlContentWithStartNode(processId, name, code, "1");

        var entity = new ProcessEntity
        {
            ProcessId = processId,
            ProcessName = name,
            ProcessCode = code,
            Version = "1",
            Status = 0,
            Description = source.Description ?? "",
            XmlContent = xml,
            AppType = source.AppType ?? "",
            Icon = source.Icon ?? ""
        };
        _wf.CreateProcess(entity);

        var saveReq = new ProcessEditSaveRequest
        {
            ProcessId = processId,
            Version = "1",
            ProcessName = name,
            ProcessCode = code,
            Description = source.Description,
            AppType = source.AppType,
            Icon = source.Icon,
            Status = 0,
            FormType = srcMeta?.FormType ?? 10,
            FormId = srcMeta?.FormId,
            FormCustomCreatePath = srcMeta?.FormCustomCreatePath,
            FormCustomViewPath = srcMeta?.FormCustomViewPath,
            Visible = 0,
            StartUserType = srcMeta?.StartUserType ?? 0,
            StartUserIds = srcMeta?.StartUserIds,
            StartDeptIds = srcMeta?.StartDeptIds,
            ManagerUserIds = srcMeta?.ManagerUserIds,
            ExtraJson = srcMeta?.ExtraJson,
            BusinessType = srcMeta?.BusinessType,
            AuthType = SailAuthScope.Unit,
            AuthId = req.TargetUnitId!.Trim(),
            AuthCode = "",
            AuthLevel = srcAuth.AuthLevel
        };

        return _meta.Save(saveReq, user);
    }
}
