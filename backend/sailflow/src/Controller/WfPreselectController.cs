using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.utils;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>
/// 实例预选办理人（独立控制器；勿并入 Wf2Xml）。
/// 路由：api/WfPreselect/{Action}
/// </summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfPreselectController : ControllerBase
{
    readonly SailPreselectAssigneeService _preselect;

    public WfPreselectController(SailPreselectAssigneeService preselect) =>
        _preselect = preselect ?? throw new ArgumentNullException(nameof(preselect));

    /// <summary>GET api/WfPreselect/ListTargets?processId=&amp;version=</summary>
    [HttpGet]
    public ResponseResult<List<PreselectTargetDto>> ListTargets(
        [FromQuery] string processId,
        [FromQuery] string? version = "1")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(processId))
                return ResponseResult<List<PreselectTargetDto>>.Error("processId is required.");
            var list = _preselect.ListTargets(processId, version).ToList();
            return ResponseResult<List<PreselectTargetDto>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<PreselectTargetDto>>.Error(ex.Message);
        }
    }

    /// <summary>
    /// GET api/WfPreselect/ProbeNext?processId=&amp;version=&amp;processInstanceId=&amp;activityId=
    /// </summary>
    [HttpGet]
    public ResponseResult<PreselectNextProbeDto> ProbeNext(
        [FromQuery] string? processId,
        [FromQuery] string? version,
        [FromQuery] string? processInstanceId,
        [FromQuery] string activityId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(activityId))
                return ResponseResult<PreselectNextProbeDto>.Error("activityId is required.");
            var dto = _preselect.ProbeNext(processId, version, processInstanceId, activityId);
            return ResponseResult<PreselectNextProbeDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ResponseResult<PreselectNextProbeDto>.Error(ex.Message);
        }
    }

    /// <summary>GET api/WfPreselect/Get?processInstanceId=&amp;activityId=</summary>
    [HttpGet]
    public ResponseResult<List<PreselectPerformerDto>> Get(
        [FromQuery] string processInstanceId,
        [FromQuery] string activityId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(processInstanceId) || string.IsNullOrWhiteSpace(activityId))
                return ResponseResult<List<PreselectPerformerDto>>.Error(
                    "processInstanceId and activityId are required.");
            var list = _preselect.Load(processInstanceId, activityId)
                .Select(PreselectPerformerDto.From)
                .ToList();
            return ResponseResult<List<PreselectPerformerDto>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<PreselectPerformerDto>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfPreselect/Replace · 整节点覆盖（可多节点）</summary>
    [HttpPost]
    public ResponseResult Replace([FromBody] PreselectAssigneesRequest req)
    {
        try
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ProcessInstanceId))
                return ResponseResult.Error("ProcessInstanceId is required.");
            if (req.Performers == null || req.Performers.Count == 0)
                return ResponseResult.Error("Performers is required.");

            var map = new Dictionary<string, PerformerList>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in req.Performers)
            {
                if (string.IsNullOrWhiteSpace(kv.Key))
                    continue;
                var list = new PerformerList();
                if (kv.Value != null)
                {
                    foreach (var dto in kv.Value)
                    {
                        if (dto == null || string.IsNullOrWhiteSpace(dto.UserId))
                            continue;
                        list.Add(dto.ToPerformer());
                    }
                }

                map[kv.Key.Trim()] = list;
            }

            var n = _preselect.PersistFromPerformerMap(req.ProcessInstanceId.Trim(), map);
            return ResponseResult.Success($"replaced {n} node(s)");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfPreselect/Clear · ActivityId 空则清整单</summary>
    [HttpPost]
    public ResponseResult Clear([FromBody] PreselectAssigneesRequest req)
    {
        try
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ProcessInstanceId))
                return ResponseResult.Error("ProcessInstanceId is required.");
            if (string.IsNullOrWhiteSpace(req.ActivityId))
                _preselect.ClearAll(req.ProcessInstanceId);
            else
                _preselect.Clear(req.ProcessInstanceId, req.ActivityId);
            return ResponseResult.Success("cleared");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }
}
