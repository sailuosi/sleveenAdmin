using mooSQL.data;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Entity;
using sailflow.engine.Event;
using sailflow.engine.Hooks;
using sailflow.engine.Service;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 启动成功后写 __sail_summary__ / __sail_process_no__ 流程变量（CreateHook 时尚无实例 Id）。
/// </summary>
public sealed class SailProcessExtraStartedHook(SailClient sail) : IFlowHook
{
    public string Name => "sail.process.extra.started";

    public void Handle(FlowHookContext ctx)
    {
        if (ctx.EventType != EventFireTypeEnum.OnProcessStarted)
            return;
        if (string.IsNullOrWhiteSpace(ctx.ProcessInstanceId) || string.IsNullOrWhiteSpace(ctx.ProcessId))
            return;

        try
        {
            var kit = ctx.Kit ?? sail.UseRunSQL();
            var version = string.IsNullOrWhiteSpace(ctx.Version) ? "1" : ctx.Version;
            var extra = SailProcessExtraService.Parse(LoadExtraJson(kit, ctx.ProcessId, version));
            var wf = new WorkflowService(sail);
            var formValues = SailProcessVariableHelper.LoadFormValues(wf, ctx.ProcessInstanceId);

            // process no from conditions already persisted by Host PersistConditions after Start;
            // also accept value already on BizCode
            var pi = kit.useRepo<ProcessInstanceEntity, string>().GetById(ctx.ProcessInstanceId);
            string? processNo = null;
            if (formValues.TryGetValue(SailProcessExtraTemplate.ProcessNoVar, out var fromVar)
                && !string.IsNullOrWhiteSpace(fromVar))
                processNo = fromVar;
            else if (!string.IsNullOrWhiteSpace(pi?.BizCode))
                processNo = pi!.BizCode;

            if (!string.IsNullOrWhiteSpace(processNo)
                && !formValues.ContainsKey(SailProcessExtraTemplate.ProcessNoVar))
            {
                UpsertVar(wf, ctx, SailProcessExtraTemplate.ProcessNoVar, processNo!);
            }

            var summary = SailProcessExtraTemplate.BuildSummary(extra.SummarySetting, formValues);
            if (!string.IsNullOrWhiteSpace(summary))
                UpsertVar(wf, ctx, SailProcessExtraTemplate.SummaryVar, summary);
        }
        catch
        {
            /* 摘要/编码变量失败不阻断启动 */
        }
    }

    static string? LoadExtraJson(mooSQL.data.SQLBuilder kit, string processId, string version)
    {
        var clip = kit.useClip();
        clip.from<SailProcessMetaEntity>(out var m);
        clip.where(() => m.ProcessId, processId);
        clip.where(() => m.Version, version);
        return clip.select(m).queryList().FirstOrDefault()?.ExtraJson;
    }

    static void UpsertVar(IWorkflowService wf, FlowHookContext ctx, string name, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        var exist = wf.GetProcessVariable(new ProcessVariableQuery
        {
            VariableSceope = ProcessVariableScopeEnum.Process,
            ProcessInstanceId = ctx.ProcessInstanceId,
            Name = name
        });
        if (exist != null && !string.IsNullOrWhiteSpace(exist.ProcessVariableId))
        {
            exist.Value = value;
            exist.UpdatedDateTime = DateTime.UtcNow;
            wf.SaveProcessVariable(exist);
            return;
        }

        wf.SaveProcessVariable(new ProcessVariableEntity
        {
            VariableScope = ProcessVariableScopeEnum.Process.ToString(),
            BizPK = ctx.BizPK,
            ProcessId = ctx.ProcessId,
            ProcessInstanceId = ctx.ProcessInstanceId,
            Name = name,
            Value = value,
            UpdatedDateTime = DateTime.UtcNow
        });
    }
}
