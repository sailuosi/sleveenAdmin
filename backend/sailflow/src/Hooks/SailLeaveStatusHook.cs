using sailflow.biz.Approval;
using sailflow.events;
using sailflow.hooks;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.FormBiz.Handlers;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 请假样例 Status Hook（遗留）。默认宿主已改挂 <c>UseApprovalBusiness</c>；
/// 本类委托 <see cref="LeaveFormBizHandler"/> 的 Finish/Aborted，避免双写语义分叉。
/// </summary>
public sealed class SailLeaveStatusHook : FlowHookBase
{
    private readonly LeaveFormBizHandler _handler;

    public SailLeaveStatusHook(SailLeaveService leave) =>
        _handler = new LeaveFormBizHandler(
            leave ?? throw new ArgumentNullException(nameof(leave)));

    public override string Name => "sail.demo.leave.status";
    public override string Title => "请假状态回写（遗留）";

    public override void Handle(FlowHookContext ctx)
    {
        if (!string.Equals(ctx.BizChannel, SailLeaveStatus.BizChannel, StringComparison.OrdinalIgnoreCase))
            return;
        if (string.IsNullOrWhiteSpace(ctx.BizPK))
            return;

        var decision = SailApprovalLifecycleMapper.Map(ctx.EventType);
        if (decision.Kind == SailApprovalLifecycleKind.None)
            return;

        var bizCtx = new SailApprovalBusinessContext
        {
            FormCode = SailLeaveStatus.BizChannel,
            BizChannel = ctx.BizChannel,
            BizPK = ctx.BizPK,
            FinishEvent = ctx.EventType,
            Outcome = decision.Outcome,
            AbortKind = decision.AbortKind,
            Kit = ctx.Kit
        };

        if (decision.Kind == SailApprovalLifecycleKind.Finish)
            _handler.OnFinish(bizCtx);
        else
            _handler.OnAborted(bizCtx);
    }
}
