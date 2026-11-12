using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.engine.Entity;
using sailflow.engine.Service;
using sailflow.WebUtility;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>
/// 任务 2 T2-3：流程变量定义（契约对齐 sfdapi WfVariable；wf_variable）。
/// 路由：api/WfVariable/{Action}
/// </summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfVariableController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public WfVariableController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>GET api/WfVariable/GetList?processId=&amp;version=&amp;activityId=</summary>
    [HttpGet]
    public ResponseResult<IList<VariableEntity>> GetList(
        [FromQuery] string? processId,
        [FromQuery] string? version,
        [FromQuery] string? activityId)
    {
        try
        {
            var list = _workflowService.GetVariableDefinitionList(
                processId ?? "",
                version ?? "1",
                activityId ?? "");
            return ResponseResult<IList<VariableEntity>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<IList<VariableEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfVariable/SaveList?processId=&amp;version=&amp;activityId= — 整节点覆盖。</summary>
    [HttpPost]
    public ResponseResult SaveList(
        [FromQuery] string? processId,
        [FromQuery] string? version,
        [FromQuery] string? activityId,
        [FromBody] IList<VariableEntity>? list)
    {
        try
        {
            if (string.IsNullOrEmpty(processId) || string.IsNullOrEmpty(activityId))
                return ResponseResult.Error("processId 与 activityId 不能为空");

            _workflowService.SaveVariableDefinitionList(
                processId,
                version ?? "1",
                activityId,
                list ?? new List<VariableEntity>());
            return ResponseResult.Success();
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>DELETE api/WfVariable/Delete?id=</summary>
    [HttpDelete]
    public ResponseResult Delete([FromQuery] string id)
    {
        try
        {
            _workflowService.DeleteVariableDefinition(id);
            return ResponseResult.Success();
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }
}
