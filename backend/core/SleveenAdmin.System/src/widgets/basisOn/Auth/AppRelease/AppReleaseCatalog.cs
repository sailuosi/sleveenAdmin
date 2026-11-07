namespace SleveenAdmin.System.Auth.AppRelease;

/// <summary>
/// 前端发版静态清单（强控版本，无库表）。
///
/// 发版步骤（必须同时完成）：
/// 1. 前端 .env.prod / .env.test 修改 VITE_APP_VERSION 为新版本号并重新打包部署
/// 2. 在下方 Releases 的 index 0 插入同号对象（Version / UpdateNotes / UpdateTip），旧项自动后移
/// 3. 部署本后端（或与前端同步部署）；若只改后端版本而前端未发，客户端会提示刷新且防死循环后常驻提示
///
/// 示例：
/// Releases.Insert(0, new AppReleaseInfo
/// {
///     Version = "1.0.1",
///     UpdateNotes = "修复菜单缓存；优化列表查询",
///     UpdateTip = "系统已更新，即将自动刷新以加载新版本",
///     PublishTime = DateTime.Now
/// });
/// </summary>
public static class AppReleaseCatalog
{
    /// <summary>
    /// 发版记录：最新版本始终放在 index 0。
    /// 每次发版请 Insert(0, ...)，不要改历史项的 Version。
    /// </summary>
    public static readonly List<AppReleaseInfo> Releases =
    [
        new AppReleaseInfo
        {
            Version = "1.0.0",
            UpdateNotes = "初始版本",
            UpdateTip = "系统已更新，即将自动刷新以加载新版本",
            PublishTime = new DateTime(2026, 11, 6)
        }
    ];

    /// <summary>
    /// 当前生效版本（list[0]）；空列表时返回安全占位，避免接口抛错。
    /// </summary>
    public static AppReleaseInfo Current
    {
        get
        {
            if (Releases.Count == 0)
            {
                return new AppReleaseInfo
                {
                    Version = "0.0.0",
                    UpdateNotes = string.Empty,
                    UpdateTip = "系统已更新，请刷新页面",
                    PublishTime = null
                };
            }

            return Releases[0];
        }
    }
}
