using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>我的委托（委托代批配置 · sf_surrogate）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfSurrogateController : ControllerBase
{
    readonly SailSurrogateService _svc;

    public WfSurrogateController(SailSurrogateService svc) =>
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));

    public class SurrogateQuery
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? ProcessId { get; set; }
        public int? Enabled { get; set; }
    }

    /// <summary>POST api/WfSurrogate/MyPage — 仅当前用户作为委托人的配置。</summary>
    [HttpPost]
    public ResponseResult<List<SailSurrogateEntity>> MyPage([FromBody] SurrogateQuery? query)
    {
        try
        {
            query ??= new SurrogateQuery();
            var grantor = SailAuthIdentity.RequireAccount(User);
            var (items, total) = _svc.QueryMine(
                grantor, query.PageIndex, query.PageSize, query.ProcessId, query.Enabled);
            var result = ResponseResult<List<SailSurrogateEntity>>.Success(items);
            result.TotalRowsCount = total;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult<List<SailSurrogateEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfSurrogate/Save</summary>
    [HttpPost]
    public ResponseResult<SailSurrogateEntity> Save([FromBody] SailSurrogateEntity? row)
    {
        try
        {
            if (row == null)
                return ResponseResult<SailSurrogateEntity>.Error("body required");
            SailAuthIdentity.BindCreator(User, out var grantorId, out var grantorName);
            var saved = _svc.Save(row, grantorId, grantorName);
            return ResponseResult<SailSurrogateEntity>.Success(saved, "saved");
        }
        catch (Exception ex)
        {
            return ResponseResult<SailSurrogateEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfSurrogate/SetEnabled/{id}?enabled=1</summary>
    [HttpPost]
    public ResponseResult SetEnabled(string id, [FromQuery] int enabled = 1)
    {
        try
        {
            var grantor = SailAuthIdentity.RequireAccount(User);
            if (!_svc.SetEnabled(id, grantor, enabled == 1))
                return ResponseResult.Error("not found");
            return ResponseResult.Success("ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfSurrogate/Delete/{id}</summary>
    [HttpPost]
    public ResponseResult Delete(string id)
    {
        try
        {
            var grantor = SailAuthIdentity.RequireAccount(User);
            if (!_svc.Delete(id, grantor))
                return ResponseResult.Error("not found");
            return ResponseResult.Success("deleted");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }
}
