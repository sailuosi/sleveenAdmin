using sailflow.biz.Approval;

namespace SleveenAdmin.Sailflow.FormBiz;

/// <summary>
/// 宿主兼容别名：一业务一类 Handler，契约真源为 <see cref="ISailApprovalBusiness"/>。
/// </summary>
public interface ISailFormBizHandler : ISailApprovalBusiness
{
}

/// <summary>兼容别名：审批业务上下文。</summary>
public sealed class SailFormBizContext : SailApprovalBusinessContext
{
}

/// <summary>可选空实现；委托 <see cref="SailApprovalBusinessBase"/>。</summary>
public abstract class SailFormBizHandlerBase : SailApprovalBusinessBase, ISailFormBizHandler
{
}
