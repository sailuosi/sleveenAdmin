using sailflow.engine.Result;

namespace SleveenAdmin.Sailflow.Operations;

/// <summary>Host 办理动词 Handler：一个按钮 Id（或一组）对应唯一实现。</summary>
public interface ISailOperationHandler
{
    IReadOnlyList<string> SupportedButtonIds { get; }
    Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default);
}
