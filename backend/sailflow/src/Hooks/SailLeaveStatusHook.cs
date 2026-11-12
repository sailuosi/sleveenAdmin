using sailflow.engine.Event;
using sailflow.engine.Hooks;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 请假样例 Status Hook（遗留）。默认宿主已改挂 <c>UseFormBizLifecycleHook</c>；
/// 本类委托 <see cref="FormBiz.Handlers.LeaveFormBizHandler.OnFinish"/>，避免双写语义分叉。
/// </summary>
public sealed class SailLeaveStatusHook : IFlowHook
{
    private readonly FormBiz.Handlers.LeaveFormBizHandler _handler;

    public SailLeaveStatusHook(SailLeaveService leave) =>
        _handler = new FormBiz.Handlers.LeaveFormBizHandler(
            leave ?? throw new ArgumentNullException(nameof(leave)));

    public string Name => "sail.demo.leave.status";

    public void Handle(FlowHookContext ctx)
    {
        if (!string.Equals(ctx.AppName, SailLeaveStatus.AppName, StringComparison.OrdinalIgnoreCase))
            return;
        if (string.IsNullOrWhiteSpace(ctx.AppInstanceId))
            return;

        _handler.OnFinish(new FormBiz.SailFormBizContext
        {
            FormCode = SailLeaveStatus.AppName,
            AppName = ctx.AppName,
            AppInstanceId = ctx.AppInstanceId,
            FinishEvent = ctx.EventType
        });
    }
}
