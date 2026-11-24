using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.utils;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>我的审批常用语 · sf_approval_phrase。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfApprovalPhraseController : ControllerBase
{
    readonly SailApprovalPhraseService _svc;

    public WfApprovalPhraseController(SailApprovalPhraseService svc) =>
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));

    public class SortBatchRequest
    {
        public List<string>? Ids { get; set; }
    }

    /// <summary>GET api/WfApprovalPhrase/MyList?enabledOnly=1</summary>
    [HttpGet]
    public ResponseResult<List<SailApprovalPhraseEntity>> MyList([FromQuery] int enabledOnly = 1)
    {
        try
        {
            var userId = SailAuthIdentity.RequireAccount(User);
            return ResponseResult<List<SailApprovalPhraseEntity>>.Success(
                _svc.ListMine(userId, enabledOnly != 0));
        }
        catch (Exception ex)
        {
            return ResponseResult<List<SailApprovalPhraseEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfApprovalPhrase/Save</summary>
    [HttpPost]
    public ResponseResult<SailApprovalPhraseEntity> Save([FromBody] SailApprovalPhraseEntity? row)
    {
        try
        {
            if (row == null)
                return ResponseResult<SailApprovalPhraseEntity>.Error("body required");
            var userId = SailAuthIdentity.RequireAccount(User);
            return ResponseResult<SailApprovalPhraseEntity>.Success(_svc.Save(row, userId), "saved");
        }
        catch (Exception ex)
        {
            return ResponseResult<SailApprovalPhraseEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfApprovalPhrase/Delete/{id}</summary>
    [HttpPost]
    public ResponseResult Delete(string id)
    {
        try
        {
            var userId = SailAuthIdentity.RequireAccount(User);
            return _svc.Delete(id, userId)
                ? ResponseResult.Success("deleted")
                : ResponseResult.Error("not found");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfApprovalPhrase/UpdateSortBatch · { ids: string[] }</summary>
    [HttpPost]
    public ResponseResult UpdateSortBatch([FromBody] SortBatchRequest? body)
    {
        try
        {
            var userId = SailAuthIdentity.RequireAccount(User);
            var n = _svc.UpdateSortBatch(body?.Ids ?? [], userId);
            return ResponseResult.Success($"sorted {n}");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }
}
