using System.Text.RegularExpressions;
using SleveenAdmin.Sailflow.Dto;

namespace SleveenAdmin.Sailflow.Utility;

/// <summary>Extra 标题/打印/HTTP 的 {字段} 替换。</summary>
public static class SailProcessExtraTemplate
{
    static readonly Regex Placeholder = new(@"\{([^{}]+)\}", RegexOptions.Compiled);

    public const string SummaryVar = "__sail_summary__";
    public const string ProcessNoVar = "__sail_process_no__";

    public static string Replace(
        string? template,
        IReadOnlyDictionary<string, string>? values)
    {
        if (string.IsNullOrEmpty(template))
            return "";
        if (values == null || values.Count == 0)
            return template;

        return Placeholder.Replace(template, m =>
        {
            var key = m.Groups[1].Value.Trim();
            if (values.TryGetValue(key, out var v))
                return v ?? "";
            // 大小写不敏感回退
            foreach (var kv in values)
            {
                if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
                    return kv.Value ?? "";
            }
            return "";
        });
    }

    public static Dictionary<string, string> BuildBagBag(
        string? processName,
        string? title,
        string? starterName,
        string? processInstanceId,
        string? processId,
        string? bizCode,
        string? processNo,
        IReadOnlyDictionary<string, string>? formValues)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ProcessName"] = processName ?? "",
            ["Title"] = title ?? processName ?? "",
            ["Starter"] = starterName ?? "",
            ["ProcessInstanceId"] = processInstanceId ?? "",
            ["ProcessId"] = processId ?? "",
            ["BizCode"] = bizCode ?? "",
            ["ProcessNo"] = processNo ?? bizCode ?? ""
        };
        if (formValues != null)
        {
            foreach (var kv in formValues)
            {
                if (string.IsNullOrWhiteSpace(kv.Key)) continue;
                map[kv.Key] = kv.Value ?? "";
            }
        }
        return map;
    }

    public static string BuildSummary(
        SailSummarySettingDto? setting,
        IReadOnlyDictionary<string, string>? formValues)
    {
        formValues ??= new Dictionary<string, string>();
        IEnumerable<string> keys;
        if (setting?.Enable == true && setting.Summary is { Count: > 0 })
            keys = setting.Summary;
        else
            keys = formValues.Keys.Take(3);

        var parts = new List<string>();
        foreach (var key in keys)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (!formValues.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
            {
                foreach (var kv in formValues)
                {
                    if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(kv.Value))
                    {
                        v = kv.Value;
                        break;
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(v))
                parts.Add(v!.Trim());
        }
        return string.Join(" · ", parts);
    }

    public static string FormatProcessNo(SailProcessIdRuleDto rule, string infix, int seq)
    {
        var len = rule.Length < 5 ? 5 : rule.Length;
        var seqPart = Math.Max(1, seq).ToString().PadLeft(len, '0');
        return (rule.Prefix ?? "") + (infix ?? "") + (rule.Postfix ?? "") + seqPart;
    }

    public static string ResolveInfixBucket(string? infix)
    {
        var now = DateTime.Now;
        return (infix ?? "").Trim().ToUpperInvariant() switch
        {
            "DAY" => now.ToString("yyyyMMdd"),
            "HOUR" => now.ToString("yyyyMMddHH"),
            "MINUTE" => now.ToString("yyyyMMddHHmm"),
            "SECOND" => now.ToString("yyyyMMddHHmmss"),
            _ => ""
        };
    }
}
