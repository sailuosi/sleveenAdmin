namespace SleveenAdmin.Sailflow.Dto;

/// <summary>流程「更多设置」ExtraJson 运行时模型（与前端 packExtraJson 对齐）。</summary>
public sealed class SailProcessExtraDto
{
    public bool AllowCancelRunningProcess { get; set; } = true;
    public bool AllowWithdrawTask { get; set; }
    public SailProcessIdRuleDto ProcessIdRule { get; set; } = new();
    /// <summary>0不处理 · 1本实例曾办过则自动过 · 2仅连续节点同人自动过。</summary>
    public int AutoApprovalType { get; set; }
    public SailTitleSettingDto TitleSetting { get; set; } = new();
    public SailSummarySettingDto SummarySetting { get; set; } = new();
    public SailHttpTriggerSettingDto? ProcessBeforeTriggerSetting { get; set; }
    public SailHttpTriggerSettingDto? ProcessAfterTriggerSetting { get; set; }
    public SailHttpTriggerSettingDto? TaskBeforeTriggerSetting { get; set; }
    public SailHttpTriggerSettingDto? TaskAfterTriggerSetting { get; set; }
    public SailPrintTemplateSettingDto PrintTemplateSetting { get; set; } = new();
}

public sealed class SailProcessIdRuleDto
{
    public bool Enable { get; set; }
    public string Prefix { get; set; } = "";
    public string Infix { get; set; } = "";
    public string Postfix { get; set; } = "";
    public int Length { get; set; } = 5;
}

public sealed class SailTitleSettingDto
{
    public bool Enable { get; set; }
    public string Title { get; set; } = "";
}

public sealed class SailSummarySettingDto
{
    public bool Enable { get; set; }
    public List<string> Summary { get; set; } = new();
}

public sealed class SailHttpTriggerSettingDto
{
    public string Url { get; set; } = "";
    public List<SailHttpKvDto> Header { get; set; } = new();
    public List<SailHttpKvDto> Body { get; set; } = new();
    public List<SailHttpKvDto> Response { get; set; } = new();
}

public sealed class SailHttpKvDto
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public sealed class SailPrintTemplateSettingDto
{
    public bool Enable { get; set; }
    public string Template { get; set; } = "";
}
