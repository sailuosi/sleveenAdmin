using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.engine.Result;
using sailflow.WebUtility;
using SleveenAdmin.Saiflow.Entity;
using SleveenAdmin.Saiflow.FormBiz;
using SleveenAdmin.Saiflow.Service;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>请假样例：委托 §H LeaveFormBizHandler（FormCode=SailLeave）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfLeaveController : ControllerBase
{
    private readonly SailLeaveService _leave;
    private readonly SailFormBizHost _bizHost;

    public WfLeaveController(SailLeaveService leave, SailFormBizHost bizHost)
    {
        _leave = leave;
        _bizHost = bizHost;
    }

    public class LeaveCreateRequest
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public decimal Days { get; set; }
        public string? Reason { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProcessId { get; set; }
        public string? Version { get; set; }
    }

    public class LeaveQuery
    {
        public string? UserId { get; set; }
        public int Limit { get; set; } = 100;
    }

    /// <summary>POST api/WfLeave/EnsureTable</summary>
    [HttpPost]
    public ResponseResult EnsureTable()
    {
        try
        {
            _leave.EnsureTable();
            return ResponseResult.Success("ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfLeave/Create · §H OnSubmit + StartProcess</summary>
    [HttpPost]
    public async Task<ResponseResult<SailLeaveEntity>> Create([FromBody] LeaveCreateRequest? req)
    {
        try
        {
            if (req == null || string.IsNullOrWhiteSpace(req.ProcessId))
                return ResponseResult<SailLeaveEntity>.Error("ProcessId required");
            if (req.Days <= 0)
                return ResponseResult<SailLeaveEntity>.Error("Days must be > 0");

            SailAuthIdentity.BindCreator(User, out var account, out var displayName);
            var (ctx, exec) = await _bizHost.SubmitAndStartAsync(new SubmitBizFormRequest
            {
                FormCode = SailLeaveStatus.AppName,
                ProcessId = req.ProcessId,
                Version = req.Version,
                UserId = account,
                UserName = displayName,
                PayloadJson = JsonSerializer.Serialize(new
                {
                    days = req.Days,
                    reason = req.Reason,
                    startDate = req.StartDate,
                    endDate = req.EndDate
                })
            });

            if (!string.IsNullOrWhiteSpace(ctx.Error))
                return ResponseResult<SailLeaveEntity>.Error(ctx.Error);

            if (exec == null || exec.Status != WfExecutedStatus.Success)
                return ResponseResult<SailLeaveEntity>.Error(exec?.Message ?? "StartProcess failed");

            var row = ctx.Data as SailLeaveEntity ?? _leave.Get(ctx.AppInstanceId);
            if (row == null)
                return ResponseResult<SailLeaveEntity>.Error("leave row missing after submit");

            return ResponseResult<SailLeaveEntity>.Success(row, "created");
        }
        catch (Exception ex)
        {
            return ResponseResult<SailLeaveEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfLeave/Query</summary>
    [HttpPost]
    public ResponseResult<List<SailLeaveEntity>> Query([FromBody] LeaveQuery? query)
    {
        try
        {
            _leave.EnsureTable();
            query ??= new LeaveQuery();
            query.UserId = SailAuthIdentity.RequireAccount(User);
            var list = _leave.ListByUser(query.UserId, query.Limit <= 0 ? 100 : query.Limit);
            return ResponseResult<List<SailLeaveEntity>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<SailLeaveEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/WfLeave/Get/{id}</summary>
    [HttpGet]
    public ResponseResult<SailLeaveEntity> Get(string id)
    {
        try
        {
            _leave.EnsureTable();
            var row = _leave.Get(id);
            if (row == null) return ResponseResult<SailLeaveEntity>.Error("not found");
            return ResponseResult<SailLeaveEntity>.Success(row);
        }
        catch (Exception ex)
        {
            return ResponseResult<SailLeaveEntity>.Error(ex.Message);
        }
    }
}
