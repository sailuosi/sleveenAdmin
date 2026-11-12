namespace SleveenAdmin.Sailflow.Dto;

/// <summary>
/// 流程 xml 文件查询入参（POST api/Wf2Xml/QueryProcessFile）
/// </summary>
public class ProcessFileQuery
{
    public string ProcessId { get; set; } = string.Empty;
    public string? Version { get; set; }
}
