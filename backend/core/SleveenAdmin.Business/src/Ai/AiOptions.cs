namespace SleveenAdmin.Business.Ai;

public class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>Placeholder | OpenAI</summary>
    public string Provider { get; set; } = AiProviders.Placeholder;

    public string Model { get; set; } = "gpt-4o-mini";

    public string? ApiKey { get; set; }

    /// <summary>可选，OpenAI 兼容 API 基址（如 https://api.deepseek.com）。</summary>
    public string? Endpoint { get; set; }

    /// <summary>MAF Agent 名称。</summary>
    public string AgentName { get; set; } = "sleveen-admin助手";

    /// <summary>MAF Agent 系统指令。</summary>
    public string? Instructions { get; set; }
}
