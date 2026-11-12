using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.WebUtility;
using SleveenAdmin.Saiflow.Dto;
using SleveenAdmin.Saiflow.Service;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>流程编辑壳：基本信息 / 表单设计 / 更多设置（元数据）+ 定义行。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfProcessEditController : ControllerBase
{
    private readonly SailProcessMetaService _meta;

    public WfProcessEditController(SailProcessMetaService meta) => _meta = meta;

    public class GetRequest
    {
        public string? ProcessId { get; set; }
        public string? Version { get; set; }
    }

    /// <summary>POST api/WfProcessEdit/Get · { ProcessId, Version }</summary>
    [HttpPost]
    public ResponseResult<ProcessEditBundle> Get([FromBody] GetRequest? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ProcessId))
                return ResponseResult<ProcessEditBundle>.Error("ProcessId required");
            return ResponseResult<ProcessEditBundle>.Success(
                _meta.GetBundle(body.ProcessId!, body.Version ?? "1"));
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessEditBundle>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfProcessEdit/Save · ProcessEditSaveRequest</summary>
    [HttpPost]
    public ResponseResult<ProcessEditBundle> Save([FromBody] ProcessEditSaveRequest? body)
    {
        try
        {
            if (body == null) return ResponseResult<ProcessEditBundle>.Error("body required");
            var bundle = _meta.Save(body);
            return ResponseResult<ProcessEditBundle>.Success(bundle, "saved");
        }
        catch (Exception ex)
        {
            return ResponseResult<ProcessEditBundle>.Error(ex.Message);
        }
    }
}
