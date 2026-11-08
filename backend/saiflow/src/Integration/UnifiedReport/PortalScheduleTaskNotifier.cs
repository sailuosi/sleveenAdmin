using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using saiflow.engine.Entity;
using saiflow.engine.Manager;
using saiflow.engine.Client;
using saiflow.engine.Hooks;
using saiflow.msg.SideEffect;

namespace SleveenAdmin.Saiflow.Integration.UnifiedReport;

/// <summary>
/// Reliable 待办推送：流程待办 → 集成平台统一日程（Created=add，Completed/Cancelled=update）。
/// 走 AsyncOutbox；失败抛出由 Drain 重试 / 死信。骨架见 <see cref="ReliableTaskNotifierBase"/>。
/// </summary>
public sealed class PortalScheduleTaskNotifier : ReliableTaskNotifierBase
{
    private const string OpPush = "推送待办";
    private const string OpUpdate = "更新待办";
    private static readonly string TimeFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly SailClient _sail;
    private readonly UnifiedReportClient _client;
    private readonly UnifiedReportOptions _options;
    private readonly SailTodoPortalRecordService _records;
    private readonly ILogger<PortalScheduleTaskNotifier> _logger;

    public PortalScheduleTaskNotifier(
        SailClient sail,
        UnifiedReportClient client,
        IOptions<UnifiedReportOptions> options,
        SailTodoPortalRecordService records,
        ILogger<PortalScheduleTaskNotifier> logger)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _records = records ?? throw new ArgumentNullException(nameof(records));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override string Name => "portal.schedule";

    protected override async Task SendCoreAsync(TaskNotifyContext ctx, CancellationToken ct)
    {
        if (!_options.Enabled || ctx == null)
            return;

        if (ctx.Kind is not (TaskNotifyKind.Created or TaskNotifyKind.Completed or TaskNotifyKind.Cancelled))
            return;

        var taskInfo = BuildTaskInfo(ctx);
        var opType = ctx.Kind == TaskNotifyKind.Created ? OpPush : OpUpdate;

        try
        {
            if (ctx.Kind == TaskNotifyKind.Created)
                await _client.AddScheduleAsync(taskInfo, ct).ConfigureAwait(false);
            else
                await _client.UpdateScheduleAsync(taskInfo, ct).ConfigureAwait(false);

            TryInsertRecord(taskInfo, opType, success: true, null);
        }
        catch (Exception ex)
        {
            TryInsertRecord(taskInfo, opType, success: false, ex.Message);
            throw;
        }
    }

    private TaskInfoDto BuildTaskInfo(TaskNotifyContext ctx)
    {
        TaskEntity? task = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(ctx.TaskId))
                task = new TaskManager(_sail).GetTask(ctx.TaskId);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[PortalSchedule] 读取 TaskEntity 失败 taskId={TaskId}", ctx.TaskId);
        }

        var activityName = task?.ActivityName ?? ctx.ActivityCode ?? "";
        var appName = string.IsNullOrWhiteSpace(ctx.AppName) ? _options.AppName : ctx.AppName;
        var title = string.IsNullOrWhiteSpace(activityName)
            ? appName
            : $"{appName}-{activityName}";

        var createCode = task?.CreatedUserId ?? ctx.AssignedUserId ?? "";
        var createName = task?.CreatedUserName ?? "";
        var receiveCode = ctx.AssignedUserId ?? task?.AssignedUserId ?? "";
        var receiveName = ctx.AssignedUserName ?? task?.AssignedUserName ?? "";

        var now = DateTime.Now.ToString(TimeFormat);
        var dto = new TaskInfoDto
        {
            AppId = _options.AppId,
            AppName = string.IsNullOrWhiteSpace(_options.AppName) ? appName : _options.AppName,
            AppScheduleId = ctx.TaskId ?? "",
            Name = title,
            ActivityName = activityName,
            CreateUserCode = createCode,
            CreateUserName = createName,
            ReceiveUserCode = receiveCode,
            ReceiveUserName = receiveName,
            PcUrl = BuildPcUrl(ctx.TaskId, ctx.ProcessInstanceId),
            PhUrl = string.IsNullOrWhiteSpace(_options.PhUrlBase) ? "" : _options.PhUrlBase.Trim(),
            CountUrl = "",
            OpenType = "",
            RemindType = "0000",
            SecLevel = "0",
            SecLevelName = "普通代办",
            SourceType = "0",
            Type = _options.Type,
            UpdateTime = now,
        };

        if (ctx.Kind == TaskNotifyKind.Created)
        {
            dto.State = "0";
            dto.StartTime = task?.CreatedDateTime?.ToLocalTime().ToString(TimeFormat) ?? now;
        }
        else
        {
            dto.State = "1";
            dto.StartTime = null;
        }

        return dto;
    }

    private string BuildPcUrl(string? taskId, string? processInstanceId)
    {
        var baseUrl = (_options.PcUrlBase ?? "").Trim();
        var tid = Uri.EscapeDataString(taskId ?? "");
        var pid = Uri.EscapeDataString(processInstanceId ?? "");
        var path = $"sail/process-detail?taskId={tid}&processInstanceId={pid}";
        if (string.IsNullOrEmpty(baseUrl))
            return "/" + path;
        if (baseUrl.EndsWith('/') || baseUrl.EndsWith('#'))
            return baseUrl + path;
        if (baseUrl.EndsWith("#/", StringComparison.Ordinal))
            return baseUrl + path;
        return baseUrl.TrimEnd('/') + "/" + path;
    }

    private void TryInsertRecord(TaskInfoDto taskInfo, string opType, bool success, string? resultMsg)
    {
        try
        {
            var taskId = string.IsNullOrWhiteSpace(taskInfo.AppScheduleId)
                ? null
                : taskInfo.AppScheduleId.Trim();
            _records.Insert(
                taskId,
                taskInfo.AppScheduleId,
                taskInfo.ReceiveUserCode,
                taskInfo.ReceiveUserName,
                opType,
                success,
                resultMsg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[PortalSchedule] 写入推送流水失败 scheduleId={ScheduleId} opType={OpType}",
                taskInfo.AppScheduleId, opType);
        }
    }
}
