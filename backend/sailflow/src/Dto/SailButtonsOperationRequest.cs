using sailflow.engine.Common;

namespace SleveenAdmin.Saiflow.Dto;

/// <summary>统一办理入口请求：码表 ButtonId + Runner。</summary>
public sealed class SailButtonsOperationRequest
{
    public string ButtonId { get; set; } = "";
    public FlowCommandContext? Runner { get; set; }
}
