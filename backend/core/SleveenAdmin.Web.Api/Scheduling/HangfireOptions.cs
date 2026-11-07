namespace SleveenAdmin.Web.Api.Scheduling;

public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";

    public bool Enabled { get; set; } = false;

    public string DashboardPath { get; set; } = "/hangfire";

    public string SchemaName { get; set; } = "Hangfire";

    public string RefreshTokenCleanupCron { get; set; } = "0 3 * * *";

    /// <summary>Sail 流程热→冷归档 cron（默认每天 3:30）。</summary>
    public string SailProcessArchiveCron { get; set; } = "30 3 * * *";

    /// <summary>是否注册 Sail 归档作业（需 Hangfire:Enabled）。</summary>
    public bool SailProcessArchiveEnabled { get; set; } = true;
}
