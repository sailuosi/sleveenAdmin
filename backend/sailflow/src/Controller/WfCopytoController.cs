using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.biz.query;
using sailflow.engine.Client;
using sailflow.WebUtility;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>流程抄送（产品表 sf_proc_copyto）。抄送语义用 copyto，禁止裸 copy。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfCopytoController : ControllerBase
{
    private readonly SailCopytoService _copyto;

    public WfCopytoController(SailClient sail) => _copyto = new SailCopytoService(sail);

    public class CopytoCreateRequest
    {
        public List<CopytoTarget>? Targets { get; set; }
        public string? ProcessInstanceId { get; set; }
        public string? ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public string? BizPK { get; set; }
        public string? TaskId { get; set; }
        public string? TaskName { get; set; }
        public string? StartUserId { get; set; }
        public string? CreatorId { get; set; }
        public string? CreatorName { get; set; }
        public string? Reason { get; set; }
    }

    public class CopytoTarget
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class CopytoQuery
    {
        public string? UserId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>POST api/WfCopyto/Create</summary>
    [HttpPost]
    public ResponseResult Create([FromBody] CopytoCreateRequest req)
    {
        try
        {
            if (req?.Targets == null || req.Targets.Count == 0)
                return ResponseResult.Error("Targets required");

            SailAuthIdentity.BindCreator(User, out var creatorId, out var creatorName);
            var now = DateTime.Now;
            var rows = req.Targets
                .Where(t => !string.IsNullOrWhiteSpace(t.UserId))
                .Select(t => new SailProcessCopytoEntity
                {
                    ProcessCopytoId = Guid.NewGuid().ToString("N"),
                    UserId = t.UserId!,
                    UserName = t.UserName ?? t.UserId!,
                    StartUserId = req.StartUserId ?? "",
                    ProcessInstanceId = req.ProcessInstanceId ?? "",
                    ProcessId = req.ProcessId ?? "",
                    ProcessName = req.ProcessName ?? "",
                    BizPK = req.BizPK ?? "",
                    TaskId = req.TaskId ?? "",
                    TaskName = req.TaskName ?? "",
                    CreatorId = creatorId,
                    CreatorName = creatorName,
                    Reason = req.Reason ?? "",
                    CreateTime = now
                });
            var n = _copyto.CreateMany(rows);
            return ResponseResult.Success($"created {n}");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfCopyto/MyPage</summary>
    [HttpPost]
    public ResponseResult<List<SailProcessCopytoEntity>> MyPage([FromBody] CopytoQuery query)
    {
        try
        {
            query ??= new CopytoQuery();
            query.UserId = SailAuthIdentity.RequireAccount(User);
            var (items, total) = _copyto.QueryMyPage(query.UserId, query.PageIndex, query.PageSize);
            var result = ResponseResult<List<SailProcessCopytoEntity>>.Success(items.ToList());
            result.TotalRowsCount = total;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult<List<SailProcessCopytoEntity>>.Error(ex.Message);
        }
    }
}
