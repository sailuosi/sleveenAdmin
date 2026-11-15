using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.engine.Common;
using sailflow.engine.Result;
using sailflow.engine.Service;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Controller;

public sealed class WfBizStartRequest
{
    public string? BusinessType { get; set; }
    public string? UnitId { get; set; }
    public string? ProcessId { get; set; }
    public string? Version { get; set; }
    public string? AppInstanceCode { get; set; }
    public Dictionary<string, string>? Conditions { get; set; }
}

/// <summary>按业务标识 + 单位解析生效定义后发起（禁止仅凭任意 ProcessId 绕过）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfBizStartController : ControllerBase
{
    private readonly SailReleaseService _release;
    private readonly IWorkflowService _wf;

    public WfBizStartController(SailReleaseService release, IWorkflowService wf)
    {
        _release = release;
        _wf = wf;
    }

    /// <summary>POST api/WfBizStart/Resolve</summary>
    [HttpPost]
    public ResponseResult<SailResolvedDefinition> Resolve([FromBody] WfBizStartRequest? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.BusinessType))
                return ResponseResult<SailResolvedDefinition>.Error("BusinessType required");
            return ResponseResult<SailResolvedDefinition>.Success(
                _release.Resolve(body.BusinessType!, body.UnitId ?? ""));
        }
        catch (Exception ex)
        {
            return ResponseResult<SailResolvedDefinition>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfBizStart/Start · businessType+unit 解析后 StartProcess</summary>
    [HttpPost]
    public async Task<ResponseResult<object>> Start([FromBody] WfBizStartRequest? body, CancellationToken ct)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.BusinessType))
                return ResponseResult<object>.Error("BusinessType required");

            var resolved = _release.Resolve(body.BusinessType!, body.UnitId ?? "");

            // 若前端带了 ProcessId，必须与 Resolve 一致，防止绕过
            if (!string.IsNullOrWhiteSpace(body.ProcessId)
                && !string.Equals(body.ProcessId, resolved.ProcessId, StringComparison.OrdinalIgnoreCase))
                return ResponseResult<object>.Error("ProcessId 与当前生效定义不一致，请刷新后重试");

            if (!string.IsNullOrWhiteSpace(body.Version)
                && !string.Equals(body.Version, resolved.Version, StringComparison.OrdinalIgnoreCase))
                return ResponseResult<object>.Error("Version 与当前生效定义不一致，请刷新后重试");

            var runner = new FlowCommandContext
            {
                ProcessId = resolved.ProcessId,
                Version = resolved.Version,
                BizType = body.BusinessType,
                BizCode = body.AppInstanceCode,
                Conditions = body.Conditions ?? new Dictionary<string, string>()
            };
            SailAuthIdentity.BindRunner(User, runner);
            if (!string.IsNullOrWhiteSpace(body.UnitId))
            {
                runner.StartUnitId = body.UnitId;
                runner.CompanyId = body.UnitId;
            }

            var exec = await _wf.StartProcessAsync(runner, ct).ConfigureAwait(false);
            if (exec == null || exec.Status != WfExecutedStatus.Success)
                return ResponseResult<object>.Error(exec?.Message ?? "StartProcess failed");

            return ResponseResult<object>.Success(new
            {
                resolved,
                processInstanceId = exec.ProcessInstanceIdStarted,
                message = exec.Message
            }, "started");
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }
}
