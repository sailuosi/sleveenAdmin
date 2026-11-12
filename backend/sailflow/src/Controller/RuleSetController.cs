using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.engine.Entity;
using sailflow.engine.Manager;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Service;
using sailflow.WebUtility;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>
/// 规则集（契约对齐 RuleSet；wf_rule_set）。
/// 路由：api/RuleSet/{Action}
/// </summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class RuleSetController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    private readonly SailClient _sail;

    public RuleSetController(IWorkflowService workflowService, SailClient sail)
    {
        _workflowService = workflowService;
        _sail = sail;
    }

    /// <summary>GET api/RuleSet/GetList</summary>
    [HttpGet]
    public ResponseResult<IList<RuleSetEntity>> GetList()
    {
        try
        {
            return ResponseResult<IList<RuleSetEntity>>.Success(_workflowService.GetRuleSetList());
        }
        catch (Exception ex)
        {
            return ResponseResult<IList<RuleSetEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/RuleSet/Get?ruleSetCode=</summary>
    [HttpGet]
    public ResponseResult<RuleSetEntity> Get([FromQuery] string? ruleSetCode)
    {
        try
        {
            return ResponseResult<RuleSetEntity>.Success(_workflowService.GetRuleSet(ruleSetCode ?? ""));
        }
        catch (Exception ex)
        {
            return ResponseResult<RuleSetEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/RuleSet/Save</summary>
    [HttpPost]
    public ResponseResult Save([FromBody] RuleSetEntity? entity)
    {
        try
        {
            if (entity == null
                || string.IsNullOrEmpty(entity.RuleSetCode)
                || string.IsNullOrEmpty(entity.RuleSetName))
            {
                return ResponseResult.Error("ruleSetCode and ruleSetName are required");
            }

            if (!RuleSetModeHelper.TryParseFromPersisted(entity.Mode, out var modeEnum))
            {
                return ResponseResult.Error(
                    "mode must be \"" + RuleSetModeHelper.PersistedRuleTypes + "\" or \"" +
                    RuleSetModeHelper.PersistedBindingsJson + "\"");
            }

            var modePersisted = RuleSetModeHelper.ToPersistedString(modeEnum);
            var mgr = new RuleSetExecutionManager(_sail);
            if (!mgr.ValidateRuleContent(modePersisted, entity.RuleContent ?? "", out var vmsg))
                return ResponseResult.Error(vmsg ?? "rule_content validation failed");

            entity.Mode = modePersisted;
            _workflowService.SaveRuleSet(entity);
            return ResponseResult.Success();
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>DELETE api/RuleSet/Delete?ruleSetCode=</summary>
    [HttpDelete]
    public ResponseResult Delete([FromQuery] string? ruleSetCode)
    {
        try
        {
            _workflowService.DeleteRuleSet(ruleSetCode ?? "");
            return ResponseResult.Success();
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/RuleSet/Execute — NRules / bindingsJson 试跑。</summary>
    [HttpPost]
    public ResponseResult<RuleExecutionResultEntity> Execute([FromBody] RuleExecutionEntity? request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RuleSetCode))
                return ResponseResult<RuleExecutionResultEntity>.Error("ruleSetCode is required");

            var ruleSet = new RuleSetManager(_sail).GetByCode(request.RuleSetCode);
            if (ruleSet == null)
                return ResponseResult<RuleExecutionResultEntity>.Error("rule set not found");

            var result = new RuleSetExecutionManager(_sail).Execute(ruleSet, request.Variables);
            return ResponseResult<RuleExecutionResultEntity>.Success(result);
        }
        catch (Exception ex)
        {
            return ResponseResult<RuleExecutionResultEntity>.Error(ex.Message);
        }
    }
}
