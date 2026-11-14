using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Integration.UnifiedReport;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>
/// stub: console-only; real port later。
/// 门户待办推送运维（补推占位）。
/// </summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfPortalController(SailPortalSyncService sync) : ControllerBase
{
    public class SyncPushRequest
    {
        public string? ProcessInstanceId { get; set; }
    }

    /// <summary>POST api/WfPortal/SyncPush · 按实例补推（伪实现：仅控制台）</summary>
    [HttpPost]
    public async Task<ResponseResult<SailPortalSyncService.SyncResult>> SyncPush(
        [FromBody] SyncPushRequest? body,
        CancellationToken ct)
    {
        try
        {
            var pi = body?.ProcessInstanceId?.Trim() ?? "";
            var result = await sync.SyncByProcessInstanceAsync(pi, ct).ConfigureAwait(false);
            if (result.Errors.Count > 0 && result.Pushed == 0 && result.AlreadyPushed == 0
                && result.WaitingTaskCount == 0)
                return ResponseResult<SailPortalSyncService.SyncResult>.Error(
                    string.Join("; ", result.Errors));
            var ok = ResponseResult<SailPortalSyncService.SyncResult>.Success(result);
            ok.Message =
                $"stub: waiting={result.WaitingTaskCount}, pushed={result.Pushed}, already={result.AlreadyPushed}, failed={result.Failed}";
            return ok;
        }
        catch (Exception ex)
        {
            return ResponseResult<SailPortalSyncService.SyncResult>.Error(ex.Message);
        }
    }
}
