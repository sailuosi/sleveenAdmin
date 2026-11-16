using System.Text.Json;
using mooSQL.data;
using sailflow.engine.Client;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.Service;

/// <summary>解析 sf_process_meta.ExtraJson；缺省与产品 seed 一致。</summary>
[Scoped]
public class SailProcessExtraService
{
    static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    readonly SailClient _sail;

    public SailProcessExtraService(SailClient sail)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
    }

    public SailProcessExtraDto Get(string? processId, string? version)
    {
        if (string.IsNullOrWhiteSpace(processId))
            return Defaults();

        SailHostSchemaInitializer.EnsureHostTables(_sail);
        var ver = string.IsNullOrWhiteSpace(version) ? "1" : version.Trim();
        var clip = _sail.UseRunSQL().useClip();
        clip.from<SailProcessMetaEntity>(out var m);
        clip.where(() => m.ProcessId, processId.Trim());
        clip.where(() => m.Version, ver);
        var meta = clip.select(m).queryList().FirstOrDefault();
        return Parse(meta?.ExtraJson);
    }

    public static SailProcessExtraDto Parse(string? extraJson)
    {
        if (string.IsNullOrWhiteSpace(extraJson))
            return Defaults();
        try
        {
            var dto = JsonSerializer.Deserialize<SailProcessExtraDto>(extraJson, JsonOpts);
            return Normalize(dto ?? Defaults());
        }
        catch
        {
            return Defaults();
        }
    }

    public static SailProcessExtraDto Defaults() => Normalize(new SailProcessExtraDto());

    static SailProcessExtraDto Normalize(SailProcessExtraDto dto)
    {
        dto.ProcessIdRule ??= new SailProcessIdRuleDto();
        if (dto.ProcessIdRule.Length < 5)
            dto.ProcessIdRule.Length = 5;
        dto.TitleSetting ??= new SailTitleSettingDto();
        dto.SummarySetting ??= new SailSummarySettingDto();
        dto.SummarySetting.Summary ??= new List<string>();
        dto.PrintTemplateSetting ??= new SailPrintTemplateSettingDto();
        if (dto.AutoApprovalType is < 0 or > 2)
            dto.AutoApprovalType = 0;
        return dto;
    }
}
