using System.Text.Json;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Saiflow.Hooks;

/// <summary>
/// 开发核验用：流程 Hook 回调时把 <see cref="FlowHookContext"/> 打到控制台。
/// 仅 Sync；勿用于生产噪声场景。
/// </summary>
public sealed class SailDebugConsoleHook : IFlowHook
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Name => "sail.debug.console.hook";

    public void Handle(FlowHookContext ctx)
    {
        try
        {
            Console.WriteLine($"[SailDebug.Hook] {JsonSerializer.Serialize(ctx, JsonOpts)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SailDebug.Hook] Event={ctx?.EventType} serialize-failed: {ex.Message}");
        }
    }
}
