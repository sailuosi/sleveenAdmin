using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mooSQL.data;
using sailflow.biz.query;
using sailflow.data;
using sailflow.engine.Client;
using sailflow.engine.Service;
using sailflow.xml;
using sailflow.hub.Form;
using sailflow.hub.Resource;
using sailflow.utils;
using SleveenAdmin.Sailflow.Assignee;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Operations;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.UnitScope;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>
/// 设计器定义类 MVP（契约对齐 Wf2Xml；经本机 Web.Api）。
/// 路由：api/Wf2Xml/{Action}
/// </summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public partial class Wf2XmlController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly WorkflowQueryService _queryService;
    private readonly SailClient _sail;
    private readonly SailOperationDispatcher _operationDispatcher;
    private readonly SailProcessMetaService _processMeta;
    private readonly SailProcessExtraService _processExtra;
    private readonly SailUnitAccess _unitAccess;
    private readonly SailReleaseService _release;
    private readonly ILogger<Wf2XmlController> _logger;

    public Wf2XmlController(
        IWorkflowService workflowService,
        WorkflowQueryService queryService,
        SailClient sail,
        SailOperationDispatcher operationDispatcher,
        SailProcessMetaService processMeta,
        SailProcessExtraService processExtra,
        SailUnitAccess unitAccess,
        SailReleaseService release,
        ILogger<Wf2XmlController> logger)
    {
        _workflowService = workflowService;
        _queryService = queryService;
        _sail = sail;
        _operationDispatcher = operationDispatcher;
        _processMeta = processMeta;
        _processExtra = processExtra;
        _unitAccess = unitAccess;
        _release = release;
        _logger = logger;
    }

    /// <summary>GET api/Wf2Xml/GetProcessListSimple?workspace=templates|unit（含 Auth*）</summary>
    [HttpGet]
    public ResponseResult<List<object>> GetProcessListSimple([FromQuery] string? workspace = null)
    {
        try
        {
            var list = _queryService.GetProcessListSimple().ToList();
            var filtered = _unitAccess.FilterProcesses(User, list, workspace).ToList();
            var shaped = filtered.Select(p =>
            {
                var auth = _unitAccess.GetProcessAuth(p.ProcessId, p.Version ?? "1");
                return (object)new
                {
                    p.ProcessId,
                    p.ProcessName,
                    p.ProcessCode,
                    p.Version,
                    p.Status,
                    p.AppType,
                    p.Description,
                    p.Icon,
                    p.ProcessDefId,
                    AuthType = auth.AuthType,
                    AuthCode = auth.AuthCode,
                    AuthId = auth.AuthId,
                    AuthLevel = auth.AuthLevel
                };
            }).ToList();
            return ResponseResult<List<object>>.Success(shaped);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<object>>.Error("GetProcessListSimple failed: " + ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/GetProcessFile/{id}</summary>
    [HttpGet]
    public ResponseResult<ProcessFileEntity> GetProcessFile(string id)
    {
        try
        {
            var entity = _workflowService.GetProcessFileById(id);
            NormalizeXmlContent(entity);
            return ResponseResult<ProcessFileEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessFileEntity>.Error("GetProcessFile failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/QueryProcessFile</summary>
    [HttpPost]
    public ResponseResult<ProcessFileEntity> QueryProcessFile([FromBody] ProcessFileQuery query)
    {
        try
        {
            var entity = _workflowService.GetProcessFile(query.ProcessId, query.Version);
            NormalizeXmlContent(entity);
            return ResponseResult<ProcessFileEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessFileEntity>.Error("QueryProcessFile failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/SaveProcessFile</summary>
    [HttpPost]
    public ResponseResult SaveProcessFile([FromBody] ProcessFileEntity entity)
    {
        try
        {
            // 不再 PadRight(4096)：SQLite/PG 无害但会污染内容；短 XML 原样存储即可
            entity.XmlContent = NormalizeXmlContent(entity.XmlContent);
            _workflowService.SaveProcessFile(entity);
            return ResponseResult.Success("SaveProcessFile ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error("SaveProcessFile failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/CreateProcess</summary>
    [HttpPost]
    public ResponseResult<ProcessEntity> CreateProcess([FromBody] ProcessFileEntity fileEntity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileEntity.ProcessName)
                || string.IsNullOrWhiteSpace(fileEntity.ProcessCode)
                || string.IsNullOrWhiteSpace(fileEntity.Version))
            {
                return ResponseResult<ProcessEntity>.Error("ProcessName / ProcessCode / Version required");
            }

            if (string.IsNullOrEmpty(fileEntity.ProcessId))
                fileEntity.ProcessId = ProcessXmlBuilder.NewProcessId();
            else
                fileEntity.ProcessId = ProcessXmlBuilder.ToBpmnElementId(fileEntity.ProcessId);
            if (string.IsNullOrEmpty(fileEntity.Version))
                fileEntity.Version = "1";

            var entity = new ProcessEntity
            {
                ProcessId = fileEntity.ProcessId,
                ProcessName = fileEntity.ProcessName,
                ProcessCode = fileEntity.ProcessCode,
                Version = fileEntity.Version,
                Status = fileEntity.Status,
                Description = fileEntity.Description,
                AppType = "",
                Icon = ""
            };

            fileEntity.XmlContent = ProcessXmlBuilder.GenerateXmlContentWithStartNode(
                fileEntity.ProcessId,
                fileEntity.ProcessName,
                fileEntity.ProcessCode,
                fileEntity.Version);
            entity.XmlContent = fileEntity.XmlContent;

            // 新建默认同待分配；归属在 WfProcessEdit/Save 写入 Auth*
            var processId = _workflowService.CreateProcess(entity);
            entity.ProcessDefId = processId;
            return ResponseResult<ProcessEntity>.Success(entity, "CreateProcess ok");
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessEntity>.Error("CreateProcess failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/UpdateProcess</summary>
    [HttpPost]
    public ResponseResult UpdateProcess([FromBody] ProcessEntity entity)
    {
        try
        {
            var processEntity = _workflowService.GetProcessByVersion(entity.ProcessId, entity.Version);
            _unitAccess.RequireProcessAccess(User, processEntity, write: true);
            processEntity.ProcessName = entity.ProcessName;
            processEntity.ProcessCode = entity.ProcessCode;
            processEntity.XmlFileName = entity.XmlFileName;
            processEntity.AppType = entity.AppType;
            processEntity.Description = entity.Description;
            processEntity.Status = entity.Status;
            if (entity.Icon != null)
                processEntity.Icon = entity.Icon;
            if (!string.IsNullOrEmpty(entity.XmlContent))
                processEntity.XmlContent = PaddingContentWithRightSpace(entity.XmlContent);

            _workflowService.UpdateProcess(processEntity);
            return ResponseResult.Success("UpdateProcess ok");
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResponseResult.Error("UpdateProcess failed: " + ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/DeleteProcessById/{id}（逻辑删除）</summary>
    [HttpGet]
    public ResponseResult DeleteProcessById(string id)
    {
        try
        {
            SoftDeleteProcessByDefId(id);
            return ResponseResult.Success("DeleteProcessById ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error("DeleteProcessById failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/DeleteProcess · { ProcessId, Version }（逻辑删除）</summary>
    [HttpPost]
    public ResponseResult DeleteProcess([FromBody] ProcessQuery query)
    {
        try
        {
            if (query == null || string.IsNullOrWhiteSpace(query.ProcessId))
                return ResponseResult.Error("ProcessId required");
            SoftDeleteProcess(query.ProcessId, query.Version ?? "1");
            return ResponseResult.Success("DeleteProcess ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error("DeleteProcess failed: " + ex.Message);
        }
    }

    void SoftDeleteProcess(string processId, string version)
    {
        var entity = _workflowService.GetProcessByVersion(processId, version);
        if (entity == null) throw new InvalidOperationException("process not found");
        _unitAccess.RequireProcessAccess(User, entity, write: true);
        if (SailSoftDelete.IsDeleted(entity.Deleted)) return;
        SailSoftDelete.MarkDeleted(v => entity.Deleted = v, t => entity.DeleteTime = t);
        entity.UpdatedDateTime = DateTime.Now;
        _workflowService.UpdateProcess(entity);
    }

    void SoftDeleteProcessByDefId(string processDefId)
    {
        var clip = _sail.UseRunSQL().useClip();
        clip.from<ProcessEntity>(out var p);
        clip.where(() => p.ProcessDefId, processDefId);
        var entity = clip.select(p).queryList().FirstOrDefault()
            ?? throw new InvalidOperationException("process not found");
        SoftDeleteProcess(entity.ProcessId, entity.Version ?? "1");
    }

    /// <summary>POST api/Wf2Xml/UpdateProcessUsingState · { ProcessId, Version, UsingState } · 1启用 0停用</summary>
    [HttpPost]
    public ResponseResult UpdateProcessUsingState([FromBody] ProcessUsingStateRequest body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ProcessId))
                return ResponseResult.Error("ProcessId required");

            var version = string.IsNullOrWhiteSpace(body.Version) ? "1" : body.Version!;
            // 启用=发布：硬拦结构 + R6 Resolve/Leave；停用不校验
            if (body.UsingState == 1)
            {
                var process = _workflowService.GetProcessByVersion(body.ProcessId, version);
                if (process == null || string.IsNullOrWhiteSpace(process.XmlContent))
                    return ResponseResult.Error("发布失败：流程定义或 XmlContent 为空");

                var vr = _workflowService.ValidateProcess(process);
                if (vr.ProcessValidatedResultType != ProcessValidateResultTypeEnum.Successed)
                {
                    var detail = !string.IsNullOrWhiteSpace(vr.Message)
                        ? vr.Message
                        : (vr.Issues != null && vr.Issues.Count > 0
                            ? string.Join("; ", vr.Issues)
                            : vr.ProcessValidatedResultType.ToString());
                    return ResponseResult.Error("发布校验未通过：" + detail);
                }
            }

            if (body.UsingState == 1 || body.UsingState == 0)
            {
                var process = _workflowService.GetProcessByVersion(body.ProcessId, version);
                if (process != null)
                    _unitAccess.RequireProcessAccess(User, process, write: true);
            }

            _workflowService.UpdateProcessUsingState(body.ProcessId, version, body.UsingState);
            try
            {
                _release.OnUsingStateChanged(body.ProcessId, version, body.UsingState);
            }
            catch (Exception releaseEx)
            {
                _logger.LogWarning(releaseEx, "Publish snapshot/route failed after UsingState change");
            }
            return ResponseResult.Success("UpdateProcessUsingState ok");
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResponseResult.Error("UpdateProcessUsingState failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/UpgradeProcess · { ProcessId, Version, NewVersion }</summary>
    [HttpPost]
    public ResponseResult UpgradeProcess([FromBody] ProcessUpgradeRequest body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ProcessId))
                return ResponseResult.Error("ProcessId required");
            var newVersion = string.IsNullOrWhiteSpace(body.NewVersion)
                ? NextVersion(body.Version ?? "1")
                : body.NewVersion;
            _workflowService.UpgradeProcess(body.ProcessId, body.Version ?? "1", newVersion);
            return ResponseResult.Success($"UpgradeProcess ok → {newVersion}");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error("UpgradeProcess failed: " + ex.Message);
        }
    }

    private static string NextVersion(string version)
    {
        if (int.TryParse(version, out var n))
            return (n + 1).ToString();
        return version + ".1";
    }

    /// <summary>POST api/Wf2Xml/ValidateProcess</summary>
    [HttpPost]
    public ResponseResult<ProcessValidateResult> ValidateProcess([FromBody] ProcessEntity entity)
    {
        try
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.XmlContent))
                return ResponseResult<ProcessValidateResult>.Error("XmlContent required");

            var validateResult = _workflowService.ValidateProcess(entity);
            return ResponseResult<ProcessValidateResult>.Success(validateResult, "ValidateProcess ok");
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessValidateResult>.Error("ValidateProcess failed: " + ex.Message);
        }
    }

    /// <summary>GET api/Wf2Xml/InitNewBPMNFile</summary>
    [HttpGet]
    public ResponseResult<ProcessFileEntity> InitNewBPMNFile()
    {
        try
        {
            var entity = ProcessXmlBuilder.InitNewBPMNFile();
            return ResponseResult<ProcessFileEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessFileEntity>.Error("InitNewBPMNFile failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/CreateProcessByTemplate · { TemplateName }（Blank 走 InitNew；其余读 samples）</summary>
    [HttpPost]
    public ResponseResult<ProcessFileEntity> CreateProcessByTemplate([FromBody] ProcessTemplate? processTemplate)
    {
        try
        {
            var name = processTemplate?.TemplateName?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
                return ResponseResult<ProcessFileEntity>.Error("TemplateName required");

            string xmlContent;
            if (string.Equals(name, "Blank", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Default", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Simple", StringComparison.OrdinalIgnoreCase))
            {
                xmlContent = ProcessXmlBuilder.InitNewBPMNFile().XmlContent;
            }
            else
            {
                xmlContent = BpmnFileSampleDefine.RebuildBpmnProcessXmlContentByTemplate(name);
            }

            var entity = _workflowService.CreateProcessByXML(xmlContent);
            return ResponseResult<ProcessFileEntity>.Success(entity, "CreateProcessByTemplate ok");
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessFileEntity>.Error("CreateProcessByTemplate failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/GetRoleAll</summary>
    [HttpPost]
    public ResponseResult<List<Role>> GetRoleAll()
    {
        try
        {
            return ResponseResult<List<Role>>.Success(_workflowService.GetRoleAll().ToList());
        }
        catch (Exception ex)
        {
            return ResponseResult<List<Role>>.Error("GetRoleAll failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/GetUserAll · R3 优先返回 SysUser.Account 作为 UserId</summary>
    [HttpPost]
    public ResponseResult<List<User>> GetUserAll()
    {
        try
        {
            var accounts = SleveenUserIdentity.LoadActiveAccounts();
            if (accounts.Count > 0)
            {
                var host = accounts
                    .Select(kv => new User { UserId = kv.Key, UserName = kv.Value })
                    .ToList();
                return ResponseResult<List<User>>.Success(host);
            }

            var sail = _workflowService.GetUserAll()?.ToList() ?? new List<User>();
            SleveenUserIdentity.RemapUsers(sail);
            return ResponseResult<List<User>>.Success(sail);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<User>>.Error("GetUserAll failed: " + ex.Message);
        }
    }

    /// <summary>POST api/Wf2Xml/GetFormAll</summary>
    [HttpPost]
    public ResponseResult<List<Form>> GetFormAll()
    {
        try
        {
            return ResponseResult<List<Form>>.Success(_workflowService.GetFormAll().ToList());
        }
        catch (Exception ex)
        {
            return ResponseResult<List<Form>>.Error("GetFormAll failed: " + ex.Message);
        }
    }

    /// <summary>去掉历史 PadRight(4096) 尾部空格；禁止截断。</summary>
    private static string NormalizeXmlContent(string? content)
        => string.IsNullOrEmpty(content) ? (content ?? "") : content.TrimEnd();

    private static void NormalizeXmlContent(ProcessFileEntity? entity)
    {
        if (entity == null) return;
        entity.XmlContent = NormalizeXmlContent(entity.XmlContent);
    }

    /// <summary>
    /// 兼容旧调用：Oracle 短字段垫宽已废弃；仅 TrimEnd，绝不截断。
    /// </summary>
    private static string PaddingContentWithRightSpace(string content)
        => NormalizeXmlContent(content);
}

/// <summary>启用/停用流程版本（Status：1 启用 · 0 停用）。</summary>
public class ProcessUsingStateRequest
{
    public string? ProcessId { get; set; }
    public string? Version { get; set; }
    public byte UsingState { get; set; }
}

/// <summary>升级流程版本。</summary>
public class ProcessUpgradeRequest
{
    public string? ProcessId { get; set; }
    public string? Version { get; set; }
    public string? NewVersion { get; set; }
}
