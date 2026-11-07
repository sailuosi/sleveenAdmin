using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using saiflow.biz.query;
using saiflow.engine.Client;
using saiflow.WebUtility;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>流程抄送（产品表 sf_process_instance_copy）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfCopyController : ControllerBase
{
    private readonly SailCopyService _copy;

    public WfCopyController(SailClient sail) => _copy = new SailCopyService(sail);

    public class CopyCreateRequest
    {
        public List<CopyTarget>? Targets { get; set; }
        public string? ProcessInstanceId { get; set; }
        public string? ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public string? AppInstanceId { get; set; }
        public string? TaskId { get; set; }
        public string? TaskName { get; set; }
        public string? StartUserId { get; set; }
        public string? CreatorId { get; set; }
        public string? CreatorName { get; set; }
        public string? Reason { get; set; }
    }

    public class CopyTarget
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class CopyQuery
    {
        public string? UserId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>POST api/WfCopy/Create</summary>
    [HttpPost]
    public ResponseResult Create([FromBody] CopyCreateRequest req)
    {
        try
        {
            if (req?.Targets == null || req.Targets.Count == 0)
                return ResponseResult.Error("Targets required");

            SailAuthIdentity.BindCreator(User, out var creatorId, out var creatorName);
            var now = DateTime.Now;
            var rows = req.Targets
                .Where(t => !string.IsNullOrWhiteSpace(t.UserId))
                .Select(t => new SailProcessCopyEntity
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserId = t.UserId!,
                    UserName = t.UserName ?? t.UserId!,
                    StartUserId = req.StartUserId ?? "",
                    ProcessInstanceId = req.ProcessInstanceId ?? "",
                    ProcessId = req.ProcessId ?? "",
                    ProcessName = req.ProcessName ?? "",
                    AppInstanceId = req.AppInstanceId ?? "",
                    TaskId = req.TaskId ?? "",
                    TaskName = req.TaskName ?? "",
                    CreatorId = creatorId,
                    CreatorName = creatorName,
                    Reason = req.Reason ?? "",
                    CreateTime = now
                });
            var n = _copy.CreateMany(rows);
            return ResponseResult.Success($"created {n}");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfCopy/MyPage</summary>
    [HttpPost]
    public ResponseResult<List<SailProcessCopyEntity>> MyPage([FromBody] CopyQuery query)
    {
        try
        {
            query ??= new CopyQuery();
            query.UserId = SailAuthIdentity.RequireAccount(User);
            var (items, total) = _copy.QueryMyPage(query.UserId, query.PageIndex, query.PageSize);
            var result = ResponseResult<List<SailProcessCopyEntity>>.Success(items.ToList());
            result.TotalRowsCount = total;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult<List<SailProcessCopyEntity>>.Error(ex.Message);
        }
    }
}
