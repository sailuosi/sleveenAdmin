using mooSQL.data;
using sailflow.engine.Client;
using sailflow.engine.Entity;
using sailflow.engine.Event;
using sailflow.engine.Hooks;
using sailflow.engine.Logging;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// Sync Hook：办结/旁关后按 EndType 写 Host 本地化 EndText（引擎只写 EndType）。
/// 同事务用 ctx.Kit，勿另开连接。
/// </summary>
public sealed class SailEndTextLocalizeHook(SailClient sail) : FlowHookBase
{
    public override string Name => "sail.host.endtext.localize";
    public override string Title => "办结文案本地化";

    public override void Handle(FlowHookContext ctx)
    {
        if (ctx == null || string.IsNullOrWhiteSpace(ctx.TaskId))
            return;
        if (ctx.EventType is not (EventFireTypeEnum.OnTaskCompleted or EventFireTypeEnum.OnTaskCreated))
            return;
        // Created 不写；仅 Completed 族（含 Cancelled/Entrusted 映射）
        if (ctx.EventType == EventFireTypeEnum.OnTaskCreated)
            return;

        var kit = ctx.Kit;
        if (kit == null)
            return;

        try
        {
            var clip = kit.useClip();
            clip.from<TaskEntity>(out var t)
                .where(() => t.TaskId, ctx.TaskId);
            var task = clip.select(t).queryList()?.FirstOrDefault();
            if (task == null)
                return;
            if (string.IsNullOrWhiteSpace(task.EndType))
                return;
            if (!string.IsNullOrWhiteSpace(task.EndText))
                return;

            var actor = !string.IsNullOrWhiteSpace(task.EndedUserName)
                ? task.EndedUserName
                : (task.EndedUserId ?? ctx.SignName ?? "");
            var text = SailEndTextLocalizer.Format(task.EndType, actor);
            if (string.IsNullOrWhiteSpace(text))
                return;

            task.EndText = text;
            kit.useRepo<TaskEntity>().Update(task);
        }
        catch (Exception ex)
        {
            sail.Log?.Warning(
                SailLogCategories.SyncHook,
                "name={0} TaskId={1}: {2}",
                Name, ctx.TaskId, ex.Message);
        }
    }
}

/// <summary>EndType → 简体中文短句（Host 文案；可后续换资源文件）。</summary>
public static class SailEndTextLocalizer
{
    public static string? Format(string? endType, string? actorName)
    {
        if (string.IsNullOrWhiteSpace(endType))
            return null;
        var who = string.IsNullOrWhiteSpace(actorName) ? "他人" : actorName.Trim();
        return endType.Trim() switch
        {
            "SelfComplete" => "自行办结",
            "ProxyTaken" => $"{who}代办结",
            "RaceTaken" => $"{who}抢办结",
            "DelegatedOut" => "已委派（待还回）",
            "TransferredOut" => "已转办（不还回）",
            "Removed" => "已减签/改派",
            "ConsultRecalled" => "沟通已收回",
            "ActivityCancelled" => "活动取消",
            "Withdrawn" => "已撤回",
            "SendBacked" => "已退回",
            _ => null
        };
    }
}
