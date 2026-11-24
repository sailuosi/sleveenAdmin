using mooSQL.data;
using mooSQL.data.model;
using sailflow.AI.Entity;
using sailflow.AI.Runtime;
using sailflow.biz.query;
using sailflow.hosting;
using sailflow.data;
using sailflow.hub.Form;
using sailflow.hub.Logging;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Integration.UnifiedReport;

namespace SleveenAdmin.Sailflow.Hosting;

/// <summary>
/// 宿主侧 Schema 唯一入口：核心流程表 + AI + 产品表 + 索引 + 可选种子。
/// 建表不在 sailflow 包内；本类属于业务系统（sleveen-admin）。
/// 任务明细读模型为 <c>TaskViewEntity</c>（Select+SooJoin），无需物理视图。
/// </summary>
public static class SailHostSchemaInitializer
{
    public sealed class InitDbResult
    {
        public string Progress { get; set; } = string.Empty;
        public int SeededRows { get; set; }
    }

    public static InitDbResult InitDb(SailClient client, bool seed = true, bool includeAi = true, bool registerAiExecutor = true)
    {
        ArgumentNullException.ThrowIfNull(client);

        var kit = client.UseRunSQL();
        var progress = EnsureAllTables(kit, includeAi);
        var seeded = seed ? SailProductSeed.EnsureAll(kit) : 0;

        if (includeAi && registerAiExecutor)
            AiRuntime.UseDefaultExecutor(client);

        return new InitDbResult { Progress = progress, SeededRows = seeded };
    }

    public static string EnsureAllTables(SailClient client, bool includeAi = true)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnsureAllTables(client.UseRunSQL(), includeAi);
    }

    public static string EnsureAllTables(SQLBuilder kit, bool includeAi = true)
    {
        ArgumentNullException.ThrowIfNull(kit);

        var progress = EnsureCoreTables(kit);
        if (includeAi)
            progress += EnsureAiTables(kit);
        progress += EnsureProductTables(kit);
        return progress;
    }

    public static string EnsureHostTables(SailClient client) => EnsureAllTables(client, includeAi: true);

    public static string EnsureHostTables(SQLBuilder kit) => EnsureAllTables(kit, includeAi: true);

    static string EnsureCoreTables(SQLBuilder kit)
    {
        var wk = kit.useDBInit()
            .upsertMode()
            .columnTypeByTag("sail.id", DataFam.Guid)
            .columnTypeByTag("sail.xml", DataFam.Json)
            .createTable<ProcessEntity>()
            .createTable<ProcessInstanceEntity>()
            .createTable<ActivityInstanceEntity>()
            .createTable<TaskEntity>()
            .createTable<TrackEntity>()
            .createTable<VariableEntity>()
            .createTable<ProcessVariableEntity>()
            .createTable<LogEntity>()
            .createTable<JobInfoEntity>()
            .createTable<RuleSetEntity>()
            .createTable<JobLogEntity>()
            .createTable<HookOutboxEntity>()
            .createTable<FormEntity>()
            .createTable<FieldActivityEditEntity>()
            .createTable<SailProcessCopytoEntity>();

        EnsureHotPathIndexes(kit);
        return wk.WorkingProgress?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 待办/实例热路径索引：DDLBuilder.hasIndex → setIndex → doCreateIndex（无 IF NOT EXISTS）。
    /// </summary>
    public static void EnsureHotPathIndexes(SQLBuilder kit)
    {
        ArgumentNullException.ThrowIfNull(kit);

        TryCreateIndex(kit, "IX_sf_task_pi", "sf_task", "tk_proc_instance_id");
        TryCreateIndex(kit, "IX_sf_task_sign_state", "sf_task",
            "tk_sign_id", "tk_task_state", "tk_proc_instance_id");
        TryCreateIndex(kit, "IX_sf_task_state_created", "sf_task",
            "tk_task_state", "tk_crt_time");
        TryCreateIndex(kit, "IX_sf_task_state_due", "sf_task",
            "tk_task_state", "tk_due_at");
        TryCreateIndex(kit, "IX_sf_task_consult_src", "sf_task",
            "tk_consult_src_task_id", "tk_task_state");
        TryCreateIndex(kit, "IX_sf_ai_pi_state", "sf_act_instance",
            "ai_proc_instance_id", "ai_activity_state");
        TryCreateIndex(kit, "IX_sf_pi_state_created", "sf_proc_instance",
            "pi_process_state", "pi_crt_time");
        TryCreateIndex(kit, "IX_sf_pi_creator_state", "sf_proc_instance",
            "pi_crt_user_id", "pi_process_state", "pi_crt_time");
        TryCreateIndex(kit, "IX_sf_pv_pi_name", "sf_proc_variable",
            "pv_proc_instance_id", "pv_name");
        TryCreateIndex(kit, "IX_sf_ti_pi", "sf_track",
            "ti_proc_instance_id");
        TryCreateIndex(kit, "IX_sf_hook_outbox_status", "sf_hook_outbox",
            "ho_status", "ho_nxt_retry_at");
    }

    static void TryCreateIndex(SQLBuilder kit, string name, string table, params string[] columns)
    {
        try
        {
            var ddl = kit.useDDL();
            if (ddl.hasIndex(name))
                return;

            ddl.clear()
                .setTable(table)
                .setIndex(new List<string>(columns), name)
                .doCreateIndex();
        }
        catch
        {
            // 缺表或权限不足时忽略
        }
    }

    static string EnsureAiTables(SQLBuilder kit)
    {
        var wk = kit.useDBInit()
            .upsertMode()
            .columnTypeByTag("sail.id", DataFam.Guid)
            .columnTypeByTag("sail.xml", DataFam.Json)
            .createTable<AiModelProviderEntity>()
            .createTable<AiActivityConfigEntity>();
        return wk.WorkingProgress?.ToString() ?? string.Empty;
    }

    static string EnsureProductTables(SQLBuilder kit)
    {
        var wk = kit.useDBInit()
            .upsertMode()
            .columnTypeByTag("sail.id", DataFam.Guid)
            .columnTypeByTag("sail.xml", DataFam.Json)
            .createTable<SailLeaveEntity>()
            .createTable<SailProcessCategoryEntity>()
            .createTable<SailProcessMetaEntity>()
            .createTable<SailProcessAuthEntity>()
            .createTable<SailPublishedReleaseEntity>()
            .createTable<SailEffectiveSourceEntity>()
            .createTable<SailTodoPortalRecordEntity>()
            .createTable<SailPreselectAssigneeEntity>()
            .createTable<SailSurrogateEntity>()
            .createTable<SailProcessIdSeqEntity>()
            .createTable<SailApprovalPhraseEntity>();
        TryCreateIndex(kit, "IX_sf_preselect_pi_act", "sf_preselect_assignee",
            "process_instance_id", "activity_id");
        TryCreateIndex(kit, "IX_sf_surrogate_grantor", "sf_surrogate",
            "grantor_id", "enabled", "process_id");
        TryCreateIndex(kit, "IX_sf_process_id_seq_pid_bucket", "sf_process_id_seq",
            "process_id", "bucket");
        TryCreateIndex(kit, "IX_sf_approval_phrase_user", "sf_approval_phrase",
            "user_id", "enabled", "sort_order");
        return wk.WorkingProgress?.ToString() ?? string.Empty;
    }

    /// <summary>兼容旧调用：等价于 <see cref="SailProductSeed.EnsureAll"/>。</summary>
    public static int EnsureMinimalSeed(SQLBuilder kit) => SailProductSeed.EnsureAll(kit);
}
