namespace SleveenAdmin.Sailflow.UnitScope;

/// <summary>宿主侧流程 Auth*（NuGet ProcessEntity 未暴露时经 SQL 读写 p_auth_*）。</summary>
public sealed class SailProcessAuthBag
{
    public string ProcessId { get; set; } = "";
    public string Version { get; set; } = "1";
    public string AuthType { get; set; } = "";
    public string AuthCode { get; set; } = "";
    public string AuthId { get; set; } = "";
    public int? AuthLevel { get; set; }
}
