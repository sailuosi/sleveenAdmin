using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sailflow.AI.Configuration;
using sailflow.AI.Hosting;
using sailflow.AI.Service;
using sailflow.biz.Hosting;
using sailflow.biz.query;
using sailflow.data.Hosting;
using sailflow.engine.Client;
using sailflow.engine.Hosting;
using sailflow.engine.Service;
using sailflow.hub.Hosting;
using sailflow.hub.Hosting;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Sailflow.Assignee;
using SleveenAdmin.Sailflow.FormBiz;
using SleveenAdmin.Sailflow.Hooks;
using SleveenAdmin.Sailflow.Integration.UnifiedReport;
using SleveenAdmin.Sailflow.Operations;
using SleveenAdmin.Sailflow.Resource;
using SleveenAdmin.Sailflow.SideEffects;
using SleveenAdmin.Sailflow.Timeout;
using mooSQL.data;

namespace SleveenAdmin.Sailflow.Hosting;

public static class SailflowHostSetup
{
    public static IServiceCollection AddSailflowHost(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<UnifiedReportOptions>(configuration.GetSection(UnifiedReportOptions.SectionName));
        services.AddHttpClient(nameof(UnifiedReportClient), (sp, http) =>
        {
            var opts = sp.GetRequiredService<IOptions<UnifiedReportOptions>>().Value;
            var timeoutMs = Math.Max(opts.ReadTimeoutMs, opts.ConnectTimeoutMs);
            http.Timeout = TimeSpan.FromMilliseconds(Math.Max(timeoutMs, 1000));
        });
        services.AddSingleton<UnifiedReportClient>();

        // DBCash 经 AddMooSql 绑定；建表由宿主 PostBuild（SailHostSchemaInitializer），不在 sailflow 包内。
        services.AddSingleton(sp =>
        {
            _ = sp.GetRequiredService<DBInsCash>();
            var client = SailConfigBizExtensions.CreateBuilder(configuration)
                .UseBizJson()
                .UseBizRules()
                .UseBizAction()
                .UseBizAssignees()
                .UseBizData() // sailflow.data：Table 日志 Sink + 归档扩展
                .UseDataLifecycleFromConfig(configuration) // Sail:DataLifecycle → 保留窗/冷库/日志分表
                .UseDebugConsoleHook() // 开发核验：流程/待办 Hook → 控制台
                .UseFlowEventFileLog() // BestEffort：流程事件 Channel → 串行写文件（可丢）
                .UseFormBizLifecycleHook() // §H：FormCode → OnFinish（含请假 Status）
                .UseNodePolicyTimeoutStamp() // §A：NodePolicy 超时 → OverdueDateTime
                // 立刻归档默认不挂：需要时 .UseImmediateProcessArchiveHook()（sailflow.data）
                // 旧 UseLeaveStatusHook 已由 FormBiz 生命周期 Hook 替代
                .UsePortalScheduleTaskNotifier(sp) // Reliable：UnifiedReport:Enabled 时 Outbox Drain + 待办推送
                .UseSleveenOrgResource() // 组织人员：具化 IDeptService/IResourceService → Locator
                .UseInstanceLockCleaner(o =>
                {
                    // 本机 AppInstance 执行锁空闲销户（Timer）；未 Use 则字典只增不减
                    o.Period = TimeSpan.FromMinutes(5);
                    o.IdleTtl = TimeSpan.FromMinutes(15);
                })
                .ConfigureServices(s =>
                {
                    s.UseDatabase(
                        _ => DBCash.GetDBInstance(SailflowDefaults.DbPosition),
                        SailflowDefaults.DbPosition);
                    s.UseBizLocalize();
                    // 不用引擎 UseResource（无组织人员表）；由 UseSleveenOrgResource 注入契约实现
                    s.UseForm();
                    s.UseBizAI(o =>
                    {
                        o.RegisterExecutor = true;
                        o.EnsureTables = false;
                    });
                    s.AddFeature(new SailFeatureDescriptor
                    {
                        Name = "Sleveen.Sailflow.Schema",
                        Phase = SailFeaturePhase.PostBuild,
                        Order = 900,
                        DependsOn = new[] { SailFeatureNames.Database },
                        PostBuild = (c, _) => SailHostSchemaInitializer.InitDb(
                            c, seed: true, includeAi: true, registerAiExecutor: false)
                    });
                })
                .Build();

            // R3：UnitMap→Org/User；RoleGroup→VisitorBag/Duty；JobRelative→maps|访客袋；Account 身份键
            client.UseSleveenOrgLookups(configuration);
            // §A：NodePolicy 空人/同人（同人=3 → assignStartResolveType 走管道，如 deptBoss）
            client.UseSleveenNodePolicyAssignee(configuration);

            // LogSinkKind=HostLogger 时转发 ILogger；Table/File 由 UseBizData PostBuild 处理
            if (client.DataLifecycleOptions?.LogSinkKind == EngineLogSinkKind.HostLogger)
                client.useHostLoggerLogSink(sp.GetRequiredService<ILoggerFactory>());

            return client;
        });

        // 引擎 / biz 跨程序集：生成器扫不到，且 I*Service 接口需手工挂
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<WorkflowQueryService>();
        services.AddScoped<IAiModelDataService, AiModelDataService>();
        // 本程序集 [Scoped] → GeneratedDiRegistration
        services.AddSleveenAdminSailflowServices();

        services.AddSailFormBiz();
        services.AddSailOperationHandlers();
        services.AddSailNodePolicyJobs(); // §A：策略跳过代办 + 超时扫描

        var aiOpts = AiAppConfigProviderOptions.Load(configuration);
        ApiKeyCryptoHelper.Configure(aiOpts);

        return services;
    }
}
