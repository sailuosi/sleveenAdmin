using sailflow.biz.Approval;
using sailflow.hooks;

namespace SleveenAdmin.Sailflow.FormBiz;

/// <summary>
/// 遗留类型：委托 <see cref="SailApprovalBusinessLifecycleHook"/>。
/// 新宿主请用 <c>UseApprovalBusiness</c> / <c>UseFormBizLifecycleHook</c>。
/// </summary>
[Obsolete("Use sailflow.biz.Approval.SailApprovalBusinessLifecycleHook via UseApprovalBusiness.")]
public sealed class SailFormBizLifecycleHook : FlowHookBase
{
    readonly SailApprovalBusinessLifecycleHook _inner;

    public SailFormBizLifecycleHook(IEnumerable<ISailFormBizHandler> handlers)
    {
        _inner = new SailApprovalBusinessLifecycleHook(handlers);
    }

    public override string Name => _inner.Name;
    public override string Title => _inner.Title;

    public override void Handle(FlowHookContext ctx) => _inner.Handle(ctx);
}
