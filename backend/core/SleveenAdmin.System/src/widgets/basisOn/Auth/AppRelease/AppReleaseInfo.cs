namespace SleveenAdmin.System.Auth.AppRelease;

/// <summary>
/// 前端发版信息（静态清单项）
/// </summary>
public class AppReleaseInfo
{
    /// <summary>
    /// 版本号（须与前端 VITE_APP_VERSION 一致）
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 更新说明（详细变更）
    /// </summary>
    public string UpdateNotes { get; set; } = string.Empty;

    /// <summary>
    /// 更新提示（弹窗主文案）
    /// </summary>
    public string UpdateTip { get; set; } = string.Empty;

    /// <summary>
    /// 发布时间（可选）
    /// </summary>
    public DateTime? PublishTime { get; set; }
}
