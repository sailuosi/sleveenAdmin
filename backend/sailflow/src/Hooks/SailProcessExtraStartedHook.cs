using mooSQL.data;
using sailflow.hosting;
using sailflow.utils;
using sailflow.data;
using sailflow.events;
using sailflow.hooks;
using sailflow.log;
using sailflow.service;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 启动成功后写 __sail_summary__ / __sail_process_no__ 流程变量（CreateHook 时尚无实例 Id）。
/// Sync：必须复用 <see cref="FlowHookContext.Kit"/>，禁止 WorkflowService 无 kit 重载。
/// </summary>
public sealed class SailProcessExtraStartedHook(SailClient sail) : FlowHookBase
{
    public override string Name => "sail.process.extra.started";
    public override string Title => "流程摘要与编号变量";

    public override void Handle(FlowHookContext ctx)
    {
        if (ctx.EventType != EventFireTypeEnum.OnProcessStarted)
            return;
        if (string.IsNullOrWhiteSpace(ctx.ProcessInstanceId) || string.IsNullOrWhiteSpace(ctx.ProcessId))
            return;

        try
        {
            var kit = ctx.Kit ?? sail.UseRunSQL();
            var db = kit.useSQL();
            var version = string.IsNullOrWhiteSpace(ctx.Version) ? "1" : ctx.Version;
            var extra = SailProcessExtraService.Parse(LoadExtraJson(db, ctx.ProcessId, version));
            var wf = new WorkflowService(sail);
            // 同事务读变量；发起当刻 PersistConditions 尚未跑，列表常为空，靠 BizCode / Extra 仍可写
            var formValues = SailProcessVariableHelper.LoadFormValues(wf, db, ctx.ProcessInstanceId);

            var pi = db.useRepo<ProcessInstanceEntity, string>().GetById(ctx.ProcessInstanceId);
            string? processNo = null;
            if (formValues.TryGetValue(SailProcessExtraTemplate.ProcessNoVar, out var fromVar)
                && !string.IsNullOrWhiteSpace(fromVar))
                processNo = fromVar;
            else if (!string.IsNullOrWhiteSpace(pi?.BizCode))
                processNo = pi!.BizCode;

            if (!string.IsNullOrWhiteSpace(processNo)
                && !formValues.ContainsKey(SailProcessExtraTemplate.ProcessNoVar))
            {
                UpsertVar(wf, db, ctx, SailProcessExtraTemplate.ProcessNoVar, processNo!);
            }

            var summary = SailProcessExtraTemplate.BuildSummary(extra.SummarySetting, formValues);
            if (!string.IsNullOrWhiteSpace(summary))
                UpsertVar(wf, db, ctx, SailProcessExtraTemplate.SummaryVar, summary);
        }
        catch (Exception ex)
        {
            /* 摘要/编码变量失败不阻断启动 */
            sail.Log?.Warning(
                SailLogCategories.SyncHook,
                "name={0} swallow PI={1}: {2}",
                Name, ctx.ProcessInstanceId, ex.Message);
        }
    }

    static string? LoadExtraJson(SQLBuilder kit, string processId, string version)
    {
        var clip = kit.useClip();
        clip.from<SailProcessMetaEntity>(out var m);
        clip.where(() => m.ProcessId, processId);
        clip.where(() => m.Version, version);
        return clip.select(m).queryList().FirstOrDefault()?.ExtraJson;
    }

    static void UpsertVar(
        IWorkflowService wf,
        SQLBuilder kit,
        FlowHookContext ctx,
        string name,
        string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        var exist = wf.GetProcessVariable(kit, new ProcessVariableQuery
        {
            VariableSceope = ProcessVariableScopeEnum.Process,
            ProcessInstanceId = ctx.ProcessInstanceId,
            Name = name
        });
        if (exist != null && !string.IsNullOrWhiteSpace(exist.ProcessVariableId))
        {
            exist.Value = value;
            exist.UpdatedDateTime = DateTime.UtcNow;
            wf.SaveProcessVariable(kit, exist);
            return;
        }

        wf.SaveProcessVariable(kit, new ProcessVariableEntity
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
