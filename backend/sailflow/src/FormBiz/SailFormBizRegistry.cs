using sailflow.biz.Approval;
using sailflow.hooks;

namespace SleveenAdmin.Sailflow.FormBiz;

/// <summary>§H：按 FormCode 路由；同码重复登记失败。</summary>
public sealed class SailFormBizRegistry
{
    readonly SailApprovalBusinessRegistry _inner;

    public SailFormBizRegistry(IEnumerable<ISailFormBizHandler> handlers)
    {
        _inner = new SailApprovalBusinessRegistry(handlers);
    }

    public ISailFormBizHandler? TryGet(string? formCode)
    {
        var h = _inner.TryGet(formCode);
        if (h == null) return null;
        return h as ISailFormBizHandler ?? new Wrapper(h);
    }

    public ISailFormBizHandler GetRequired(string formCode)
    {
        var h = TryGet(formCode);
        if (h == null)
            throw new InvalidOperationException(
                $"No form biz handler registered for FormCode '{formCode}'.");
        return h;
    }

    public IReadOnlyCollection<string> RegisteredCodes => _inner.RegisteredCodes;

    sealed class Wrapper(ISailApprovalBusiness inner) : ISailFormBizHandler
    {
        public string FormCode => inner.FormCode;
        public void OnInit(SailApprovalBusinessContext ctx) => inner.OnInit(ctx);
        public void OnQuery(SailApprovalBusinessContext ctx) => inner.OnQuery(ctx);
        public void OnSubmit(SailApprovalBusinessContext ctx) => inner.OnSubmit(ctx);
        public void OnStarted(SailApprovalBusinessContext ctx) => inner.OnStarted(ctx);
        public void OnConsent(SailApprovalBusinessContext ctx) => inner.OnConsent(ctx);
        public void OnBackToModify(SailApprovalBusinessContext ctx) => inner.OnBackToModify(ctx);
        public void OnCancel(SailApprovalBusinessContext ctx) => inner.OnCancel(ctx);
        public void OnFinish(SailApprovalBusinessContext ctx) => inner.OnFinish(ctx);
        public void OnAborted(SailApprovalBusinessContext ctx) => inner.OnAborted(ctx);
        public HookResult? OnCompleting(FlowOperation operation, SailApprovalBusinessContext ctx)
            => inner.OnCompleting(operation, ctx);
    }
}
