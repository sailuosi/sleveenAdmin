using System.Text.Json;
using sailflow.biz.Approval;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.FormBiz.Handlers;

/// <summary>§H 请假样例：一业务一类；Finish/Aborted 回写 Status。</summary>
public sealed class LeaveFormBizHandler(SailLeaveService leave) : SailFormBizHandlerBase
{
    static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public override string FormCode => SailLeaveStatus.BizChannel;

    public override void OnInit(SailApprovalBusinessContext ctx)
    {
        ctx.Data = new
        {
            days = 1,
            reason = "",
            startDate = (DateTime?)null,
            endDate = (DateTime?)null
        };
    }

    public override void OnQuery(SailApprovalBusinessContext ctx)
    {
        leave.EnsureTable();
        if (string.IsNullOrWhiteSpace(ctx.BizPK))
        {
            ctx.Error = "BizPK required";
            return;
        }
        ctx.Data = leave.Get(ctx.BizPK);
        if (ctx.Data == null)
            ctx.Error = "not found";
    }

    public override void OnSubmit(SailApprovalBusinessContext ctx)
    {
        leave.EnsureTable();
        var req = ParsePayload(ctx.PayloadJson);
        if (req.Days <= 0)
        {
            ctx.Error = "Days must be > 0";
            return;
        }

        var row = new SailLeaveEntity
        {
            UserId = ctx.UserId,
            UserName = ctx.UserName,
            Days = req.Days,
            Reason = req.Reason ?? "",
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Status = SailLeaveStatus.Running,
            ProcessId = ctx.ProcessId,
            ProcessVersion = string.IsNullOrWhiteSpace(ctx.Version) ? "1" : ctx.Version
        };
        row = leave.Insert(row);

        ctx.BizChannel = FormCode;
        ctx.BizPK = row.Id;
        ctx.Conditions["days"] = row.Days.ToString("0.##");
        ctx.Data = row;
    }

    public override void OnStarted(SailApprovalBusinessContext ctx)
    {
        if (ctx.Data is not SailLeaveEntity row) return;
        if (string.IsNullOrWhiteSpace(ctx.ProcessInstanceId)) return;
        row.ProcessInstanceId = ctx.ProcessInstanceId;
        leave.Update(row);
        ctx.Data = row;
    }

    public override void OnFinish(SailApprovalBusinessContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.BizPK)) return;
        var status = ResolveFinishStatus(ctx);
        if (status <= 0) return;
        leave.ApplyStatusByBizPK(ctx.BizPK, status, ctx.Kit);
    }

    public override void OnAborted(SailApprovalBusinessContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.BizPK)) return;
        leave.ApplyStatusByBizPK(ctx.BizPK, SailLeaveStatus.Closed, ctx.Kit);
    }

    static int ResolveFinishStatus(SailApprovalBusinessContext ctx)
    {
        if (string.Equals(ctx.Outcome, ApprovalOutcomeCodes.Approved, StringComparison.OrdinalIgnoreCase))
            return SailLeaveStatus.Approved;
        if (string.Equals(ctx.Outcome, ApprovalOutcomeCodes.Rejected, StringComparison.OrdinalIgnoreCase))
            return SailLeaveStatus.Rejected;
        return 0;
    }

    static LeaveSubmitPayload ParsePayload(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new LeaveSubmitPayload();
        try
        {
            return JsonSerializer.Deserialize<LeaveSubmitPayload>(json, JsonOpts) ?? new LeaveSubmitPayload();
        }
        catch
        {
            return new LeaveSubmitPayload();
        }
    }

    sealed class LeaveSubmitPayload
    {
        public decimal Days { get; set; }
        public string? Reason { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
