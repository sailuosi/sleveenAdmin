using sailflow.engine.Hooks;
using SleveenAdmin.Foundation.Diagnostics;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 宿主侧流程/待办控制台薄封装（调用 Foundation <see cref="DevConsole"/>）。
/// 字段布局对齐 pxxt8 <c>SailTpConsole</c>。
/// </summary>
public static class SailDevConsole
{
    public static void WriteHook(string series, FlowHookContext ctx)
    {
        if (ctx == null) return;
        try
        {
            DevConsole.WriteBlock(
                $"流程钩子 · {series}",
                $"事件：{ctx.EventType} ／ 操作：{ctx.Operation}",
                DevConsole.Field("应用", ctx.BizChannel),
                DevConsole.Field("业务主键", ctx.BizPK),
                DevConsole.Field("流程Id", ctx.ProcessId),
                DevConsole.Field("流程编码", ctx.ProcessCode),
                DevConsole.Field("版本", ctx.Version),
                DevConsole.Field("流程实例", ctx.ProcessInstanceId),
                DevConsole.Field("活动节点", $"{ctx.ActivityCode} ({ctx.ActivityId})"),
                DevConsole.Field("活动实例", ctx.ActivityInstanceId),
                DevConsole.Field("待办Id", ctx.TaskId),
                DevConsole.Field("办理人", $"{ctx.AssignedUserName} ({ctx.AssignedUserId})"),
                DevConsole.Field("同事务Kit", ctx.Kit != null ? "是" : "否"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[流程钩子控制台] {series} 打印失败：{ex.Message}");
        }
    }

    public static void WriteNotify(string series, TaskNotifyContext ctx, string? extraDescription = null)
    {
        if (ctx == null) return;
        try
        {
            var desc = string.IsNullOrWhiteSpace(extraDescription)
                ? $"通知类型：{KindLabel(ctx.Kind)}"
                : $"通知类型：{KindLabel(ctx.Kind)} ／ {extraDescription.Trim()}";
            DevConsole.WriteBlock(
                $"待办通知 · {series}",
                desc,
                DevConsole.Field("应用", ctx.BizChannel),
                DevConsole.Field("业务主键", ctx.BizPK),
                DevConsole.Field("流程Id", ctx.ProcessId),
                DevConsole.Field("流程编码", ctx.ProcessCode),
                DevConsole.Field("版本", ctx.Version),
                DevConsole.Field("流程实例", ctx.ProcessInstanceId),
                DevConsole.Field("活动节点", $"{ctx.ActivityCode} ({ctx.ActivityId})"),
                DevConsole.Field("活动实例", ctx.ActivityInstanceId),
                DevConsole.Field("待办Id", ctx.TaskId),
                DevConsole.Field("办理人", $"{ctx.AssignedUserName} ({ctx.AssignedUserId})"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[待办通知控制台] {series} 打印失败：{ex.Message}");
        }
    }

    static string KindLabel(TaskNotifyKind kind) => kind switch
    {
        TaskNotifyKind.Created => "创建",
        TaskNotifyKind.Completed => "完成",
        TaskNotifyKind.Cancelled => "取消",
        TaskNotifyKind.Entrusted => "委托",
        _ => kind.ToString()
    };
}
