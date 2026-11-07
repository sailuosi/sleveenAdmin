using Newtonsoft.Json;

namespace SleveenAdmin.Saiflow.Integration.UnifiedReport;

/// <summary>集成平台统一日程待办报文（对齐 szkfpt TaskInfoVO）。</summary>
public sealed class TaskInfoDto
{
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; set; }

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public string? Type { get; set; }

    [JsonProperty("appId", NullValueHandling = NullValueHandling.Ignore)]
    public string? AppId { get; set; }

    [JsonProperty("appName", NullValueHandling = NullValueHandling.Ignore)]
    public string? AppName { get; set; }

    [JsonProperty("appScheduleId", NullValueHandling = NullValueHandling.Ignore)]
    public string? AppScheduleId { get; set; }

    [JsonProperty("pcUrl", NullValueHandling = NullValueHandling.Ignore)]
    public string? PcUrl { get; set; }

    [JsonProperty("phUrl", NullValueHandling = NullValueHandling.Ignore)]
    public string? PhUrl { get; set; }

    [JsonProperty("activityName", NullValueHandling = NullValueHandling.Ignore)]
    public string? ActivityName { get; set; }

    /// <summary>0 流程 · 1 消息</summary>
    [JsonProperty("sourceType", NullValueHandling = NullValueHandling.Ignore)]
    public string? SourceType { get; set; }

    [JsonProperty("secLevel", NullValueHandling = NullValueHandling.Ignore)]
    public string? SecLevel { get; set; }

    [JsonProperty("secLevelName", NullValueHandling = NullValueHandling.Ignore)]
    public string? SecLevelName { get; set; }

    /// <summary>0 待办 · 1 已办</summary>
    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string? State { get; set; }

    [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
    public string? StartTime { get; set; }

    [JsonProperty("updateTime", NullValueHandling = NullValueHandling.Ignore)]
    public string? UpdateTime { get; set; }

    [JsonProperty("createUserCode", NullValueHandling = NullValueHandling.Ignore)]
    public string? CreateUserCode { get; set; }

    [JsonProperty("createUserName", NullValueHandling = NullValueHandling.Ignore)]
    public string? CreateUserName { get; set; }

    [JsonProperty("receiveUserCode", NullValueHandling = NullValueHandling.Ignore)]
    public string? ReceiveUserCode { get; set; }

    [JsonProperty("receiveUserName", NullValueHandling = NullValueHandling.Ignore)]
    public string? ReceiveUserName { get; set; }

    [JsonProperty("openType", NullValueHandling = NullValueHandling.Ignore)]
    public string? OpenType { get; set; }

    [JsonProperty("countUrl", NullValueHandling = NullValueHandling.Ignore)]
    public string? CountUrl { get; set; }

    [JsonProperty("remindType", NullValueHandling = NullValueHandling.Ignore)]
    public string? RemindType { get; set; }
}
