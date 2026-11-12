using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.engine.Entity;
using sailflow.engine.Manager;
using sailflow.engine.Client;
using sailflow.WebUtility;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>Hook/Notify Outbox 治理（死信查询与重试）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfOutboxController : ControllerBase
{
    private readonly SailClient _sail;

    public WfOutboxController(SailClient sail) => _sail = sail;

    public class OutboxQuery
    {
        public string? Status { get; set; }
        public string? ProcessCode { get; set; }
        public string? HandlerName { get; set; }
        public int Limit { get; set; } = 100;
    }

    public class ResetIdsRequest
    {
        public List<string>? Ids { get; set; }
    }

    /// <summary>POST api/WfOutbox/Query · 默认查 Dead</summary>
    [HttpPost]
    public ResponseResult<List<HookOutboxEntity>> Query([FromBody] OutboxQuery? query)
    {
        try
        {
            query ??= new OutboxQuery();
            var limit = query.Limit > 0 ? query.Limit : 100;
            var mgr = new HookOutboxManager(_sail);
            var kit = _sail.UseRunSQL();
            var status = string.IsNullOrWhiteSpace(query.Status) ? HookOutboxStatus.Dead : query.Status;
            List<HookOutboxEntity> list;
            if (string.Equals(status, HookOutboxStatus.Dead, StringComparison.OrdinalIgnoreCase))
            {
                list = mgr.QueryDead(kit, limit, query.ProcessCode ?? "", query.HandlerName ?? "").ToList();
            }
            else
            {
                list = mgr.QueryByStatus(kit, status, limit).ToList();
            }

            var result = ResponseResult<List<HookOutboxEntity>>.Success(list);
            result.TotalRowsCount = list.Count;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult<List<HookOutboxEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/WfOutbox/CountByStatus?status=Dead</summary>
    [HttpGet]
    public ResponseResult<object> CountByStatus([FromQuery] string status = HookOutboxStatus.Dead)
    {
        try
        {
            var mgr = new HookOutboxManager(_sail);
            var kit = _sail.UseRunSQL();
            var n = mgr.CountByStatus(kit, status);
            return ResponseResult<object>.Success(new { status, count = n });
        }
        catch (Exception ex)
        {
            return ResponseResult<object>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfOutbox/ResetDead/{id}</summary>
    [HttpPost]
    public ResponseResult ResetDead(string id)
    {
        try
        {
            var mgr = new HookOutboxManager(_sail);
            var kit = _sail.UseRunSQL();
            mgr.ResetDead(kit, id);
            return ResponseResult.Success("ResetDead ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfOutbox/ResetDeadBatch · { Ids: string[] }</summary>
    [HttpPost]
    public ResponseResult ResetDeadBatch([FromBody] ResetIdsRequest? body)
    {
        try
        {
            var ids = body?.Ids ?? new List<string>();
            var mgr = new HookOutboxManager(_sail);
            var kit = _sail.UseRunSQL();
            var n = mgr.ResetDeadBatch(kit, ids);
            return ResponseResult.Success($"reset {n}");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfOutbox/ResetAllDead?limit=500</summary>
    [HttpPost]
    public ResponseResult ResetAllDead([FromQuery] int limit = 500)
    {
        try
        {
            var mgr = new HookOutboxManager(_sail);
            var kit = _sail.UseRunSQL();
            var n = mgr.ResetAllDead(kit, limit <= 0 ? 500 : limit);
            return ResponseResult.Success($"reset {n}");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }
}
