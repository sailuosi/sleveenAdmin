using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.UnitScope;

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

    /// <summary>GET api/WfCategory/List?unitId=</summary>
    [HttpGet]
    public ResponseResult<List<SailProcessCategoryEntity>> List([FromQuery] string? unitId = null)
    {
        try
        {
            return ResponseResult<List<SailProcessCategoryEntity>>.Success(_cats.List(User, unitId));
        }
        catch (Exception ex)
        {
            return ResponseResult<List<SailProcessCategoryEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/WfCategory/NextSort?authType=&amp;unitId=</summary>
    [HttpGet]
    public ResponseResult<object> NextSort([FromQuery] string? authType = null, [FromQuery] string? unitId = null)
    {
        try
        {
            return ResponseResult<object>.Success(new { sortOrder = _cats.NextSort(authType, unitId) });
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfCategory/Save</summary>
    [HttpPost]
    public ResponseResult<SailProcessCategoryEntity> Save([FromBody] SailProcessCategoryEntity? body)
    {
        try
        {
            if (body == null) return ResponseResult<SailProcessCategoryEntity>.Error("body required");
            return ResponseResult<SailProcessCategoryEntity>.Success(_cats.Save(body, User), "saved");
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
            var n = _cats.Delete(id, User);
            return n > 0 ? ResponseResult.Success("deleted") : ResponseResult.Error("not found");
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult.Error(ex.Message);
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

    public class SortBatchRequest
    {
        public List<string>? Ids { get; set; }
    }

    /// <summary>POST api/WfCategory/UpdateSortBatch · { ids: string[] }</summary>
    [HttpPost]
    public ResponseResult UpdateSortBatch([FromBody] SortBatchRequest? body)
    {
        try
        {
            var n = _cats.UpdateSortBatch(body?.Ids ?? [], User);
            return ResponseResult.Success($"sorted {n}");
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }
}
