using mooSQL.data;
using sailflow.engine.Client;
using sailflow.engine.Hooks;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 实例落库前：写 Title；按 processIdRule 生成编码写入 BizCode（已有 BizCode 则不覆盖，编码仍入 bag）。
/// </summary>
public sealed class SailProcessExtraCreateHook(SailClient sail) : ProcessInstanceCreateHookBase
{
    public override string Name => "sail.process.extra.create";
    public override string Title => "实例标题与流程编码";

    public override void OnCreating(ProcessInstanceCreateContext ctx)
    {
        if (ctx?.Instance == null || ctx.Process == null)
            return;

        var extra = LoadExtra(ctx.Kit, ctx.Process.ProcessId, ctx.Process.Version);
        var formValues = ToBagMap(ctx.Runner?.Conditions);

        // Title
        if (extra.TitleSetting.Enable
            && !string.IsNullOrWhiteSpace(extra.TitleSetting.Title))
        {
            var bag = SailProcessExtraTemplate.BuildBagBag(
                ctx.Instance.ProcessName,
                null,
                ctx.Instance.CreatedUserName,
                null,
                ctx.Process.ProcessId,
                ctx.Instance.BizCode,
                null,
                formValues);
            var title = SailProcessExtraTemplate.Replace(extra.TitleSetting.Title, bag).Trim();
            if (!string.IsNullOrWhiteSpace(title))
                ctx.Instance.Title = title;
        }

        if (string.IsNullOrWhiteSpace(ctx.Instance.Title))
            ctx.Instance.Title = ctx.Instance.ProcessName;

        ctx.Runner ??= new sailflow.engine.Common.FlowCommandContext();
        ctx.Runner.Conditions ??= new Dictionary<string, string>();

        // 摘要：写入 Conditions，由 Host PersistConditions 落库（此时尚无实例 Id）
        var summary = SailProcessExtraTemplate.BuildSummary(extra.SummarySetting, formValues);
        if (!string.IsNullOrWhiteSpace(summary))
            ctx.Runner.Conditions[SailProcessExtraTemplate.SummaryVar] = summary;

        // Process code
        if (!extra.ProcessIdRule.Enable)
            return;

        var kit = ctx.Kit ?? sail.UseRunSQL();
        var processNo = NextProcessNo(kit, ctx.Process.ProcessId, extra.ProcessIdRule);
        if (string.IsNullOrWhiteSpace(processNo))
            return;

        ctx.Runner.Conditions[SailProcessExtraTemplate.ProcessNoVar] = processNo;

        if (string.IsNullOrWhiteSpace(ctx.Instance.BizCode))
            ctx.Instance.BizCode = processNo;
    }

    SailProcessExtraDto LoadExtra(SQLBuilder? kit, string? processId, string? version)
        => SailProcessExtraService.Parse(LoadExtraJson(kit, processId, version));

    string? LoadExtraJson(SQLBuilder? kit, string? processId, string? version)
    {
        if (string.IsNullOrWhiteSpace(processId))
            return null;
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version;
        // 同事务优先；无 kit 时（极少）再独立连接
        var owned = kit == null;
        var db = kit?.useSQL() ?? sail.UseRunSQL();
        try
        {
            var clip = db.useClip();
            clip.from<SailProcessMetaEntity>(out var m);
            clip.where(() => m.ProcessId, processId);
            clip.where(() => m.Version, ver);
            return clip.select(m).queryList().FirstOrDefault()?.ExtraJson;
        }
        finally
        {
            if (owned)
                db.Dispose();
        }
    }

    static Dictionary<string, string> ToBagMap(IDictionary<string, string>? conditions)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (conditions == null) return map;
        foreach (var kv in conditions)
        {
            if (string.IsNullOrWhiteSpace(kv.Key)) continue;
            map[kv.Key] = kv.Value ?? "";
        }
        return map;
    }

    static string NextProcessNo(SQLBuilder kit, string processId, SailProcessIdRuleDto rule)
    {
        var bucket = SailProcessExtraTemplate.ResolveInfixBucket(rule.Infix);
        var clip = kit.useClip();
        clip.from<SailProcessIdSeqEntity>(out var s);
        clip.where(() => s.ProcessId, processId);
        clip.where(() => s.Bucket, bucket);
        var row = clip.select(s).queryList().FirstOrDefault();
        var now = DateTime.UtcNow;
        int seq;
        if (row == null)
        {
            seq = 1;
            kit.useRepo<SailProcessIdSeqEntity>().Insert(new SailProcessIdSeqEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                ProcessId = processId,
                Bucket = bucket,
                Seq = seq,
                UpdatedAt = now
            });
        }
        else
        {
            seq = row.Seq + 1;
            row.Seq = seq;
            row.UpdatedAt = now;
            kit.useRepo<SailProcessIdSeqEntity>().Update(row);
        }

        return SailProcessExtraTemplate.FormatProcessNo(rule, bucket, seq);
    }
}
