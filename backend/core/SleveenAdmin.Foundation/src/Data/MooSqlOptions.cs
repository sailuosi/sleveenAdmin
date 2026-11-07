using mooSQL.config;

namespace SleveenAdmin.Foundation.Data;

/// <summary>
/// 数据库连接配置（节名与 api10 一致：DBCoonfig）。
/// </summary>
public class DBCoonfigOptions
{
    public const string SectionName = "DBCoonfig";

    /// <summary>
    /// 启用控制台打印 SQL（预留，与 api10 字段对齐）。
    /// </summary>
    public bool EnableConsoleSql { get; set; }

    public List<DBPosition> Connections { get; set; } = [];
}
