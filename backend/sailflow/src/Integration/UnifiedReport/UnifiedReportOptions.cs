namespace SleveenAdmin.Sailflow.Integration.UnifiedReport;

/// <summary>统一用户中心 / 集成平台待办同步配置（对齐 szkfpt <c>unified-report.*</c>）。</summary>
public sealed class UnifiedReportOptions
{
    public const string SectionName = "UnifiedReport";

    /// <summary>关闭时 Notifier 直接 no-op（本地开发默认关）。</summary>
    public bool Enabled { get; set; }

    public string BaseUrl { get; set; } = "";

    /// <summary>日程 API 根；空则回退 <see cref="BaseUrl"/>。</summary>
    public string ScheduleBaseUrl { get; set; } = "";

    public string ClientId { get; set; } = "";

    /// <summary>Token 请求用 Basic 凭证，形如 <c>Basic xxx</c>。</summary>
    public string BasicAuthorization { get; set; } = "";

    public string GrantType { get; set; } = "client_credentials";

    public string TokenOperationCode { get; set; } = "jcpt.uums.oauth.token";

    public string ReportOperationCode { get; set; } = "jcpt.uums.userinfo.reportUser";

    public string AppId { get; set; } = "";

    public string AppCode { get; set; } = "";

    public string AppName { get; set; } = "";

    /// <summary>前端 hash 根，如 <c>http://host/#/</c>。</summary>
    public string PcUrlBase { get; set; } = "";

    public string PhUrlBase { get; set; } = "";

    /// <summary>TaskInfo.type（业务系统在集成平台登记的类型码）。</summary>
    public string Type { get; set; } = "";

    public int TokenAheadSeconds { get; set; } = 60;

    public int ConnectTimeoutMs { get; set; } = 5000;

    public int ReadTimeoutMs { get; set; } = 10000;

    public string AddScheduleOperationCode { get; set; } = "jcpt.portal.schedule.addSchedules";

    public string UpdateScheduleOperationCode { get; set; } = "jcpt.portal.schedule.updateSchedules";

    public string DeleteScheduleOperationCode { get; set; } = "jcpt.portal.schedule.deleteSchedules";
}
