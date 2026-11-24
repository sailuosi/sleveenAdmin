using System.Text;
using System.Text.Json;
using mooSQL.data;
using sailflow.hosting;
using sailflow.data;
using sailflow.events;
using sailflow.hooks;
using sailflow.log;
using sailflow.msg.SideEffect;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Hooks;

public sealed class SailProcessExtraHttpPayload
{
    public string Url { get; init; } = "";
    public Dictionary<string, string> Headers { get; init; } = new();
    public string BodyJson { get; init; } = "{}";
}

/// <summary>Extra HTTP 通知：BestEffort Channel 异步 POST，不回滚引擎。</summary>
public sealed class SailProcessExtraHttpTriggerHook : BestEffortFlowHookBase<SailProcessExtraHttpPayload>
{
    public const string HttpClientName = "SailProcessExtraHttp";

    readonly SailClient _sail;
    readonly IHttpClientFactory? _httpFactory;

    public SailProcessExtraHttpTriggerHook(
        SailClient sail,
        IHttpClientFactory? httpFactory = null,
        BestEffortChannelOptions? channel = null)
        : base(channel ?? new BestEffortChannelOptions { Capacity = 2048, DropWhenFull = true })
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
        _httpFactory = httpFactory;
    }

    public override string Name => "sail.process.extra.http";
    public override string Title => "流程 Extra HTTP 通知";

    protected override bool TryCreatePayload(FlowHookContext ctx, out SailProcessExtraHttpPayload payload)
    {
        payload = null!;
        if (string.IsNullOrWhiteSpace(ctx.ProcessId))
            return false;

        try
        {
            var kit = ctx.Kit ?? _sail.UseRunSQL();
            var version = string.IsNullOrWhiteSpace(ctx.Version)
                ? ResolveVersion(kit, ctx.ProcessInstanceId)
                : ctx.Version!;
            var extra = SailProcessExtraService.Parse(LoadExtraJson(kit, ctx.ProcessId, version));
            var setting = PickSetting(extra, ctx.EventType);
            if (setting == null || string.IsNullOrWhiteSpace(setting.Url))
                return false;

            var pi = string.IsNullOrWhiteSpace(ctx.ProcessInstanceId)
                ? null
                : kit.useRepo<ProcessInstanceEntity, string>().GetById(ctx.ProcessInstanceId);

            var formBag = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // 轻量：不拉全量变量，用上下文字段 + 默认 JSON
            var bag = SailProcessExtraTemplate.BuildBagBag(
                pi?.ProcessName,
                pi?.Title,
                pi?.CreatedUserName,
                ctx.ProcessInstanceId,
                ctx.ProcessId,
                pi?.BizCode,
                null,
                formBag);

            bag["EventType"] = ctx.EventType.ToString();
            bag["TaskId"] = ctx.TaskId ?? "";
            bag["ActivityId"] = ctx.ActivityId ?? "";
            bag["UserId"] = ctx.SignId ?? "";
            bag["UserName"] = ctx.SignName ?? "";
            bag["BizPK"] = ctx.BizPK ?? "";
            bag["BizChannel"] = ctx.BizChannel ?? "";

            var url = SailProcessExtraTemplate.Replace(setting.Url.Trim(), bag);
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
                return false;

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in setting.Header ?? [])
            {
                if (string.IsNullOrWhiteSpace(h.Key)) continue;
                headers[h.Key.Trim()] = SailProcessExtraTemplate.Replace(h.Value, bag);
            }

            string bodyJson;
            if (setting.Body is { Count: > 0 })
            {
                var bodyObj = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var b in setting.Body)
                {
                    if (string.IsNullOrWhiteSpace(b.Key)) continue;
                    bodyObj[b.Key.Trim()] = SailProcessExtraTemplate.Replace(b.Value, bag);
                }
                bodyJson = JsonSerializer.Serialize(bodyObj);
            }
            else
            {
                bodyJson = JsonSerializer.Serialize(new
                {
                    eventType = ctx.EventType.ToString(),
                    processId = ctx.ProcessId,
                    version,
                    processInstanceId = ctx.ProcessInstanceId,
                    taskId = ctx.TaskId,
                    activityId = ctx.ActivityId,
                    userId = ctx.SignId,
                    userName = ctx.SignName,
                    bizPK = ctx.BizPK,
                    bizChannel = ctx.BizChannel,
                    title = pi?.Title,
                    processName = pi?.ProcessName
                });
            }

            payload = new SailProcessExtraHttpPayload
            {
                Url = url,
                Headers = headers,
                BodyJson = bodyJson
            };
            return true;
        }
        catch (Exception ex)
        {
            _sail.Log?.Warning(SailLogCategories.AsyncHook, "[Extra.Http] payload failed: {0}", ex.Message);
            return false;
        }
    }

    protected override async Task ConsumeAsync(
        SailProcessExtraHttpPayload payload,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpFactory?.CreateClient(HttpClientName) ?? new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, payload.Url)
            {
                Content = new StringContent(payload.BodyJson, Encoding.UTF8, "application/json")
            };
            foreach (var h in payload.Headers)
            {
                if (string.Equals(h.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!req.Headers.TryAddWithoutValidation(h.Key, h.Value))
                    req.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }

            using var resp = await client.SendAsync(req, cancellationToken).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                _sail.Log?.Warning(
                    SailLogCategories.AsyncHook,
                    "[Extra.Http] {0} → {1}",
                    payload.Url, (int)resp.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _sail.Log?.Warning(
                SailLogCategories.AsyncHook,
                "[Extra.Http] send failed {0}: {1}",
                payload.Url, ex.Message);
        }
    }

    static SailHttpTriggerSettingDto? PickSetting(SailProcessExtraDto extra, EventFireTypeEnum ev)
        => ev switch
        {
            EventFireTypeEnum.OnProcessStarted => extra.ProcessBeforeTriggerSetting,
            EventFireTypeEnum.OnProcessCompleted
                or EventFireTypeEnum.OnProcessClosed => extra.ProcessAfterTriggerSetting,
            EventFireTypeEnum.OnTaskCreated => extra.TaskBeforeTriggerSetting,
            EventFireTypeEnum.OnTaskCompleted => extra.TaskAfterTriggerSetting,
            _ => null
        };

    static string ResolveVersion(SQLBuilder kit, string? processInstanceId)
    {
        if (string.IsNullOrWhiteSpace(processInstanceId))
            return "1";
        var pi = kit.useRepo<ProcessInstanceEntity, string>().GetById(processInstanceId);
        return string.IsNullOrWhiteSpace(pi?.Version) ? "1" : pi!.Version!;
    }

    static string? LoadExtraJson(SQLBuilder kit, string processId, string version)
    {
        var clip = kit.useClip();
        clip.from<SailProcessMetaEntity>(out var m);
        clip.where(() => m.ProcessId, processId);
        clip.where(() => m.Version, version);
        return clip.select(m).queryList().FirstOrDefault()?.ExtraJson;
    }
}
