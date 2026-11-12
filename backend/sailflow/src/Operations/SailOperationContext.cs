using sailflow.engine.Common;

namespace SleveenAdmin.Saiflow.Operations;

/// <summary>统一办理入口上下文（按钮码 → Handler）。</summary>
public sealed class SailOperationContext
{
    public required string ButtonId { get; init; }
    public required FlowCommandContext Runner { get; init; }
}
