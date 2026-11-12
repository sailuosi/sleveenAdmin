using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>流程分类目录；绑定写入 Process.AppType。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfCategoryController : ControllerBase
{
    private readonly SailCategoryService _cats;

    public WfCategoryController(SailCategoryService cats) => _cats = cats;

    public class BindRequest
    {
        public string? ProcessId { get; set; }
        public string? Version { get; set; }
        public string? CategoryCode { get; set; }
    }

    /// <summary>GET api/WfCategory/List</summary>
    [HttpGet]
    public ResponseResult<List<SailProcessCategoryEntity>> List()
    {
        try
        {
            return ResponseResult<List<SailProcessCategoryEntity>>.Success(_cats.List());
        }
        catch (Exception ex)
        {
            return ResponseResult<List<SailProcessCategoryEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfCategory/Save</summary>
    [HttpPost]
    public ResponseResult<SailProcessCategoryEntity> Save([FromBody] SailProcessCategoryEntity? body)
    {
        try
        {
            if (body == null) return ResponseResult<SailProcessCategoryEntity>.Error("body required");
            return ResponseResult<SailProcessCategoryEntity>.Success(_cats.Save(body), "saved");
        }
        catch (Exception ex)
        {
            return ResponseResult<SailProcessCategoryEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfCategory/Delete/{idOrCode}</summary>
    [HttpPost]
    public ResponseResult Delete(string id)
    {
        try
        {
            var n = _cats.Delete(id);
            return n > 0 ? ResponseResult.Success("deleted") : ResponseResult.Error("not found");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfCategory/BindProcess · { ProcessId, Version, CategoryCode }</summary>
    [HttpPost]
    public ResponseResult BindProcess([FromBody] BindRequest? body)
    {
        try
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ProcessId))
                return ResponseResult.Error("ProcessId required");
            _cats.BindProcess(body.ProcessId!, body.Version ?? "1", body.CategoryCode);
            return ResponseResult.Success("bound");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>GET api/WfCategory/CountByCategory</summary>
    [HttpGet]
    public ResponseResult<object> CountByCategory()
    {
        try
        {
            return ResponseResult<object>.Success(_cats.CountProcessesByCategory());
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }
}
