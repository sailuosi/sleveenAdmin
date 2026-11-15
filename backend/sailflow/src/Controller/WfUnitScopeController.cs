using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.UnitScope;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>单位流程：上下文、复制、审批人配置。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfUnitScopeController : ControllerBase
{
    private readonly SailUnitAccess _access;
    private readonly SailCopyToUnitService _copy;
    private readonly SailAssigneeConfigService _assignee;
    private readonly SailReleaseService _release;

    public WfUnitScopeController(
        SailUnitAccess access,
        SailCopyToUnitService copy,
        SailAssigneeConfigService assignee,
        SailReleaseService release)
    {
        _access = access;
        _copy = copy;
        _assignee = assignee;
        _release = release;
    }

    /// <summary>GET api/WfUnitScope/Context</summary>
    [HttpGet]
    public ResponseResult<object> Context()
    {
        try
        {
            var platform = _access.IsPlatformAdmin(User);
            var units = _access.GetManagedUnitIds(User);
            return ResponseResult<object>.Success(new
            {
                isPlatformAdmin = platform,
                managedUnitIds = units,
                unitAdminDuty = SailAuthScope.UnitAdminDutyCode,
                availableAssigneeStrategies = SailAssigneeConfigService.AvailableStrategies
            });
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfUnitScope/CopyToUnit</summary>
    [HttpPost]
    public ResponseResult<object> CopyToUnit([FromBody] SailCopyToUnitRequest? body)
    {
        try
        {
            if (body == null) return ResponseResult<object>.Error("body required");
            var bundle = _copy.Copy(User, body);
            return ResponseResult<object>.Success(bundle, "copied");
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfUnitScope/GetAssigneeConfig · { ProcessId, Version }</summary>
    [HttpPost]
    public ResponseResult<SailAssigneeConfigBundle> GetAssigneeConfig([FromBody] SailAssigneeConfigSaveRequest? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ProcessId))
                return ResponseResult<SailAssigneeConfigBundle>.Error("ProcessId required");
            return ResponseResult<SailAssigneeConfigBundle>.Success(
                _assignee.Get(User, body.ProcessId!, body.Version ?? "1"));
        }
        catch (Exception ex)
        {
            return ResponseResult<SailAssigneeConfigBundle>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfUnitScope/SaveAssigneeConfig</summary>
    [HttpPost]
    public ResponseResult<SailAssigneeConfigBundle> SaveAssigneeConfig([FromBody] SailAssigneeConfigSaveRequest? body)
    {
        try
        {
            if (body == null) return ResponseResult<SailAssigneeConfigBundle>.Error("body required");
            return ResponseResult<SailAssigneeConfigBundle>.Success(_assignee.Save(User, body), "saved");
        }
        catch (Exception ex)
        {
            return ResponseResult<SailAssigneeConfigBundle>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfUnitScope/Resolve · { BusinessType, UnitId }</summary>
    [HttpPost]
    public ResponseResult<SailResolvedDefinition> Resolve([FromBody] WfBizStartRequest? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.BusinessType))
                return ResponseResult<SailResolvedDefinition>.Error("BusinessType required");
            var resolved = _release.Resolve(body.BusinessType!, body.UnitId ?? "");
            return ResponseResult<SailResolvedDefinition>.Success(resolved);
        }
        catch (Exception ex)
        {
            return ResponseResult<SailResolvedDefinition>.Error(ex.Message);
        }
    }
}
