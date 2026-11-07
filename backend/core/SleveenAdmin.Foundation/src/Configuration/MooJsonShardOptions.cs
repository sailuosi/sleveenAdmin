namespace SleveenAdmin.Foundation.Configuration;

/// <summary>
/// 代码侧追加的 JSON 分片扫描选项（与 appsettings 中的同名配置合并）。
/// </summary>
public sealed class MooJsonShardOptions
{
    /// <summary>
    /// 额外扫描目录：相对路径相对于 BaseDirectory / ContentRoot；也可为绝对路径。
    /// 空字符串表示根目录本身（根目录默认已加入）。
    /// </summary>
    public List<string> ScanDirectories { get; } = [];

    /// <summary>
    /// 额外忽略的文件名或 glob（如 <c>*.local.json</c>）。
    /// </summary>
    public List<string> IgnoreFiles { get; } = [];

    /// <summary>
    /// 是否启用 <c>reloadOnChange</c>。默认 true。
    /// </summary>
    public bool ReloadOnChange { get; set; } = true;
}
